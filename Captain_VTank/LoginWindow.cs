using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Captain_VTank.Network;
using Captain_VTank.Service;

namespace Captain_VTank
{
    public partial class LoginWindow : Form
    {
        #region Members
        private delegate void UpdateInfoLabelDelegate(string info);
        private delegate void ChangeStateDelegate();
        #endregion

        #region Constructor
        public LoginWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Attempt to login.
        /// </summary>
        private void Login()
        {
            string username = usernameField.Text;
            string password = passwordField.Text;

            string errorMessage = null;
            if (!CredentialsAreValid(username, password, out errorMessage))
            {
                MessageBox.Show(this, errorMessage, "Cannot login.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                ChangeInfoLabel("Logging in...");
                NetworkManager.LoginAsync(username, password, OnLoginSuccess, OnLoginFailed);
            }
        }

        private bool CredentialsAreValid(string username, string password, out string errorMessage)
        {
            if (String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password))
            {
                errorMessage = "Username and password fields cannot be empty.";
                return false;
            }

            Regex regex = new Regex("[^A-Za-z0-9]");
            if (regex.IsMatch(username))
            {
                errorMessage = "Usernames can only contain letters and numbers.";
                return false;
            }

            errorMessage = "";
            return true;
        }

        private void OnLoginSuccess(Admin.AdminSessionPrx prx)
        {
            Invoke(new ChangeStateDelegate(ChangeToMainWindow));
        }

        private void OnLoginFailed(string reason)
        {
            Invoke(new UpdateInfoLabelDelegate(ChangeInfoLabel), reason);
        }

        private void ChangeInfoLabel(string newText)
        {
            infoLabel.Text = newText;
        }

        private void ChangeToMainWindow()
        {
            this.Visible = false;

            Services.MainWindow = new MainWindow();
            Services.MainWindow.Show();

            Hide();
        }
        #endregion

        #region Event Handlers
        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            Login();
        }

        private void passwordField_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                Login();
            }
        }
        #endregion
    }
}
