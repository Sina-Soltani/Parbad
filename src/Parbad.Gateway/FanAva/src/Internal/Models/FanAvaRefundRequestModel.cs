// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Parbad.Gateway.FanAva.Internal.Models
{
    internal class FanAvaRefundRequestModel
    {
        public FanAvaRequestModel.WSContextModel WSContext { get; set; }

        public string Token { get; set; }

        [JsonProperty("RefNum")]
        public string InvoiceNumber { get; set; }
    }
}
