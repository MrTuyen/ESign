using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace OnSign.Common.Helpers
{
    public static class EncryptDecryptHelper
    {
        private static readonly string key = "onsign";
        private static readonly string salt = "novaon";

        public static string EncryptQueryString(string inputText)
        {
            try
            {
                byte[] plainText = Encoding.UTF8.GetBytes(inputText);

                using (RijndaelManaged rijndaelCipher = new RijndaelManaged())
                {
                    PasswordDeriveBytes secretKey = new PasswordDeriveBytes(Encoding.ASCII.GetBytes(key), Encoding.ASCII.GetBytes(salt));
                    using (ICryptoTransform encryptor = rijndaelCipher.CreateEncryptor(secretKey.GetBytes(32), secretKey.GetBytes(16)))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainText, 0, plainText.Length);
                                cryptoStream.FlushFinalBlock();
                                string base64 = Convert.ToBase64String(memoryStream.ToArray());

                                // Generate a string that won't get screwed up when passed as a query string.
                                string urlEncoded = HttpUtility.UrlEncode(base64);
                                return urlEncoded;
                            }
                        }
                    }
                }
            }
            catch (Exception objEx)
            {
                ConfigHelper.Instance.WriteLogException($"Lỗi EncryptQueryString. inputText: {inputText}", objEx, MethodBase.GetCurrentMethod().Name, "EncryptQueryString");
                return inputText;
            }
        }

        public static string DecryptQueryString(string inputText)
        {
            try
            {
                byte[] encryptedData = Convert.FromBase64String(inputText);
                PasswordDeriveBytes secretKey = new PasswordDeriveBytes(Encoding.ASCII.GetBytes(key), Encoding.ASCII.GetBytes(salt));

                using (RijndaelManaged rijndaelCipher = new RijndaelManaged())
                {
                    using (ICryptoTransform decryptor = rijndaelCipher.CreateDecryptor(secretKey.GetBytes(32), secretKey.GetBytes(16)))
                    {
                        using (MemoryStream memoryStream = new MemoryStream(encryptedData))
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                byte[] plainText = new byte[encryptedData.Length];
                                cryptoStream.Read(plainText, 0, plainText.Length);
                                string utf8 = Encoding.UTF8.GetString(plainText);
                                return utf8;
                            }
                        }
                    }
                }
            }
            catch (Exception objEx)
            {
                ConfigHelper.Instance.WriteLogException($"Lỗi DecryptQueryString. inputText: {inputText}", objEx, MethodBase.GetCurrentMethod().Name, "DecryptQueryString");
                return inputText;
            }
        }
    }
}
