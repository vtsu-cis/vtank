using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Captain_VTank.Service
{
    public static class Services
    {
        private static bool isRunning = true;
        private static object locker = new object();

        public static bool IsRunning
        {
            get
            {
                lock (locker)
                {
                    return isRunning;
                }
            }

            private set
            {
                lock (locker)
                {
                    isRunning = value;
                }
            }
        }

        public static LoginWindow LoginWindow = null;
        public static MainWindow MainWindow = null;

        public static void Shutdown()
        {
            IsRunning = false;

            if (MainWindow != null)
            {
                MainWindow.Visible = false;
            }

            if (LoginWindow != null)
            {
                LoginWindow.Close();
            }

            Network.NetworkManager.Shutdown();

            Environment.Exit(0);
        }
    }
}
