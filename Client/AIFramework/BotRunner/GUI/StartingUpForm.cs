using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace VTankBotRunner.GUI
{
    public partial class StartingUpForm : Form
    {
        public delegate void StartupFinishedCallback();

        public event StartupFinishedCallback StartupFinished;

        private Thread thread;
        private System.Windows.Forms.Timer timer;

        public StartingUpForm(Thread startupThread)
        {
            thread = startupThread;

            InitializeComponent();

            timer = new System.Windows.Forms.Timer()
            {
                Interval = 100,
            };

            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (!thread.IsAlive)
            {
                Visible = false;
                timer.Enabled = false;
                timer.Dispose();
                Close();

                if (StartupFinished != null)
                    StartupFinished();
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
            Environment.Exit(0);
        }
    }
}
