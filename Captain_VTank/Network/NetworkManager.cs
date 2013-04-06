using System;
using System.Collections.Generic;
using System.Threading;

namespace Captain_VTank.Network
{
    public static class NetworkManager
    {
        #region Constants
        private static readonly string CONFIG_FILE = "config.client";
        private static readonly VTankObject.Version version = new VTankObject.Version();

        private static Glacier2.RouterPrx router;
        private static Main.SessionFactoryPrx sessionFactoryPrx;
        private static Admin.AdminSessionPrx adminSessionPrx;
        private static LoginCallback.LoginSuccessful successCallback;
        private static LoginCallback.LoginFailed failureCallback;
        #endregion

        #region Members
        private static Ice.Communicator comm;
        private static bool keepAlive = false;
        private static Thread keepAliveThread = null;
        private static object kLock = new object();
        #endregion

        #region Properties
        public static bool KeepAlive
        {
            get
            {
                lock (kLock)
                {
                    return keepAlive;
                }
            }

            set
            {
                lock (kLock)
                {
                    keepAlive = value;
                    if (keepAlive)
                    {
                        if (keepAliveThread == null)
                        {
                            keepAliveThread = new Thread(new ThreadStart(DoKeepAlive));
                            keepAliveThread.Start();
                        }
                    }
                    else
                    {
                        if (keepAliveThread != null)
                        {
                            keepAliveThread.Interrupt();
                        }

                        keepAliveThread = null;
                    }
                }
            }
        }
        #endregion

        #region Methods
        private static void OnLoginSuccess(Admin.AdminSessionPrx proxy)
        {
            adminSessionPrx = proxy;
            KeepAlive = true;

            successCallback(proxy);
        }

        private static void OnLoginFailure(string reason)
        {
            Shutdown();
            failureCallback(reason);
        }

        public static void LoginAsync(string username, string password,
            LoginCallback.LoginSuccessful onSuccess, LoginCallback.LoginFailed onFailure)
        {
            successCallback = onSuccess;
            failureCallback = onFailure;

            string[] args = new string[] { "--Ice.Config=" + CONFIG_FILE };
            comm = Ice.Util.initialize(ref args);

            Ice.ObjectPrx routerPrx = comm.getDefaultRouter();
            router = Glacier2.RouterPrxHelper.checkedCast(routerPrx);
            if (router == null)
            {
                throw new InvalidOperationException(
                    "The target host does not use a Glacier2 router.");
            }

            Glacier2.SessionPrx sessionProxy = router.createSession("CaptainVTank", "");
            sessionFactoryPrx = Main.SessionFactoryPrxHelper.uncheckedCast(sessionProxy);
            sessionFactoryPrx.AdminLogin_async(new LoginCallback(OnLoginSuccess, OnLoginFailure),
                username, password, version); // TODO: Use real version.
        }

        public static void DoKeepAlive()
        {
            try
            {
                while (KeepAlive)
                {
                    Thread.Sleep(4500);
                    GetProxy().KeepAlive();
                }
            }
            catch (Ice.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("You have been disconnected:\n\n" + ex.Message,
                    "Disconnected!", System.Windows.Forms.MessageBoxButtons.OK, 
                    System.Windows.Forms.MessageBoxIcon.Error);

                Environment.Exit(1);
            }
            catch (System.Threading.ThreadInterruptedException) { }
        }

        public static Admin.AdminSessionPrx GetProxy()
        {
            return adminSessionPrx;
        }

        public static void Shutdown()
        {
            try
            {
                if (comm != null)
                {
                    comm.shutdown();
                }

                if (adminSessionPrx != null)
                {
                    adminSessionPrx.destroy();
                }

                if (router != null)
                {
                    router.destroySession();
                }
            }
            catch (Exception) { }
        }
        #endregion
    }
}
