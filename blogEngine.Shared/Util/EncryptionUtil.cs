using System;
using System.IO;
using System.Security.Cryptography;

using FellowshipTech.Common;

namespace Foundation.Common.Util {
	/// <summary>
	/// Summary description for EncryptionUtil.
	/// </summary>
	public class EncryptionUtil {
		public static string EncryptValue(string valueToEncrypt, Common.Enum.EncryptionValueType valueType) {
			if(valueToEncrypt != null && valueToEncrypt != string.Empty){
				string key = EncryptionUtil.GetKey(valueType);
				string iv = EncryptionUtil.GetIV(valueType);
                if(valueType == Foundation.Common.Enum.EncryptionValueType.URLParms) {
                    return Hex(EncryptionUtil.EncryptValue(key, iv, valueToEncrypt));
                    //return EncryptionUtil.EncryptValue(key, iv, valueToEncrypt);
                }
                else {
                    return EncryptionUtil.EncryptValue(key, iv, valueToEncrypt);
                }
			}
			else{
				return null;
			}
		}

		public static string DecryptValue(string valueToDecrypt, Common.Enum.EncryptionValueType valueType) {
			if(valueToDecrypt != null && valueToDecrypt != string.Empty){
				string key = EncryptionUtil.GetKey(valueType);
				string iv = EncryptionUtil.GetIV(valueType);
                if(valueType == Foundation.Common.Enum.EncryptionValueType.URLParms) {
                    return EncryptionUtil.DecryptValue(key, iv, DeHex(valueToDecrypt));
                    //return EncryptionUtil.DecryptValue(key, iv, valueToDecrypt);
                }
                else {
                    return EncryptionUtil.DecryptValue(key, iv, valueToDecrypt);
                }				
			}
			else{
				return null;
			}
		}

        private static string DeHex(string hexstring) {
            string ret = String.Empty;
            System.Text.StringBuilder sb = new System.Text.StringBuilder(hexstring.Length / 2);
            for(int i = 0; i <= hexstring.Length - 1; i = i + 2) {
                sb.Append((char)int.Parse(hexstring.Substring(i, 2), System.Globalization.NumberStyles.HexNumber));
            }
            return sb.ToString();
        }

        public static string Hex(string sData){
            string temp = String.Empty;;
            string newdata=String.Empty;
            System.Text.StringBuilder sb = new System.Text.StringBuilder(sData.Length * 2);

            for (int i = 0;i < sData.Length;i++){
                if ((sData.Length - (i + 1)) > 0){
                    temp = sData.Substring(i,2);
                    if (temp == @"\n") newdata += "0A";
                    else if (temp == @"\b") newdata += "20";
                    else if (temp == @"\r") newdata += "0D";
                    else if (temp == @"\c") newdata += "2C";
                    else if (temp == @"\\") newdata += "5C";
                    else if (temp == @"\0") newdata += "00";
                    else if (temp == @"\t") newdata += "07";
                    else{
                        sb.Append(  String.Format("{0:X2}", (int)(sData.ToCharArray())[i]));
                        i--;
                    }
                }
                else{
                    sb.Append(  String.Format("{0:X2}", (int)(sData.ToCharArray())[i]));
                }
                i++;
            }
            return sb.ToString();
        }

		public static string CreateRandomSalt(){			
			int saltByteSize = 16;
			System.Security.Cryptography.RNGCryptoServiceProvider rng;
			rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
			byte[] buff = new byte[saltByteSize];
			rng.GetBytes(buff);
			return Convert.ToBase64String(buff);
		}

		private static string EncryptValue(string key, string ivCode, string value){
			string sPlainText = value.ToString();			

			// first we need to translate the string into an array of bytes
			byte[] abPlainText = System.Text.Encoding.UTF8.GetBytes(sPlainText);

			// create a symmetric algorithm object
			Rijndael alg = Rijndael.Create();
			alg.Key = EncryptionUtil.StringToByteArray(key);
			alg.IV = EncryptionUtil.StringToByteArray(ivCode);

			// create a memory stream to hold the encrypted bytes
			MemoryStream ms = new MemoryStream();

			// create and setup a crypto stream
			CryptoStream cs;

			// we need base64 encoded text as a result
			// chain two crypto stream together - encrypting and base64 encoding
			CryptoStream cs_base64 = new CryptoStream(ms, new ToBase64Transform(), CryptoStreamMode.Write);
			cs = new CryptoStream(cs_base64, alg.CreateEncryptor(), CryptoStreamMode.Write);

			// perform the encryption
			cs.Write(abPlainText, 0, abPlainText.Length);

			// we need to call Close() or FlushFinalBlock() to indicate that
			// we have written all the data in and it is safe to apply padding
			cs.Close();

			// get the encrypted bytes
			byte[] abCipherText = ms.ToArray();
			String sCipherText = System.Text.Encoding.UTF8.GetString(abCipherText);
			return sCipherText;
		}

		private static string DecryptValue(string key, string ivCode, string value) {
			string rVal = null;

			// we could reuse the same Rijndael object, but let's pretend that it's a different program that does the decryption
			Rijndael alg_dec = Rijndael.Create();

			// set the decryption key and IV
			alg_dec.Key = EncryptionUtil.StringToByteArray(key);
			alg_dec.IV = EncryptionUtil.StringToByteArray(ivCode);

			// create a memory stream to hold the decrypted bytes
			MemoryStream ms_dec = new MemoryStream();

			// create and setup a crypto stream
			CryptoStream cs_dec;

			// or our encrypted data is a base64 encoded string
			// chain two crypto streams together - base64 decoding and decrypting
			CryptoStream cs_tmp = new CryptoStream(ms_dec, alg_dec.CreateDecryptor(), CryptoStreamMode.Write);
			cs_dec = new CryptoStream(cs_tmp, new FromBase64Transform(), CryptoStreamMode.Write);

			// get the bytes out of string
			byte[] abCipherText = System.Text.Encoding.UTF8.GetBytes(value);

			// perform the decryption
			cs_dec.Write(abCipherText, 0, abCipherText.Length);
			cs_dec.Close();
			byte[] abRoundtrippedText = ms_dec.ToArray();

			// now we have our round-tripped text in a byte array
			// turn it into string and output to the console
			string sRoundtrippedText = System.Text.Encoding.UTF8.GetString(abRoundtrippedText);
			rVal = sRoundtrippedText;		
			return rVal;
		}

		private static string GetKey(Common.Enum.EncryptionValueType valueType) {
			string key = null;

			switch (valueType) {
				case Common.Enum.EncryptionValueType.FTUserPassword : {
					key = "K2Uh7T3lhnB9j1l$";
					break;
				}
				case Common.Enum.EncryptionValueType.HCode : {
					key = "K2Uh7T3lhnB9j1l$";
					break;
				}
				case Common.Enum.EncryptionValueType.ICode : {
					key = "K2Uh7T3lhnB9j1l$";
					break;
				}
				case Common.Enum.EncryptionValueType.DataExchange : {
					key = "K2Uh7T3lhnB9j1l$";
					break;
				}
				case Common.Enum.EncryptionValueType.MedicalModule : {
					key = "FT3h7T3lhnB9j1l$";
					break;
				}
				case Common.Enum.EncryptionValueType.FormIndividual : {
					key = "FT3h7T3lhnB9j1l$";
					break;
				}
				case Common.Enum.EncryptionValueType.OnlinePayment : {
					key = "3LdOre9sk1?doYmZ";
					break;	
				}
				case Common.Enum.EncryptionValueType.UniqueID : {
					key = "K2Uh7T3lhnB9j1l$";
					break;	
				}
				case Common.Enum.EncryptionValueType.URLParms : {
					key = "I2Uh7T3lhnB9j1l$";
					break;	
				}
			}

			return key;
		}

		private static string GetIV(Common.Enum.EncryptionValueType valueType) {
			string iv = null;

			switch (valueType) {
				case Common.Enum.EncryptionValueType.FTUserPassword : {
					iv = "VkRisp&nksTsko%4";
					break;
				}
				case Common.Enum.EncryptionValueType.HCode : {
					iv = "VkRisp&nksTsko%4";
					break;
				}
				case Common.Enum.EncryptionValueType.ICode : {
					iv = "VkRisp&nksTsko%4";
					break;
				}
				case Common.Enum.EncryptionValueType.DataExchange : {
					iv = "VkRisp&nksTsko%4";
					break;
				}
				case Common.Enum.EncryptionValueType.MedicalModule : {
					iv = "MkMisp&nksTsko%4";
					break;
				}
				case Common.Enum.EncryptionValueType.FormIndividual : {
					iv = "MkMisp&nksTsko%4";
					break;
				}
				case Common.Enum.EncryptionValueType.OnlinePayment : {
					iv = "G2KdiV&rKqd62BaE";
					break;
				}
				case Common.Enum.EncryptionValueType.UniqueID : {
					iv = "VkRisp&nksTsko%4";
					break;
				}
				case Common.Enum.EncryptionValueType.URLParms : {
					iv = "WkRisp&nksTsko%4";
					break;
				}
			}

			return iv;
		}

		private static byte[] StringToByteArray(string s) {
			byte[] result = new byte[s.Length];
			for(int i = 0; i < s.Length;i++) {
				result[i] = (byte)s[i];
			}
			return result;
		}
	}
}
