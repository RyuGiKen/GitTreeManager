# Git Tree Manager

Windows 桌面的 Git 仓库 + worktree 批量配置工具（.NET Framework 4.7.2 WinForms）。把 `git init/clone` + `worktree add` + `config --worktree user.name/email` 这套命令压缩成"填表 → 一键执行"，并补几个 TortoiseGit 右键没有的常用动作。

## 快速开始

1. 顶部填**仓库路径**（输入框会实时变底色：绿=已是仓库、橙=待创建、红=普通文件夹）
2. 选**新建**或**克隆已有**，点右侧 `创建` / `克隆` 按钮 → 仓库根 + `extensions.worktreeConfig` 就绪
3. 切到 **Worktree** 页，逐行填 `worktree 路径 / 分支 / user.name / user.email`（或点"读取现有 worktree"回填）
4. 想先看看会跑什么 → 保持 Dry-run 勾选，点 `执行`，命令会打印到终端页
5. 确认无误 → 取消 Dry-run，再点 `执行`，二次确认后逐条实跑（**一行失败即中断全部**）

## 界面

- **顶部 仓库位置**：路径 / 模式 / 默认分支 / git.exe 路径。右侧一列 3 个按钮：浏览仓库、`创建/克隆`（文字随模式切换）、浏览 git.exe。git.exe 启动时自动探测常见安装位（`GIT_HOME` → `Program Files\Git\cmd` → `%LOCALAPPDATA%\Programs\Git` → ...）。
- **Tab 1 · Worktree**：DataGridView，最多 99 行，列 = `# / 类型 / worktree 路径 / 分支 / user.name / user.email`。工具条：添加行 / 删除行 / 上移 / 下移 / 读取现有 worktree / 清空；右下角 `执行` 只跑本表流水线。
- **Tab 2 · 终端**：深色面板，实时显示每条跑过/将要跑的 git 命令（token 上色），支持直接敲命令回车执行、`↑↓` 翻历史、`Ctrl+C` 或点"中断"终止当前子进程。工具条：清空 / 中断 / 导出 .bat / 导出 .ps1。
- **全局条**（跨 tab 常驻）：`Dry-run` 复选框 + `预览全部命令`（一次 dump Worktree 流水线 + 5 个常用功能的命令）。
- **底部 5 个常用功能**（详见下一节）。

## 行的类型是自动判的（不给用户勾）

| 情况 | 类型列 | 指令 | 底色 |
|---|---|---|---|
| `<path>\.git` 是**目录** | 主仓库 | `config --local user.name` | 淡蓝 |
| `<path>\.git` 是**文件**（`gitdir: ...`） | worktree | `config --worktree user.name` | 白色 |
| 路径不存在 | worktree（待创建） | `worktree add [-b] <branch>` + `config --worktree` | 白色 |
| 路径存在但**属于另一个仓库**（跨仓库） | worktree | 不参与本次执行 | 淡橙 + 路径打删除线 |

RepoPath 允许直接指向 linked worktree（.git 是文件的场景），git 会解析 common dir 走通全套操作。

## 常用功能 5 个按钮

| 按钮 | 干什么 | 关键命令 |
|---|---|---|
| 清理多余提交和引用记录 | 回收不在分支树上的孤儿 commit / dangling 对象 | `fsck --unreachable --dangling` → `reflog expire --all` → `prune --expire=now` → `gc --prune=now` |
| 清理已合并分支 | 已合并到默认分支、且不被任何 worktree checkout、也不是默认/HEAD 的本地分支 | 扫描 `worktree list --porcelain` + `branch --merged` → 终端列出候选（勾选 UI 待做，先给出手动 `branch -d` 命令） |
| 更新远端 | 所有 remote、所有分支、tags、清理失效追踪、submodule 按需递归 | `fetch --all --prune --tags --recurse-submodules=on-demand` + `remote update --prune` |
| 仓库磁盘分析 | `.git` 内部对象、pack Top N、LFS、worktrees 元数据、reflog、hooks、工作副本体积 | `count-objects -v -H` + C# 文件系统扫描 |
| 对齐最新提交 | 让 HEAD 的 committer date/name/email 全部等于 author 的对应字段 | `show -s --format=%an/%ae/%aI HEAD` → env `GIT_COMMITTER_*` + `commit --amend --no-edit` |

**注意**："更新远端"只清本地 `refs/remotes/*` 失效追踪，不会 `git push --delete`。

## 读命令也会显式回显

`读取现有 worktree`、`清理已合并分支`、分支存在性探针——所有实际跑的 `git -C ... worktree list / branch --list / config --local / config --worktree` 都会先打印到终端，再展示返回值。所见即所跑。

## Dry-run

默认开启。所有按钮（执行 / 创建/克隆 / 5 个常用功能）在 Dry-run 下只把命令打印到终端，不启动进程。取消勾选后点执行会弹二次确认，然后逐条 `Process.Start git.exe`，单条超时 5 分钟，任一失败立即中断后续。

## 设置持久化

exe 同目录 `settings.xml`，关窗自动保存、启动自动加载。若加载时 xml 损坏，会禁用关闭时的覆盖写，防止把好配置用默认值冲掉。

## 编译

VS 2026 (v18) MSBuild：

```
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" \
  GitTreeManager.csproj -t:Rebuild -p:Configuration=Debug "-p:OutputPath=bin/Verify"
```

## 已知限制

- "清理已合并分支"目前只列候选 + 给出手动删除命令，勾选对话框待做
- 分支存在性探针在预览与执行各跑一次，中间若分支被外部改动会有 TOCTOU 差异（场景少见）
- 子模块 / bare 仓库的 `.git` 结构会被当 linked worktree 处理，指向它们时行为不保证
- `git init -b <branch>` 要求 git ≥ 2.28，低版本需先在"新建"前手动切换分支名
- 无 i18n（中文硬编码）、无日志文件持久化（关闭即失）
