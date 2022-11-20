// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;

namespace Parbad.Gateway.IranKish.Internal
{
    internal class IranKishCrypto
    {
        public static string EncryptAuthenticationEnvelope(string data, string rsaPublicKey, out string iv)
        {
            using var aes = Aes.Create();

            aes.KeySize = 128;

            aes.GenerateKey();

            aes.GenerateIV();

            var keyAes = aes.Key;
            var ivAes = aes.IV;

            var dataBytes = HexStringToByteArray(data);

            var encrypted = EncryptStringToBytes_Aes(dataBytes, aes.Key, aes.IV);

            var hsaHash = new SHA256CryptoServiceProvider().ComputeHash(encrypted);

            var aesData = CombineArrays(keyAes, hsaHash);

            var rsaData = EncryptRsa(aesData, rsaPublicKey);

            iv = ByteArrayToHexString(ivAes);

            return ByteArrayToHexString(rsaData);
        }

        private static byte[] EncryptRsa(byte[] aesCodingResult, string publicKey)
        {
            var encryptEngine = new Pkcs1Encoding(new RsaEngine());

            using (var txtreader = new StringReader(publicKey))
            {
                var keyParameter = (AsymmetricKeyParameter)new PemReader(txtreader).ReadObject();

                encryptEngine.Init(true, keyParameter);
            }

            return encryptEngine.ProcessBlock(aesCodingResult, 0, aesCodingResult.Length);
        }

        private static byte[] CombineArrays(byte[] first, byte[] second)
        {
            var bytes = new byte[first.Length + second.Length];

            Array.Copy(first, 0, bytes, 0, first.Length);

            Array.Copy(second, 0, bytes, first.Length, second.Length);

            return bytes;
        }

        private static byte[] EncryptStringToBytes_Aes(byte[] plainText, byte[] key, byte[] iv)
        {
            using var aesAlg = new AesCryptoServiceProvider();

            aesAlg.KeySize = 128;
            aesAlg.Key = key;
            aesAlg.IV = iv;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            return encryptor.TransformFinalBlock(plainText, 0, plainText.Length);
        }

        private static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] iv)
        {
            // Check arguments.
            if (cipherText is not { Length: > 0 })
            {
                throw new ArgumentNullException(nameof(cipherText));
            }

            if (key is not { Length: > 0 })
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (iv is not { Length: > 0 })
            {
                throw new ArgumentNullException(nameof(iv));
            }

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using var aesAlg = Aes.Create();

            aesAlg.Key = key;
            aesAlg.IV = iv;

            // Create a decryptor to perform the stream transform.
            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for decryption.
            using (var msDecrypt = new MemoryStream(cipherText))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        // Read the decrypted bytes from the decrypting stream
                        // and place them in a string.
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }

            return plaintext;
        }

        private static byte[] HexStringToByteArray(string hexString)
            => Enumerable.Range(0, hexString.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(value: hexString.Substring(startIndex: x, length: 2), fromBase: 16))
                         .ToArray();

        private static string ByteArrayToHexString(byte[] bytes)
            => bytes.Select(t => t.ToString(format: "X2")).Aggregate((a, b) => $"{a}{b}");
    }
}
