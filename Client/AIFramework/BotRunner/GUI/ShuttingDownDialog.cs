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
    public partial class ShuttingDownDialog : Form
    {
        private Thread thread;
        private System.Windows.Forms.Timer timer;

        public ShuttingDownDialog(Thread shutdownThread)
        {
            thread = shutdownThread;

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
                Close();
                Environment.Exit(0);
            }
        }

        private void forceQuitButton_Click(object sender, EventArgs e)
        {
            try
            {
                thread.Abort();
            }
            catch { }

            Environment.Exit(1);
        }
    }
}
