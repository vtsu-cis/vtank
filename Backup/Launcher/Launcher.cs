using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using VTankOptionsSpace;
using System.Runtime.InteropServices;

namespace Launcher
{
    public partial class Launcher : Form
    {
        private static string versionString;

        public Launcher()
        {
            SuspendLayout();
            InitializeComponent();

            this.Text += String.Format("{0}", GetVersion());
            ResumeLayout();

            BringToFront();
        }

        /// <summary>
        /// Reads the version from the "version.ini" file. If it cannot be read, the version
        /// string is "&lt; unknown &gt;".
        /// 
        /// The value read from the file is cached, allowing for repeated calls.
        /// </summary>
        /// <returns>String containing the version of the VTank client.</returns>
        private string GetVersion()
        {
            if (versionString != null)
                return versionString;

            try
            {
                string text = File.ReadAllText("version.ini").Trim();
                if (text.Length == 0)
                    throw new IOException("Version file is empty!");

                versionString = text.Split('=')[1];
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                versionString = "??";
            }

            return versionString;
        }

        #region Event Handlers
        private void exitPanel_MouseDown(object sender, MouseEventArgs e)
        {
            this.Close();
        }

        private void websitePanel_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo("http://vtc.edu/vtank");

                Process.Start(info);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                MessageBox.Show(this, 
                    "I can't open your default browser.\n\nPlease visit:\nhttp://vtc.edu/vtank",
                    "Can't open browser!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void aboutPanel_MouseDown(object sender, MouseEventArgs e)
        {
            About about = new About();
            about.ShowDialog(this);
        }

        private void optionsPanel_MouseDown(object sender, MouseEventArgs e)
        {
            OptionsForm options = new OptionsForm();
            options.ShowDialog(this);
        }

        private void playPanel_MouseDown(object sender, MouseEventArgs e)
        {
            this.Visible = false;
            SplashWindow window = new SplashWindow();
            DialogResult result = window.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                this.Close();
            }
            else
            {
                SuspendLayout();
                this.Visible = true;
                this.BringToFront();
                ResumeLayout();
            }
        }
        #endregion
    }
}
