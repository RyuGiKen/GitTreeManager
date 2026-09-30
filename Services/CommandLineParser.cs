using System.Collections.Generic;
using System.Text;

namespace GitTreeManager.Services
{
    /// <summary>
    /// 反向解析 Windows 命令行字符串为 argv，遵循 CommandLineToArgvW 规则：
    ///   1) 空格/Tab 在非引号段内分词。
    ///   2) 双引号切换引号态；引号内空格不分词。
    ///   3) 连续 N 个反斜杠紧跟 " → 保留 N/2 个反斜杠，再按 N 奇偶决定把 " 视为字面量 (奇) 或段边界 (偶)。
    ///   4) 反斜杠不跟 " → 全部字面量保留。
    /// </summary>
    internal static class CommandLineParser
    {
        public static string[] Split(string commandLine)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(commandLine)) return result.ToArray();

            int i = 0;
            int n = commandLine.Length;
            while (i < n)
            {
                // 跳过分隔空白
                while (i < n && (commandLine[i] == ' ' || commandLine[i] == '\t')) i++;
                if (i >= n) break;

                var sb = new StringBuilder();
                bool inQuotes = false;
                bool tokenStarted = false;
                while (i < n)
                {
                    char c = commandLine[i];
                    if (c == '\\')
                    {
                        int back = 0;
                        while (i < n && commandLine[i] == '\\') { back++; i++; }
                        if (i < n && commandLine[i] == '"')
                        {
                            sb.Append('\\', back / 2);
                            if ((back & 1) == 1)
                            {
                                sb.Append('"');
                                i++;
                            }
                            else
                            {
                                inQuotes = !inQuotes;
                                tokenStarted = true;
                                i++;
                            }
                        }
                        else
                        {
                            sb.Append('\\', back);
                            tokenStarted = true;
                        }
                        continue;
                    }
                    if (c == '"')
                    {
                        inQuotes = !inQuotes;
                        tokenStarted = true;
                        i++;
                        continue;
                    }
                    if (!inQuotes && (c == ' ' || c == '\t')) break;
                    sb.Append(c);
                    tokenStarted = true;
                    i++;
                }
                if (tokenStarted) result.Add(sb.ToString());
                else if (sb.Length > 0) result.Add(sb.ToString());
            }
            return result.ToArray();
        }
    }
}
