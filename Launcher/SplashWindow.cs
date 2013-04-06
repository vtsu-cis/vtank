using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

namespace Launcher
{
    public partial class SplashWindow : Form
    {
        private Thread thread;
        private string failureReason;
        private ThreadState threadState;
        private delegate void Finished();

        private enum ThreadState
        {
            NOT_FINISHED,
            SUCCESSFUL,
            FAILED
        }

        private ThreadState State
        {
            get
            {
                lock (this)
                {
                    return threadState;
                }
            }

            set
            {
                lock (this)
                {
                    threadState = value;
                }
            }
        }

        public SplashWindow()
        {
            InitializeComponent();
            this.DialogResult = DialogResult.OK;
            State = ThreadState.NOT_FINISHED;
        }

        private void SplashWindow_Load(object sender, EventArgs e)
        {
            thread = new Thread(new ThreadStart(LaunchProcess));
            thread.Start();
        }

        private void OnFinish()
        {
            if (State == ThreadState.FAILED)
            {
                MessageBox.Show(this,
                    String.Format(
                        "I can't open 'Client.exe'.\n\nPlease contact support at the VTank website:\n" +
                        "http://vtc.edu/vtank\n\n" +
                        "Details: {0}.", failureReason),
                    "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Abort;
            }
            else
            {
                DialogResult = DialogResult.OK;
            }

            this.Close();
        }


        #region Get Active Window Methods
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            IntPtr handle = IntPtr.Zero;
            StringBuilder Buff = new StringBuilder(nChars);
            handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }
        #endregion


        private void LaunchProcess()
        {
            try
            {
                const string clientExePath = "Client.exe";
                ProcessStartInfo info = new ProcessStartInfo(clientExePath);
                info.UseShellExecute = false;
                //info.WorkingDirectory = @"C:\Program Files\VTank\";

                Process process = Process.Start(info);

                //bool finished = false;
                long elapsed = 0;
                long lastTime = (long)(DateTime.Now - DateTime.FromBinary(0)).TotalMilliseconds;;
                const long MAX_WAIT_TIME = 15000;
                while (GetActiveWindowTitle() != "VTank") //&& !finished) Might want this at some point
                {
                    try
                    {
                        /*int moduleCount = process.Modules.Count;
                        if (!(moduleCount >= 4 && process.Responding))
                        {*/
                            long currentTime = (long)(DateTime.Now - DateTime.FromBinary(0)).TotalMilliseconds;
                            elapsed += currentTime - lastTime;
                            lastTime = currentTime;
                            if (elapsed > MAX_WAIT_TIME)
                                break;
                        /*}
                        else
                        {
                            finished = true;
                        }*/
                    }
                    catch { }
                    finally
                    {
                        Thread.Sleep(25);
                    }
                }
                State = ThreadState.SUCCESSFUL;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);

                failureReason = ex.Message;

                State = ThreadState.FAILED;
            }

            Invoke(new Finished(OnFinish));
        }
    }
}
