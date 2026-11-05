// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Parbad.Gateway.AsanPardakht;

/// <summary>
/// The remote encryption service of AsanPardakht's former SOAP API.
/// </summary>
[Obsolete("Not used: since Parbad 3.12 the AsanPardakht gateway uses AsanPardakht's REST API, which needs no encryption. This interface will be removed in Parbad 4.0.")]
public interface IAsanPardakhtCrypto
{
    Task<string> Encrypt(string input, string key, string iv);

    Task<string> Decrypt(string input, string key, string iv);
}