using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Client.src.util
{
    public static class Hash
    {
        /// <summary>
        /// Calculates an irreversible hash with the SHA1 algorithm.
        /// </summary>
        /// <param name="text">String to hash.</param>
        /// <param name="enc">Character encoding.</param>
        /// <returns>SHA1 hash.</returns>
        public static string CalculateSHA1(string text, Encoding enc)
        {
            byte[] buffer = enc.GetBytes(text);
            SHA1CryptoServiceProvider SHA1 = new SHA1CryptoServiceProvider();
            string hash = BitConverter.ToString(
                SHA1.ComputeHash(buffer)).Replace("-", "").ToLower();

            return hash;
        }

        /// <summary>
        /// Calculates SHA1 hash, using the ASCII encoding.
        /// </summary>
        /// <param name="text">input string</param>
        /// <param name="enc">Character encoding</param>
        /// <returns>SHA1 hash</returns>
        public static string CalculateSHA1(string text)
        {
            return CalculateSHA1(text, Encoding.ASCII);
        }

        /// <summary>
        /// Thanks: http://www.codeproject.com/KB/files/dt_file_hasher.aspx
        /// Get the SHA1 hash of any file.
        /// </summary>
        /// <param name="pathName">Path to the file, including the filename obviously.</param>
        /// <returns>40-character hexadecimal string.</returns>
        public static string CalculateSHA1OfFile(string pathName)
        {
            SHA1CryptoServiceProvider SHA1 = new SHA1CryptoServiceProvider();
            byte[] arrbytHashValue;

            using (FileStream oFileStream = new FileStream(pathName, FileMode.Open))
            {
                arrbytHashValue = SHA1.ComputeHash(oFileStream);
            }

            return BitConverter.ToString(arrbytHashValue).Replace("-", "").ToLower();
        }

    }
}
