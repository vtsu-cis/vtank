namespace Network.Util
{
    /// <summary>
    /// Results are packaged with the following data: whether or not the attempt succeeded,
    /// the object (if it succeeded), and the exception (if it failed).
    /// </summary>
    public class Result<T>
    {
        #region Properties
        /// <summary>
        /// Whether or not the attempt was successful.
        /// </summary>
        public bool Success
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the requested return object, if any.
        /// </summary>
        public T ReturnedResult
        {
            get;
            set;
        }

        /// <summary>
        /// Get the exception, if the attempt failed.
        /// </summary>
        public System.Exception Exception
        {
            get;
            set;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Use this constructor when the result has no return value, but it succeeded.
        /// </summary>
        public Result()
        {
            ReturnedResult = default(T);
            Success = true;
        }

        /// <summary>
        /// Use this constructor when the attempt succeeded.
        /// </summary>
        /// <param name="result">Result of the attempt.</param>
        public Result(T result)
        {
            ReturnedResult = result;
            Success = true;
        }

        /// <summary>
        /// Use this constructor when the attempt failed.
        /// </summary>
        /// <param name="ex">Exception that occurred.</param>
        public Result(System.Exception ex)
        {
            Exception = ex;
            Success = false;
        }
        #endregion
    };

    /// <summary>
    /// Like the generic version, but it makes it easier to handle external callbacks at the
    /// cost of being slightly less easy to use.
    /// </summary>
    public class Result
    {
        #region Properties
        /// <summary>
        /// Gets or sets the success of this result.
        /// </summary>
        public bool Success
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the result of the attempt, if any.
        /// </summary>
        public object ReturnedResult
        {
            get;
            set;
        }

        /// <summary>
        /// Exception that occurred, if any.
        /// </summary>
        public System.Exception Exception
        {
            get;
            set;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new result object.
        /// </summary>
        /// <param name="successful">Whether or not the attempt was successful.</param>
        /// <param name="o">Returned value.</param>
        /// <param name="e">Exception that occurred, if any.</param>
        public Result(bool successful, object o, System.Exception e)
        {
            Success = successful;
            ReturnedResult = o;
            Exception = e;
        }
        #endregion
    };

    #region Callback Declarations
    public delegate void ActionFinished<T>(Result<T> result);
    public delegate void ActionFinished(Result result);
    /// <summary>
    /// The NoReturnValue struct is used for results which have no return value.
    /// </summary>
    public struct NoReturnValue
    {
    };
    #endregion
}
