using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using VTankBotRunner.Util;

namespace VTankBotRunner.GUI
{
    public partial class OptionsDialog : Form
    {
        public OptionsDialog()
        {
            InitializeComponent();
        }

        private void RegisterChange()
        {
            saveButton.Enabled = true;
            applyButton.Enabled = true;
        }

        #region Event Handlers
        private void OptionsDialog_Load(object sender, EventArgs e)
        {
            // Fill default values.
            botAutoRemoveEnabled.Checked = BotRunnerOptions.GetValueBool("AutoBotRemoveEnabled");

            saveButton.Enabled = false;
            applyButton.Enabled = false;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            BotRunnerOptions.ReadOptions();
            Close();
        }

        private void botAutoRemoveHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show(this,
                "Bot Auto-Remove:\nConfigure whether bots should be automatically " +
                "removed as more players join the game\n\nThe bots are re-added after players leave.",
                "Bot Auto-Remove Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            BotRunnerOptions.SaveOptions();
            Close();
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            BotRunnerOptions.SaveOptions();
            applyButton.Enabled = false;
        }

        private void botAutoRemoveEnabled_CheckedChanged(object sender, EventArgs e)
        {
            BotRunnerOptions.SetValue("AutoBotRemoveEnabled", botAutoRemoveEnabled.Checked);
            RegisterChange();
        }
        #endregion
    }
}
