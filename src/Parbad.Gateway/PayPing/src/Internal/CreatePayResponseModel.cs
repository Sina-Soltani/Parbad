// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Parbad.Gateway.PayPing.Internal;

/// <summary>The answer of POST /v3/pay.</summary>
internal class CreatePayResponseModel
{
    [JsonProperty("paymentCode")]
    public string? PaymentCode { get; set; }

    [JsonProperty("url")]
    public string? Url { get; set; }
}