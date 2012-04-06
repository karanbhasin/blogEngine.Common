using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Foundation.Common.PasswordExtensions {
    public static class SecurePassword {

        /// <summary>
        /// Compares a hash password to a value + salt to see if they match
        /// </summary>
        /// <param name="source">the password has that will be compared</param>
        /// <param name="salt">the salt used to hash the incoming value</param>
        /// <param name="value">the password value that was supplied by the user</param>
        /// <returns></returns>
        public static bool ComparePassword(this string source, string salt, string value) {

            return value.CreateHash(salt) == source;
        }

        /// <summary>
        /// Creates a new hash value from a string and a salt value
        /// </summary>
        /// <param name="source">The value that is being encrypted</param>
        /// <param name="salt">The salt value for the source value</param>
        /// <returns></returns>
        public static string CreateHash(this string source, string salt) {

            HMACSHA1 hmacsha1 = new HMACSHA1();
            hmacsha1.Key = ASCIIEncoding.ASCII.GetBytes(SecurityKey);
            hmacsha1.ComputeHash(source.ConvertToByte(salt));

            return ASCIIEncoding.ASCII.GetString(hmacsha1.Hash);
        }

        /// <summary>
        /// Creates a salt value based off the Random Number Genereator of the Cyrptography class
        /// </summary>
        /// <returns>a 24 byte string</returns>
        public static string CreateSalt() {
            //Generate a cryptographic random number.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[24];
            rng.GetBytes(buff);

            // Return a Base64 string representation of the random number.
            return Convert.ToBase64String(buff);
        }


        #region Private Properties

        /// <summary>
        /// Holds the security key being used for password encryption
        /// </summary>
        private static string SecurityKey {
            get {
                return Foundation.Common.Util.ConfigUtil.GetAppSetting("Global.Password.Key");
            }
        }
        #endregion Private Properties

        #region Private Methods

        /// <summary>
        /// Converts a password string plus the salt to a byte array
        /// </summary>
        /// <param name="password">the value of the password</param>
        /// <param name="salt">the salt value</param>
        /// <returns></returns>
        private static byte[] ConvertToByte(this string source, string salt) {
            return ASCIIEncoding.ASCII.GetBytes(source + salt);
        }
        #endregion Private Methods
    }
}
