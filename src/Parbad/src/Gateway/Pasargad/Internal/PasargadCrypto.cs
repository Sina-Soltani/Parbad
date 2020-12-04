using System;
using System.Security.Cryptography;
using System.Text;

namespace Parbad.Gateway.Pasargad.Internal
{
    internal class PasargadCrypto : IPasargadCrypto
    {
        public string Encrypt(string privateKey, string data)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                byte[] encryptedData;
#if NETSTANDARD2_0
                rsa.FromXml(privateKey);
                encryptedData = rsa.SignData(Encoding.UTF8.GetBytes(data), HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
#else
                rsa.FromXmlString(privateKey);
                encryptedData = rsa.SignData(Encoding.UTF8.GetBytes(data), new SHA1CryptoServiceProvider());
#endif
                return Convert.ToBase64String(encryptedData);
            }
        }
    }
}
