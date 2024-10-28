// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Parbad.Gateway.ZarinPal.Internal;

internal class ZarinPalVerificationModel
{
    public long Amount { set; get; }

    [JsonProperty("merchant_id")]
    public string MerchantId { set; get; }

    public string Authority { set; get; }
}
