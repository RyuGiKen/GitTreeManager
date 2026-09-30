using System;

namespace GitTreeManager.Services
{
    /// <summary>
    /// 探测系统里最常见的 git.exe 位置。按候选顺序试第一个存在的文件；
    /// 都找不到时回退到 "git"（依赖 PATH 由 CreateProcess 自己解析）。
    /// 用户仍可在 UI 里手动改。
    /// </summary>
    internal static class GitLocator
    {
        public static string Detect()
        {
            foreach (var p in Candidates())
            {
                try { if (!string.IsNullOrEmpty(p) && System.IO.File.Exists(p)) return p; }
                catch { }
            }
            return "git";
        }

        private static System.Collections.Generic.IEnumerable<string> Candidates()
        {
            string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string pfx = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string local = Environment.GetEnvironmentVariable("LOCALAPPDATA");
            string gitHome = Environment.GetEnvironmentVariable("GIT_HOME");

            if (!string.IsNullOrEmpty(gitHome))
            {
                yield return Combine(gitHome, "bin", "git.exe");
                yield return Combine(gitHome, "cmd", "git.exe");
            }
            if (!string.IsNullOrEmpty(pf))
            {
                yield return Combine(pf, "Git", "cmd", "git.exe");
                yield return Combine(pf, "Git", "bin", "git.exe");
                yield return Combine(pf, "Git", "bin", "git-cmd.exe");
            }
            if (!string.IsNullOrEmpty(pfx))
            {
                yield return Combine(pfx, "Git", "cmd", "git.exe");
                yield return Combine(pfx, "Git", "bin", "git.exe");
            }
            if (!string.IsNullOrEmpty(local))
            {
                yield return Combine(local, "Programs", "Git", "cmd", "git.exe");
                yield return Combine(local, "Programs", "Git", "bin", "git.exe");
            }
        }

        private static string Combine(string a, params string[] rest)
        {
            if (string.IsNullOrEmpty(a)) return null;
            var path = a;
            foreach (var r in rest) path = System.IO.Path.Combine(path, r);
            return path;
        }
    }
}
