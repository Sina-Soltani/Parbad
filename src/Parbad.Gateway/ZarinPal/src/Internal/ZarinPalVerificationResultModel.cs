// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Parbad.Gateway.ZarinPal.Internal
{
    internal class ZarinPalVerificationResultModel
    {
        public int Code { get; set; }

        [JsonProperty("ref_id")]
        public string RefId { get; set; }
        
        [JsonProperty("card_hash")]
        public string CardHash { get; set; }

        [JsonProperty("card_pan")]
        public string CardPan { get; set; }
    }
}
