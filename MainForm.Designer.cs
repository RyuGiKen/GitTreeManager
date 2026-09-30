namespace GitTreeManager
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.grpRepo = new System.Windows.Forms.GroupBox();
            this.lblRepoPath = new System.Windows.Forms.Label();
            this.txtRepoPath = new System.Windows.Forms.TextBox();
            this.btnBrowseRepo = new System.Windows.Forms.Button();
            this.rbNew = new System.Windows.Forms.RadioButton();
            this.rbClone = new System.Windows.Forms.RadioButton();
            this.lblRemoteUrl = new System.Windows.Forms.Label();
            this.txtRemoteUrl = new System.Windows.Forms.TextBox();
            this.lblDefaultBranch = new System.Windows.Forms.Label();
            this.txtDefaultBranch = new System.Windows.Forms.TextBox();
            this.tabs = new System.Windows.Forms.TabControl();
            this.tabWorktrees = new System.Windows.Forms.TabPage();
            this.pnlWtToolbar = new System.Windows.Forms.Panel();
            this.btnWtAdd = new System.Windows.Forms.Button();
            this.btnWtRemove = new System.Windows.Forms.Button();
            this.btnWtUp = new System.Windows.Forms.Button();
            this.btnWtDown = new System.Windows.Forms.Button();
            this.btnWtRead = new System.Windows.Forms.Button();
            this.btnWtClear = new System.Windows.Forms.Button();
            this.lblWtCount = new System.Windows.Forms.Label();
            this.dgvWorktrees = new System.Windows.Forms.DataGridView();
            this.colNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colBranch = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEmail = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlWtBottom = new System.Windows.Forms.Panel();
            this.chkDryRun = new System.Windows.Forms.CheckBox();
            this.btnPreview = new System.Windows.Forms.Button();
            this.btnExecute = new System.Windows.Forms.Button();
            this.tabLog = new System.Windows.Forms.TabPage();
            this.pnlTermToolbar = new System.Windows.Forms.Panel();
            this.btnTermClear = new System.Windows.Forms.Button();
            this.btnTermCancel = new System.Windows.Forms.Button();
            this.btnTermExportBat = new System.Windows.Forms.Button();
            this.btnTermExportPs1 = new System.Windows.Forms.Button();
            this.lblTermHint = new System.Windows.Forms.Label();
            this.rchTerm = new System.Windows.Forms.RichTextBox();
            this.pnlTermInput = new System.Windows.Forms.Panel();
            this.lblPrompt = new System.Windows.Forms.Label();
            this.txtTermInput = new System.Windows.Forms.TextBox();
            this.grpCommon = new System.Windows.Forms.GroupBox();
            this.btnCleanOrphan = new System.Windows.Forms.Button();
            this.btnCleanMerged = new System.Windows.Forms.Button();
            this.btnUpdateRemote = new System.Windows.Forms.Button();
            this.btnDiskAnalyze = new System.Windows.Forms.Button();
            this.btnAlignCommit = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.stsGit = new System.Windows.Forms.ToolStripStatusLabel();
            this.stsSep1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.stsRepo = new System.Windows.Forms.ToolStripStatusLabel();
            this.stsSep2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.stsLast = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnGitBrowse = new System.Windows.Forms.Button();
            this.lblGitExe = new System.Windows.Forms.Label();
            this.txtGitExe = new System.Windows.Forms.TextBox();
            this.btnInitRepo = new System.Windows.Forms.Button();
            this.pnlGlobal = new System.Windows.Forms.Panel();
            this.lblGlobalScope = new System.Windows.Forms.Label();
            this.grpRepo.SuspendLayout();
            this.tabs.SuspendLayout();
            this.tabWorktrees.SuspendLayout();
            this.pnlWtToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWorktrees)).BeginInit();
            this.pnlWtBottom.SuspendLayout();
            this.tabLog.SuspendLayout();
            this.pnlTermToolbar.SuspendLayout();
            this.rchTerm.SuspendLayout();
            this.pnlTermInput.SuspendLayout();
            this.grpCommon.SuspendLayout();
            this.pnlGlobal.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            //
            // grpRepo
            //
            this.grpRepo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpRepo.Controls.Add(this.lblRepoPath);
            this.grpRepo.Controls.Add(this.txtRepoPath);
            this.grpRepo.Controls.Add(this.btnBrowseRepo);
            this.grpRepo.Controls.Add(this.rbNew);
            this.grpRepo.Controls.Add(this.rbClone);
            this.grpRepo.Controls.Add(this.lblRemoteUrl);
            this.grpRepo.Controls.Add(this.txtRemoteUrl);
            this.grpRepo.Controls.Add(this.lblDefaultBranch);
            this.grpRepo.Controls.Add(this.txtDefaultBranch);
            this.grpRepo.Controls.Add(this.lblGitExe);
            this.grpRepo.Controls.Add(this.txtGitExe);
            this.grpRepo.Controls.Add(this.btnGitBrowse);
            this.grpRepo.Controls.Add(this.btnInitRepo);
            this.grpRepo.Location = new System.Drawing.Point(12, 12);
            this.grpRepo.Name = "grpRepo";
            this.grpRepo.Size = new System.Drawing.Size(1096, 138);
            this.grpRepo.TabIndex = 0;
            this.grpRepo.TabStop = false;
            this.grpRepo.Text = "仓库位置";
            //
            // lblRepoPath
            //
            this.lblRepoPath.AutoSize = true;
            this.lblRepoPath.Location = new System.Drawing.Point(14, 30);
            this.lblRepoPath.Name = "lblRepoPath";
            this.lblRepoPath.Size = new System.Drawing.Size(53, 12);
            this.lblRepoPath.TabIndex = 0;
            this.lblRepoPath.Text = "本地路径";
            //
            // txtRepoPath
            //
            this.txtRepoPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRepoPath.Location = new System.Drawing.Point(96, 26);
            this.txtRepoPath.Name = "txtRepoPath";
            this.txtRepoPath.Size = new System.Drawing.Size(852, 21);
            this.txtRepoPath.TabIndex = 1;
            //
            // btnBrowseRepo
            //
            this.btnBrowseRepo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseRepo.Location = new System.Drawing.Point(968, 25);
            this.btnBrowseRepo.Name = "btnBrowseRepo";
            this.btnBrowseRepo.Size = new System.Drawing.Size(116, 25);
            this.btnBrowseRepo.TabIndex = 2;
            this.btnBrowseRepo.Text = "浏览...";
            this.btnBrowseRepo.UseVisualStyleBackColor = true;
            this.btnBrowseRepo.Click += new System.EventHandler(this.btnBrowseRepo_Click);
            //
            // rbNew
            //
            this.rbNew.AutoSize = true;
            this.rbNew.Checked = true;
            this.rbNew.Location = new System.Drawing.Point(16, 60);
            this.rbNew.Name = "rbNew";
            this.rbNew.Size = new System.Drawing.Size(83, 16);
            this.rbNew.TabIndex = 3;
            this.rbNew.TabStop = true;
            this.rbNew.Text = "新建仓库";
            this.rbNew.UseVisualStyleBackColor = true;
            this.rbNew.CheckedChanged += new System.EventHandler(this.RbMode_CheckedChanged);
            //
            // rbClone
            //
            this.rbClone.AutoSize = true;
            this.rbClone.Location = new System.Drawing.Point(116, 60);
            this.rbClone.Name = "rbClone";
            this.rbClone.Size = new System.Drawing.Size(83, 16);
            this.rbClone.TabIndex = 4;
            this.rbClone.Text = "克隆已有";
            this.rbClone.UseVisualStyleBackColor = true;
            this.rbClone.CheckedChanged += new System.EventHandler(this.RbMode_CheckedChanged);
            //
            // lblRemoteUrl
            //
            this.lblRemoteUrl.AutoSize = true;
            this.lblRemoteUrl.Location = new System.Drawing.Point(230, 62);
            this.lblRemoteUrl.Name = "lblRemoteUrl";
            this.lblRemoteUrl.Size = new System.Drawing.Size(53, 12);
            this.lblRemoteUrl.TabIndex = 5;
            this.lblRemoteUrl.Text = "远程 URL";
            //
            // txtRemoteUrl
            //
            this.txtRemoteUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRemoteUrl.Enabled = false;
            this.txtRemoteUrl.Location = new System.Drawing.Point(312, 58);
            this.txtRemoteUrl.Name = "txtRemoteUrl";
            this.txtRemoteUrl.Size = new System.Drawing.Size(636, 21);
            this.txtRemoteUrl.TabIndex = 6;
            //
            // lblDefaultBranch
            //
            this.lblDefaultBranch.AutoSize = true;
            this.lblDefaultBranch.Location = new System.Drawing.Point(14, 94);
            this.lblDefaultBranch.Name = "lblDefaultBranch";
            this.lblDefaultBranch.Size = new System.Drawing.Size(53, 12);
            this.lblDefaultBranch.TabIndex = 7;
            this.lblDefaultBranch.Text = "默认分支";
            //
            // txtDefaultBranch
            //
            this.txtDefaultBranch.Location = new System.Drawing.Point(96, 90);
            this.txtDefaultBranch.Name = "txtDefaultBranch";
            this.txtDefaultBranch.Size = new System.Drawing.Size(156, 21);
            this.txtDefaultBranch.TabIndex = 8;
            this.txtDefaultBranch.Text = "main";
            //
            // lblGitExe
            //
            this.lblGitExe.AutoSize = true;
            this.lblGitExe.Location = new System.Drawing.Point(260, 94);
            this.lblGitExe.Name = "lblGitExe";
            this.lblGitExe.Size = new System.Drawing.Size(53, 12);
            this.lblGitExe.TabIndex = 9;
            this.lblGitExe.Text = "git.exe";
            //
            // txtGitExe
            //
            this.txtGitExe.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGitExe.Location = new System.Drawing.Point(326, 90);
            this.txtGitExe.Name = "txtGitExe";
            this.txtGitExe.Size = new System.Drawing.Size(622, 21);
            this.txtGitExe.TabIndex = 10;
            this.txtGitExe.Text = "git";
            //
            // btnGitBrowse
            //
            this.btnGitBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGitBrowse.Location = new System.Drawing.Point(968, 89);
            this.btnGitBrowse.Name = "btnGitBrowse";
            this.btnGitBrowse.Size = new System.Drawing.Size(116, 25);
            this.btnGitBrowse.TabIndex = 11;
            this.btnGitBrowse.Text = "浏览...";
            this.btnGitBrowse.UseVisualStyleBackColor = true;
            this.btnGitBrowse.Click += new System.EventHandler(this.btnGitBrowse_Click);
            //
            // btnInitRepo
            //
            this.btnInitRepo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInitRepo.Location = new System.Drawing.Point(968, 57);
            this.btnInitRepo.Name = "btnInitRepo";
            this.btnInitRepo.Size = new System.Drawing.Size(116, 25);
            this.btnInitRepo.TabIndex = 12;
            this.btnInitRepo.Text = "创建";
            this.btnInitRepo.UseVisualStyleBackColor = true;
            this.btnInitRepo.Click += new System.EventHandler(this.btnInitRepo_Click);
            //
            // tabs
            //
            this.tabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabs.Controls.Add(this.tabWorktrees);
            this.tabs.Controls.Add(this.tabLog);
            this.tabs.Location = new System.Drawing.Point(12, 156);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(1096, 440);
            this.tabs.TabIndex = 1;
            //
            // tabWorktrees
            //
            this.tabWorktrees.Controls.Add(this.pnlWtToolbar);
            this.tabWorktrees.Controls.Add(this.dgvWorktrees);
            this.tabWorktrees.Controls.Add(this.pnlWtBottom);
            this.tabWorktrees.Location = new System.Drawing.Point(4, 22);
            this.tabWorktrees.Name = "tabWorktrees";
            this.tabWorktrees.Padding = new System.Windows.Forms.Padding(4);
            this.tabWorktrees.Size = new System.Drawing.Size(1088, 412);
            this.tabWorktrees.TabIndex = 0;
            this.tabWorktrees.Text = "Worktree";
            this.tabWorktrees.UseVisualStyleBackColor = true;
            //
            // pnlWtToolbar
            //
            this.pnlWtToolbar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlWtToolbar.Controls.Add(this.btnWtAdd);
            this.pnlWtToolbar.Controls.Add(this.btnWtRemove);
            this.pnlWtToolbar.Controls.Add(this.btnWtUp);
            this.pnlWtToolbar.Controls.Add(this.btnWtDown);
            this.pnlWtToolbar.Controls.Add(this.btnWtRead);
            this.pnlWtToolbar.Controls.Add(this.btnWtClear);
            this.pnlWtToolbar.Controls.Add(this.lblWtCount);
            this.pnlWtToolbar.Location = new System.Drawing.Point(4, 4);
            this.pnlWtToolbar.Name = "pnlWtToolbar";
            this.pnlWtToolbar.Size = new System.Drawing.Size(1080, 32);
            this.pnlWtToolbar.TabIndex = 0;
            //
            // btnWtAdd
            //
            this.btnWtAdd.Location = new System.Drawing.Point(6, 3);
            this.btnWtAdd.Name = "btnWtAdd";
            this.btnWtAdd.Size = new System.Drawing.Size(80, 26);
            this.btnWtAdd.TabIndex = 0;
            this.btnWtAdd.Text = "添加行";
            this.btnWtAdd.UseVisualStyleBackColor = true;
            this.btnWtAdd.Click += new System.EventHandler(this.btnWtAdd_Click);
            //
            // btnWtRemove
            //
            this.btnWtRemove.Location = new System.Drawing.Point(92, 3);
            this.btnWtRemove.Name = "btnWtRemove";
            this.btnWtRemove.Size = new System.Drawing.Size(80, 26);
            this.btnWtRemove.TabIndex = 1;
            this.btnWtRemove.Text = "删除行";
            this.btnWtRemove.UseVisualStyleBackColor = true;
            this.btnWtRemove.Click += new System.EventHandler(this.btnWtRemove_Click);
            //
            // btnWtUp
            //
            this.btnWtUp.Location = new System.Drawing.Point(178, 3);
            this.btnWtUp.Name = "btnWtUp";
            this.btnWtUp.Size = new System.Drawing.Size(50, 26);
            this.btnWtUp.TabIndex = 2;
            this.btnWtUp.Text = "上移";
            this.btnWtUp.UseVisualStyleBackColor = true;
            this.btnWtUp.Click += new System.EventHandler(this.btnWtUp_Click);
            //
            // btnWtDown
            //
            this.btnWtDown.Location = new System.Drawing.Point(234, 3);
            this.btnWtDown.Name = "btnWtDown";
            this.btnWtDown.Size = new System.Drawing.Size(50, 26);
            this.btnWtDown.TabIndex = 3;
            this.btnWtDown.Text = "下移";
            this.btnWtDown.UseVisualStyleBackColor = true;
            this.btnWtDown.Click += new System.EventHandler(this.btnWtDown_Click);
            //
            // btnWtRead
            //
            this.btnWtRead.Location = new System.Drawing.Point(290, 3);
            this.btnWtRead.Name = "btnWtRead";
            this.btnWtRead.Size = new System.Drawing.Size(140, 26);
            this.btnWtRead.TabIndex = 4;
            this.btnWtRead.Text = "读取现有 worktree";
            this.btnWtRead.UseVisualStyleBackColor = true;
            this.btnWtRead.Click += new System.EventHandler(this.btnWtRead_Click);
            //
            // btnWtClear
            //
            this.btnWtClear.Location = new System.Drawing.Point(436, 3);
            this.btnWtClear.Name = "btnWtClear";
            this.btnWtClear.Size = new System.Drawing.Size(80, 26);
            this.btnWtClear.TabIndex = 5;
            this.btnWtClear.Text = "清空";
            this.btnWtClear.UseVisualStyleBackColor = true;
            this.btnWtClear.Click += new System.EventHandler(this.btnWtClear_Click);
            //
            // lblWtCount
            //
            this.lblWtCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWtCount.AutoSize = true;
            this.lblWtCount.Location = new System.Drawing.Point(990, 10);
            this.lblWtCount.Name = "lblWtCount";
            this.lblWtCount.Size = new System.Drawing.Size(53, 12);
            this.lblWtCount.TabIndex = 6;
            this.lblWtCount.Text = "行数 0/99";
            //
            // dgvWorktrees
            //
            this.dgvWorktrees.AllowUserToAddRows = false;
            this.dgvWorktrees.AllowUserToDeleteRows = false;
            this.dgvWorktrees.AllowUserToResizeRows = false;
            this.dgvWorktrees.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvWorktrees.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvWorktrees.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colNum,
            this.colType,
            this.colPath,
            this.colBranch,
            this.colName,
            this.colEmail});
            this.dgvWorktrees.Location = new System.Drawing.Point(4, 40);
            this.dgvWorktrees.MultiSelect = false;
            this.dgvWorktrees.Name = "dgvWorktrees";
            this.dgvWorktrees.RowHeadersVisible = false;
            this.dgvWorktrees.RowTemplate.Height = 23;
            this.dgvWorktrees.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvWorktrees.Size = new System.Drawing.Size(1080, 314);
            this.dgvWorktrees.TabIndex = 1;
            //
            // colNum
            //
            this.colNum.HeaderText = "#";
            this.colNum.Name = "colNum";
            this.colNum.ReadOnly = true;
            this.colNum.Width = 40;
            //
            // colType
            //
            this.colType.HeaderText = "类型";
            this.colType.Name = "colType";
            this.colType.ReadOnly = true;
            this.colType.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.colType.Width = 68;
            //
            // colPath
            //
            this.colPath.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colPath.FillWeight = 45F;
            this.colPath.HeaderText = "worktree 路径";
            this.colPath.Name = "colPath";
            //
            // colBranch
            //
            this.colBranch.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colBranch.FillWeight = 22F;
            this.colBranch.HeaderText = "分支";
            this.colBranch.Name = "colBranch";
            //
            // colName
            //
            this.colName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colName.FillWeight = 16F;
            this.colName.HeaderText = "user.name";
            this.colName.Name = "colName";
            //
            // colEmail
            //
            this.colEmail.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colEmail.FillWeight = 17F;
            this.colEmail.HeaderText = "user.email";
            this.colEmail.Name = "colEmail";
            //
            // pnlWtBottom
            //
            this.pnlWtBottom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlWtBottom.Controls.Add(this.btnExecute);
            this.pnlWtBottom.Location = new System.Drawing.Point(4, 360);
            this.pnlWtBottom.Name = "pnlWtBottom";
            this.pnlWtBottom.Size = new System.Drawing.Size(1080, 46);
            this.pnlWtBottom.TabIndex = 2;
            //
            // chkDryRun
            //
            this.chkDryRun.AutoSize = true;
            this.chkDryRun.Checked = true;
            this.chkDryRun.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDryRun.Location = new System.Drawing.Point(8, 6);
            this.chkDryRun.Name = "chkDryRun";
            this.chkDryRun.Size = new System.Drawing.Size(227, 16);
            this.chkDryRun.TabIndex = 0;
            this.chkDryRun.Text = "Dry-run（全局：勾选后所有操作只打印命令、不执行）";
            this.chkDryRun.UseVisualStyleBackColor = true;
            //
            // btnPreview
            //
            this.btnPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPreview.Location = new System.Drawing.Point(970, 2);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(120, 25);
            this.btnPreview.TabIndex = 2;
            this.btnPreview.Text = "预览全部命令";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            //
            // btnExecute
            //
            this.btnExecute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExecute.Location = new System.Drawing.Point(954, 10);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(120, 28);
            this.btnExecute.TabIndex = 2;
            this.btnExecute.Text = "执行";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            //
            // tabLog (终端)
            //
            this.tabLog.Controls.Add(this.pnlTermToolbar);
            this.tabLog.Controls.Add(this.rchTerm);
            this.tabLog.Controls.Add(this.pnlTermInput);
            this.tabLog.Location = new System.Drawing.Point(4, 22);
            this.tabLog.Name = "tabLog";
            this.tabLog.Padding = new System.Windows.Forms.Padding(4);
            this.tabLog.Size = new System.Drawing.Size(1088, 412);
            this.tabLog.TabIndex = 1;
            this.tabLog.Text = "终端";
            this.tabLog.UseVisualStyleBackColor = true;
            //
            // pnlTermToolbar
            //
            this.pnlTermToolbar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlTermToolbar.Controls.Add(this.btnTermClear);
            this.pnlTermToolbar.Controls.Add(this.btnTermCancel);
            this.pnlTermToolbar.Controls.Add(this.btnTermExportBat);
            this.pnlTermToolbar.Controls.Add(this.btnTermExportPs1);
            this.pnlTermToolbar.Controls.Add(this.lblTermHint);
            this.pnlTermToolbar.Location = new System.Drawing.Point(4, 4);
            this.pnlTermToolbar.Name = "pnlTermToolbar";
            this.pnlTermToolbar.Size = new System.Drawing.Size(1080, 32);
            this.pnlTermToolbar.TabIndex = 0;
            //
            // btnTermClear
            //
            this.btnTermClear.Location = new System.Drawing.Point(6, 3);
            this.btnTermClear.Name = "btnTermClear";
            this.btnTermClear.Size = new System.Drawing.Size(90, 26);
            this.btnTermClear.TabIndex = 0;
            this.btnTermClear.Text = "清空";
            this.btnTermClear.UseVisualStyleBackColor = true;
            this.btnTermClear.Click += new System.EventHandler(this.btnTermClear_Click);
            //
            // btnTermCancel
            //
            this.btnTermCancel.Enabled = false;
            this.btnTermCancel.Location = new System.Drawing.Point(102, 3);
            this.btnTermCancel.Name = "btnTermCancel";
            this.btnTermCancel.Size = new System.Drawing.Size(90, 26);
            this.btnTermCancel.TabIndex = 1;
            this.btnTermCancel.Text = "中断";
            this.btnTermCancel.UseVisualStyleBackColor = true;
            this.btnTermCancel.Click += new System.EventHandler(this.btnTermCancel_Click);
            //
            // btnTermExportBat
            //
            this.btnTermExportBat.Location = new System.Drawing.Point(198, 3);
            this.btnTermExportBat.Name = "btnTermExportBat";
            this.btnTermExportBat.Size = new System.Drawing.Size(100, 26);
            this.btnTermExportBat.TabIndex = 2;
            this.btnTermExportBat.Text = "导出 .bat";
            this.btnTermExportBat.UseVisualStyleBackColor = true;
            this.btnTermExportBat.Click += new System.EventHandler(this.btnTermExportBat_Click);
            //
            // btnTermExportPs1
            //
            this.btnTermExportPs1.Location = new System.Drawing.Point(304, 3);
            this.btnTermExportPs1.Name = "btnTermExportPs1";
            this.btnTermExportPs1.Size = new System.Drawing.Size(100, 26);
            this.btnTermExportPs1.TabIndex = 3;
            this.btnTermExportPs1.Text = "导出 .ps1";
            this.btnTermExportPs1.UseVisualStyleBackColor = true;
            this.btnTermExportPs1.Click += new System.EventHandler(this.btnTermExportPs1_Click);
            //
            // lblTermHint
            //
            this.lblTermHint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTermHint.AutoSize = true;
            this.lblTermHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblTermHint.Location = new System.Drawing.Point(760, 10);
            this.lblTermHint.Name = "lblTermHint";
            this.lblTermHint.Size = new System.Drawing.Size(0, 12);
            this.lblTermHint.TabIndex = 4;
            this.lblTermHint.Text = "Ctrl+C 中断 · ↑↓ 翻历史 · 输入自动加 git 前缀";
            //
            // rchTerm
            //
            this.rchTerm.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rchTerm.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.rchTerm.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rchTerm.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
            this.rchTerm.Font = new System.Drawing.Font("Consolas", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rchTerm.HideSelection = false;
            this.rchTerm.Location = new System.Drawing.Point(4, 40);
            this.rchTerm.Name = "rchTerm";
            this.rchTerm.ReadOnly = true;
            this.rchTerm.Size = new System.Drawing.Size(1080, 328);
            this.rchTerm.TabIndex = 1;
            this.rchTerm.Text = "";
            this.rchTerm.WordWrap = false;
            //
            // pnlTermInput
            //
            this.pnlTermInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlTermInput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.pnlTermInput.Controls.Add(this.lblPrompt);
            this.pnlTermInput.Controls.Add(this.txtTermInput);
            this.pnlTermInput.Location = new System.Drawing.Point(4, 372);
            this.pnlTermInput.Name = "pnlTermInput";
            this.pnlTermInput.Size = new System.Drawing.Size(1080, 30);
            this.pnlTermInput.TabIndex = 2;
            //
            // lblPrompt
            //
            this.lblPrompt.AutoSize = true;
            this.lblPrompt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.lblPrompt.Font = new System.Drawing.Font("Consolas", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPrompt.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(86)))), ((int)(((byte)(156)))), ((int)(((byte)(214)))));
            this.lblPrompt.Location = new System.Drawing.Point(6, 7);
            this.lblPrompt.Name = "lblPrompt";
            this.lblPrompt.Size = new System.Drawing.Size(0, 17);
            this.lblPrompt.TabIndex = 0;
            this.lblPrompt.Text = "$";
            //
            // txtTermInput
            //
            this.txtTermInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTermInput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.txtTermInput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtTermInput.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
            this.txtTermInput.Font = new System.Drawing.Font("Consolas", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTermInput.Location = new System.Drawing.Point(24, 6);
            this.txtTermInput.Name = "txtTermInput";
            this.txtTermInput.Size = new System.Drawing.Size(1050, 18);
            this.txtTermInput.TabIndex = 1;
            this.txtTermInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtTermInput_KeyDown);
            //
            // grpCommon
            //
            this.grpCommon.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpCommon.Controls.Add(this.btnCleanOrphan);
            this.grpCommon.Controls.Add(this.btnCleanMerged);
            this.grpCommon.Controls.Add(this.btnUpdateRemote);
            this.grpCommon.Controls.Add(this.btnDiskAnalyze);
            this.grpCommon.Controls.Add(this.btnAlignCommit);
            this.grpCommon.Location = new System.Drawing.Point(12, 630);
            this.grpCommon.Name = "grpCommon";
            this.grpCommon.Size = new System.Drawing.Size(1096, 62);
            this.grpCommon.TabIndex = 2;
            this.grpCommon.TabStop = false;
            this.grpCommon.Text = "常用功能";
            //
            // btnCleanOrphan
            //
            this.btnCleanOrphan.Location = new System.Drawing.Point(12, 24);
            this.btnCleanOrphan.Name = "btnCleanOrphan";
            this.btnCleanOrphan.Size = new System.Drawing.Size(200, 30);
            this.btnCleanOrphan.TabIndex = 0;
            this.btnCleanOrphan.Text = "清理多余提交和引用记录";
            this.btnCleanOrphan.UseVisualStyleBackColor = true;
            this.btnCleanOrphan.Click += new System.EventHandler(this.btnCleanOrphan_Click);
            //
            // btnCleanMerged
            //
            this.btnCleanMerged.Location = new System.Drawing.Point(230, 24);
            this.btnCleanMerged.Name = "btnCleanMerged";
            this.btnCleanMerged.Size = new System.Drawing.Size(200, 30);
            this.btnCleanMerged.TabIndex = 1;
            this.btnCleanMerged.Text = "清理已合并分支";
            this.btnCleanMerged.UseVisualStyleBackColor = true;
            this.btnCleanMerged.Click += new System.EventHandler(this.btnCleanMerged_Click);
            //
            // btnUpdateRemote
            //
            this.btnUpdateRemote.Location = new System.Drawing.Point(448, 24);
            this.btnUpdateRemote.Name = "btnUpdateRemote";
            this.btnUpdateRemote.Size = new System.Drawing.Size(200, 30);
            this.btnUpdateRemote.TabIndex = 2;
            this.btnUpdateRemote.Text = "更新远端";
            this.btnUpdateRemote.UseVisualStyleBackColor = true;
            this.btnUpdateRemote.Click += new System.EventHandler(this.btnUpdateRemote_Click);
            //
            // btnDiskAnalyze
            //
            this.btnDiskAnalyze.Location = new System.Drawing.Point(666, 24);
            this.btnDiskAnalyze.Name = "btnDiskAnalyze";
            this.btnDiskAnalyze.Size = new System.Drawing.Size(200, 30);
            this.btnDiskAnalyze.TabIndex = 3;
            this.btnDiskAnalyze.Text = "仓库磁盘分析";
            this.btnDiskAnalyze.UseVisualStyleBackColor = true;
            this.btnDiskAnalyze.Click += new System.EventHandler(this.btnDiskAnalyze_Click);
            //
            // btnAlignCommit
            //
            this.btnAlignCommit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAlignCommit.Location = new System.Drawing.Point(884, 24);
            this.btnAlignCommit.Name = "btnAlignCommit";
            this.btnAlignCommit.Size = new System.Drawing.Size(200, 30);
            this.btnAlignCommit.TabIndex = 4;
            this.btnAlignCommit.Text = "对齐最新提交";
            this.btnAlignCommit.UseVisualStyleBackColor = true;
            this.btnAlignCommit.Click += new System.EventHandler(this.btnAlignCommit_Click);
            //
            // pnlGlobal
            //
            this.pnlGlobal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlGlobal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlGlobal.Controls.Add(this.chkDryRun);
            this.pnlGlobal.Controls.Add(this.btnPreview);
            this.pnlGlobal.Controls.Add(this.lblGlobalScope);
            this.pnlGlobal.Location = new System.Drawing.Point(12, 602);
            this.pnlGlobal.Name = "pnlGlobal";
            this.pnlGlobal.Size = new System.Drawing.Size(1096, 28);
            this.pnlGlobal.TabIndex = 4;
            //
            // lblGlobalScope
            //
            this.lblGlobalScope.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGlobalScope.AutoSize = true;
            this.lblGlobalScope.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblGlobalScope.Location = new System.Drawing.Point(680, 7);
            this.lblGlobalScope.Name = "lblGlobalScope";
            this.lblGlobalScope.Size = new System.Drawing.Size(0, 12);
            this.lblGlobalScope.TabIndex = 3;
            this.lblGlobalScope.Text = "影响 Worktree 执行 · 创建/克隆 · 5 个常用功能";
            //
            // statusStrip
            //
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stsGit,
            this.stsSep1,
            this.stsRepo,
            this.stsSep2,
            this.stsLast});
            this.statusStrip.Location = new System.Drawing.Point(0, 699);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1120, 22);
            this.statusStrip.TabIndex = 3;
            //
            // stsGit
            //
            this.stsGit.Name = "stsGit";
            this.stsGit.Size = new System.Drawing.Size(63, 17);
            this.stsGit.Text = "git: 检测中";
            //
            // stsSep1
            //
            this.stsSep1.Name = "stsSep1";
            this.stsSep1.Size = new System.Drawing.Size(10, 17);
            this.stsSep1.Text = "|";
            //
            // stsRepo
            //
            this.stsRepo.Name = "stsRepo";
            this.stsRepo.Size = new System.Drawing.Size(59, 17);
            this.stsRepo.Text = "仓库: 未选";
            //
            // stsSep2
            //
            this.stsSep2.Name = "stsSep2";
            this.stsSep2.Size = new System.Drawing.Size(10, 17);
            this.stsSep2.Text = "|";
            //
            // stsLast
            //
            this.stsLast.Name = "stsLast";
            this.stsLast.Size = new System.Drawing.Size(65, 17);
            this.stsLast.Text = "等待操作";
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1120, 721);
            this.Controls.Add(this.grpRepo);
            this.Controls.Add(this.tabs);
            this.Controls.Add(this.pnlGlobal);
            this.Controls.Add(this.grpCommon);
            this.Controls.Add(this.statusStrip);
            this.MinimumSize = new System.Drawing.Size(920, 640);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Git Tree Manager";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.grpRepo.ResumeLayout(false);
            this.grpRepo.PerformLayout();
            this.tabs.ResumeLayout(false);
            this.tabWorktrees.ResumeLayout(false);
            this.pnlWtToolbar.ResumeLayout(false);
            this.pnlWtToolbar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWorktrees)).EndInit();
            this.pnlWtBottom.ResumeLayout(false);
            this.pnlWtBottom.PerformLayout();
            this.tabLog.ResumeLayout(false);
            this.pnlTermToolbar.ResumeLayout(false);
            this.pnlTermToolbar.PerformLayout();
            this.rchTerm.ResumeLayout(false);
            this.pnlTermInput.ResumeLayout(false);
            this.pnlTermInput.PerformLayout();
            this.grpCommon.ResumeLayout(false);
            this.pnlGlobal.ResumeLayout(false);
            this.pnlGlobal.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.GroupBox grpRepo;
        private System.Windows.Forms.Label lblRepoPath;
        private System.Windows.Forms.TextBox txtRepoPath;
        private System.Windows.Forms.Button btnBrowseRepo;
        private System.Windows.Forms.RadioButton rbNew;
        private System.Windows.Forms.RadioButton rbClone;
        private System.Windows.Forms.Label lblRemoteUrl;
        private System.Windows.Forms.TextBox txtRemoteUrl;
        private System.Windows.Forms.Label lblDefaultBranch;
        private System.Windows.Forms.TextBox txtDefaultBranch;
        private System.Windows.Forms.Label lblGitExe;
        private System.Windows.Forms.TextBox txtGitExe;
        private System.Windows.Forms.Button btnGitBrowse;
        private System.Windows.Forms.Button btnInitRepo;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage tabWorktrees;
        private System.Windows.Forms.Panel pnlWtToolbar;
        private System.Windows.Forms.Button btnWtAdd;
        private System.Windows.Forms.Button btnWtRemove;
        private System.Windows.Forms.Button btnWtUp;
        private System.Windows.Forms.Button btnWtDown;
        private System.Windows.Forms.Button btnWtRead;
        private System.Windows.Forms.Button btnWtClear;
        private System.Windows.Forms.Label lblWtCount;
        private System.Windows.Forms.DataGridView dgvWorktrees;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn colType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPath;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBranch;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEmail;
        private System.Windows.Forms.Panel pnlWtBottom;
        private System.Windows.Forms.CheckBox chkDryRun;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.TabPage tabLog;
        private System.Windows.Forms.Panel pnlTermToolbar;
        private System.Windows.Forms.Button btnTermClear;
        private System.Windows.Forms.Button btnTermCancel;
        private System.Windows.Forms.Button btnTermExportBat;
        private System.Windows.Forms.Button btnTermExportPs1;
        private System.Windows.Forms.Label lblTermHint;
        private System.Windows.Forms.RichTextBox rchTerm;
        private System.Windows.Forms.Panel pnlTermInput;
        private System.Windows.Forms.Label lblPrompt;
        private System.Windows.Forms.TextBox txtTermInput;
        private System.Windows.Forms.GroupBox grpCommon;
        private System.Windows.Forms.Panel pnlGlobal;
        private System.Windows.Forms.Label lblGlobalScope;
        private System.Windows.Forms.Button btnCleanOrphan;
        private System.Windows.Forms.Button btnCleanMerged;
        private System.Windows.Forms.Button btnUpdateRemote;
        private System.Windows.Forms.Button btnDiskAnalyze;
        private System.Windows.Forms.Button btnAlignCommit;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel stsGit;
        private System.Windows.Forms.ToolStripStatusLabel stsSep1;
        private System.Windows.Forms.ToolStripStatusLabel stsRepo;
        private System.Windows.Forms.ToolStripStatusLabel stsSep2;
        private System.Windows.Forms.ToolStripStatusLabel stsLast;
    }
}
