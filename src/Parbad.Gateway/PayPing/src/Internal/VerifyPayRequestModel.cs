// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Parbad.Gateway.PayPing.Internal;

/// <summary>The body of POST /v3/pay/verify.</summary>
internal class PayPingVerifyRequestModel
{
    [JsonProperty("paymentRefId")]
    public long PaymentRefId { get; set; }

    [JsonProperty("paymentCode")]
    public string? PaymentCode { get; set; }

    [JsonProperty("amount")]
    public long Amount { get; set; }
}