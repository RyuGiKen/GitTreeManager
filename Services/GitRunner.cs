using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace GitTreeManager.Services
{
    /// <summary>终端输出的语义类型，UI 层按类型上色。</summary>
    public enum TermKind
    {
        /// <summary>提示符 / 环境头（灰）</summary>
        Prompt,
        /// <summary>用户/程序执行的命令本身（蓝）</summary>
        Cmd,
        /// <summary>stdout（默认前景）</summary>
        Std,
        /// <summary>stderr（红）</summary>
        Err,
        /// <summary>退出码 / 状态说明（暗灰）</summary>
        Meta,
        /// <summary>整批成功（绿）</summary>
        Done,
        /// <summary>整批失败 / 异常（红加粗）</summary>
        Fail
    }

    /// <summary>终端输出接收器；UI 层实现这个接口把行写入 RichTextBox 并上色。</summary>
    public interface ITerminalSink
    {
        void Write(string line, TermKind kind);
        /// <summary>写一条 git 命令，UI 层按 token 上色；index/total 用于 [N/M] 前缀，null 表示不加。</summary>
        void WriteCommand(GitCommand cmd, int? index, int? total);
    }

    /// <summary>
    /// 顺序执行一批 GitCommand。支持 dry-run / 超时 / 环境变量注入 / 跨命令 stdout 捕获 / 用户中断。
    /// StopOnFirstFailure 默认 true（一行失败即中断全部）。
    /// </summary>
    public class GitRunner
    {
        private static readonly Regex Placeholder = new Regex(@"\{([a-zA-Z_][a-zA-Z0-9_]*)\}", RegexOptions.Compiled);
        private volatile Process _current;
        private volatile bool _cancelRequested;

        public int TimeoutMs { get; set; } = 60_000;
        public bool StopOnFirstFailure { get; set; } = true;

        /// <summary>请求中断当前正在运行的 git 子进程；对 dry-run / 空闲状态无副作用。</summary>
        public void Cancel()
        {
            _cancelRequested = true;
            var p = _current;
            if (p != null)
            {
                try { if (!p.HasExited) p.Kill(); } catch { }
            }
        }

        public bool RunAll(string gitExe, IList<GitCommand> commands, bool dryRun, ITerminalSink sink)
        {
            _cancelRequested = false;
            if (commands == null || commands.Count == 0)
            {
                if (sink != null) sink.Write("[skip] 没有待执行的命令。", TermKind.Meta);
                return true;
            }
            if (string.IsNullOrWhiteSpace(gitExe)) gitExe = "git";

            var vars = new Dictionary<string, string>(StringComparer.Ordinal);
            bool allOk = true;
            for (int i = 0; i < commands.Count; i++)
            {
                if (_cancelRequested)
                {
                    if (sink != null) sink.Write("—— 用户中断 ——", TermKind.Fail);
                    allOk = false;
                    break;
                }
                var raw = commands[i];
                if (raw == null) continue;
                var cmd = Expand(raw, vars);
                if (sink != null) sink.WriteCommand(cmd, i + 1, commands.Count);

                if (dryRun)
                {
                    if (sink != null) sink.Write("    (dry-run: 未执行)", TermKind.Meta);
                    continue;
                }

                int exit;
                string stdOut, stdErr;
                try
                {
                    exit = ExecuteOnce(gitExe, cmd, sink, out stdOut, out stdErr);
                }
                catch (Exception ex)
                {
                    if (sink != null) sink.Write("    !! 启动失败: " + ex.Message, TermKind.Err);
                    allOk = false;
                    if (StopOnFirstFailure) break;
                    continue;
                }

                if (!string.IsNullOrEmpty(cmd.CaptureStdoutAs))
                {
                    vars[cmd.CaptureStdoutAs] = (stdOut ?? "").Trim();
                }

                bool isErr = exit != 0;
                if (sink != null) sink.Write("    exit = " + exit, isErr ? TermKind.Err : TermKind.Meta);
                if (isErr)
                {
                    allOk = false;
                    if (StopOnFirstFailure) break;
                }
            }
            if (sink != null) sink.Write(allOk ? "—— 全部完成 ——" : "—— 因失败中断 ——", allOk ? TermKind.Done : TermKind.Fail);
            return allOk;
        }

        /// <summary>交互式执行单条命令（用户直接输入，非批量流水线）。stdout/stderr 实时写终端。</summary>
        public int RunInteractive(string gitExe, string[] args, string workingDir, ITerminalSink sink)
        {
            _cancelRequested = false;
            if (string.IsNullOrWhiteSpace(gitExe)) gitExe = "git";
            var cmd = new GitCommand(args) { WorkingDirectory = workingDir };
            if (sink != null) sink.WriteCommand(cmd, null, null);
            string so, se;
            try
            {
                int exit = ExecuteOnce(gitExe, cmd, sink, out so, out se);
                if (sink != null) sink.Write("exit = " + exit, exit == 0 ? TermKind.Meta : TermKind.Err);
                return exit;
            }
            catch (Exception ex)
            {
                if (sink != null) sink.Write("!! " + ex.Message, TermKind.Err);
                return -1;
            }
        }

        public string DetectGitVersion(string gitExe)
        {
            try
            {
                string outp, errp;
                ExecuteOnce(string.IsNullOrWhiteSpace(gitExe) ? "git" : gitExe,
                    new GitCommand("--version"), null, out outp, out errp);
                if (!string.IsNullOrWhiteSpace(outp))
                {
                    int nl = outp.IndexOfAny(new[] { '\r', '\n' });
                    return nl > 0 ? outp.Substring(0, nl) : outp.Trim();
                }
            }
            catch { }
            return null;
        }

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

        private int ExecuteOnce(string gitExe, GitCommand cmd, ITerminalSink sink, out string stdOut, out string stdErr)
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
                _current = p;
                p.OutputDataReceived += (s, e) =>
                {
                    if (e.Data == null) return;
                    osb.AppendLine(e.Data);
                    if (sink != null) sink.Write("    " + e.Data, TermKind.Std);
                };
                p.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data == null) return;
                    esb.AppendLine(e.Data);
                    if (sink != null) sink.Write("    " + e.Data, TermKind.Err);
                };
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                bool finished = p.WaitForExit(TimeoutMs);
                if (!finished)
                {
                    try { p.Kill(); } catch { }
                    _current = null;
                    stdOut = osb.ToString();
                    stdErr = esb.ToString();
                    throw new TimeoutException("git 命令超时 " + TimeoutMs + " ms，已终止");
                }
                int exit = p.ExitCode;
                _current = null;
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
    }
}
