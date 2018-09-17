namespace Network.Exception
{
    using System;

    /// <summary>
    /// Unknown exceptions are thrown when something unusual occurs.
    /// </summary>
    public class UnknownException : Exception
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
        public UnknownException(string _message)
        {
            message = _message;
        }
    }
}
