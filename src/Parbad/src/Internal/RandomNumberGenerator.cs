// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using CryptoRandomNumberGenerator = System.Security.Cryptography.RandomNumberGenerator;

namespace Parbad.Internal
{
    internal static class RandomNumberGenerator
    {
        // Number of non-negative long values (2^63).
        private const ulong NonNegativeRange = (ulong)long.MaxValue + 1;

        /// <summary>
        /// Returns a cryptographically random number in the range [<paramref name="minimum"/>, <see cref="long.MaxValue"/>].
        /// </summary>
        /// <param name="minimum">The inclusive lower bound. It must not be negative.</param>
        public static long NextInt64(long minimum)
        {
            if (minimum < 0) throw new ArgumentOutOfRangeException(nameof(minimum), minimum, "The minimum must not be negative.");

            var count = (ulong)(long.MaxValue - minimum) + 1;

            // Rejection sampling: a plain modulo would favour the lowest values, because 2^63 is generally
            // not a multiple of count. Samples at or above the largest multiple of count are drawn again;
            // that happens with a probability below 50%, so fewer than two draws are needed on average.
            var limit = NonNegativeRange - NonNegativeRange % count;

            using var generator = CryptoRandomNumberGenerator.Create();

            var data = new byte[8];

            while (true)
            {
                generator.GetBytes(data);

                // Clearing the sign bit gives a uniform value in [0, 2^63) and, unlike Math.Abs, cannot overflow.
                var sample = BitConverter.ToUInt64(data, 0) & long.MaxValue;

                if (sample < limit)
                {
                    return minimum + (long)(sample % count);
                }
            }
        }
    }
}
