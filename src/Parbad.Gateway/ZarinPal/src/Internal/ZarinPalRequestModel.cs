// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Parbad.Gateway.ZarinPal.Internal;

internal class ZarinPalRequestModel
{
    [JsonProperty("merchant_id")]
    public string MerchantId { get; set; }

    [JsonProperty("callback_url")]
    public string CallbackUrl { get; set; }

    public string Description { get; set; }

    public long Amount { get; set; }

    public ZarinPalRequestMetadata Metadata { get; set; }
}

/// <summary>
/// The optional details of a request; ZarinPal shows them in its panel. Empty values are not sent.
/// </summary>
internal class ZarinPalRequestMetadata
{
    [JsonProperty("mobile", NullValueHandling = NullValueHandling.Ignore)]
    public string Mobile { get; set; }

    [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
    public string Email { get; set; }

    [JsonProperty("order_id", NullValueHandling = NullValueHandling.Ignore)]
    public string OrderId { get; set; }
}
