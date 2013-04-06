namespace Network.Exception
{
    using System;

    /// <summary>
    /// An InvalidValueException is thrown by the communicator when the user attempts
    /// to pass in a value that makes no sense.
    /// </summary>
    public class InvalidValueException : Exception
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
        /// Create the exception.
        /// </summary>
        /// <param name="message">User-friendly exception message.</param>
        public InvalidValueException(string _message)
        {
            message = _message;
        }
    }
}
