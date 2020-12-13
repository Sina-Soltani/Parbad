// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Parbad.Gateway.Mellat
{
    public class MellatCumulativeDynamicAccount
    {
        public MellatCumulativeDynamicAccount(long subServiceId, Money amount, long payerId)
        {
            if (subServiceId <= 0) throw new ArgumentOutOfRangeException(nameof(subServiceId), $"{nameof(subServiceId)} cannot be zero or a negative number.");
            if (payerId < 0) throw new ArgumentOutOfRangeException(nameof(payerId), $"{nameof(payerId)} cannot be a negative number");

            SubServiceId = subServiceId;
            Amount = amount;
            PayerId = payerId;
        }

        public long SubServiceId { get; }

        public Money Amount { get; }

        public long PayerId { get; }

        public override string ToString()
        {
            return PayerId == 0
                ? $"{SubServiceId},{(long)Amount},"
                : $"{SubServiceId},{(long)Amount},{PayerId}";
        }
    }
}