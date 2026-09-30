using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitTreeManager.Services
{
    /// <summary>
    /// 文件系统侧磁盘扫描：补充 `git count-objects` 给不出的 pack / LFS / worktrees 元数据 / reflog 各自占用。
    /// 只读；不修改任何文件。所有目录访问都用 try/catch 静默跳过权限错误，保证不炸。
    /// </summary>
    public static class DiskAnalyzer
    {
        public static void Report(ITerminalSink sink, string repoPath, int topN = 10)
        {
            if (sink == null) return;
            if (string.IsNullOrWhiteSpace(repoPath) || !Directory.Exists(repoPath))
            {
                sink.Write("  [skip] 仓库路径无效：" + repoPath, TermKind.Err);
                return;
            }
            string gitDir = Path.Combine(repoPath, ".git");
            if (!Directory.Exists(gitDir))
            {
                // worktree 下 .git 是文件指回主仓库；跟随指针找到主 .git
                var resolved = ResolveMainGitDir(gitDir, repoPath);
                if (resolved == null) { sink.Write("  [skip] 未找到 .git 目录", TermKind.Err); return; }
                gitDir = resolved;
            }
            sink.Write("  .git 目录: " + gitDir, TermKind.Meta);

            // 1) pack 目录 Top N
            var packDir = Path.Combine(gitDir, "objects", "pack");
            var packs = SafeFiles(packDir, "*.pack");
            long packTotal = packs.Sum(p => p.Length);
            sink.Write("  [pack] " + packs.Count + " 个, 总 " + Human(packTotal), TermKind.Std);
            foreach (var p in packs.OrderByDescending(f => f.Length).Take(Math.Max(1, topN)))
                sink.Write("    " + Right(Human(p.Length), 10) + "  " + p.Name, TermKind.Std);

            // 2) 松散对象数量与体积
            var loose = DirStatLoose(Path.Combine(gitDir, "objects"));
            sink.Write("  [松散对象] " + loose.count + " 个, " + Human(loose.bytes), TermKind.Std);

            // 3) LFS
            var lfs = DirStat(Path.Combine(gitDir, "lfs", "objects"));
            if (lfs.count > 0) sink.Write("  [LFS] " + lfs.count + " 个, " + Human(lfs.bytes), TermKind.Std);

            // 4) worktrees 元数据（每个已登记 worktree 在 .git/worktrees/<name> 下的一份目录）
            var wtDirs = SafeSubdirs(Path.Combine(gitDir, "worktrees"));
            if (wtDirs.Count > 0)
            {
                sink.Write("  [已登记的 worktrees] " + wtDirs.Count, TermKind.Std);
                foreach (var d in wtDirs)
                {
                    var st = DirStat(d.FullName);
                    sink.Write("    " + Right(Human(st.bytes), 10) + "  " + d.Name, TermKind.Std);
                }
            }

            // 5) reflog
            var logs = DirStat(Path.Combine(gitDir, "logs"));
            sink.Write("  [reflog] " + logs.count + " 个文件, " + Human(logs.bytes), TermKind.Std);

            // 6) hooks / 其他杂项 (简单汇总)
            var misc = DirStat(Path.Combine(gitDir, "hooks"));
            sink.Write("  [hooks] " + misc.count + " 个文件, " + Human(misc.bytes), TermKind.Std);

            // 7) 整体 .git
            var total = DirStat(gitDir);
            sink.Write("══ .git 总占用 " + Human(total.bytes) + " (" + total.count + " 个文件) ══", TermKind.Done);

            // 8) 工作副本大小（排除 .git）
            var workTotal = DirStatExcluding(repoPath, gitDir);
            sink.Write("══ 工作副本 (不含 .git) " + Human(workTotal.bytes) + " (" + workTotal.count + " 个文件) ══", TermKind.Done);
        }

        private static string ResolveMainGitDir(string gitPointerPath, string repoPath)
        {
            try
            {
                if (File.Exists(gitPointerPath))
                {
                    string txt = File.ReadAllText(gitPointerPath).Trim();
                    if (txt.StartsWith("gitdir:"))
                    {
                        string p = txt.Substring("gitdir:".Length).Trim();
                        if (!Path.IsPathRooted(p)) p = Path.GetFullPath(Path.Combine(repoPath, p));
                        if (Directory.Exists(p))
                        {
                            // p 通常是 .git/worktrees/<name>；主仓库 .git 是它的祖父目录
                            var parent = Directory.GetParent(p);
                            if (parent != null)
                            {
                                var grand = parent.Parent;
                                if (grand != null && Directory.Exists(grand.FullName) &&
                                    string.Equals(grand.Parent != null ? grand.Parent.Name : "", "worktrees", StringComparison.OrdinalIgnoreCase))
                                    return parent.FullName;
                            }
                            return p;
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        private static List<FileInfo> SafeFiles(string dir, string pattern)
        {
            var list = new List<FileInfo>();
            try
            {
                if (!Directory.Exists(dir)) return list;
                foreach (var f in Directory.EnumerateFiles(dir, pattern))
                {
                    try { list.Add(new FileInfo(f)); } catch { }
                }
            }
            catch { }
            return list;
        }

        private static List<DirectoryInfo> SafeSubdirs(string dir)
        {
            var list = new List<DirectoryInfo>();
            try
            {
                if (!Directory.Exists(dir)) return list;
                foreach (var d in Directory.EnumerateDirectories(dir)) list.Add(new DirectoryInfo(d));
            }
            catch { }
            return list;
        }

        /// <summary>数 loose objects：.git/objects/xx/* 里排除 pack/ 与 info/。</summary>
        private static (int count, long bytes) DirStatLoose(string objectsDir)
        {
            int c = 0; long b = 0;
            try
            {
                if (!Directory.Exists(objectsDir)) return (0, 0);
                foreach (var sub in Directory.EnumerateDirectories(objectsDir))
                {
                    string name = Path.GetFileName(sub);
                    if (string.Equals(name, "pack", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(name, "info", StringComparison.OrdinalIgnoreCase)) continue;
                    foreach (var f in SafeFiles(sub, "*")) { c++; b += f.Length; }
                }
            }
            catch { }
            return (c, b);
        }

        private static (int count, long bytes) DirStat(string dir)
        {
            int c = 0; long b = 0;
            try
            {
                if (!Directory.Exists(dir)) return (0, 0);
                var stack = new Stack<string>();
                stack.Push(dir);
                while (stack.Count > 0)
                {
                    string cur = stack.Pop();
                    try
                    {
                        foreach (var f in Directory.EnumerateFiles(cur))
                        {
                            try { var fi = new FileInfo(f); c++; b += fi.Length; } catch { }
                        }
                        foreach (var d in Directory.EnumerateDirectories(cur)) stack.Push(d);
                    }
                    catch { }
                }
            }
            catch { }
            return (c, b);
        }

        /// <summary>递归统计 root 目录，跳过 excluded 及其所有子目录。</summary>
        private static (int count, long bytes) DirStatExcluding(string root, string excluded)
        {
            int c = 0; long b = 0;
            string ex = Path.GetFullPath(excluded).TrimEnd('\\', '/');
            try
            {
                if (!Directory.Exists(root)) return (0, 0);
                var stack = new Stack<string>();
                stack.Push(root);
                while (stack.Count > 0)
                {
                    string cur = stack.Pop();
                    try
                    {
                        foreach (var f in Directory.EnumerateFiles(cur))
                        {
                            try { var fi = new FileInfo(f); c++; b += fi.Length; } catch { }
                        }
                        foreach (var d in Directory.EnumerateDirectories(cur))
                        {
                            string full = Path.GetFullPath(d).TrimEnd('\\', '/');
                            if (string.Equals(full, ex, StringComparison.OrdinalIgnoreCase)) continue;
                            stack.Push(d);
                        }
                    }
                    catch { }
                }
            }
            catch { }
            return (c, b);
        }

        private static string Human(long b)
        {
            string[] u = { "B", "KiB", "MiB", "GiB", "TiB" };
            double v = b; int i = 0;
            while (v >= 1024 && i < u.Length - 1) { v /= 1024; i++; }
            return v.ToString("0.##") + " " + u[i];
        }

        private static string Right(string s, int w)
        {
            if (s.Length >= w) return s;
            return new string(' ', w - s.Length) + s;
        }
    }
}
