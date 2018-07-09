using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Parbad.Utilities
{
    internal static class CommonTools
    {
        internal static byte[] SerializeObject(object obj)
        {
            IFormatter formatter = new BinaryFormatter();

            byte[] buffer;

            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);

                buffer = stream.ToArray();
            }

            return buffer;
        }

        internal static T DeserializeObject<T>(byte[] buffer)
        {
            IFormatter formatter = new BinaryFormatter();

            T result;

            using (var stream = new MemoryStream(buffer))
            {
                result = (T)formatter.Deserialize(stream);
            }

            return result;
        }

        internal static string Encrypt(byte[] buffer)
        {
            const string encryptionKey = "xxxxxxxxxxxxxx";

            using (var encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });

                encryptor.Key = pdb.GetBytes(32);

                encryptor.IV = pdb.GetBytes(16);

                using (var stream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(stream, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(buffer, 0, buffer.Length);
                    }

                    return Convert.ToBase64String(stream.ToArray());
                }
            }
        }

        internal static byte[] Decrypt(string text)
        {
            const string encryptionKey = "xxxxxxxxxxxxxx";

            text = text.Replace(" ", "+");

            byte[] cipherBytes = Convert.FromBase64String(text);

            byte[] resultBuffer;

            using (var encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });

                encryptor.Key = pdb.GetBytes(32);

                encryptor.IV = pdb.GetBytes(16);

                using (var stream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(stream, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);
                    }

                    resultBuffer = stream.ToArray();
                }
            }

            return resultBuffer;
        }

        internal static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return string.Empty;
            }

            var buffer = Encoding.UTF8.GetBytes(password);

            using (var md5 = new MD5CryptoServiceProvider())
            {
                for (int index = 0; index < 5; index++)
                {
                    buffer = md5.ComputeHash(buffer, 0, buffer.Length);
                }
            }

            return Convert.ToBase64String(buffer);
        }

        internal static string GetRedirectUrlIncludingGateway(string gatewayName, string redirectUrl)
        {
            var uri = new Uri(redirectUrl);

            string queryString = "";

            if (string.IsNullOrEmpty(uri.Query))
            {
                queryString = "?";
            }
            else if (uri.Query == "?")
            {

            }
            else
            {
                queryString = "&";
            }

            queryString += $"gateway={gatewayName}";

            string newUrl = redirectUrl + queryString;

            return new Uri(newUrl).ToString();
        }

        internal static bool IsValidUrl(string url)
        {
            try
            {
                new Uri(url);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}