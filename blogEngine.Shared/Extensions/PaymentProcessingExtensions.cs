using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FellowshipTech.Common.Extensions.PaymentProcessing {
    public static class PaymentProcessingExtensions {

        /// <summary>
        /// Converts a encrypted credit card number to a display number for the presentation
        /// </summary>
        /// <param name="source">The string that should be the encrypted cc number</param>
        /// <returns></returns>
        public static string CreditCardDisplay(this string source) {

            string rVal = null;

            if (!string.IsNullOrEmpty(source)) {

                rVal = "****";

                // Decrypt the credit card number
                string normalCCNumber = source.Decrypt();

                // Take the last 4 numbers and add some asteriks to it 
                // unless there is less than 4 numbers than just appaend asteriks to the cc number
                if (normalCCNumber.Length > 3) {

                    rVal += normalCCNumber.Substring(normalCCNumber.Length - 4);
                }
                else {
                    rVal += normalCCNumber;
                }
            }

            return rVal;
        }
        /// <summary>
        /// Encrypts a credit card number
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string Encrypt(this string source) {
            return FellowshipTech.Common.Util.EncryptionUtil.EncryptValue(source, FellowshipTech.Common.Enum.EncryptionValueType.OnlinePayment);
        }

        /// <summary>
        /// Decrypts a credit card number
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string Decrypt(this string source) {

            string rVal = null;
            try {
                rVal = FellowshipTech.Common.Util.EncryptionUtil.DecryptValue(source, FellowshipTech.Common.Enum.EncryptionValueType.OnlinePayment);
            }
            catch (System.Exception e) {
            }

            return rVal;
        }

        /// <summary>
        /// Compares two credit card numbers to see if they are equal
        /// </summary>
        /// <param name="source">An unencrypted credit card number</param>
        /// <param name="value">An encrypted credit card number</param>
        /// <returns></returns>
        public static bool IsCreditCardEqual(this string source, string value) {

            // Decrypt the value that was passed in
            string decryptedCCNumber = value.Decrypt();

            if (!string.IsNullOrEmpty(decryptedCCNumber)) {
                return source.IsEqualTo(decryptedCCNumber);
            }
            else {
                // The two values are not equal if the derypted card is null
                return false;
            }
        }
    }
}
