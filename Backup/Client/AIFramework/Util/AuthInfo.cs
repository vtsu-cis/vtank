using System;

namespace AIFramework.Util
{
    public class AuthInfo
    {
        #region Properties
        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string Username
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the raw password.
        /// </summary>
        public string Password
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructs an auth info object.
        /// </summary>
        /// <param name="username">Username stored in the string.</param>
        /// <param name="password">Raw password.</param>
        public AuthInfo(string username, string password)
        {
            Username = username;
            Password = password;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Converts this object into it's string-form object, which takes the
        /// following format:
        /// 
        /// username:password
        /// (or:)
        /// username!password
        /// 
        /// It returns "username:password" if the password is *not* encrypted.
        /// If the delimiter is an exclamation mark, the password *is* encrypted.
        /// </summary>
        /// <param name="encrypted">Whether or not to encrypt the password.</param>
        /// <returns>String-form of this object.</returns>
        public string ToString(bool encrypted)
        {
            char delimiter = encrypted ? '!' : ':';
            string password = encrypted ? EncryptPassword() : Password;

            return Username + delimiter + password;
        }

        /// <summary>
        /// Returns the encrypted version of this object's password.
        /// TODO: This currently does nothing.
        /// </summary>
        /// <returns>Encrypted version of the password.</returns>
        private string EncryptPassword()
        {
            // TODO: This doesn't encrypt things at all!
            return Password;
        }
        #endregion

        #region Overridden Methods
        /// <summary>
        /// Converts this object into it's string-form object, which takes the
        /// following format:
        /// 
        /// username:password
        /// 
        /// This means that it spits out the unencrpyted password. For the encrypted
        /// version, call 'ToString(true)'.
        /// </summary>
        /// <returns>Converted string-form for this auth information.</returns>
        public override string ToString()
        {
            return ToString(false);
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Convert authentication information from a string into an AuthInfo object.
        /// The given string must follow this format:
        /// 
        /// username:password
        /// (or:)
        /// username!password
        /// 
        /// The colon indicates an unencrpyted password, while an exclamation mark
        /// indicates an encrypted password.
        /// </summary>
        /// <param name="userstring">Compatible AuthInfo string.</param>
        /// <returns>AuthInfo object.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the string is malformed.</exception>
        public static AuthInfo FromString(string userstring)
        {
            if (!userstring.Contains(":") && !userstring.Contains("!"))
            {
                throw new InvalidOperationException(
                    "Invalid userstring: no delimitmer.");
            }

            char[] delimiter = new char[] { userstring.Contains(":") ? ':' : '!' };

            string[] data = userstring.Split(delimiter, 2);
            string username = data[0];
            string password = data[1];

            if (delimiter[0] == '!')
            {
                // Password is encrypted...
                password = DecryptPassword(password);
            }

            return new AuthInfo(username, password);
        }

        /// <summary>
        /// Utility method for decrypting an encrypted password.
        /// </summary>
        /// <param name="input">Input to decrypt.</param>
        /// <returns>Output.</returns>
        private static string DecryptPassword(string input)
        {
            // TODO: This doesn't do anything.
            return input;
        }
        #endregion
    }
}
