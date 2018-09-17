namespace Network.Exception
{
    using System;

    /// <summary>
    /// Invalid host exceptions are thrown if the target host is not a valid one.
    /// </summary>
    public class InvalidHostException : Exception
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
        public InvalidHostException(string _message)
        {
            message = _message;
        }
    }
}
