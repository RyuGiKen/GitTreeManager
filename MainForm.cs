using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GitTreeManager.Models;
using GitTreeManager.Services;

namespace GitTreeManager
{
    public partial class MainForm : Form
    {
        private const int MaxRows = 99;

        private readonly ConfigStore _store = new ConfigStore();
        private readonly GitCommandBuilder _builder = new GitCommandBuilder();
        private readonly GitRunner _runner = new GitRunner();
        private AppSettings _current = new AppSettings();

        public MainForm()
        {
            InitializeComponent();
        }

        // ---------- 生命周期 ----------

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                _current = _store.Load();
            }
            catch (Exception ex)
            {
                Log("[warn] 读取配置失败: " + ex.Message);
                _current = new AppSettings();
            }
            // 首次运行或配置里 gitExe 只是占位 "git" 时，自动探测默认安装位置预填。
            if (string.IsNullOrWhiteSpace(_current.GitExe) ||
                string.Equals(_current.GitExe, "git", StringComparison.OrdinalIgnoreCase))
            {
                _current.GitExe = GitLocator.Detect();
            }
            ApplySettingsToUi(_current);
            RefreshGitVersion();
            UpdateRowLabel();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                var s = CollectSettingsFromUi();
                _store.Save(s);
            }
            catch (Exception ex)
            {
                Log("[warn] 保存配置失败: " + ex.Message);
            }
            base.OnFormClosing(e);
        }

        // ---------- 顶部仓库区 ----------

        private void btnBrowseRepo_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "选择仓库根目录（可以是尚不存在的路径）";
                if (!string.IsNullOrWhiteSpace(txtRepoPath.Text))
                {
                    var probe = Path.GetDirectoryName(txtRepoPath.Text);
                    if (!string.IsNullOrEmpty(probe) && Directory.Exists(probe)) dlg.SelectedPath = probe;
                }
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    txtRepoPath.Text = dlg.SelectedPath;
                    stsRepo.Text = "仓库: " + dlg.SelectedPath;
                }
            }
        }

        private void btnGitBrowse_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "定位 git.exe";
                dlg.Filter = "git.exe|git.exe|所有文件|*.*";
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    txtGitExe.Text = dlg.FileName;
                }
            }
        }

        private void RbMode_CheckedChanged(object sender, EventArgs e)
        {
            // 只有当"克隆已有"真正被勾选时才启用远程 URL；避免初始化期间双 false 抢焦点。
            if (rbClone != null && txtRemoteUrl != null)
            {
                txtRemoteUrl.Enabled = rbClone.Checked;
            }
            if (btnInitRepo != null && rbClone != null)
            {
                btnInitRepo.Text = rbClone.Checked ? "克隆" : "创建";
            }
        }

        private void btnInitRepo_Click(object sender, EventArgs e)
        {
            var s = CollectSettingsFromUi();
            string err = ValidateSettings(s);
            if (err != null)
            {
                MessageBox.Show(this, err, "参数不完整", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var cmds = _builder.BuildInitOrCloneOnly(s);
            Log("[功能] " + (s.Mode == RepoMode.Clone ? "克隆" : "创建") + "仓库：仅执行仓库级命令，不含 worktree 流水线。");
            Log("── 预览命令 ─────────────────────────");
            foreach (var c in cmds) Log("$ " + c.Display);
            RunIfEnabled(cmds);
        }

        // ---------- Worktree 表格 ----------

        private void btnWtAdd_Click(object sender, EventArgs e)
        {
            if (dgvWorktrees.Rows.Count >= MaxRows)
            {
                MessageBox.Show(this, "列表最大 " + MaxRows + " 行，已达上限。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int idx = dgvWorktrees.Rows.Add();
            dgvWorktrees.Rows[idx].Cells[colNum.Name].Value = idx + 1;
            UpdateRowLabel();
        }

        private void btnWtRemove_Click(object sender, EventArgs e)
        {
            if (dgvWorktrees.CurrentRow == null) return;
            int i = dgvWorktrees.CurrentRow.Index;
            if (i < 0 || i >= dgvWorktrees.Rows.Count) return;
            dgvWorktrees.Rows.RemoveAt(i);
            RenumberRows();
            UpdateRowLabel();
        }

        private void btnWtUp_Click(object sender, EventArgs e)
        {
            if (dgvWorktrees.CurrentRow == null) return;
            int i = dgvWorktrees.CurrentRow.Index;
            if (i <= 0) return;
            SwapRows(i, i - 1);
            dgvWorktrees.Rows[i - 1].Selected = true;
            UpdateRowLabel();
        }

        private void btnWtDown_Click(object sender, EventArgs e)
        {
            if (dgvWorktrees.CurrentRow == null) return;
            int i = dgvWorktrees.CurrentRow.Index;
            if (i >= dgvWorktrees.Rows.Count - 1) return;
            SwapRows(i, i + 1);
            dgvWorktrees.Rows[i + 1].Selected = true;
            UpdateRowLabel();
        }

        private void btnWtRead_Click(object sender, EventArgs e)
        {
            Log("[TODO] 读取现有 worktree：git -C <repo> worktree list --porcelain  →  回填表格。");
            Log("       下一轮实现。当前按钮仅占位。");
        }

        private void btnWtClear_Click(object sender, EventArgs e)
        {
            if (dgvWorktrees.Rows.Count == 0) return;
            if (MessageBox.Show(this, "确认清空全部 " + dgvWorktrees.Rows.Count + " 行？",
                    "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            dgvWorktrees.Rows.Clear();
            UpdateRowLabel();
        }

        // ---------- 执行 ----------

        private void btnPreview_Click(object sender, EventArgs e)
        {
            var s = CollectSettingsFromUi();
            var cmds = _builder.BuildInitAndWorktrees(s);
            Log("── 预览 ─────────────────────────────");
            foreach (var c in cmds) Log("$ " + c.Display);
            Log("── 共 " + cmds.Count + " 条命令 ───────────────────");
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            var s = CollectSettingsFromUi();
            string err = ValidateSettings(s);
            if (err != null)
            {
                MessageBox.Show(this, err, "参数不完整", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            bool dry = chkDryRun.Checked;
            if (!dry)
            {
                var r = MessageBox.Show(this,
                    "即将实际执行 git 命令，可能创建目录或修改配置。是否继续？",
                    "二次确认", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (r != DialogResult.Yes) return;
            }
            var cmds = _builder.BuildInitAndWorktrees(s);
            _runner.RunAll(s.GitExe, cmds, dry, line => AppendLogThreadSafe(line));
        }

        // ---------- 常用功能 ----------

        /// <summary>清理多余提交和引用记录：不在分支树上的孤儿 commit + 让 reflog 不再抓住它们 + prune + gc 收尾。</summary>
        private void btnCleanOrphan_Click(object sender, EventArgs e)
        {
            var s = CollectSettingsFromUi();
            Log("[功能] 清理多余提交和引用记录（不在分支树上的孤儿 commit / dangling 对象）");
            Log("── 预览命令 ─────────────────────────");
            var cmds = _builder.BuildCleanOrphanCommits(s);
            foreach (var c in cmds) Log("$ " + c.Display);
            Log("── 提示：必须先 `reflog expire`，否则对象仍被 reflog 视作可达、`prune` 不会动。");
            RunIfEnabled(cmds);
        }

        /// <summary>清理已合并分支：排除 worktree 占用与默认分支后，列出可删的合并入默认分支的本地分支。</summary>
        private void btnCleanMerged_Click(object sender, EventArgs e)
        {
            var s = CollectSettingsFromUi();
            Log("[功能] 清理已合并分支");
            Log("── 预览命令 ─────────────────────────");
            var cmds = _builder.BuildCleanMergedBranches(s);
            foreach (var c in cmds) Log("$ " + c.Display);
            Log("── 提示：git 会拒绝删除被任何 worktree checkout 的分支（`fatal: cannot delete branch ... used by worktree at ...`）；");
            Log("       本功能先 `worktree list --porcelain` 拿锁定集，再从 `branch --merged` 里剔除，最后弹勾选。");
            RunIfEnabled(cmds);
        }

        /// <summary>更新远端：所有 remote + 所有分支 + tags + 清理失效追踪 + 递归 submodule。</summary>
        private void btnUpdateRemote_Click(object sender, EventArgs e)
        {
            var s = CollectSettingsFromUi();
            Log("[功能] 更新远端（所有 remote、所有分支、tags、prune 失效追踪、含 submodule）");
            Log("── 预览命令 ─────────────────────────");
            var cmds = _builder.BuildUpdateAllRemotes(s);
            foreach (var c in cmds) Log("$ " + c.Display);
            RunIfEnabled(cmds);
        }

        /// <summary>仓库磁盘分析：.git 内部各分区的体积与对象数量，帮判断该不该 gc / 有没有 LFS 撑爆。</summary>
        private void btnDiskAnalyze_Click(object sender, EventArgs e)
        {
            var s = CollectSettingsFromUi();
            Log("[功能] 仓库磁盘分析");
            Log("── 预览命令（git 侧）─────────────");
            var cmds = _builder.BuildDiskAnalysis(s);
            foreach (var c in cmds) Log("$ " + c.Display);
            Log("── 待补：文件系统侧扫描（.git/objects/pack 大文件 Top10、lfs/objects 总量、worktrees/* 各自占用）");
            Log("       下一轮直接由 C# Directory.GetFiles + FileInfo.Length 遍历输出，不走 git。");
            RunIfEnabled(cmds);
        }

        /// <summary>对齐最新提交：让 HEAD 的 committer date/name/email 分别等于 author 的对应字段。</summary>
        private void btnAlignCommit_Click(object sender, EventArgs e)
        {
            var s = CollectSettingsFromUi();
            Log("[功能] 对齐最新提交（committer ← author）");
            Log("── 预览命令 ─────────────────────────");
            var cmds = _builder.BuildAlignLatestCommit(s);
            foreach (var c in cmds) Log("$ " + c.Display);
            Log("── 等价 shell 写法：");
            Log("     GIT_COMMITTER_NAME=\"$(git show -s --format=%an HEAD)\" \\");
            Log("     GIT_COMMITTER_EMAIL=\"$(git show -s --format=%ae HEAD)\" \\");
            Log("     GIT_COMMITTER_DATE=\"$(git show -s --format=%aI HEAD)\" \\");
            Log("     git commit --amend --no-edit");
            Log("── 实跑时用 ProcessStartInfo.EnvironmentVariables 注入 3 个 GIT_COMMITTER_*，值由前 3 条 show 命令回填。");
            RunIfEnabled(cmds);
        }

        /// <summary>Dry-run 关闭时把命令交给 GitRunner 实跑；否则只输出预览。</summary>
        private void RunIfEnabled(IList<GitCommand> cmds)
        {
            if (cmds == null || cmds.Count == 0) return;
            if (!chkDryRun.Checked)
            {
                var r = MessageBox.Show(this,
                    "即将实际执行以上 " + cmds.Count + " 条命令，是否继续？",
                    "二次确认", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (r != DialogResult.Yes) return;
                var s = CollectSettingsFromUi();
                _runner.RunAll(s.GitExe, cmds, false, AppendLogThreadSafe);
            }
        }

        // ---------- 日志页工具条 ----------

        private void btnLogCopy_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtLog.Text)) return;
            Clipboard.SetText(txtLog.Text);
            stsLast.Text = "日志已复制到剪贴板";
        }

        private void btnLogClear_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
        }

        private void btnLogSaveBat_Click(object sender, EventArgs e)
        {
            SaveLogAs("batch");
        }

        private void btnLogSavePs1_Click(object sender, EventArgs e)
        {
            SaveLogAs("powershell");
        }

        private void SaveLogAs(string kind)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = kind == "batch" ? "批处理脚本 (*.bat)|*.bat" : "PowerShell 脚本 (*.ps1)|*.ps1";
                dlg.FileName = kind == "batch" ? "git-tree-manager.bat" : "git-tree-manager.ps1";
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                var sb = new StringBuilder();
                if (kind == "batch") sb.AppendLine("@echo off").AppendLine("rem generated by GitTreeManager");
                else sb.AppendLine("# generated by GitTreeManager");
                foreach (var raw in txtLog.Lines)
                {
                    if (raw.StartsWith("$ ")) sb.AppendLine(raw.Substring(2));
                    else if (kind == "batch") sb.AppendLine("echo " + raw.Replace("\"", "\""));
                    else sb.AppendLine("Write-Host \"" + raw.Replace("\"", "`\"") + "\"");
                }
                File.WriteAllText(dlg.FileName, sb.ToString(), new UTF8Encoding(false));
                stsLast.Text = "已导出: " + dlg.FileName;
            }
        }

        // ---------- 内部工具 ----------

        private void SwapRows(int a, int b)
        {
            var ra = dgvWorktrees.Rows[a];
            var rb = dgvWorktrees.Rows[b];
            for (int c = 1; c < dgvWorktrees.Columns.Count; c++)
            {
                var tmp = ra.Cells[c].Value;
                ra.Cells[c].Value = rb.Cells[c].Value;
                rb.Cells[c].Value = tmp;
            }
            RenumberRows();
        }

        private void RenumberRows()
        {
            for (int i = 0; i < dgvWorktrees.Rows.Count; i++)
            {
                dgvWorktrees.Rows[i].Cells[colNum.Name].Value = i + 1;
            }
        }

        private void UpdateRowLabel()
        {
            lblWtCount.Text = "行数 " + dgvWorktrees.Rows.Count + "/" + MaxRows;
        }

        private void ApplySettingsToUi(AppSettings s)
        {
            txtRepoPath.Text = s.RepoPath ?? "";
            txtRemoteUrl.Text = s.RemoteUrl ?? "";
            txtDefaultBranch.Text = string.IsNullOrEmpty(s.DefaultBranch) ? "main" : s.DefaultBranch;
            txtGitExe.Text = string.IsNullOrEmpty(s.GitExe) ? "git" : s.GitExe;
            if (s.Mode == RepoMode.Clone) rbClone.Checked = true; else rbNew.Checked = true;
            chkDryRun.Checked = s.DryRun;
            dgvWorktrees.Rows.Clear();
            if (s.Entries != null)
            {
                foreach (var en in s.Entries)
                {
                    int idx = dgvWorktrees.Rows.Add();
                    var row = dgvWorktrees.Rows[idx];
                    row.Cells[colNum.Name].Value = idx + 1;
                    row.Cells[colPath.Name].Value = en.Path;
                    row.Cells[colBranch.Name].Value = en.Branch;
                    row.Cells[colName.Name].Value = en.UserName;
                    row.Cells[colEmail.Name].Value = en.UserEmail;
                }
            }
        }

        private AppSettings CollectSettingsFromUi()
        {
            var s = new AppSettings
            {
                RepoPath = txtRepoPath.Text.Trim(),
                Mode = rbClone.Checked ? RepoMode.Clone : RepoMode.New,
                RemoteUrl = txtRemoteUrl.Text.Trim(),
                DefaultBranch = string.IsNullOrWhiteSpace(txtDefaultBranch.Text) ? "main" : txtDefaultBranch.Text.Trim(),
                GitExe = string.IsNullOrWhiteSpace(txtGitExe.Text) ? "git" : txtGitExe.Text.Trim(),
                DryRun = chkDryRun.Checked,
                Entries = new List<WorktreeEntry>()
            };
            foreach (DataGridViewRow row in dgvWorktrees.Rows)
            {
                s.Entries.Add(new WorktreeEntry
                {
                    Path = AsStr(row.Cells[colPath.Name].Value),
                    Branch = AsStr(row.Cells[colBranch.Name].Value),
                    UserName = AsStr(row.Cells[colName.Name].Value),
                    UserEmail = AsStr(row.Cells[colEmail.Name].Value)
                });
            }
            _current = s;
            return s;
        }

        private static string AsStr(object v) { return v == null ? "" : v.ToString(); }

        private string ValidateSettings(AppSettings s)
        {
            if (string.IsNullOrWhiteSpace(s.RepoPath)) return "请填写本地路径。";
            if (s.Mode == RepoMode.Clone && string.IsNullOrWhiteSpace(s.RemoteUrl)) return "克隆模式必须填写远程 URL。";
            return null;
        }

        private void RefreshGitVersion()
        {
            var v = _runner.DetectGitVersion(txtGitExe.Text);
            stsGit.Text = v == null ? "git: 未检测到" : ("git: " + v);
        }

        private void Log(string line) { AppendLogThreadSafe(line); }

        private void AppendLogThreadSafe(string line)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.BeginInvoke(new Action<string>(AppendLogThreadSafe), line);
                return;
            }
            txtLog.AppendText(line + Environment.NewLine);
            stsLast.Text = line.Length > 80 ? line.Substring(0, 80) + "..." : line;
        }
    }
}
