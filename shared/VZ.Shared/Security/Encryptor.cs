using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace VZ.Shared.Security
{
    public class Encryptor
    {
        private byte[] _key;
        private byte[] _iv;

        public Encryptor()
            : this(Config.Encryption.Key, Config.Encryption.IV)
        {
        }

        public Encryptor(string key, string iv)
            : this(Convert.FromBase64String(key), Convert.FromBase64String(iv))
        {
        }

        public Encryptor(byte[] key, byte[] iv)
        {
            _key = key;
            _iv = iv;
        }

        private static Encryptor _instance = new Encryptor();
        public static Encryptor Instance => _instance;

        public string Encrypt(string plainText)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (_key == null || _key.Length <= 0)
                throw new ArgumentNullException("_key");
            if (_iv == null || _iv.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an Aes object
            // with the specified _key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = _key;
                aesAlg.IV = _iv;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream.
            //return encrypted;
            return BitConverter.ToString(encrypted).Replace("-", "").ToLower();
        }

        // see https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.aes?view=netframework-4.8
        public string Decrypt(string encryptedText)
        {
            byte[] cipherText = StringToByteArray(encryptedText);

            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (_key == null || _key.Length <= 0)
                throw new ArgumentNullException("_key");
            if (_iv == null || _iv.Length <= 0)
                throw new ArgumentNullException("iv");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = _key;
                aesAlg.IV = _iv;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }

        public string GenerateToken(object input)
        {
            var stringObject = Newtonsoft.Json.JsonConvert.SerializeObject(input);
            var encryptedObject = Encrypt(stringObject);
            var utf8Object = StringToByteArray(encryptedObject);
            return Convert.ToBase64String(utf8Object);
        }

        private static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}
