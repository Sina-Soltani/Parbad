// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Parbad.Gateway.ZarinPal.Internal
{
    internal class ZarinPalRequestModel
    {
        [JsonProperty("merchant_id")]
        public string MerchantId { get; set; }

        [JsonProperty("callback_url")]
        public string CallbackUrl { get; set; }

        public string Description { get; set; }

        public long Amount { get; set; }

        public string Mobile { get; set; }

        public string Email { get; set; }
    }
}
