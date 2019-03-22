// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.Properties;

namespace Parbad
{
    public class RefundInvoice
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RefundInvoice"/>.
        /// </summary>
        /// <param name="trackingNumber"></param>
        /// <param name="amount"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public RefundInvoice(long trackingNumber, decimal amount = 0)
        {
            if (trackingNumber <= 0) throw new ArgumentOutOfRangeException(nameof(trackingNumber));
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), Resources.AmountCannotBeNegative);

            TrackingNumber = trackingNumber;
            Amount = amount;
        }

        public long TrackingNumber { get; }

        public decimal Amount { get; }
    }
}
