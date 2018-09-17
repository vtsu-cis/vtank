using System;
using System.Collections.Generic;
using System.Text;

namespace AIFramework.Util
{
    internal static class Debugger
    {
        #region Static Members
        private static Network.Util.Logger logger = null;
        #endregion

        #region Static Methods
        /// <summary>
        /// Write a message to the debugger.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <param name="args">Formatting arguments for the string.</param>
        public static void Write(string message, params object[] args)
        {
            CreateLoggerIfNeeded();

            string info = String.Format(message, args);
            Console.WriteLine(info);
            logger.Info(info);
        }

        /// <summary>
        /// Write an error message to the debugger.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <param name="args">Formatting arguments for the string.</param>
        public static void Error(string message, params object[] args)
        {
            CreateLoggerIfNeeded();

            string error = String.Format(message, args);
            Console.Error.WriteLine(error);
            logger.Error(error);
        }

        /// <summary>
        /// Create a logger if it hasn't been created yet.
        /// </summary>
        private static void CreateLoggerIfNeeded()
        {
            if (logger == null)
            {
                logger = new Network.Util.Logger("AIFramework.log");
            }
        }
        #endregion
    }
}
