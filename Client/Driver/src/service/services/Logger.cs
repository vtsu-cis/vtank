using System;
using System.IO;

namespace Client.src.service.services
{
    /// <summary>
    /// The logger makes it easier to write data to a log file quickly.
    /// </summary>
    public class Logger
    {
        #region Members
        private StreamWriter file;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialize the logger using the default FileMode.Append mode for opening
        /// the log file.
        /// </summary>
        /// <param name="_filename">File to write to.</param>
        public Logger(string _filename) 
            : this(_filename, FileMode.Append)
        {
        }
        
        /// <summary>
        /// Initialize the logger.
        /// </summary>
        /// <param name="_filename">File to write to.</param>
        /// <param name="_fileMode">Mode to open the file as.</param>
        public Logger(string _filename, FileMode _fileMode)
        {
            try
            {
                file = new StreamWriter(new FileStream(_filename, _fileMode));
            }
            catch (System.Exception)
            {
                Console.WriteLine("Warning: Cannot create or access" + _filename + " log file");
            }
        }
        #endregion

        #region Destructor
        /// <summary>
        /// Closes the file pointer.
        /// </summary>
        ~Logger()
        {
            Close();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Write an error to file.
        /// </summary>
        /// <param name="message">Message to write.</param>
        public void Error(string message)
        {
            try
            {
                file.WriteLine(String.Format("ERROR  : {0}", message.Replace("\n", "\n\t")));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[ERROR] Cannot log message to log file: {0}", ex);
                Console.Error.WriteLine("        Intended to log: {0}", message);
            }
        }

        /// <summary>
        /// Write an exception to the file.
        /// </summary>
        /// <param name="ex"></param>
        public void Exception(Exception ex)
        {
            System.Text.StringBuilder buffer = new System.Text.StringBuilder();
            buffer.Append(ex.StackTrace.ToString().Replace("\n", "\t\n"));
            buffer.AppendLine("\t" + ex.Message);

            Exception next;
            while (ex.InnerException != null)
            {
                next = ex.InnerException;
                buffer.AppendLine("\t...");
                buffer.Append("\t" + ex.StackTrace.ToString().Replace("\n", "\t\n"));
                buffer.AppendLine("\t" + ex.Message);
            }

            string message = buffer.ToString();
            try
            {
                file.Write(String.Format("EXCEPTION: {0}", message));
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("[ERROR] Cannot log message to log file: {0}", exception);
                Console.Error.WriteLine("        Intended to log: {0}", message);
            }
        }

        /// <summary>
        /// Write an error to file.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <param name="arg">Arguments, if any.</param>
        public void Error(string message, params object[] arg)
        {
            Error(String.Format(message, arg));
        }

        /// <summary>
        /// Write a standard event to file.
        /// </summary>
        /// <param name="message">Message to write.</param>
        public void Info(string message)
        {
            try
            {
                file.WriteLine(String.Format("INFO   : {0}", message.Replace("\n", "\n\t")));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[ERROR] Cannot log message to log file: {0}", ex);
                Console.Error.WriteLine("        Intended to log: {0}", message);
            }
        }

        /// <summary>
        /// Write a standard event to file.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <param name="arg">Formatting arugments.</param>
        public void Info(string message, params object[] arg)
        {
            Info(String.Format(message, arg));
        }

        /// <summary>
        /// Write a warning event to file.
        /// </summary>
        /// <param name="message">Message to write.</param>
        public void Warning(string message)
        {
            try
            {
                file.WriteLine(String.Format("WARNING: {0}", message.Replace("\n", "\n\t")));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[ERROR] Cannot log message to log file: {0}", ex);
                Console.Error.WriteLine("        Intended to log: {0}", message);
            }
        }

        /// <summary>
        /// Write a warning event to file.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <param name="arg">Formatting arguments.</param>
        public void Warning(string message, params object[] arg)
        {
            Warning(String.Format(message, arg));
        }

        /// <summary>
        /// Explicitly close the file pointer. Note that the file is automatically
        /// closed at the Logger's termination, and that further attempts to write
        /// data to the log will fail.
        /// </summary>
        public void Close()
        {
            try
            {
                file.Flush();
                file.Close();
            }
            catch (System.Exception) { }
            finally
            {
                file = null;
            }
        }
        #endregion
    }
}
