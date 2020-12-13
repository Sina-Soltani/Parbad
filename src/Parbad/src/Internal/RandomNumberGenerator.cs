// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography;

namespace Parbad.Internal
{
    internal static class RandomNumberGenerator
    {
        public static long Next()
        {
            using var provider = new RNGCryptoServiceProvider();

            var data = new byte[8];

            provider.GetBytes(data);

            var randomNumber = BitConverter.ToInt64(data, 0);

            return Math.Abs(randomNumber);
        }
    }
}
