namespace VTankBotRunner.GUI
{
    partial class OptionsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsDialog));
            this.cancelButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.botAutoRemovePanel = new System.Windows.Forms.Panel();
            this.botAutoRemoveEnabled = new System.Windows.Forms.CheckBox();
            this.botAutoRemoveHelp = new System.Windows.Forms.LinkLabel();
            this.botAutoRemovePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(396, 376);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 0;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // applyButton
            // 
            this.applyButton.Enabled = false;
            this.applyButton.Location = new System.Drawing.Point(315, 376);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(75, 23);
            this.applyButton.TabIndex = 1;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.Enabled = false;
            this.saveButton.Location = new System.Drawing.Point(234, 376);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 2;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Bot Auto-Remove";
            // 
            // botAutoRemovePanel
            // 
            this.botAutoRemovePanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.botAutoRemovePanel.Controls.Add(this.botAutoRemoveEnabled);
            this.botAutoRemovePanel.Location = new System.Drawing.Point(12, 25);
            this.botAutoRemovePanel.Name = "botAutoRemovePanel";
            this.botAutoRemovePanel.Size = new System.Drawing.Size(106, 24);
            this.botAutoRemovePanel.TabIndex = 5;
            // 
            // botAutoRemoveEnabled
            // 
            this.botAutoRemoveEnabled.AutoSize = true;
            this.botAutoRemoveEnabled.Location = new System.Drawing.Point(22, 2);
            this.botAutoRemoveEnabled.Name = "botAutoRemoveEnabled";
            this.botAutoRemoveEnabled.Size = new System.Drawing.Size(59, 17);
            this.botAutoRemoveEnabled.TabIndex = 7;
            this.botAutoRemoveEnabled.Text = "Enable";
            this.botAutoRemoveEnabled.UseVisualStyleBackColor = true;
            this.botAutoRemoveEnabled.CheckedChanged += new System.EventHandler(this.botAutoRemoveEnabled_CheckedChanged);
            // 
            // botAutoRemoveHelp
            // 
            this.botAutoRemoveHelp.AutoSize = true;
            this.botAutoRemoveHelp.Location = new System.Drawing.Point(103, 8);
            this.botAutoRemoveHelp.Name = "botAutoRemoveHelp";
            this.botAutoRemoveHelp.Size = new System.Drawing.Size(19, 13);
            this.botAutoRemoveHelp.TabIndex = 6;
            this.botAutoRemoveHelp.TabStop = true;
            this.botAutoRemoveHelp.Text = "(?)";
            this.botAutoRemoveHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.botAutoRemoveHelp_LinkClicked);
            // 
            // OptionsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(483, 411);
            this.Controls.Add(this.botAutoRemoveHelp);
            this.Controls.Add(this.botAutoRemovePanel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.cancelButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OptionsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Options";
            this.Load += new System.EventHandler(this.OptionsDialog_Load);
            this.botAutoRemovePanel.ResumeLayout(false);
            this.botAutoRemovePanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel botAutoRemovePanel;
        private System.Windows.Forms.LinkLabel botAutoRemoveHelp;
        private System.Windows.Forms.CheckBox botAutoRemoveEnabled;
    }
}