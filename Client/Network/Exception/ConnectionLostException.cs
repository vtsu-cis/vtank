namespace Network.Exception
{
    using System;

    /// <summary>
    /// Exception thrown if the connection to the server is severred.
    /// </summary>
    public class ConnectionLostException : Exception
    {
        private string message;
        /// <summary>
        /// The user-friendly message for this exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return message;
            }
        }

        /// <summary>
        /// Create a new exception.
        /// </summary>
        /// <param name="message">User-friendly message; this is what the end-user
        /// sees.</param>
        public ConnectionLostException(string _message)
        {
            message = _message;
        }
    }
}
