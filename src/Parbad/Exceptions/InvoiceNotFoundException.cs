// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Parbad.Exceptions
{
    [Serializable]
    public class InvoiceNotFoundException : Exception
    {
        public InvoiceNotFoundException(string paymentToken) : base($"No invoice found with the token {paymentToken}")
        {
            PaymentToken = paymentToken;
        }

        public InvoiceNotFoundException(long trackingNumber) : base(
            $"No invoice found with the tracking number {trackingNumber}")
        {
            TrackingNumber = trackingNumber;
        }

        public long TrackingNumber { get; }

        public string PaymentToken { get; }
    }
}
