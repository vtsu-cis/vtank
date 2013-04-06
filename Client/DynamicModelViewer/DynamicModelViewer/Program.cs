using System;
using System.Windows.Forms;

namespace DynamicModelViewer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                using (Game1 game = new Game1())
                {
                    game.Run();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Crash: {0}", ex), "Crashed!", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

