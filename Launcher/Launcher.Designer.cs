namespace Launcher
{
    partial class Launcher
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Launcher));
            this.playPanel = new System.Windows.Forms.Panel();
            this.optionsPanel = new System.Windows.Forms.Panel();
            this.aboutPanel = new System.Windows.Forms.Panel();
            this.websitePanel = new System.Windows.Forms.Panel();
            this.exitPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // playPanel
            // 
            this.playPanel.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.playPanel.BackgroundImage = global::Launcher.Properties.Resources.play;
            this.playPanel.Location = new System.Drawing.Point(463, 12);
            this.playPanel.Name = "playPanel";
            this.playPanel.Size = new System.Drawing.Size(160, 75);
            this.playPanel.TabIndex = 1;
            this.playPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.playPanel_MouseDown);
            // 
            // optionsPanel
            // 
            this.optionsPanel.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.optionsPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("optionsPanel.BackgroundImage")));
            this.optionsPanel.Location = new System.Drawing.Point(463, 96);
            this.optionsPanel.Name = "optionsPanel";
            this.optionsPanel.Size = new System.Drawing.Size(160, 75);
            this.optionsPanel.TabIndex = 2;
            this.optionsPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.optionsPanel_MouseDown);
            // 
            // aboutPanel
            // 
            this.aboutPanel.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.aboutPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("aboutPanel.BackgroundImage")));
            this.aboutPanel.Location = new System.Drawing.Point(464, 180);
            this.aboutPanel.Name = "aboutPanel";
            this.aboutPanel.Size = new System.Drawing.Size(160, 75);
            this.aboutPanel.TabIndex = 3;
            this.aboutPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.aboutPanel_MouseDown);
            // 
            // websitePanel
            // 
            this.websitePanel.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.websitePanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("websitePanel.BackgroundImage")));
            this.websitePanel.Location = new System.Drawing.Point(463, 264);
            this.websitePanel.Name = "websitePanel";
            this.websitePanel.Size = new System.Drawing.Size(160, 75);
            this.websitePanel.TabIndex = 4;
            this.websitePanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.websitePanel_MouseDown);
            // 
            // exitPanel
            // 
            this.exitPanel.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.exitPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("exitPanel.BackgroundImage")));
            this.exitPanel.Location = new System.Drawing.Point(463, 348);
            this.exitPanel.Name = "exitPanel";
            this.exitPanel.Size = new System.Drawing.Size(160, 75);
            this.exitPanel.TabIndex = 5;
            this.exitPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.exitPanel_MouseDown);
            // 
            // Launcher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(632, 446);
            this.Controls.Add(this.exitPanel);
            this.Controls.Add(this.websitePanel);
            this.Controls.Add(this.aboutPanel);
            this.Controls.Add(this.optionsPanel);
            this.Controls.Add(this.playPanel);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Launcher";
            this.Text = "VTank Launcher - ";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel playPanel;
        private System.Windows.Forms.Panel optionsPanel;
        private System.Windows.Forms.Panel aboutPanel;
        private System.Windows.Forms.Panel websitePanel;
        private System.Windows.Forms.Panel exitPanel;
    }
}

