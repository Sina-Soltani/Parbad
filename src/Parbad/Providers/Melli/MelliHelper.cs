using System;
using System.Security.Cryptography;
using System.Text;

namespace Parbad.Providers.Melli
{
    internal class MelliHelper
    {
        public static string SignRequestData(string terminalId, string merchantKey, long orderId, long amount)
        {
            var dataBytes = Encoding.UTF8.GetBytes($"{terminalId};{orderId};{amount}");

            var symmetric = SymmetricAlgorithm.Create("TripleDes");
            symmetric.Mode = CipherMode.ECB;
            symmetric.Padding = PaddingMode.PKCS7;

            var encryptor = symmetric.CreateEncryptor(Convert.FromBase64String(merchantKey), new byte[8]);

            return Convert.ToBase64String(encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length));
        }

        public static string SignVerifyData(string merchantKey, string token)
        {
            var dataBytes = Encoding.UTF8.GetBytes(token);

            var symmetric = SymmetricAlgorithm.Create("TripleDes");
            symmetric.Mode = CipherMode.ECB;
            symmetric.Padding = PaddingMode.PKCS7;

            var encryptor = symmetric.CreateEncryptor(Convert.FromBase64String(merchantKey), new byte[8]);

            return Convert.ToBase64String(encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length));
        }
    }
}