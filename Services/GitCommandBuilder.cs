using System.Collections.Generic;
using GitTreeManager.Models;

namespace GitTreeManager.Services
{
    /// <summary>终端命令行里的一段视觉 token；MainForm 按 Kind 上色。</summary>
    public struct TermToken
    {
        public string Text;
        public TermTokenKind Kind;
        public TermToken(string text, TermTokenKind kind) { Text = text; Kind = kind; }
    }

    public enum TermTokenKind
    {
        Plain,      // 空格、无分类
        Meta,       // [N/M] 序号前缀
        Cmd,        // "git" 本体
        Keyword,    // 子命令：config / worktree / add / init / clone / fsck / ...
        Option,     // 选项：-C / -b / --local / --worktree / --prune=now / ...
        Path,       // Windows 绝对路径
        Constant,   // true / false
        EnvVar,     // GIT_COMMITTER_NAME=...
        Value       // 其他参数（值）
    }

    /// <summary>
    /// 一条待执行的 git 调用。Args 不含 "git" 本身。
    /// 支持三种元信息（供 GitRunner 与 Display 使用）：
    ///   WorkingDirectory  进程工作目录；一般用 `-C &lt;repo&gt;` 更直观，此项留空。
    ///   Env               环境变量；值里可以写 {key} 占位符引用之前捕获的 stdout。
    ///   CaptureStdoutAs   若非空，把该命令的 stdout Trim 后存入 vars[key]，供后续命令占位符使用。
    /// </summary>
    public sealed class GitCommand
    {
        private static readonly HashSet<string> Subcommands = new HashSet<string>
        {
            "init","clone","config","worktree","add","remove","prune","list",
            "fsck","reflog","expire","gc","fetch","pull","push","remote","update",
            "commit","amend","reset","checkout","switch","merge","rebase",
            "show","log","status","diff","branch","tag","rev-parse","rev-list",
            "count-objects","verify-pack","ls-files","ls-remote","symbolic-ref",
            "submodule","stash","blame","bisect","cherry-pick","revert"
        };

        public string[] Args { get; private set; }
        public string WorkingDirectory { get; set; }
        public Dictionary<string, string> Env { get; private set; }
        public string CaptureStdoutAs { get; set; }

        public GitCommand(params string[] args)
        {
            Args = args ?? new string[0];
        }

        public GitCommand WithEnv(string key, string value)
        {
            if (Env == null) Env = new Dictionary<string, string>();
            Env[key] = value;
            return this;
        }

        public GitCommand CaptureAs(string key)
        {
            CaptureStdoutAs = key;
            return this;
        }

        public string Display
        {
            get
            {
                var sb = new System.Text.StringBuilder();
                if (Env != null)
                {
                    foreach (var kv in Env)
                    {
                        sb.Append(kv.Key).Append('=').Append(CommandLineEscaper.Escape(kv.Value)).Append(' ');
                    }
                }
                sb.Append("git");
                foreach (var a in Args)
                {
                    sb.Append(' ').Append(CommandLineEscaper.Escape(a));
                }
                return sb.ToString();
            }
        }

        /// <summary>拆成上色 token 序列；MainForm 的终端渲染器逐段贴色。</summary>
        public IList<TermToken> Colorize(string prefix = null)
        {
            var list = new List<TermToken>();
            if (!string.IsNullOrEmpty(prefix)) list.Add(new TermToken(prefix, TermTokenKind.Meta));
            if (Env != null)
            {
                bool first = true;
                foreach (var kv in Env)
                {
                    if (!first) list.Add(new TermToken(" ", TermTokenKind.Plain));
                    list.Add(new TermToken(kv.Key + "=" + CommandLineEscaper.Escape(kv.Value), TermTokenKind.EnvVar));
                    first = false;
                }
                if (!first) list.Add(new TermToken(" ", TermTokenKind.Plain));
            }
            list.Add(new TermToken("git", TermTokenKind.Cmd));
            for (int i = 0; i < Args.Length; i++)
            {
                list.Add(new TermToken(" ", TermTokenKind.Plain));
                string a = Args[i];
                list.Add(new TermToken(CommandLineEscaper.Escape(a), Classify(a)));
            }
            return list;
        }

        private static TermTokenKind Classify(string arg)
        {
            if (string.IsNullOrEmpty(arg)) return TermTokenKind.Value;
            if (arg == "true" || arg == "false") return TermTokenKind.Constant;
            if (arg.Length >= 2 && arg[0] == '-') return TermTokenKind.Option;
            if (IsWindowsPath(arg)) return TermTokenKind.Path;
            if (Subcommands.Contains(arg)) return TermTokenKind.Keyword;
            return TermTokenKind.Value;
        }

        private static bool IsWindowsPath(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            if (s.Length >= 3 && char.IsLetter(s[0]) && s[1] == ':' && (s[2] == '\\' || s[2] == '/')) return true;
            if (s.StartsWith("\\\\")) return true;
            return false;
        }
    }

    /// <summary>把 AppSettings 翻译成 git 命令序列。纯拼装，不执行，方便单测和 dry-run 预览。</summary>
    public class GitCommandBuilder
    {
        // ---------- 仓库级：仅创建 / 克隆 ----------
        // 由 grpRepo 右侧主操作按钮"创建/克隆"触发；只做仓库初始化和 worktreeConfig 开关，
        // 不涉及 worktree 添加。要跑完整流水线仍用 BuildInitAndWorktrees（Tab 里的"执行"按钮）。
        public IList<GitCommand> BuildInitOrCloneOnly(AppSettings s)
        {
            var list = new List<GitCommand>();
            if (s == null || string.IsNullOrWhiteSpace(s.RepoPath)) return list;
            if (s.Mode == RepoMode.New)
                list.Add(new GitCommand("init", "-b", s.DefaultBranch ?? "main", s.RepoPath));
            else
            {
                if (!string.IsNullOrWhiteSpace(s.DefaultBranch))
                    list.Add(new GitCommand("clone", "-b", s.DefaultBranch, s.RemoteUrl ?? "", s.RepoPath));
                else
                    list.Add(new GitCommand("clone", s.RemoteUrl ?? "", s.RepoPath));
            }
            list.Add(new GitCommand("-C", s.RepoPath, "config", "extensions.worktreeConfig", "true"));
            return list;
        }

        // ---------- Worktree 流水线（Tab 内 "执行" 按钮用） ----------
        // 只处理表格里的每一行，不再做 init/clone（那是 grpRepo 右侧 "创建/克隆" 的职责）。
        // IsMain=true 的行：不 worktree add；config 用 --local 前缀。
        // IsMain=false 的行：worktree add（branchExists 探针决定要不要 -b）；config 用 --worktree。
        public IList<GitCommand> BuildWorktreeApply(AppSettings s, System.Func<string, bool> branchExists = null)
        {
            var list = new List<GitCommand>();
            if (s == null || string.IsNullOrWhiteSpace(s.RepoPath)) return list;
            // 保证 per-worktree config 支持开启；幂等
            list.Add(new GitCommand("-C", s.RepoPath, "config", "extensions.worktreeConfig", "true"));
            foreach (var en in s.Entries ?? new List<WorktreeEntry>())
            {
                if (en == null) continue;
                string path = (en.Path ?? "").Trim();
                if (string.IsNullOrWhiteSpace(path)) continue;
                if (en.IsMain)
                {
                    if (!string.IsNullOrWhiteSpace(en.UserName))
                        list.Add(new GitCommand("-C", path, "config", "--local", "user.name", en.UserName));
                    if (!string.IsNullOrWhiteSpace(en.UserEmail))
                        list.Add(new GitCommand("-C", path, "config", "--local", "user.email", en.UserEmail));
                    continue;
                }
                bool exists = false;
                if (branchExists != null && !string.IsNullOrWhiteSpace(en.Branch))
                {
                    try { exists = branchExists(en.Branch); } catch { exists = false; }
                }
                if (exists)
                    list.Add(new GitCommand("-C", s.RepoPath, "worktree", "add", path, en.Branch));
                else
                    list.Add(new GitCommand("-C", s.RepoPath, "worktree", "add", path, "-b", en.Branch ?? ""));
                if (!string.IsNullOrWhiteSpace(en.UserName))
                    list.Add(new GitCommand("-C", path, "config", "--worktree", "user.name", en.UserName));
                if (!string.IsNullOrWhiteSpace(en.UserEmail))
                    list.Add(new GitCommand("-C", path, "config", "--worktree", "user.email", en.UserEmail));
            }
            return list;
        }

        // ---------- 功能 1：清理多余提交和引用记录 ----------
        // 语义：不在任何分支树上的孤儿 commit + dangling blob/tree；以及 reflog 里对它们的引用记录。
        // 顺序：fsck 列清单 → reflog expire 释放引用 → prune 删对象 → gc 收尾打包。
        // 缺 reflog expire 的话，对象仍被 reflog 视作可达，prune 不会动。
        public IList<GitCommand> BuildCleanOrphanCommits(AppSettings s)
        {
            var list = new List<GitCommand>();
            if (s == null || string.IsNullOrWhiteSpace(s.RepoPath)) return list;
            list.Add(new GitCommand("-C", s.RepoPath, "fsck", "--full", "--unreachable", "--dangling", "--no-reflogs").CaptureAs("fsck_report"));
            list.Add(new GitCommand("-C", s.RepoPath, "reflog", "expire", "--expire=now", "--all"));
            list.Add(new GitCommand("-C", s.RepoPath, "prune", "--expire=now", "--verbose"));
            list.Add(new GitCommand("-C", s.RepoPath, "gc", "--prune=now"));
            return list;
        }

        // ---------- 功能 2：清理已合并分支 ----------
        // 语义：已合并进默认分支、且不是任何 worktree 当前 checkout、也不是默认分支/HEAD 的本地分支。
        // git 会拒绝 `branch -d` 删除被 worktree 占用的分支（"used by worktree at"），所以必须先取锁定集。
        // 本方法只产扫描命令；上层解析 fsck/wt 输出 → 弹勾选 → 追加 `git branch -d <x>`。
        public IList<GitCommand> BuildCleanMergedBranches(AppSettings s)
        {
            var list = new List<GitCommand>();
            if (s == null || string.IsNullOrWhiteSpace(s.RepoPath)) return list;
            string target = string.IsNullOrWhiteSpace(s.DefaultBranch) ? "main" : s.DefaultBranch;
            list.Add(new GitCommand("-C", s.RepoPath, "worktree", "list", "--porcelain").CaptureAs("worktree_porcelain"));
            list.Add(new GitCommand("-C", s.RepoPath, "branch", "--merged", target, "--format=%(refname:short)").CaptureAs("merged_branches"));
            list.Add(new GitCommand("-C", s.RepoPath, "rev-parse", "--abbrev-ref", "HEAD").CaptureAs("current_branch"));
            // TODO 上层：diff = merged − worktree锁定 − default − current → 弹勾选 → git branch -d
            return list;
        }

        // ---------- 功能 3：更新远端 ----------
        // 语义：所有 remote 的所有分支 + tags + 失效追踪清理 + submodule 按需递归。
        public IList<GitCommand> BuildUpdateAllRemotes(AppSettings s)
        {
            var list = new List<GitCommand>();
            if (s == null || string.IsNullOrWhiteSpace(s.RepoPath)) return list;
            list.Add(new GitCommand("-C", s.RepoPath, "fetch", "--all", "--prune", "--tags",
                "--recurse-submodules=on-demand"));
            list.Add(new GitCommand("-C", s.RepoPath, "remote", "update", "--prune"));
            return list;
        }

        // ---------- 功能 4：仓库磁盘分析 ----------
        // git 侧只跑 count-objects（`verify-pack` 必须紧跟具体 .idx 路径，空跑会 exit=129）。
        // pack Top N / LFS / worktrees 元数据 / reflog 各自占用由 DiskAnalyzer.Report 在文件系统侧扫描。
        public IList<GitCommand> BuildDiskAnalysis(AppSettings s)
        {
            var list = new List<GitCommand>();
            if (s == null || string.IsNullOrWhiteSpace(s.RepoPath)) return list;
            list.Add(new GitCommand("-C", s.RepoPath, "count-objects", "-v", "-H").CaptureAs("count_objects"));
            return list;
        }

        // ---------- 功能 5：对齐最新提交 ----------
        // 语义：让 HEAD 的 committer date/name/email 分别等于 author 的对应字段。
        // 用户示例：GIT_COMMITTER_DATE="$(git show -s --format=%aI HEAD)" git commit --amend --no-edit
        // 这里同样处理 name/email，三个字段一起对齐。
        // 实跑流程：先跑 3 条 show 捕获 stdout → 用 {占位符} 注入到 amend 的 env。
        public IList<GitCommand> BuildAlignLatestCommit(AppSettings s)
        {
            var list = new List<GitCommand>();
            if (s == null || string.IsNullOrWhiteSpace(s.RepoPath)) return list;
            list.Add(new GitCommand("-C", s.RepoPath, "show", "-s", "--format=%an", "HEAD").CaptureAs("author_name"));
            list.Add(new GitCommand("-C", s.RepoPath, "show", "-s", "--format=%ae", "HEAD").CaptureAs("author_email"));
            list.Add(new GitCommand("-C", s.RepoPath, "show", "-s", "--format=%aI", "HEAD").CaptureAs("author_date_iso"));
            list.Add(new GitCommand("-C", s.RepoPath, "commit", "--amend", "--no-edit")
                .WithEnv("GIT_COMMITTER_NAME", "{author_name}")
                .WithEnv("GIT_COMMITTER_EMAIL", "{author_email}")
                .WithEnv("GIT_COMMITTER_DATE", "{author_date_iso}")
                .CaptureAs("amend_result"));
            return list;
        }
    }
}
