// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Parbad.Gateway.ZarinPal.Models;

/// <summary>
/// Contains the original verification result that is sent by ZarinPal gateway.
/// </summary>
public class ZarinPalOriginalVerificationResult
{
    public int Code { get; set; }

    [JsonProperty("ref_id")]
    public string RefId { get; set; }

    [JsonProperty("card_hash")]
    public string CardHash { get; set; }

    [JsonProperty("card_pan")]
    public string CardPan { get; set; }

    [JsonProperty("fee_type")]
    public string FeeType { get; set; }

    public string Fee { get; set; }
}
