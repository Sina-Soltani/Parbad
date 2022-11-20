// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Parbad.Gateway.ZarinPal.Internal
{
    public class ZarinPalRefundResultModel
    {
        public ZarinPalRefundResultModelData Data { get; set; }
    }

    public class ZarinPalRefundResultModelData
    {
        public int Code { get; set; }

        public string Iban { get; set; }

        [JsonProperty("ref_id")]
        public string RefId { get; set; }

        public string Message { get; set; }
    }
}
