using System;
using System.Collections.Generic;
using System.Text;
using AIFramework.Runner;
using System.IO;
using AIFramework.Util;
using System.Windows.Forms;
using VTankBotRunner.GUI;

namespace VTankBotRunner
{
    class Program
    {
        public const string DefaultConfigFile = "Bots.cfg";

        #region Main
        /// <summary>
        /// Entry point of the program executed when no external runner exists.
        /// </summary>
        /// <param name="args">Command-line arguments.
        /// The following arguments are applicable:
        ///     -c [filename]
        ///     --config-file filename
        ///         Allows user to specify a custom configuration file.
        ///     
        ///     -ng
        ///     --nogui
        ///     nogui
        ///         Allows the user to run the bots from the console without any GUI interaction.
        /// </param>
        [STAThread()]
        public static void Main(string[] args)
        {
            bool loadGUI = true;
            string configFile = DefaultConfigFile;
            if (args.Length > 1)
            {
                // Command-line arguments exist.
                for (int i = 0; i < args.Length; ++i)
                {
                    if (args[i] == "-c" || args[i] == "--config-file")
                    {
                        // Argument: custom configuration file.
                        if (args.Length >= i + 1)
                        {
                            PrintUsage("-c", "filename");
                        }

                        configFile = args[i + 1];
                        ++i;
                    }

                    else if (args[i] == "-ng" || args[i] == "--nogui" || args[i] == "nogui")
                    {
                        // Argument: Hide the GUI.
                        loadGUI = false;
                    }
                }
            }

            
            try
            {
                WeaponLoader.LoadFiles();
                BotRunner runner = new BotRunner();

                if (loadGUI)
                {
                    try
                    {
                        Application.Run(new MainWindow(configFile, runner));
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Error: " + e.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(1);
                    }
                }
                else
                {
                    runner.Parse(configFile);
                    runner.Start(true);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error!");
                Console.Error.WriteLine(e);
                Console.Error.WriteLine(e.StackTrace);

                throw;
            }
        }

        /// <summary>
        /// Utility function for printing command-line argument usage and exiting
        /// the program.
        /// </summary>
        /// <param name="command">Command for the usage.</param>
        /// <param name="args">Arguments for the command.</param>
        public static void PrintUsage(string command, params object[] args)
        {
            Console.Error.WriteLine("Usage: {0} {1}", command, args);
            Environment.Exit(1);
        }
        #endregion
    }
}
