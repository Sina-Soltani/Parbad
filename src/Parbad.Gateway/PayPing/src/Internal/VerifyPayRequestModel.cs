// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Parbad.Gateway.PayPing.Internal
{
    /// <summary>The body of the former PayPing API v2 verification.</summary>
    [Obsolete("The body of the PayPing API v2, which PayPing has replaced with v3. Not used by Parbad any more; it will be removed in a future version.")]
    public class VerifyPayRequestModel
    {
        public long Amount { get; set; }

        public string RefId { get; set; }
    }

    /// <summary>The body of POST /v3/pay/verify.</summary>
    internal class PayPingVerifyRequestModel
    {
        [JsonProperty("paymentRefId")]
        public long PaymentRefId { get; set; }

        [JsonProperty("paymentCode")]
        public string PaymentCode { get; set; }

        [JsonProperty("amount")]
        public long Amount { get; set; }
    }
}
