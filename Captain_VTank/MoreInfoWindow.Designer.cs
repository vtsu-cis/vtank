namespace Captain_VTank
{
    partial class MoreInfoWindow
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
            this.label1 = new System.Windows.Forms.Label();
            this.usernameBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.creationDateBox = new System.Windows.Forms.TextBox();
            this.creationDateLabel = new System.Windows.Forms.Label();
            this.lastLoginLabel = new System.Windows.Forms.Label();
            this.lastLoginBox = new System.Windows.Forms.TextBox();
            this.roleComboBox = new System.Windows.Forms.ComboBox();
            this.privilegeLabel = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.clientLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(156, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Detailed information about user:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // usernameBox
            // 
            this.usernameBox.Enabled = false;
            this.usernameBox.Location = new System.Drawing.Point(91, 32);
            this.usernameBox.Name = "usernameBox";
            this.usernameBox.Size = new System.Drawing.Size(117, 20);
            this.usernameBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Username:";
            // 
            // creationDateBox
            // 
            this.creationDateBox.Enabled = false;
            this.creationDateBox.Location = new System.Drawing.Point(91, 58);
            this.creationDateBox.Name = "creationDateBox";
            this.creationDateBox.Size = new System.Drawing.Size(117, 20);
            this.creationDateBox.TabIndex = 3;
            // 
            // creationDateLabel
            // 
            this.creationDateLabel.AutoSize = true;
            this.creationDateLabel.Location = new System.Drawing.Point(12, 61);
            this.creationDateLabel.Name = "creationDateLabel";
            this.creationDateLabel.Size = new System.Drawing.Size(73, 13);
            this.creationDateLabel.TabIndex = 4;
            this.creationDateLabel.Text = "Date Created:";
            // 
            // lastLoginLabel
            // 
            this.lastLoginLabel.AutoSize = true;
            this.lastLoginLabel.Location = new System.Drawing.Point(12, 87);
            this.lastLoginLabel.Name = "lastLoginLabel";
            this.lastLoginLabel.Size = new System.Drawing.Size(59, 13);
            this.lastLoginLabel.TabIndex = 5;
            this.lastLoginLabel.Text = "Last Login:";
            // 
            // lastLoginBox
            // 
            this.lastLoginBox.Location = new System.Drawing.Point(91, 84);
            this.lastLoginBox.Name = "lastLoginBox";
            this.lastLoginBox.Size = new System.Drawing.Size(117, 20);
            this.lastLoginBox.TabIndex = 6;
            // 
            // roleComboBox
            // 
            this.roleComboBox.FormattingEnabled = true;
            this.roleComboBox.Location = new System.Drawing.Point(91, 110);
            this.roleComboBox.Name = "roleComboBox";
            this.roleComboBox.Size = new System.Drawing.Size(117, 21);
            this.roleComboBox.TabIndex = 7;
            // 
            // privilegeLabel
            // 
            this.privilegeLabel.AutoSize = true;
            this.privilegeLabel.Location = new System.Drawing.Point(12, 113);
            this.privilegeLabel.Name = "privilegeLabel";
            this.privilegeLabel.Size = new System.Drawing.Size(50, 13);
            this.privilegeLabel.TabIndex = 8;
            this.privilegeLabel.Text = "Privilege:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(91, 137);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(117, 20);
            this.textBox1.TabIndex = 9;
            // 
            // clientLabel
            // 
            this.clientLabel.Location = new System.Drawing.Point(12, 140);
            this.clientLabel.Name = "clientLabel";
            this.clientLabel.Size = new System.Drawing.Size(73, 17);
            this.clientLabel.TabIndex = 10;
            this.clientLabel.Text = "Client:";
            // 
            // MoreInfoWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(425, 309);
            this.Controls.Add(this.clientLabel);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.privilegeLabel);
            this.Controls.Add(this.roleComboBox);
            this.Controls.Add(this.lastLoginBox);
            this.Controls.Add(this.lastLoginLabel);
            this.Controls.Add(this.creationDateLabel);
            this.Controls.Add(this.creationDateBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.usernameBox);
            this.Controls.Add(this.label1);
            this.Name = "MoreInfoWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " - More Information";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox usernameBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox creationDateBox;
        private System.Windows.Forms.Label creationDateLabel;
        private System.Windows.Forms.Label lastLoginLabel;
        private System.Windows.Forms.TextBox lastLoginBox;
        private System.Windows.Forms.ComboBox roleComboBox;
        private System.Windows.Forms.Label privilegeLabel;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label clientLabel;
    }
}