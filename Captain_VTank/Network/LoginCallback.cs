using System;

namespace Captain_VTank.Network
{
    public class LoginCallback : Main.AMI_SessionFactory_AdminLogin
    {
        public delegate void LoginSuccessful(Admin.AdminSessionPrx adminProxy);
        public delegate void LoginFailed(string why);

        private LoginSuccessful successCallback;
        private LoginFailed failedCallback;

        public LoginCallback(LoginSuccessful successCallback, LoginFailed failedCallback)
        {
            this.successCallback = successCallback;
            this.failedCallback = failedCallback;
        }

        public override void ice_response(Admin.AdminSessionPrx ret__)
        {
            successCallback(ret__);
        }

        public override void ice_exception(Ice.Exception ex)
        {
            string errorMessage;
            if (ex is Exceptions.VTankException)
            {
                Exceptions.VTankException exception = (Exceptions.VTankException)ex;
                errorMessage = exception.reason;
            }
            else
            {
                errorMessage = "Connection error: " + ex.Message;
            }

            failedCallback(errorMessage);
        }
    }
}
