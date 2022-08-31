using SAB.Library.Core.Crypt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OnSign.Common.Utilites
{
    public class TokenUtility
    {
        public TokenUtility()
        {

        }

        public static string EncryptToken(params string[] userRole)
        {
            var base64 = Base64Key();
            var _plainText = MakePlainText(base64, userRole);
            _plainText = EncryptTokenLV1(_plainText);
            _plainText = EncryptTokenLV2(_plainText);
            var result = EncryptTokenLV3(_plainText);
            return result;
        }

        public static string DecryptToken(string token)
        {
            var base64 = Base64Key();
            var _plainText = DecryptTokenLV1(token);
            _plainText = DecryptTokenLV2(_plainText);
            _plainText = DecryptTokenLV3(_plainText);
            var result = GetPlainText(_plainText, base64);
            return result;
        }

        public static string MD5Hash(string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            //compute hash from the bytes of text  
            md5.ComputeHash(Encoding.ASCII.GetBytes(text));

            //get hash result after compute it  
            byte[] result = md5.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                //change it into 2 hexadecimal digits  
                //for each byte  
                strBuilder.Append(result[i].ToString("x2"));
            }

            return strBuilder.ToString();
        }

        public static string SHA1Hash(string value)
        {
            var data = Encoding.ASCII.GetBytes(value);
            var hashData = new SHA1Managed().ComputeHash(data);
            var hash = string.Empty;
            foreach (var b in hashData)
            {
                hash += b.ToString("X2");
            }
            return hash;
        }

        private static string Base64Key()
        {
            var result = $"{DateTime.Now.Day}_{DateTime.Now.Month}_{DateTime.Now.Year}";
            return Base64Encode(result);
        }

        private static string MakePlainText(string base64Key, params string[] hiddenText)
        {
            //var key = (DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + base64Key.Length) / 4;
            //var stringKey = key.ToString();
            var _plainText = String.Join(",", hiddenText);
            _plainText = Base64Encode(_plainText);

            //var temp = string.Empty;
            //for (int i = 0; i < stringKey.Length; i++)
            //{
            //    if (!string.IsNullOrEmpty(temp))
            //    {
            //        temp = Encrypt(temp, stringKey[i].ToString());
            //    }
            //    else
            //    {
            //        temp = Encrypt(_plainText, stringKey[i].ToString());
            //    }
            //}

            var result = Encrypt(_plainText, base64Key);
            return result;
        }

        private static string GetPlainText(string hiddenText, string base64Key)
        {
            var _plainText = Decrypt(hiddenText, base64Key);
            //var key = (DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + base64Key.Length) / 4;

            //var stringKey = key.ToString();
            //string temp = string.Empty;

            //for (int i = stringKey.Length - 1; i >= 0; i--)
            //{
            //    if (!string.IsNullOrEmpty(temp))
            //    {
            //        temp = Decrypt(temp, stringKey[i].ToString());
            //    }
            //    else
            //    {
            //        temp = Decrypt(_plainText, stringKey[i].ToString());
            //    }
            //}

            var result = Base64Decode(_plainText);
            return result;
        }

        private static string DecryptTokenLV1(string token)
        {
            var key = LibraryKey.Key1;
            return Decrypt(token, key);
        }

        private static string DecryptTokenLV2(string token)
        {
            var key = LibraryKey.Key2;
            return Decrypt(token, key);
        }

        private static string DecryptTokenLV3(string token)
        {
            var key = LibraryKey.Key3;
            return Decrypt(token, key);
        }

        private static string HashKey(string key)
        {
            var result = string.Empty;
            for (int i = 0; i < key.Length; i++)
            {
                var _key = LibraryKey.Keys.FirstOrDefault(x => x.Key == int.Parse(key[i].ToString())).Value;
                result += _key;
            }

            return MD5Hash(MD5Hash(MD5Hash(SHA1Hash(SHA1Hash(SHA1Hash(result))))));
        }

        private static string Encrypt(string plainText, string passPhrase)
        {
            return Cryptography.Encrypt(plainText, passPhrase);
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            //var saltStringBytes = Generate256BitsOfRandomEntropy();
            //var ivStringBytes = Generate256BitsOfRandomEntropy();
            //var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            //using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            //{
            //    var keyBytes = password.GetBytes(Keysize / 8);
            //    using (var symmetricKey = new RijndaelManaged())
            //    {
            //        symmetricKey.BlockSize = 256;
            //        symmetricKey.Mode = CipherMode.CBC;
            //        symmetricKey.Padding = PaddingMode.PKCS7;
            //        using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
            //        {
            //            using (var memoryStream = new MemoryStream())
            //            {
            //                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            //                {
            //                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            //                    cryptoStream.FlushFinalBlock();
            //                    // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
            //                    var cipherTextBytes = saltStringBytes;
            //                    cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
            //                    cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
            //                    memoryStream.Close();
            //                    cryptoStream.Close();
            //                    return Convert.ToBase64String(cipherTextBytes);
            //                }
            //            }
            //        }
            //    }
            //}
        }

        private static string Decrypt(string cipherText, string passPhrase)
        {
            return Cryptography.Decrypt(cipherText, passPhrase);
            //// Get the complete stream of bytes that represent:
            //// [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            //var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            //// Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            //var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            //// Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            //var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            //// Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            //var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            //using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            //{
            //    var keyBytes = password.GetBytes(Keysize / 8);
            //    using (var symmetricKey = new RijndaelManaged())
            //    {
            //        symmetricKey.BlockSize = 256;
            //        symmetricKey.Mode = CipherMode.CBC;
            //        symmetricKey.Padding = PaddingMode.PKCS7;
            //        using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
            //        {
            //            using (var memoryStream = new MemoryStream(cipherTextBytes))
            //            {
            //                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
            //                {
            //                    var plainTextBytes = new byte[cipherTextBytes.Length];
            //                    var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            //                    memoryStream.Close();
            //                    cryptoStream.Close();
            //                    return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
            //                }
            //            }
            //        }
            //    }
            //}
        }

        //private static byte[] Generate256BitsOfRandomEntropy()
        //{
        //    var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
        //    using (var rngCsp = new RNGCryptoServiceProvider())
        //    {
        //        // Fill the array with cryptographically secure random bytes.
        //        rngCsp.GetBytes(randomBytes);
        //    }
        //    return randomBytes;
        //}

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        private static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private static string EncryptTokenLV1(string token)
        {
            var key = LibraryKey.Key3;
            return Encrypt(token, key);
        }

        private static string EncryptTokenLV2(string token)
        {
            var key = LibraryKey.Key2;
            return Encrypt(token, key);
        }

        private static string EncryptTokenLV3(string token)
        {
            var key = LibraryKey.Key1;
            return Encrypt(token, key);
        }

        protected class LibraryKey
        {
            public static string Key1 { get; } = HashKey(DateTime.Now.Day.ToString());

            public static string Key2 { get; } = HashKey(DateTime.Now.Month.ToString());

            public static string Key3 { get; } = HashKey(DateTime.Now.Year.ToString());

            public static Dictionary<int, string> Keys
            {
                get
                {
                    var temp = new Dictionary<int, string>
                    {
                        { 0, "A@9!a#J%" },
                        { 1, "B@8!b#I%" },
                        { 2, "C@7!c#H%" },
                        { 3, "D@6!d#G%" },
                        { 4, "E@5!e#F%" },
                        { 5, "F@4!f#E%" },
                        { 6, "G@3!g#D%" },
                        { 7, "H@2!h#C%" },
                        { 8, "I@1!i#B%" },
                        { 9, "J@0!j#A%" }
                    };
                    return temp;
                }
            }

            public LibraryKey()
            {

            }
        }
    }

}