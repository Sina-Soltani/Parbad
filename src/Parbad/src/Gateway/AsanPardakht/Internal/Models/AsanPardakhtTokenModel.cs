// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Parbad.Gateway.AsanPardakht.Internal.Models
{
    internal class AsanPardakhtTokenModel
    {
        public long MerchantConfigurationId { get; set; }

        public int ServiceTypeId { get; set; }

        public long LocalInvoiceId { get; set; }

        public long AmountInRials { get; set; }

        public string LocalDate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AdditionalData { get; set; }

        [JsonProperty(PropertyName = "callbackURL")]
        public string CallbackURL { get; set; }

        public string PaymentId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? UseDefaultSharing { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<AsanPardakhtSettlementPortionModel> SettlementPortions { get; set; } = new();
    }
}
