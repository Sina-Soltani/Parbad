// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography;
using System.Text;
using Parbad.Utilities;

namespace Parbad.Gateway.Pasargad.Internal;

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
