// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Parbad.Gateway.PayPing.Internal
{
    /// <summary>
    /// The answer of POST /v3/pay/verify. An "already verified" answer carries the same fields in PascalCase, which
    /// Newtonsoft.Json matches case-insensitively.
    /// </summary>
    internal class VerifyPayResponseModel
    {
        [JsonProperty("amount")]
        public long? Amount { get; set; }

        [JsonProperty("clientRefId")]
        public string ClientRefId { get; set; }

        [JsonProperty("paymentRefId")]
        public long? PaymentRefId { get; set; }

        /// <summary>The payment code.</summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("cardNumber")]
        public string CardNumber { get; set; }

        [JsonProperty("cardHashPan")]
        public string CardHashPan { get; set; }
    }
}
