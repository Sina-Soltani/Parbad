// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Parbad.Gateway.AsanPardakht
{
    public interface IAsanPardakhtCrypto
    {
        Task<string> Encrypt(string input, string key, string iv);

        Task<string> Decrypt(string input, string key, string iv);
    }
}
