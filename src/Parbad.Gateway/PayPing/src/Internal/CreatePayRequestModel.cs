// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Parbad.Gateway.PayPing.Internal;

/// <summary>The body of POST /v3/pay.</summary>
internal class CreatePayRequestModel
{
    [JsonProperty("amount")]
    public long Amount { get; set; }

    [JsonProperty("returnUrl")]
    public string ReturnUrl { get; set; }

    [JsonProperty("clientRefId")]
    public string ClientRefId { get; set; }

    [JsonProperty("payerIdentity", NullValueHandling = NullValueHandling.Ignore)]
    public string PayerIdentity { get; set; }

    [JsonProperty("payerName", NullValueHandling = NullValueHandling.Ignore)]
    public string PayerName { get; set; }

    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }
}