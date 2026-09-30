using System.Text;

namespace GitTreeManager.Services
{
    /// <summary>
    /// Windows CommandLineToArgvW 兼容的参数转义。
    /// 用于把 argv 数组拼成 CreateProcess 的命令行字符串，避免含空格 / 引号 / 反斜杠的路径被吞。
    /// 规则参考 MSVC 的 quoting 算法：奇数个反斜杠紧跟引号时才把反斜杠翻倍并转义引号。
    /// </summary>
    internal static class CommandLineEscaper
    {
        public static string Escape(string arg)
        {
            if (string.IsNullOrEmpty(arg)) return "\"\"";
            bool needQuote = arg.IndexOfAny(new[] { ' ', '\t', '"', '\'' }) >= 0;
            if (!needQuote) return arg;

            var sb = new StringBuilder();
            sb.Append('"');
            int backslashes = 0;
            foreach (var ch in arg)
            {
                if (ch == '\\')
                {
                    backslashes++;
                    continue;
                }
                if (ch == '"')
                {
                    sb.Append('\\', backslashes * 2 + 1);
                    sb.Append('"');
                }
                else
                {
                    sb.Append('\\', backslashes);
                    sb.Append(ch);
                }
                backslashes = 0;
            }
            // 结尾反斜杠：紧邻闭合引号前需要翻倍并再转义
            sb.Append('\\', backslashes * 2);
            sb.Append('"');
            return sb.ToString();
        }

        public static string Join(string[] args)
        {
            if (args == null || args.Length == 0) return "";
            var sb = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                if (i > 0) sb.Append(' ');
                sb.Append(Escape(args[i]));
            }
            return sb.ToString();
        }
    }
}
