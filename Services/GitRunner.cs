using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace GitTreeManager.Services
{
    /// <summary>
    /// 顺序执行一批 GitCommand。
    /// dryRun 模式仅把命令与简短提示写进日志，不启动进程。
    /// 每条命令独立超时（默认 60s），失败即停。
    /// 支持命令间数据流：CaptureStdoutAs 把 stdout Trim 后存入 vars，后续命令的 Args/Env 里 {key} 占位符自动展开。
    /// </summary>
    public class GitRunner
    {
        private static readonly Regex Placeholder = new Regex(@"\{([a-zA-Z_][a-zA-Z0-9_]*)\}", RegexOptions.Compiled);

        public int TimeoutMs { get; set; } = 60_000;
        public bool StopOnFirstFailure { get; set; } = true;

        public bool RunAll(string gitExe, IList<GitCommand> commands, bool dryRun, Action<string> onLine)
        {
            if (commands == null || commands.Count == 0)
            {
                if (onLine != null) onLine("[skip] 没有待执行的命令。");
                return true;
            }
            if (string.IsNullOrWhiteSpace(gitExe)) gitExe = "git";

            var vars = new Dictionary<string, string>(StringComparer.Ordinal);
            bool allOk = true;
            for (int i = 0; i < commands.Count; i++)
            {
                var raw = commands[i];
                if (raw == null) continue;
                var cmd = Expand(raw, vars);
                string header = "[" + (i + 1) + "/" + commands.Count + "] " + cmd.Display;
                if (onLine != null) onLine(header);

                if (dryRun)
                {
                    if (onLine != null) onLine("    (dry-run: 未执行)");
                    continue;
                }

                int exit;
                string stdOut, stdErr;
                try
                {
                    exit = ExecuteOnce(gitExe, cmd, out stdOut, out stdErr);
                }
                catch (Exception ex)
                {
                    if (onLine != null) onLine("    !! 启动失败: " + ex.Message);
                    allOk = false;
                    if (StopOnFirstFailure) break;
                    continue;
                }

                if (onLine != null && !string.IsNullOrEmpty(stdOut)) EmitBlock(onLine, "    out> ", stdOut);
                if (onLine != null && !string.IsNullOrEmpty(stdErr)) EmitBlock(onLine, "    err> ", stdErr);
                if (onLine != null) onLine("    exit = " + exit);

                if (!string.IsNullOrEmpty(cmd.CaptureStdoutAs))
                {
                    vars[cmd.CaptureStdoutAs] = (stdOut ?? "").Trim();
                }

                if (exit != 0)
                {
                    allOk = false;
                    if (StopOnFirstFailure) break;
                }
            }
            if (onLine != null) onLine(allOk ? "—— 全部完成 ——" : "—— 因失败中断 ——");
            return allOk;
        }

        public string DetectGitVersion(string gitExe)
        {
            try
            {
                string outp, errp;
                ExecuteOnce(string.IsNullOrWhiteSpace(gitExe) ? "git" : gitExe,
                    new GitCommand("--version"), out outp, out errp);
                if (!string.IsNullOrWhiteSpace(outp))
                {
                    int nl = outp.IndexOfAny(new[] { '\r', '\n' });
                    return nl > 0 ? outp.Substring(0, nl) : outp.Trim();
                }
            }
            catch { }
            return null;
        }

        /// <summary>把 vars 中的 key 展开到 Args 与 Env 的 {key} 占位符上。dry-run 时也调用，方便看到"如果真跑会长啥样"。</summary>
        private static GitCommand Expand(GitCommand src, Dictionary<string, string> vars)
        {
            var args = new string[src.Args.Length];
            for (int i = 0; i < src.Args.Length; i++) args[i] = Sub(src.Args[i], vars);
            var dst = new GitCommand(args)
            {
                WorkingDirectory = src.WorkingDirectory,
                CaptureStdoutAs = src.CaptureStdoutAs
            };
            if (src.Env != null)
            {
                foreach (var kv in src.Env) dst.WithEnv(kv.Key, Sub(kv.Value, vars));
            }
            return dst;
        }

        private static string Sub(string input, Dictionary<string, string> vars)
        {
            if (string.IsNullOrEmpty(input) || vars.Count == 0) return input;
            return Placeholder.Replace(input, m =>
            {
                string key = m.Groups[1].Value;
                string val;
                return vars.TryGetValue(key, out val) ? val : m.Value;
            });
        }

        private int ExecuteOnce(string gitExe, GitCommand cmd, out string stdOut, out string stdErr)
        {
            var psi = new ProcessStartInfo
            {
                FileName = gitExe,
                Arguments = BuildArgs(cmd.Args),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
            if (!string.IsNullOrWhiteSpace(cmd.WorkingDirectory)) psi.WorkingDirectory = cmd.WorkingDirectory;
            if (cmd.Env != null)
            {
                foreach (var kv in cmd.Env) psi.EnvironmentVariables[kv.Key] = kv.Value ?? "";
            }

            var osb = new StringBuilder();
            var esb = new StringBuilder();
            using (var p = new Process { StartInfo = psi })
            {
                p.OutputDataReceived += (s, e) => { if (e.Data != null) osb.AppendLine(e.Data); };
                p.ErrorDataReceived += (s, e) => { if (e.Data != null) esb.AppendLine(e.Data); };
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                if (!p.WaitForExit(TimeoutMs))
                {
                    try { p.Kill(); } catch { }
                    stdOut = osb.ToString();
                    stdErr = esb.ToString();
                    throw new TimeoutException("git 命令超时 " + TimeoutMs + " ms，已终止");
                }
                int spin = 0;
                while (spin < 20 && osb.Length == 0 && esb.Length == 0 && !p.HasExited) { Thread.Sleep(10); spin++; }
                int exit = p.ExitCode;
                stdOut = osb.ToString();
                stdErr = esb.ToString();
                return exit;
            }
        }

        private static string BuildArgs(string[] args)
        {
            if (args == null || args.Length == 0) return "";
            var parts = new string[args.Length];
            for (int i = 0; i < args.Length; i++) parts[i] = CommandLineEscaper.Escape(args[i]);
            return string.Join(" ", parts);
        }

        private static void EmitBlock(Action<string> onLine, string prefix, string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            foreach (var line in text.Replace("\r\n", "\n").Split('\n'))
            {
                if (line.Length == 0) continue;
                onLine(prefix + line);
            }
        }
    }
}
