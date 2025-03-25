using System;
using System.Security.Cryptography;
using System.Text;

namespace Parbad.Gateway.Melli.Internal;

internal class MelliGatewayCrypto : IMelliGatewayCrypto
{
    public string Encrypt(string terminalKey, string data)
    {
        try
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);

            var symmetric = SymmetricAlgorithm.Create("TripleDes");
            symmetric.Mode = CipherMode.ECB;
            symmetric.Padding = PaddingMode.PKCS7;

            var encryptor = symmetric.CreateEncryptor(Convert.FromBase64String(terminalKey), new byte[8]);

            return Convert.ToBase64String(encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length));
        }
        catch (Exception exception)
        {
            throw new MelliGatewayDataSigningException(exception);
        }
    }
}
