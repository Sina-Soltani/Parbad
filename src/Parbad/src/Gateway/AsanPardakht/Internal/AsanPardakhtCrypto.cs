using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Parbad.Gateway.AsanPardakht.Internal
{
    internal class AsanPardakhtCrypto : IAsanPardakhtCrypto
    {
        public string Encrypt(string input, string key, string iv)
        {
            var aes = new RijndaelManaged
            {
                KeySize = 256,
                BlockSize = 256,
                Padding = PaddingMode.PKCS7,
                Key = Convert.FromBase64String(key),
                IV = Convert.FromBase64String(iv)
            };

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            byte[] buffer;

            using (var stream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
                {
                    byte[] xml = Encoding.UTF8.GetBytes(input);

                    cryptoStream.Write(xml, 0, xml.Length);
                }

                buffer = stream.ToArray();
            }

            return Convert.ToBase64String(buffer);
        }

        public string Decrypt(string input, string key, string iv)
        {
            var aes = new RijndaelManaged
            {
                KeySize = 256,
                BlockSize = 256,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                Key = Convert.FromBase64String(key),
                IV = Convert.FromBase64String(iv)
            };

            var decryptor = aes.CreateDecryptor();
            byte[] buffer;

            using (var stream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Write))
                {
                    var xml = Convert.FromBase64String(input);

                    cryptoStream.Write(xml, 0, xml.Length);
                }

                buffer = stream.ToArray();
            }

            return Encoding.UTF8.GetString(buffer);
        }
    }
}
