using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Parbad.Gateway.AsanPardakht.Internal.Models
{
    public class AsanPardakhtRequestToken
    {
        public long MerchantConfigurationId { get; set; }
        public int ServiceTypeId { get; set; } = 1;
        public long LocalInvoiceId { get; set; }
        public long AmountInRials { get; set; }
        public string CallbackURL { get; set; }
        public string AdditionalData { get; set; }
        public string LocalDate { get; set; }
        public string PaymentId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<AsanPardakhtSettlementPortionModel> SettlementPortions { get; set; }
    }
}
