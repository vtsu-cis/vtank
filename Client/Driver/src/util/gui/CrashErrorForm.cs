using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Client.src.service;
using Network.Main;

namespace GameForms.src.Forms
{
    /// <summary>
    /// This error form is used by the game to send the stack trace to developers 
    /// if a crash occurs,
    /// </summary>
    public partial class CrashErrorForm : Form
    {
        #region Fields
        private string stackTrace;
        private bool submitted = false;
        private bool cancelled = false;
        #endregion

        /// <summary>
        /// Constructor for the crash error form
        /// </summary>
        /// <param name="stackTrace">The stack trace to be sent to developers.</param>
        public CrashErrorForm(string stackTrace)
        {
            try
            {
                if (ServiceManager.Game.IsMouseVisible == false)
                    ServiceManager.Game.IsMouseVisible = true;
            }
            catch { }

            this.stackTrace = stackTrace;
            InitializeComponent();
        }

        /// <summary>
        /// Process the submission of the error.
        /// </summary>
        private void submitButton_Click(object sender, EventArgs e)
        {
            if (!submitted)
            {
                DoSubmit(false);
            }
        }

        private void DoSubmit(bool ignoreErrors)
        {
            string userInput = userInputBox.Text.Trim();

            try
            {
                ServiceManager.DestroyEchelonCommunicator();
                ServiceManager.ConnectToEchelon();
                ServiceManager.Echelon.SendErrorMessage(GameForms.Forms.LoginForm.username, userInput, stackTrace);
                ServiceManager.DestroyEchelonCommunicator();
            }
            catch (Exception ex)
            {
                if (!ignoreErrors)
                {
                    string errorMessage =
                        "An unexpected error occurred while trying to send a crash report.\n\nPlease try VTank later!";
                    MessageBox.Show(this, errorMessage, "Submission error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                submitted = true;
                this.Close();
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(this, "Are you sure you wish to cancel?\nThis will not submit a crash report.", 
                "Confirm cancellation.", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                cancelled = true;
                Close();
            }
        }

        private void CrashErrorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Visible = false;
            if (!submitted && !cancelled)
            {
                DoSubmit(true);
            }
        }
    }
}
