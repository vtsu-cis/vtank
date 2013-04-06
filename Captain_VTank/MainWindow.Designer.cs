namespace Captain_VTank
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.usersTab = new System.Windows.Forms.TabPage();
            this.refreshButton = new System.Windows.Forms.Button();
            this.accountsHeaderLabel = new System.Windows.Forms.Label();
            this.accountsTable = new System.Windows.Forms.DataGridView();
            this.accountName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.isPlayingGame = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.role = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.client = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statsTab = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.recentUsersTable = new System.Windows.Forms.DataGridView();
            this.account = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lastLoginDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.accountCreationDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.topPlayersTable = new System.Windows.Forms.DataGridView();
            this.topAccount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.topPoints = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.topBestTank = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label2 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.topPlayersRefresh = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.healthLabel = new System.Windows.Forms.Label();
            this.healthTableView = new System.Windows.Forms.DataGridView();
            this.serverID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cpuPercent = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.hdPercent = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ramPercent = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabControl1.SuspendLayout();
            this.usersTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.accountsTable)).BeginInit();
            this.statsTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.recentUsersTable)).BeginInit();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.topPlayersTable)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.healthTableView)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.usersTab);
            this.tabControl1.Controls.Add(this.statsTab);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 27);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(532, 329);
            this.tabControl1.TabIndex = 0;
            // 
            // usersTab
            // 
            this.usersTab.Controls.Add(this.refreshButton);
            this.usersTab.Controls.Add(this.accountsHeaderLabel);
            this.usersTab.Controls.Add(this.accountsTable);
            this.usersTab.Location = new System.Drawing.Point(4, 22);
            this.usersTab.Name = "usersTab";
            this.usersTab.Padding = new System.Windows.Forms.Padding(3);
            this.usersTab.Size = new System.Drawing.Size(524, 303);
            this.usersTab.TabIndex = 0;
            this.usersTab.Text = "Accounts";
            this.usersTab.UseVisualStyleBackColor = true;
            // 
            // refreshButton
            // 
            this.refreshButton.Location = new System.Drawing.Point(6, 274);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(75, 23);
            this.refreshButton.TabIndex = 2;
            this.refreshButton.Text = "Refresh";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // accountsHeaderLabel
            // 
            this.accountsHeaderLabel.AutoSize = true;
            this.accountsHeaderLabel.Location = new System.Drawing.Point(6, 3);
            this.accountsHeaderLabel.Name = "accountsHeaderLabel";
            this.accountsHeaderLabel.Size = new System.Drawing.Size(77, 13);
            this.accountsHeaderLabel.TabIndex = 1;
            this.accountsHeaderLabel.Text = "User Accounts";
            // 
            // accountsTable
            // 
            this.accountsTable.AllowUserToAddRows = false;
            this.accountsTable.AllowUserToDeleteRows = false;
            this.accountsTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.accountsTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.accountName,
            this.isPlayingGame,
            this.role,
            this.client});
            this.accountsTable.Location = new System.Drawing.Point(6, 19);
            this.accountsTable.Name = "accountsTable";
            this.accountsTable.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.accountsTable.ShowEditingIcon = false;
            this.accountsTable.Size = new System.Drawing.Size(512, 249);
            this.accountsTable.TabIndex = 0;
            // 
            // accountName
            // 
            this.accountName.HeaderText = "Name";
            this.accountName.Name = "accountName";
            this.accountName.ReadOnly = true;
            this.accountName.Width = 125;
            // 
            // isPlayingGame
            // 
            this.isPlayingGame.HeaderText = "In-Game?";
            this.isPlayingGame.Name = "isPlayingGame";
            this.isPlayingGame.ReadOnly = true;
            this.isPlayingGame.Width = 80;
            // 
            // role
            // 
            this.role.HeaderText = "Role";
            this.role.Items.AddRange(new object[] {
            "Banned",
            "Suspended",
            "Troublemaker",
            "Member",
            "Contributor",
            "Tester",
            "Developer",
            "Moderator",
            "Administrator"});
            this.role.Name = "role";
            this.role.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.role.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.role.Width = 123;
            // 
            // client
            // 
            this.client.HeaderText = "Client";
            this.client.Name = "client";
            this.client.ReadOnly = true;
            this.client.Width = 130;
            // 
            // statsTab
            // 
            this.statsTab.Controls.Add(this.label1);
            this.statsTab.Controls.Add(this.recentUsersTable);
            this.statsTab.Location = new System.Drawing.Point(4, 22);
            this.statsTab.Name = "statsTab";
            this.statsTab.Padding = new System.Windows.Forms.Padding(3);
            this.statsTab.Size = new System.Drawing.Size(524, 303);
            this.statsTab.TabIndex = 1;
            this.statsTab.Text = "Recent Users";
            this.statsTab.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(271, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "List of users who have logged in recently (past 30 days):";
            // 
            // recentUsersTable
            // 
            this.recentUsersTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.recentUsersTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.account,
            this.lastLoginDate,
            this.accountCreationDate});
            this.recentUsersTable.Location = new System.Drawing.Point(6, 19);
            this.recentUsersTable.Name = "recentUsersTable";
            this.recentUsersTable.Size = new System.Drawing.Size(512, 278);
            this.recentUsersTable.TabIndex = 0;
            // 
            // account
            // 
            this.account.HeaderText = "Account";
            this.account.Name = "account";
            this.account.ReadOnly = true;
            // 
            // lastLoginDate
            // 
            this.lastLoginDate.HeaderText = "Last Login Date";
            this.lastLoginDate.Name = "lastLoginDate";
            this.lastLoginDate.ReadOnly = true;
            this.lastLoginDate.Width = 175;
            // 
            // accountCreationDate
            // 
            this.accountCreationDate.HeaderText = "Account Creation Date";
            this.accountCreationDate.Name = "accountCreationDate";
            this.accountCreationDate.ReadOnly = true;
            this.accountCreationDate.Width = 175;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.topPlayersRefresh);
            this.tabPage1.Controls.Add(this.topPlayersTable);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(524, 303);
            this.tabPage1.TabIndex = 2;
            this.tabPage1.Text = "Top Players";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // topPlayersTable
            // 
            this.topPlayersTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.topPlayersTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.topAccount,
            this.topPoints,
            this.topBestTank});
            this.topPlayersTable.Location = new System.Drawing.Point(6, 19);
            this.topPlayersTable.Name = "topPlayersTable";
            this.topPlayersTable.Size = new System.Drawing.Size(512, 255);
            this.topPlayersTable.TabIndex = 1;
            // 
            // topAccount
            // 
            this.topAccount.HeaderText = "Account";
            this.topAccount.Name = "topAccount";
            this.topAccount.ReadOnly = true;
            this.topAccount.Width = 140;
            // 
            // topPoints
            // 
            this.topPoints.HeaderText = "Points";
            this.topPoints.Name = "topPoints";
            this.topPoints.ReadOnly = true;
            this.topPoints.Width = 140;
            // 
            // topBestTank
            // 
            this.topBestTank.HeaderText = "Most Played Tank";
            this.topBestTank.Name = "topBestTank";
            this.topBestTank.ReadOnly = true;
            this.topBestTank.Width = 140;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(111, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Top players in VTank:";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.healthTableView);
            this.tabPage2.Controls.Add(this.healthLabel);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(524, 303);
            this.tabPage2.TabIndex = 3;
            this.tabPage2.Text = "Server Health";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(556, 24);
            this.menuStrip.TabIndex = 1;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // topPlayersRefresh
            // 
            this.topPlayersRefresh.Location = new System.Drawing.Point(3, 277);
            this.topPlayersRefresh.Name = "topPlayersRefresh";
            this.topPlayersRefresh.Size = new System.Drawing.Size(75, 23);
            this.topPlayersRefresh.TabIndex = 2;
            this.topPlayersRefresh.Text = "Refresh";
            this.topPlayersRefresh.UseVisualStyleBackColor = true;
            this.topPlayersRefresh.Click += new System.EventHandler(this.topPlayersRefresh_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(127, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Health of VTank Servers:";
            // 
            // healthLabel
            // 
            this.healthLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.healthLabel.AutoSize = true;
            this.healthLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.healthLabel.ForeColor = System.Drawing.Color.Red;
            this.healthLabel.Location = new System.Drawing.Point(180, 3);
            this.healthLabel.Name = "healthLabel";
            this.healthLabel.Size = new System.Drawing.Size(172, 13);
            this.healthLabel.TabIndex = 1;
            this.healthLabel.Text = "Please wait -- loading health data...";
            // 
            // healthTableView
            // 
            this.healthTableView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.healthTableView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.serverID,
            this.cpuPercent,
            this.hdPercent,
            this.ramPercent});
            this.healthTableView.Location = new System.Drawing.Point(6, 19);
            this.healthTableView.Name = "healthTableView";
            this.healthTableView.Size = new System.Drawing.Size(512, 278);
            this.healthTableView.TabIndex = 2;
            // 
            // serverID
            // 
            this.serverID.HeaderText = "Server ID";
            this.serverID.Name = "serverID";
            this.serverID.ReadOnly = true;
            // 
            // cpuPercent
            // 
            this.cpuPercent.HeaderText = "CPU%";
            this.cpuPercent.Name = "cpuPercent";
            this.cpuPercent.ReadOnly = true;
            // 
            // hdPercent
            // 
            this.hdPercent.HeaderText = "HD%";
            this.hdPercent.Name = "hdPercent";
            this.hdPercent.ReadOnly = true;
            // 
            // ramPercent
            // 
            this.ramPercent.HeaderText = "RAM%";
            this.ramPercent.Name = "ramPercent";
            this.ramPercent.ReadOnly = true;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(556, 368);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Captain VTank - Main View";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.tabControl1.ResumeLayout(false);
            this.usersTab.ResumeLayout(false);
            this.usersTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.accountsTable)).EndInit();
            this.statsTab.ResumeLayout(false);
            this.statsTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.recentUsersTable)).EndInit();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.topPlayersTable)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.healthTableView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage usersTab;
        private System.Windows.Forms.TabPage statsTab;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.Label accountsHeaderLabel;
        private System.Windows.Forms.DataGridView accountsTable;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn accountName;
        private System.Windows.Forms.DataGridViewTextBoxColumn isPlayingGame;
        private System.Windows.Forms.DataGridViewComboBoxColumn role;
        private System.Windows.Forms.DataGridViewTextBoxColumn client;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView recentUsersTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn account;
        private System.Windows.Forms.DataGridViewTextBoxColumn lastLoginDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn accountCreationDate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView topPlayersTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn topAccount;
        private System.Windows.Forms.DataGridViewTextBoxColumn topPoints;
        private System.Windows.Forms.DataGridViewTextBoxColumn topBestTank;
        private System.Windows.Forms.Button topPlayersRefresh;
        private System.Windows.Forms.Label healthLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridView healthTableView;
        private System.Windows.Forms.DataGridViewTextBoxColumn serverID;
        private System.Windows.Forms.DataGridViewTextBoxColumn cpuPercent;
        private System.Windows.Forms.DataGridViewTextBoxColumn hdPercent;
        private System.Windows.Forms.DataGridViewTextBoxColumn ramPercent;
    }
}