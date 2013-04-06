namespace VTankBotRunner.GUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.addButton = new System.Windows.Forms.Button();
            this.removeButton = new System.Windows.Forms.Button();
            this.onlineBotsLabel = new System.Windows.Forms.Label();
            this.menu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reservedBotsLabel = new System.Windows.Forms.Label();
            this.moveToReserveButton = new System.Windows.Forms.Button();
            this.moveToOnlineButton = new System.Windows.Forms.Button();
            this.onlineBotsList = new System.Windows.Forms.ListBox();
            this.reservedList = new System.Windows.Forms.ListBox();
            this.consoleArea = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.menu.SuspendLayout();
            this.SuspendLayout();
            // 
            // addButton
            // 
            this.addButton.Location = new System.Drawing.Point(12, 174);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(58, 23);
            this.addButton.TabIndex = 1;
            this.addButton.Text = "Add";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.addButton_Click);
            // 
            // removeButton
            // 
            this.removeButton.Enabled = false;
            this.removeButton.Location = new System.Drawing.Point(92, 174);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(66, 23);
            this.removeButton.TabIndex = 2;
            this.removeButton.Text = "Remove";
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
            // 
            // onlineBotsLabel
            // 
            this.onlineBotsLabel.AutoSize = true;
            this.onlineBotsLabel.Location = new System.Drawing.Point(9, 24);
            this.onlineBotsLabel.Name = "onlineBotsLabel";
            this.onlineBotsLabel.Size = new System.Drawing.Size(61, 13);
            this.onlineBotsLabel.TabIndex = 3;
            this.onlineBotsLabel.Text = "Online Bots";
            // 
            // menu
            // 
            this.menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menu.Location = new System.Drawing.Point(0, 0);
            this.menu.Name = "menu";
            this.menu.Size = new System.Drawing.Size(361, 24);
            this.menu.TabIndex = 4;
            this.menu.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.optionsToolStripMenuItem.Text = "Options...";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.aboutToolStripMenuItem.Text = "About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // reservedBotsLabel
            // 
            this.reservedBotsLabel.AutoSize = true;
            this.reservedBotsLabel.Location = new System.Drawing.Point(203, 24);
            this.reservedBotsLabel.Name = "reservedBotsLabel";
            this.reservedBotsLabel.Size = new System.Drawing.Size(86, 13);
            this.reservedBotsLabel.TabIndex = 6;
            this.reservedBotsLabel.Text = "Bots on Reserve";
            // 
            // moveToReserveButton
            // 
            this.moveToReserveButton.Enabled = false;
            this.moveToReserveButton.Location = new System.Drawing.Point(164, 73);
            this.moveToReserveButton.Name = "moveToReserveButton";
            this.moveToReserveButton.Size = new System.Drawing.Size(36, 26);
            this.moveToReserveButton.TabIndex = 7;
            this.moveToReserveButton.Text = ">>";
            this.moveToReserveButton.UseVisualStyleBackColor = true;
            this.moveToReserveButton.Click += new System.EventHandler(this.moveToReserveButton_Click);
            // 
            // moveToOnlineButton
            // 
            this.moveToOnlineButton.Enabled = false;
            this.moveToOnlineButton.Location = new System.Drawing.Point(164, 105);
            this.moveToOnlineButton.Name = "moveToOnlineButton";
            this.moveToOnlineButton.Size = new System.Drawing.Size(36, 26);
            this.moveToOnlineButton.TabIndex = 8;
            this.moveToOnlineButton.Text = "<<";
            this.moveToOnlineButton.UseVisualStyleBackColor = true;
            this.moveToOnlineButton.Click += new System.EventHandler(this.moveToOnlineButton_Click);
            // 
            // onlineBotsList
            // 
            this.onlineBotsList.FormattingEnabled = true;
            this.onlineBotsList.Location = new System.Drawing.Point(12, 44);
            this.onlineBotsList.Name = "onlineBotsList";
            this.onlineBotsList.Size = new System.Drawing.Size(146, 121);
            this.onlineBotsList.TabIndex = 9;
            this.onlineBotsList.SelectedIndexChanged += new System.EventHandler(this.onlineBotsList_SelectedIndexChanged);
            // 
            // reservedList
            // 
            this.reservedList.FormattingEnabled = true;
            this.reservedList.Location = new System.Drawing.Point(206, 44);
            this.reservedList.Name = "reservedList";
            this.reservedList.Size = new System.Drawing.Size(143, 121);
            this.reservedList.TabIndex = 10;
            this.reservedList.SelectedIndexChanged += new System.EventHandler(this.reservedList_SelectedIndexChanged);
            // 
            // consoleArea
            // 
            this.consoleArea.Location = new System.Drawing.Point(12, 234);
            this.consoleArea.Name = "consoleArea";
            this.consoleArea.ReadOnly = true;
            this.consoleArea.Size = new System.Drawing.Size(337, 92);
            this.consoleArea.TabIndex = 11;
            this.consoleArea.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 218);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Console";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(361, 338);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.consoleArea);
            this.Controls.Add(this.reservedList);
            this.Controls.Add(this.onlineBotsList);
            this.Controls.Add(this.moveToOnlineButton);
            this.Controls.Add(this.moveToReserveButton);
            this.Controls.Add(this.reservedBotsLabel);
            this.Controls.Add(this.onlineBotsLabel);
            this.Controls.Add(this.removeButton);
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.menu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menu;
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "VTank Bot Runner";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainWindow_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.menu.ResumeLayout(false);
            this.menu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.Label onlineBotsLabel;
        private System.Windows.Forms.MenuStrip menu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.Label reservedBotsLabel;
        private System.Windows.Forms.Button moveToReserveButton;
        private System.Windows.Forms.Button moveToOnlineButton;
        private System.Windows.Forms.ListBox onlineBotsList;
        private System.Windows.Forms.ListBox reservedList;
        private System.Windows.Forms.RichTextBox consoleArea;
        private System.Windows.Forms.Label label1;
    }
}