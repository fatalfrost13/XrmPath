using XrmPath.Helpers.Utilities;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace XrmPath.Helpers
{
    public static class EncryptionUtility
    {
        public static readonly string AesEncryptionKey = ConfigurationManager.AppSettings["AESEncryptionKey"];

        private const int Keysize = 256;
        
         /// <summary>
        /// 
        /// </summary>
        /// <param name="plainText">Text to encrypt</param>
        /// <param name="passPhrase"></param>
        /// <param name="urlEncode">Set to true if this value is coming from URL. Set to false if it's unmodified and coming from server side code.</param>
        /// <returns></returns>
        public static string Encrypt(string plainText, string passPhrase = "", bool urlEncode = false)
        {
            if (string.IsNullOrEmpty(passPhrase))
            {
                passPhrase = AesEncryptionKey;
            }

            // IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same IV values can be used when decrypting.
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes);
            var keyBytes = password.GetBytes(Keysize / 8);

            using (var symmetricKey = new RijndaelManaged())
            {
                symmetricKey.BlockSize = 256;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                            cryptoStream.FlushFinalBlock();
                            // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                            var cipherTextBytes = saltStringBytes;
                            cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                            cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                            memoryStream.Close();
                            cryptoStream.Close();
                            string base64 = Convert.ToBase64String(cipherTextBytes);

                            if (urlEncode) 
                            { 
                                //if we're using this in the URL, we need to replace reserved characters so that they are not missing or lost
                                base64 = System.Net.WebUtility.UrlEncode(base64);
                            }
   
                            return base64;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cipherText">encrypted value</param>
        /// <param name="passPhrase"></param>
        /// <param name="urlDecode">Set to true if decoding from URL. Set to false if only using it in server side.</param>
        /// <returns></returns>
        public static string Decrypt(string cipherText, string passPhrase = "", bool urlDecode = false)
        {
            if (string.IsNullOrEmpty(passPhrase))
            {
                passPhrase = AesEncryptionKey;
            }

            if (urlDecode) 
            {
                cipherText = System.Net.WebUtility.UrlDecode(cipherText);
                cipherText = cipherText.Replace(" ", "+");  //decode for some reason replaces + with spaces. quick hack to fix this anomaly
            }
           
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }
        
        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            var rngCsp = new RNGCryptoServiceProvider();
 
            // Fill the array with cryptographically secure random bytes.
            rngCsp.GetBytes(randomBytes);
 
            return randomBytes;
        }
    }
}
