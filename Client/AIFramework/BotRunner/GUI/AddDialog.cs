using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace VTankBotRunner.GUI
{
    public partial class AddDialog : Form
    {
        #region Members
        private MainWindow parent;
        #endregion

        #region Constructor
        public AddDialog(MainWindow parent)
        {
            this.parent = parent;
            InitializeComponent();

            typeComboBox.Items.AddRange(new object[] {
                "SampleBot.SampleBot",
            });

            if (typeComboBox.Items.Count > 0)
                typeComboBox.SelectedItem = "SampleBot.SampleBot";
        }
        #endregion

        #region Event Handlers
        private void addButton_Click(object sender, EventArgs e)
        {
            string typeName = typeComboBox.Text.Trim();
            string username = usernameBox.Text.Trim();
            string password = passwordBox.Text;

            if (String.IsNullOrEmpty(typeName) || String.IsNullOrEmpty(username) || 
                    String.IsNullOrEmpty(password))
            {
                MessageBox.Show(this, "All fields must be filled to completion.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Type type = Type.GetType(typeName);
            if (type == null)
            {
                type = Type.GetType("VTankBotRunner." + typeName);
                if (type == null)
                {
                    MessageBox.Show(this, "Unknown type: " + typeName, 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            try
            {
                parent.BotManager.AddBot(type, username, password);
                parent.InsertTextIntoConsole(String.Format("Added bot: {0}", username));
                Close();
            }
            catch (Exception)
            {
                MessageBox.Show(this, "Unable to login as \"" + username + "\".", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void AddDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            parent = null;
        }

        private void passwordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                addButton_Click(sender, e);
            }
        }
        #endregion

        
    }
}
