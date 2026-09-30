using System;

namespace GitTreeManager.Services
{
    /// <summary>
    /// Windows 路径规范化：`git worktree list --porcelain` 等 git 输出里路径常带正斜杠 (C:/foo/bar)，
    /// 而用户在 grpRepo 里输入的通常是反斜杠 (C:\foo\bar)。统一按 Windows 习惯用反斜杠。
    /// 只对"看起来像 Windows 绝对路径"的字符串生效，其他字符串原样返回 (URL、相对路径等)。
    /// </summary>
    public static class PathUtil
    {
        public static string ToNative(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            // 盘符绝对路径：X:\ 或 X:/
            if (s.Length >= 3 && char.IsLetter(s[0]) && s[1] == ':' && (s[2] == '\\' || s[2] == '/'))
                return s.Replace('/', '\\');
            // UNC：\\server\share 或 //server/share
            if (s.StartsWith("//") || s.StartsWith("\\\\"))
                return s.Replace('/', '\\');
            return s;
        }
    }
}
