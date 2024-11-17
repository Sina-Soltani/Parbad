// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Pasargad;

/// <summary>
/// An Encryptor to sign data which will be sent by each request to Pasargad gateway.
/// </summary>
public interface IPasargadCrypto
{
    /// <summary>
    /// Encrypts the given data using the provided Private Key.
    /// </summary>
    string Encrypt(string privateKey, string data);
}
