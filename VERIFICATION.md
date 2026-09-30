# GitTreeManager V2 验证报告

**日期**：2026-09-30
**验证人**：QwenWork（助手自建临时仓库实测）
**测试根**：`C:\Users\12407024\.qwenworkcn\workspace\munqc4w4ng51bqz9\gtm-test\`

## 环境

- Git for Windows（PATH 内 `git` 可用）
- .NET Framework 4.7.2 WinForms
- 主仓库：`gtm-test/repo`（2 commits, 1 额外分支 feature/x）
- 全新仓库：`gtm-test/fresh`（1 seed commit）

## 测试矩阵

### 测试 1 · 对齐最新提交（committer ← author）

**流水线**：3 条 `git show -s --format=%an/%ae/%aI HEAD` 捕获 → env 注入 `GIT_COMMITTER_*` + `git commit --amend --no-edit`

**结果**：

```
before: author=Zhang San <zhang@example.com> 2026-01-15T10:20:30+08:00
        committer=Li Si <li@example.com>     2026-03-20T15:45:00+08:00
after : author=Zhang San <zhang@example.com> 2026-01-15T10:20:30+08:00
        committer=Zhang San <zhang@example.com> 2026-01-15T10:20:30+08:00
```

✓ 三项字段完全对齐；author 未被 `--reset-author` 反噬。

**工具等价验证**：GitRunner 已支持 `GitCommand.WithEnv + CaptureStdoutAs` 组合，`{author_name}` 占位符在批处理执行前会被前一条 show 的 stdout Trim 值替换。

### 测试 2 · 建 worktree + per-worktree config

**流水线**：`config extensions.worktreeConfig true` → `worktree add <path> [-b] <branch>` → `config --worktree user.name/email`

**结果**（fresh 仓库，一次跑通 2 行）：

```
worktree list:
.../fresh           [main]
.../fresh/wt-feat   [feature/x]     user.name=FeatDev user.email=feat@x.com
.../fresh/wt-hotfix [hotfix/urgent] user.name=HotDev  user.email=hot@x.com
主仓库 worktree 级 user.name = (未设置) ✓
```

**发现与修正**：

- 分支不存在时 `worktree add <p> -b <br>` 正常；分支已存在时会 `fatal: a branch named 'xxx' already exists`。V2 已补 branchExists 探针：MainForm 用 `git branch --list <br> --format=%(refname:short)` 预检，builder 依结果决定要不要 `-b`。
- 分支存在且已被另一个 worktree checkout 时，`worktree add <p> <br>` 仍报 `fatal: '<br>' is already used by worktree at '<other>'`。这是 git 自身的独占保护，符合用户选择的"一行失败即中断全部"策略，工具不做静默降级。

### 测试 3 · 清理已合并分支（扫描）

**流水线**：`worktree list --porcelain` + `branch --merged <default>` + `rev-parse --abbrev-ref HEAD` → 上层差集 → 弹勾选 → `branch -d`

**验证输出**（repo 里已 checkout 了 feature/x 的 wt1）：

```
$ git branch -d feature/x
error: cannot delete branch 'feature/x' used by worktree at '.../wt1'
```

✓ 证明 `worktree list --porcelain` 拿锁定集是必需步骤（对应 MEMORY 里那条"git worktree：branch -d 拒删被 worktree checkout 的分支"）。

**porcelain 解析**：MainForm.LoadWorktreesFromPorcelain 已按 `worktree <path>` / `branch refs/heads/<name>` / 空行为一条记录结束 的语法规则实现，实测数据能正确提取 `[{path, branch}]` 列表。

### 测试 4 · 清理多余提交和引用记录（孤儿 commit）

**流水线**：`fsck --full --unreachable --dangling --no-reflogs` → `reflog expire --expire=now --all` → `prune --expire=now -v` → `gc --prune=now`

**结果**：

```
制造孤儿 commit 75dc2556693f49363517b3f0aa1ccf1a6f0a55e6 (branch -D 后)
fsck 前:  unreachable commit 75dc255... (报告为 unreachable)
cat-file -e: ✓ 存在 (reflog 仍抓得住)
reflog expire + prune + gc 后:
  prune -v 输出 "75dc255... commit" (确认回收)
  cat-file -e 失败 → ✓ 已回收
```

✓ 关键链条 **必须按 fsck → reflog expire → prune → gc** 顺序；跳过 reflog expire 时 prune 不会动对象（对应 MEMORY 里"缺 reflog expire 则对象仍被 reflog 视作可达"）。

### 测试 5 · 仓库磁盘分析（git 侧）

**流水线**：`git count-objects -v -H` + `git verify-pack -v <pack>`

**结果示例**：

```
count: 0    size: 0 bytes
in-pack: 7  packs: 1    size-pack: 1.75 KiB
verify-pack 输出每个对象的 sha/type/size/size-in-pack/offset
```

✓ 命令能跑通；文件系统侧扫描（Top N pack、lfs/objects 体积）留到 V3。

### 测试 6 · 更新远端

**流水线**：`fetch --all --prune --tags --recurse-submodules=on-demand` + `remote update --prune`

**结果**：无 remote 的临时仓库跑这两条命令，exit=0，无输出。**注意**：在有 remote 时会真的拉取；`--prune` 只清本地 `refs/remotes/*`，不删远端真实分支（对应 MEMORY 里"remote prune 只清本地 refs/remotes 不碰远端真实分支"）。

## 终端交互层实测说明

**未做的自动化测试**（GUI 手工交互，用户可自行验证）：

- 打开 `bin\Verify\GitTreeManager.exe`
- Tab 2 终端页顶部提示条：`Ctrl+C 中断 · ↑↓ 翻历史 · 输入自动加 git 前缀`
- 输入 `status` 回车 → 应看到 `$ git status` 蓝，命令输出正常色
- 输入 `log --oneline -5` → 应看到最近 5 条 commit
- 按 ↑ → 输入框回填上一条
- 执行"清理已合并分支"（关 Dry-run）→ 期间"中断"按钮 Enabled；输入框 ReadOnly；Ctrl+C 或点中断 → 后台进程 Kill，日志 `[cancel] 已请求中断当前 git 子进程`
- 输入 `clear` → 终端清空

`CommandLineParser.Split` 采用 MSVC CommandLineToArgvW 反向规则（反斜杠 + 双引号奇偶决定转义/段切换），实测：
- `commit -m "hello world"` → `["commit", "-m", "hello world"]`
- `a"b"c` → `["abc"]`
- 空字符串 → `[]`

## 已知限制（V3 再做）

1. `读取现有 worktree` 已能回填路径+分支，但每行 worktree 里的 `user.name` / `user.email` 需逐条 `git -C <wt> config --worktree user.name` 读，本轮未做（涉及 N 次同步进程）。
2. `清理已合并分支` 只做到扫描，差集→弹勾选→真删的闭环未接。
3. `仓库磁盘分析` 只跑 git 侧两条命令，文件系统侧 pack Top N / LFS 体积扫描未做。
4. 表格行不做路径合法性 / 分支重名 / email 格式校验；提交前建议自行核对。
5. Dry-run 关闭时"一行失败即中断全部"，无 per-row 隔离；批量创建多 worktree 时若中间一行失败，前面已建的 worktree 会保留（不做自动回滚）。

## 变更文件（相对 V1）

- `Services/GitRunner.cs` — 新增 `TermKind` 枚举 + `ITerminalSink` 接口 + `Cancel()` + `RunInteractive()`；stdout/stderr 边读边推。
- `Services/CommandLineParser.cs` — 新增（MSVC argv 反向解析）。
- `Services/GitCommandBuilder.cs` — `BuildInitAndWorktrees` 加 `Func<string,bool> branchExists` 回调，分支存在时不加 `-b`。
- `MainForm.Designer.cs` — tabLog 整块从"日志页"改为"终端页"：`pnlTermToolbar`（清空/中断/导出 .bat/导出 .ps1 + 提示）+ `rchTerm`（只读 RichTextBox，WordWrap=false）+ `pnlTermInput`（`$` prompt + 输入框）。
- `MainForm.cs` — 实现 `ITerminalSink`；`AppendTerm(line, kind)` 上色；`txtTermInput_KeyDown` 处理 Enter/↑/↓/Ctrl+C；`SubmitInput` 解析并交互执行；`MakeBranchProbe` 提供分支存在性回调；`LoadWorktreesFromPorcelain` 解析回填；`btnWtRead_Click` 从 TODO 改为真跑；批量执行改后台线程，`SetBusyUi` 联动"中断"按钮启用。
- `GitTreeManager.csproj` — 新增 `Services/CommandLineParser.cs`。
- `VERIFICATION.md` — 本文件。
