using System;
using System.Windows.Forms;
using Captain_VTank.Service;

namespace Captain_VTank
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(Services.LoginWindow = new LoginWindow());
        }
    }
}
