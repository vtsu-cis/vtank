/*!
    \file   Main.cs
    \brief  Entry point for the program.
    \author (C) Copyright 2009 by Vermont Technical College
*/
using System;
using Client.src.service;
using System.Windows.Forms;

namespace Client
{
    public static class Program
    {
        /// <summary>
        /// Show an error message box.
        /// </summary>
        /// <param name="text">Text to show.</param>
        /// <param name="title">Title of the message box.</param>
        static void ShowErrorMessageBox(string text, string title)
        {
            System.Windows.Forms.MessageBox.Show(text, title,
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Error,
                System.Windows.Forms.MessageBoxDefaultButton.Button1);
        }

        /// <summary>
        /// The entry point for the application.
        /// </summary>
        /// <param name="args">Command-line arguments. Unused.</param>
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Only include the try/catch block on non-debug builds.
            // We want unhandled exceptions to escalate to the debugger.
#if !DEBUG
            try
            {
#endif
                // It will block here:
                ServiceManager.StartGame();                
#if !DEBUG
            }
            catch (Microsoft.Xna.Framework.NoSuitableGraphicsDeviceException e)
            {
                ShowErrorMessageBox(
                    "Unable to find suitable graphics device.\nDetails: " + e.Message,
                    "No suitable graphics device.");
                Console.Error.WriteLine(e);
            }
            catch (Ice.Exception e)
            {
                ShowErrorMessageBox(
                    "An uncaught connection-related exception occurred.\n" +
                    "Please report this error and try again.\n" +
                    "Details: " + e.ToString(),
                    "Connection lost.");
                Console.Error.WriteLine(e);
            }
            catch (Exception e)
            {
                System.Text.StringBuilder buffer = new System.Text.StringBuilder();
                buffer.AppendLine(e.StackTrace.ToString());
                buffer.Append("[Trace Message]: ");
                buffer.AppendLine(e.Message);

                Exception next = e;
                while (next.InnerException != null)
                {
                    next = next.InnerException;
                    buffer.AppendLine("... Thrown by inner exception:");
                    if (next.StackTrace != null)
                        buffer.Append(next.StackTrace.ToString());
                    buffer.Append("\n[Inner Trace Message]: ");
                    buffer.AppendLine(next.Message);
                }

                Application.Run(new GameForms.src.Forms.CrashErrorForm(buffer.ToString()));
            }
            finally 
            {
#endif
                ServiceManager.StopAllServices();
#if !DEBUG
            }

            Environment.Exit(1);
#else
            Environment.Exit(0);
#endif
        }
    }
}

