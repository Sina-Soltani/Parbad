using System;
using System.Security.Cryptography;
using System.Text;

namespace Parbad.Providers.Pasargad
{
    internal static class PasargadHelper
    {
        public static bool IsPrivateKeyValid(string privateKey)
        {
            try
            {
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(privateKey);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string SignData(string privateKey, string dataToSign)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);

                byte[] encryptedData = rsa.SignData(Encoding.UTF8.GetBytes(dataToSign), new SHA1CryptoServiceProvider());

                return Convert.ToBase64String(encryptedData);
            }
        }

        public static string GetTimeStamp(DateTime dateTime)
        {
            return dateTime.ToString("yyyy/MM/dd HH:mm:ss");
        }
    }
}