using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using GitTreeManager.Models;
using GitTreeManager.Services;

namespace GitTreeManager
{
    public partial class MainForm : Form, ITerminalSink
    {
        private const int MaxRows = 99;

        // 终端色板（终端语义：命令蓝 / stderr 红 / stdout 黑 / meta 灰 / 成功绿 / 失败红加粗）
        private static readonly Color ColCmd = Color.FromArgb(0, 0, 160);
        private static readonly Color ColStd = Color.FromArgb(30, 30, 30);
        private static readonly Color ColErr = Color.FromArgb(178, 0, 0);
        private static readonly Color ColMeta = Color.FromArgb(96, 96, 96);
        private static readonly Color ColDone = Color.FromArgb(0, 128, 0);
        private static readonly Color ColFail = Color.FromArgb(200, 0, 0);

        private readonly ConfigStore _store = new ConfigStore();
        private readonly GitCommandBuilder _builder = new GitCommandBuilder();
        private readonly GitRunner _runner = new GitRunner();
        private AppSettings _current = new AppSettings();

        // 终端交互状态
        private readonly List<string> _history = new List<string>();
        private int _histIdx = -1;
        private int _isRunning; // 0/1 via Interlocked

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
                LogMeta("[warn] 读取配置失败: " + ex.Message);
                _current = new AppSettings();
            }
            if (string.IsNullOrWhiteSpace(_current.GitExe) ||
                string.Equals(_current.GitExe, "git", StringComparison.OrdinalIgnoreCase))
            {
                _current.GitExe = GitLocator.Detect();
            }
            ApplySettingsToUi(_current);
            RefreshGitVersion();
            UpdateRowLabel();
            UpdatePrompt();
            // 类型列由 Path 下的 .git 是目录还是文件自动判定；用户改 Path 或 RepoPath 时刷新
            dgvWorktrees.CellValueChanged += DgvWt_CellValueChanged;
            txtRepoPath.TextChanged += (s, ev) => RefreshAllRowTypes();
            RefreshAllRowTypes();
            LogMeta("GitTreeManager 终端 · 输入 git 命令直接回车执行 · ↑↓ 翻历史 · Ctrl+C 中断当前命令");
        }

        /// <summary>
        /// 判定规则：
        ///   &lt;path&gt;\.git 是目录 → 主仓库
        ///   &lt;path&gt;\.git 是文件（内含 gitdir: 指针）→ 已存在的 worktree
        ///   &lt;path&gt;\.git 不存在（含路径为空 / 路径不存在）→ 视作待创建的 worktree
        /// </summary>
        private static bool DetectIsMain(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            try
            {
                string gitPath = Path.Combine(path.Trim(), ".git");
                return Directory.Exists(gitPath);
            }
            catch { return false; }
        }

        private void RefreshRowType(DataGridViewRow row)
        {
            if (row == null) return;
            string path = AsStr(row.Cells[colPath.Name].Value).Trim();
            bool isMain = DetectIsMain(path);
            row.Cells[colType.Name].Value = isMain ? "主仓库" : "worktree";
            row.DefaultCellStyle.BackColor = isMain
                ? Color.FromArgb(0xE6, 0xF2, 0xFF)
                : SystemColors.Window;
        }

        private void RefreshAllRowTypes()
        {
            foreach (DataGridViewRow r in dgvWorktrees.Rows) RefreshRowType(r);
        }

        private void DgvWt_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            // 只关心路径列变化；重命名/换 repo 通过 TextChanged 分支处理
            if (dgvWorktrees.Columns[e.ColumnIndex].Name == colPath.Name)
            {
                RefreshRowType(dgvWorktrees.Rows[e.RowIndex]);
            }
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
                LogMeta("[warn] 保存配置失败: " + ex.Message);
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
                    UpdatePrompt();
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
                    RefreshGitVersion();
                }
            }
        }

        private void RbMode_CheckedChanged(object sender, EventArgs e)
        {
            if (rbClone != null && txtRemoteUrl != null)
                txtRemoteUrl.Enabled = rbClone.Checked;
            if (btnInitRepo != null && rbClone != null)
                btnInitRepo.Text = rbClone.Checked ? "克隆" : "创建";
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
            LogMeta("[功能] " + (s.Mode == RepoMode.Clone ? "克隆" : "创建") + "仓库：仅执行仓库级命令。");
            RunBatch(cmds);
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
            RefreshRowType(dgvWorktrees.Rows[idx]);
            UpdateRowLabel();
        }

        private void btnWtRemove_Click(object sender, EventArgs e)
        {
            if (dgvWorktrees.CurrentRow == null) return;
            int i = dgvWorktrees.CurrentRow.Index;
            if (i < 0 || i >= dgvWorktrees.Rows.Count) return;
            dgvWorktrees.Rows.RemoveAt(i);
            RenumberRows();
            RefreshAllRowTypes();
            UpdateRowLabel();
        }

        private void btnWtUp_Click(object sender, EventArgs e)
        {
            if (dgvWorktrees.CurrentRow == null) return;
            int i = dgvWorktrees.CurrentRow.Index;
            if (i <= 0) return;
            SwapRows(i, i - 1);
            dgvWorktrees.Rows[i - 1].Selected = true;
            RefreshAllRowTypes();
            UpdateRowLabel();
        }

        private void btnWtDown_Click(object sender, EventArgs e)
        {
            if (dgvWorktrees.CurrentRow == null) return;
            int i = dgvWorktrees.CurrentRow.Index;
            if (i >= dgvWorktrees.Rows.Count - 1) return;
            SwapRows(i, i + 1);
            dgvWorktrees.Rows[i + 1].Selected = true;
            RefreshAllRowTypes();
            UpdateRowLabel();
        }

        private void btnWtRead_Click(object sender, EventArgs e)
        {
            var s = CollectSettingsFromUi();
            if (string.IsNullOrWhiteSpace(s.RepoPath) || !Directory.Exists(s.RepoPath))
            {
                LogMeta("[skip] 仓库路径不存在，无法读取 worktree 列表。");
                return;
            }
            string stdOut, stdErr;
            try
            {
                int exit = RunCapture(s.GitExe,
                    new GitCommand("-C", s.RepoPath, "worktree", "list", "--porcelain"),
                    out stdOut, out stdErr);
                if (exit != 0)
                {
                    LogErr("读取 worktree 失败 (exit " + exit + "): " + stdErr);
                    return;
                }
                int n = LoadWorktreesFromPorcelain(s.GitExe, stdOut);
                LogMeta("已回填 " + n + " 条 worktree（首行标为主仓库；user.name/email 已按 --local / --worktree 分别拉取）。");
                UpdatePrompt();
            }
            catch (Exception ex)
            {
                LogErr("读取 worktree 异常: " + ex.Message);
            }
        }

        private void btnWtClear_Click(object sender, EventArgs e)
        {
            if (dgvWorktrees.Rows.Count == 0) return;
            if (MessageBox.Show(this, "确认清空全部 " + dgvWorktrees.Rows.Count + " 行？",
                    "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            dgvWorktrees.Rows.Clear();
            UpdateRowLabel();
        }

        // ---------- 执行 / 预览 ----------

        /// <summary>
        /// 全局预览：一次性把 Worktree 流水线 + 5 个常用功能的命令序列都打到终端页；
        /// 分支存在探针会真跑一次以决定要不要 -b，命令本身不执行。
        /// </summary>
        private void btnPreview_Click(object sender, EventArgs e)
        {
            var s = CollectSettingsFromUi();
            var probe = MakeBranchProbe(s);

            LogMeta("═══ Worktree 流水线 ═══");
            DumpCmds(_builder.BuildWorktreeApply(s, probe));

            LogMeta("═══ 清理多余提交和引用记录 ═══");
            DumpCmds(_builder.BuildCleanOrphanCommits(s));

            LogMeta("═══ 清理已合并分支（扫描阶段） ═══");
            DumpCmds(_builder.BuildCleanMergedBranches(s));

            LogMeta("═══ 更新远端 ═══");
            DumpCmds(_builder.BuildUpdateAllRemotes(s));

            LogMeta("═══ 仓库磁盘分析 ═══");
            DumpCmds(_builder.BuildDiskAnalysis(s));

            LogMeta("═══ 对齐最新提交 ═══");
            DumpCmds(_builder.BuildAlignLatestCommit(s));

            LogMeta("═══ 预览结束 ═══");
        }

        private void DumpCmds(IList<GitCommand> cmds)
        {
            if (cmds == null || cmds.Count == 0) { LogMeta("(无命令)"); return; }
            foreach (var c in cmds) LogCmd("$ " + c.Display);
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
            // 仓库不存在 → 引导用户先点 创建/克隆
            if (!Directory.Exists(Path.Combine(s.RepoPath, ".git")))
            {
                MessageBox.Show(this,
                    "主仓库不存在或未初始化。\n请先在上方『仓库位置』填好路径后点『创建』或『克隆』，再回来执行 Worktree 流水线。",
                    "缺仓库", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            var cmds = _builder.BuildWorktreeApply(s, MakeBranchProbe(s));
            RunBatch(cmds);
        }

        /// <summary>
        /// 返回一个分支存在性探针；仓库目录不存在时（首次 init）返回 null 让 builder 一律按 -b 生成。
        /// </summary>
        private System.Func<string, bool> MakeBranchProbe(AppSettings s)
        {
            if (string.IsNullOrWhiteSpace(s.RepoPath) || !Directory.Exists(Path.Combine(s.RepoPath, ".git"))) return null;
            return branch =>
            {
                if (string.IsNullOrWhiteSpace(branch)) return false;
                string so, se;
                int exit = RunCapture(s.GitExe,
                    new GitCommand("-C", s.RepoPath, "branch", "--list", branch, "--format=%(refname:short)"),
                    out so, out se);
                return exit == 0 && !string.IsNullOrWhiteSpace(so);
            };
        }

        // ---------- 常用功能 ----------

        private void btnCleanOrphan_Click(object sender, EventArgs e)
        {
            var s = CollectSettingsFromUi();
            LogMeta("[功能] 清理多余提交和引用记录（不在分支树上的孤儿 commit / dangling 对象）");
            RunBatch(_builder.BuildCleanOrphanCommits(s));
        }

        private void btnCleanMerged_Click(object sender, EventArgs e)
        {
            var s = CollectSettingsFromUi();
            LogMeta("[功能] 清理已合并分支（排除 worktree 占用与默认分支）");
            RunBatch(_builder.BuildCleanMergedBranches(s));
        }

        private void btnUpdateRemote_Click(object sender, EventArgs e)
        {
            var s = CollectSettingsFromUi();
            LogMeta("[功能] 更新远端（所有 remote、所有分支、tags、prune 失效追踪、含 submodule）");
            RunBatch(_builder.BuildUpdateAllRemotes(s));
        }

        private void btnDiskAnalyze_Click(object sender, EventArgs e)
        {
            var s = CollectSettingsFromUi();
            LogMeta("[功能] 仓库磁盘分析");
            string repo = s.RepoPath;
            // git 侧命令 (count-objects) 与文件系统侧扫描都跑；Dry-run 只影响 git 命令是否真跑，扫描是只读的所以始终执行。
            RunBatch(_builder.BuildDiskAnalysis(s), () => DiskAnalyzer.Report(this, repo));
        }

        private void btnAlignCommit_Click(object sender, EventArgs e)
        {
            var s = CollectSettingsFromUi();
            LogMeta("[功能] 对齐最新提交（committer ← author）");
            RunBatch(_builder.BuildAlignLatestCommit(s));
        }

        /// <summary>统一入口：Dry-run 勾选就只输出预览；否则弹二次确认 + 后台线程跑，不阻塞 UI。
        /// onAfterRun 在跑完（或 dry-run 打印完）后回调；后台线程场景下在同一个线程内执行，保证顺序。</summary>
        private void RunBatch(IList<GitCommand> cmds, Action onAfterRun = null)
        {
            if (cmds == null || cmds.Count == 0)
            {
                LogMeta("[skip] 没有可执行的命令。");
                if (onAfterRun != null) onAfterRun();
                return;
            }
            bool dry = chkDryRun.Checked;
            if (dry)
            {
                for (int i = 0; i < cmds.Count; i++) LogCmd("[" + (i + 1) + "/" + cmds.Count + "] " + cmds[i].Display);
                LogMeta("── Dry-run 未执行，共 " + cmds.Count + " 条 ──");
                if (onAfterRun != null) onAfterRun();
                return;
            }
            var r = MessageBox.Show(this,
                "即将实际执行 " + cmds.Count + " 条命令，一行失败即中断全部。是否继续？",
                "二次确认", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (r != DialogResult.Yes) return;
            var s = CollectSettingsFromUi();
            if (onAfterRun == null)
            {
                StartOnBackground(() => _runner.RunAll(s.GitExe, cmds, false, this));
            }
            else
            {
                StartOnBackground(() =>
                {
                    _runner.RunAll(s.GitExe, cmds, false, this);
                    onAfterRun();
                });
            }
        }

        private void StartOnBackground(Action work)
        {
            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) != 0)
            {
                LogMeta("[busy] 上一条命令仍在执行，请先中断或等待。");
                return;
            }
            SetBusyUi(true);
            var th = new Thread(() =>
            {
                try { work(); }
                catch (Exception ex) { LogErr("!! " + ex.Message); }
                finally
                {
                    Interlocked.Exchange(ref _isRunning, 0);
                    BeginInvoke((Action)(() => SetBusyUi(false)));
                }
            }) { IsBackground = true };
            th.Start();
        }

        private void SetBusyUi(bool busy)
        {
            btnTermCancel.Enabled = busy;
            txtTermInput.ReadOnly = busy;
            btnExecute.Enabled = !busy;
            btnPreview.Enabled = !busy;
            btnInitRepo.Enabled = !busy;
        }

        // ---------- 终端页 ----------

        private void btnTermClear_Click(object sender, EventArgs e)
        {
            rchTerm.Clear();
        }

        private void btnTermCancel_Click(object sender, EventArgs e)
        {
            _runner.Cancel();
            LogMeta("[cancel] 已请求中断当前 git 子进程");
        }

        private void btnTermExportBat_Click(object sender, EventArgs e) { ExportTermScript("batch"); }
        private void btnTermExportPs1_Click(object sender, EventArgs e) { ExportTermScript("powershell"); }

        private void ExportTermScript(string kind)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = kind == "batch" ? "批处理脚本 (*.bat)|*.bat" : "PowerShell 脚本 (*.ps1)|*.ps1";
                dlg.FileName = kind == "batch" ? "git-tree-manager.bat" : "git-tree-manager.ps1";
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                var sb = new StringBuilder();
                if (kind == "batch") sb.AppendLine("@echo off").AppendLine("rem generated by GitTreeManager");
                else sb.AppendLine("# generated by GitTreeManager");
                foreach (var raw in rchTerm.Lines)
                {
                    string line = raw;
                    int idx = line.IndexOf("] $ ");
                    if (idx >= 0) line = line.Substring(idx + 2);
                    if (line.StartsWith("$ ")) { sb.AppendLine(line.Substring(2)); continue; }
                    if (kind == "batch") sb.AppendLine("echo " + line.Replace("\"", "\""));
                    else sb.AppendLine("Write-Host \"" + line.Replace("\"", "`\"") + "\"");
                }
                File.WriteAllText(dlg.FileName, sb.ToString(), new UTF8Encoding(false));
                stsLast.Text = "已导出: " + dlg.FileName;
            }
        }

        private void txtTermInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = e.Handled = true;
                SubmitInput();
            }
            else if (e.KeyCode == Keys.Up)
            {
                e.SuppressKeyPress = e.Handled = true;
                NavigateHistory(-1);
            }
            else if (e.KeyCode == Keys.Down)
            {
                e.SuppressKeyPress = e.Handled = true;
                NavigateHistory(+1);
            }
            else if (e.Control && e.KeyCode == Keys.C)
            {
                if (_isRunning == 1)
                {
                    e.SuppressKeyPress = e.Handled = true;
                    btnTermCancel_Click(sender, EventArgs.Empty);
                }
            }
        }

        private void SubmitInput()
        {
            string text = (txtTermInput.Text ?? "").Trim();
            if (text.Length == 0) { LogCmd(PromptText() + " "); return; }
            if (_history.Count == 0 || _history[_history.Count - 1] != text) _history.Add(text);
            _histIdx = _history.Count;
            txtTermInput.Text = "";

            if (string.Equals(text, "clear", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "cls", StringComparison.OrdinalIgnoreCase))
            { rchTerm.Clear(); return; }

            LogCmd(PromptText() + " " + text);

            var args = CommandLineParser.Split(text);
            if (args.Length == 0) return;
            // 允许用户输入 `git xxx` 或 `xxx`，都跑成 git xxx；首 token 若是 git 则剥掉。
            int start = 0;
            if (string.Equals(args[0], "git", StringComparison.OrdinalIgnoreCase) && args.Length > 1) start = 1;
            var trimmed = new string[args.Length - start];
            Array.Copy(args, start, trimmed, 0, trimmed.Length);

            var s = CollectSettingsFromUi();
            string wd = (!string.IsNullOrWhiteSpace(s.RepoPath) && Directory.Exists(s.RepoPath)) ? s.RepoPath : null;
            StartOnBackground(() => _runner.RunInteractive(s.GitExe, trimmed, wd, this));
        }

        private void NavigateHistory(int delta)
        {
            if (_history.Count == 0) return;
            _histIdx = Math.Max(-1, Math.Min(_history.Count, _histIdx + delta));
            txtTermInput.Text = _histIdx < 0 ? "" : (_histIdx >= _history.Count ? "" : _history[_histIdx]);
            txtTermInput.SelectionStart = txtTermInput.Text.Length;
        }

        private string PromptText()
        {
            var s = _current;
            if (s == null || string.IsNullOrWhiteSpace(s.RepoPath)) return "$";
            try
            {
                if (!Directory.Exists(s.RepoPath)) return "$ " + Path.GetFileName(s.RepoPath.TrimEnd('\\', '/'));
                // 尝试读当前分支
                string stdout, stderr;
                int exit = RunCapture(s.GitExe,
                    new GitCommand("-C", s.RepoPath, "rev-parse", "--abbrev-ref", "HEAD"), out stdout, out stderr);
                string br = exit == 0 ? stdout.Trim() : "";
                string name = Path.GetFileName(s.RepoPath.TrimEnd('\\', '/'));
                return string.IsNullOrEmpty(br) ? ("$ " + name) : ("$ " + name + " (" + br + ")");
            }
            catch { return "$"; }
        }

        private void UpdatePrompt()
        {
            lblPrompt.Text = PromptText();
        }

        private int RunCapture(string gitExe, GitCommand cmd, out string stdout, out string stderr)
        {
            // 用于内部读取，不走 sink；简单同步。
            var sbO = new StringBuilder();
            var sbE = new StringBuilder();
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = string.IsNullOrWhiteSpace(gitExe) ? "git" : gitExe,
                Arguments = BuildArgsLocal(cmd.Args),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
            using (var p = new System.Diagnostics.Process { StartInfo = psi })
            {
                p.OutputDataReceived += (s, e) => { if (e.Data != null) sbO.AppendLine(e.Data); };
                p.ErrorDataReceived += (s, e) => { if (e.Data != null) sbE.AppendLine(e.Data); };
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                if (!p.WaitForExit(15_000)) { try { p.Kill(); } catch { } stdout = sbO.ToString(); stderr = sbE.ToString(); return -1; }
                stdout = sbO.ToString();
                stderr = sbE.ToString();
                return p.ExitCode;
            }
        }

        private static string BuildArgsLocal(string[] args)
        {
            if (args == null || args.Length == 0) return "";
            var parts = new string[args.Length];
            for (int i = 0; i < args.Length; i++) parts[i] = CommandLineEscaper.Escape(args[i]);
            return string.Join(" ", parts);
        }

        private int LoadWorktreesFromPorcelain(string gitExe, string output)
        {
            dgvWorktrees.Rows.Clear();
            if (string.IsNullOrEmpty(output)) { UpdateRowLabel(); return 0; }
            string curPath = null, curBranch = null;
            var entries = new List<WorktreeEntry>();
            foreach (var raw in output.Replace("\r\n", "\n").Split('\n'))
            {
                var line = raw;
                if (line.StartsWith("worktree "))
                {
                    if (curPath != null) entries.Add(MkEntry(curPath, curBranch));
                    curPath = line.Substring("worktree ".Length).Trim();
                    curBranch = null;
                }
                else if (line.StartsWith("branch "))
                {
                    string b = line.Substring("branch ".Length).Trim();
                    if (b.StartsWith("refs/heads/")) b = b.Substring("refs/heads/".Length);
                    curBranch = b;
                }
                else if (line.Length == 0)
                {
                    if (curPath != null) { entries.Add(MkEntry(curPath, curBranch)); curPath = null; curBranch = null; }
                }
            }
            if (curPath != null) entries.Add(MkEntry(curPath, curBranch));
            foreach (var en in entries)
            {
                if (dgvWorktrees.Rows.Count >= MaxRows) break;
                int idx = dgvWorktrees.Rows.Add();
                var row = dgvWorktrees.Rows[idx];
                row.Cells[colNum.Name].Value = idx + 1;
                row.Cells[colPath.Name].Value = en.Path;
                row.Cells[colBranch.Name].Value = en.Branch;
                row.Cells[colName.Name].Value = en.UserName;
                row.Cells[colEmail.Name].Value = en.UserEmail;
            }
            RenumberRows();
            UpdateRowLabel();
            // 类型列 & 主仓库标记从 Path/.git 是目录还是文件自动推导
            RefreshAllRowTypes();
            // 逐行按最终 IsMain 状态再拉一次 scope 正确的 user.name/email
            foreach (DataGridViewRow row in dgvWorktrees.Rows)
            {
                string path = AsStr(row.Cells[colPath.Name].Value).Trim();
                bool isMain = DetectIsMain(path);
                string scope = isMain ? "--local" : "--worktree";
                row.Cells[colName.Name].Value = ReadConfig(gitExe, path, scope, "user.name");
                row.Cells[colEmail.Name].Value = ReadConfig(gitExe, path, scope, "user.email");
            }
            return entries.Count;
        }

        private string ReadConfig(string gitExe, string workDir, string scopeFlag, string key)
        {
            if (string.IsNullOrWhiteSpace(workDir)) return "";
            try
            {
                string so, se;
                int exit = RunCapture(gitExe,
                    new GitCommand("-C", workDir, "config", scopeFlag, key),
                    out so, out se);
                return exit == 0 ? (so ?? "").Trim() : "";
            }
            catch { return ""; }
        }

        private static WorktreeEntry MkEntry(string path, string branch)
        {
            return new WorktreeEntry { Path = path, Branch = branch };
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
            RefreshAllRowTypes();
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
                string path = AsStr(row.Cells[colPath.Name].Value);
                s.Entries.Add(new WorktreeEntry
                {
                    IsMain = DetectIsMain(path),
                    Path = path,
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

        // ---------- ITerminalSink ----------

        public void Write(string line, TermKind kind) { AppendTerm(line, kind); }

        private void AppendTerm(string line, TermKind kind)
        {
            if (rchTerm.InvokeRequired)
            {
                rchTerm.BeginInvoke(new Action<string, TermKind>(AppendTerm), line, kind);
                return;
            }
            string text = (line ?? "") + Environment.NewLine;
            int start = rchTerm.TextLength;
            rchTerm.AppendText(text);
            rchTerm.Select(start, (line ?? "").Length);
            rchTerm.SelectionColor = ColorFor(kind);
            if (kind == TermKind.Fail) rchTerm.SelectionFont = new Font(rchTerm.Font, FontStyle.Bold);
            else rchTerm.SelectionFont = rchTerm.Font;
            rchTerm.Select(rchTerm.TextLength, 0);
            rchTerm.ScrollToCaret();
            stsLast.Text = string.IsNullOrEmpty(line) ? "" : (line.Length > 80 ? line.Substring(0, 80) + "..." : line);
        }

        private static Color ColorFor(TermKind k)
        {
            switch (k)
            {
                case TermKind.Cmd: return ColCmd;
                case TermKind.Err: return ColErr;
                case TermKind.Meta: return ColMeta;
                case TermKind.Done: return ColDone;
                case TermKind.Fail: return ColFail;
                case TermKind.Prompt: return ColMeta;
                default: return ColStd;
            }
        }

        private void LogCmd(string line) { AppendTerm(line, TermKind.Cmd); }
        private void LogMeta(string line) { AppendTerm(line, TermKind.Meta); }
        private void LogErr(string line) { AppendTerm(line, TermKind.Err); }
    }
}
