# Git Tree Manager

一个 Windows 桌面的 Git 仓库 + worktree 批量配置工具（.NET Framework 4.7.2 WinForms）。目标：把 `git init/clone`、`extensions.worktreeConfig`、`git worktree add`、`git config --worktree user.name/email` 这一套命令行压缩成一次填表 + 一键执行，同时补几个 TortoiseGit 右键一级菜单里没有的常用动作。

## 目录结构

```
Tree/
├─ GitTreeManager.csproj      老式 csproj，TargetFrameworkVersion=v4.7.2
├─ App.config
├─ Program.cs                 入口
├─ MainForm.cs                UI 逻辑 + 事件
├─ MainForm.Designer.cs       控件布局（Designer 手写，4 处同步）
├─ Models/
│  ├─ WorktreeEntry.cs        一行 worktree 配置
│  └─ AppSettings.cs          全局配置 + RepoMode 枚举
├─ Services/
│  ├─ CommandLineEscaper.cs   Windows argv → command line 转义
│  ├─ GitCommandBuilder.cs    纯拼命令，可单测
│  ├─ GitRunner.cs            逐条执行，支持 dry-run / 超时
│  └─ ConfigStore.cs          settings.xml 读写
└─ Properties/AssemblyInfo.cs
```

## 主窗体

顶部 grpRepo：**左区**三行 = 仓库路径 + 模式单选 + 默认分支 + git.exe 路径；**右区**一列 3 个统一 116×25 按钮，与三行左内容顶对齐：Y=25 浏览仓库路径 / Y=57 创建·克隆（文字随 rbNew/rbClone 切换）/ Y=89 浏览 git.exe。git.exe 启动时按 `GIT_HOME → Program Files\Git\cmd → Program Files\Git\bin → Program Files (x86)\Git → %LOCALAPPDATA%\Programs\Git` 顺序自动探测，找不到回退 `git`。

中间 TabControl 两页：
1. **Worktree 列表**：DataGridView 最多 99 行，列 = `# / worktree 路径 / 分支 / user.name / user.email`；工具条按钮统一宽度（80/80/50/50/140/80，间距 6）；底部右侧"预览命令"与"执行"两按钮各 120×28 紧挨右锚；左下 Dry-run 复选框默认勾选。
2. **命令日志**：RichTextBox 显示每条命令 `$ git ...` 与其 stdout/stderr；工具条 4 按钮统一 100 宽 6 间距；支持复制、清空、导出 `.bat` / `.ps1`。

底部 grpCommon：一行 5 个 200×30 按钮，间距 18，全行刚好铺满 grpCommon 内部宽度。

## 命令流水线

`GitCommandBuilder.BuildInitAndWorktrees(settings)` 产出：

1. `git init -b <default> <RepoPath>` 或 `git clone -b <default> <url> <RepoPath>`
2. `git -C <RepoPath> config extensions.worktreeConfig true`
3. 每行 worktree：
   - `git -C <RepoPath> worktree add <path> -b <branch>`
   - `git -C <path> config --worktree user.name <name>`
   - `git -C <path> config --worktree user.email <email>`

**注意**：骨架阶段假定分支不存在，一律加 `-b`。下一轮补自动检测：`git branch --list <b>` 非空则去掉 `-b`；否则失败即停并回报。

## 常用功能按钮

底部一行 5 个按钮，按用户明确给的名称与语义。全部走"预览命令 → 日志页"路径；勾选取消 Dry-run 时二次确认后实跑。

| # | 按钮 | 语义 | 命令流水线 |
|---|---|---|---|
| 1 | 清理多余提交和引用记录 | 不在分支树上的孤儿 commit + dangling 对象；连 reflog 里对它们的引用记录一并释放 | `git fsck --full --unreachable --dangling --no-reflogs` → `git reflog expire --expire=now --all` → `git prune --expire=now -v` → `git gc --prune=now` |
| 2 | 清理已合并分支 | 已合并到默认分支、且不是任何 worktree 当前 checkout、也不是默认分支/HEAD 的本地分支 | 扫描：`git worktree list --porcelain` + `git branch --merged <default> --format=%(refname:short)` + `git rev-parse --abbrev-ref HEAD`；上层算差集 → 弹勾选 → `git branch -d <x>` |
| 3 | 更新远端 | 所有 remote、所有分支、tags、prune 失效追踪、submodule 按需递归 | `git fetch --all --prune --tags --recurse-submodules=on-demand` → `git remote update --prune` |
| 4 | 仓库磁盘分析 | `.git` 内部对象、pack 明细、LFS、worktrees 各自占用 | git 侧：`git count-objects -v -H` + `git verify-pack -v`；文件系统侧由 C# 遍历（下一轮补） |
| 5 | 对齐最新提交 | 让 HEAD 的 committer date/name/email 分别等于 author 的对应字段 | `git show -s --format=%an HEAD` → `%ae` → `%aI` → `GIT_COMMITTER_NAME=... GIT_COMMITTER_EMAIL=... GIT_COMMITTER_DATE=... git commit --amend --no-edit` |

**关键实现细节**
- "清理多余提交和引用记录"：缺 `reflog expire` 的话，对象仍被 reflog 视作可达，`prune` 不会动。这条链是 fsck → reflog expire → prune → gc，顺序不能颠倒。
- "清理已合并分支"：git 会拒绝 `branch -d` 删除被任何 worktree checkout 的分支（`fatal: cannot delete branch 'xxx' used by worktree at ...`），所以必须先 `worktree list --porcelain` 拿锁定集，再从 `branch --merged` 结果里减掉。
- "更新远端"只做 `--prune`（清理本地 `refs/remotes/*`），绝不 `git push --delete`——真实远端分支不能碰。
- "对齐最新提交"用 `ProcessStartInfo.EnvironmentVariables` 注入 `GIT_COMMITTER_*`，值由前 3 条 `git show` 的 stdout 回填；`GitRunner` 支持 `{key}` 占位符跨命令传值（`GitCommand.CaptureStdoutAs` + `.WithEnv(k, v)`）。**不加** `--reset-author`——那个会把 author 也重置成当前 config，我们要反过来（committer ← author）。

## Dry-run 模式

默认开启。预览命令 / 各功能按钮只把 `git ...` 拼好写入日志页，不启动进程。取消勾选后点执行会二次确认，然后逐条 `Process.Start git.exe`，捕获 stdout/stderr 回流日志，单条 60s 超时；任一失败即中断（可通过 `StopOnFirstFailure` 关掉）。

## 持久化

exe 同目录 `settings.xml`。结构：`<settings><Repo/><Options/><Worktrees><Entry .../></Worktrees></settings>`。关窗时自动保存，启动时自动加载。

## 编译

VS 2026 (v18) MSBuild：

```
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" \
  GitTreeManager.csproj -t:Rebuild -p:Configuration=Debug "-p:OutputPath=bin/Verify"
```

`OutputPath` 用正斜杠且整段带引号，避免 Git Bash 反斜杠转义踩坑。

## 已知限制 / 下一轮 TODO

- 表格手动加行不做智能去重、不做路径存在性校验；下一轮加。
- `读取现有 worktree` 按钮当前只打占位日志，未真跑 `git worktree list --porcelain` 回填。
- 常用功能 5 个按钮的**扫描/流水线命令已实装**，Dry-run 关时 `GitRunner` 会按顺序跑并回填 `{key}` 占位符。但：
  - "清理已合并分支"目前只输出扫描命令与捕获，差集计算 + 弹勾选框 + 后续 `git branch -d` 下一轮接入。
  - "仓库磁盘分析" git 侧命令已发，文件系统侧扫描（Top N pack、LFS 目录体积、worktrees 各自占用）下一轮在 C# 里补。
- 无 i18n，中文硬编码。
- 无日志文件持久化，只保留在 UI 内。
