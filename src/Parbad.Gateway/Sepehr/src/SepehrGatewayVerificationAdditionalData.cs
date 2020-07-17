// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Sepehr
{
    /// <summary>
    /// Contains some additional data after the invoice verification.
    /// </summary>
    public class SepehrGatewayVerificationAdditionalData
    {
        public long TraceNumber { get; set; }

        public long Rrn { get; set; }

        public string DigitalReceipt { get; set; }

        public string CardNumber { get; set; }
    }
}