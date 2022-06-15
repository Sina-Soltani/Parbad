using System;
using System.Collections.Generic;
using System.Text;

namespace Parbad.Gateway.AsanPardakht.Internal.Models
{
    public class AsanPardakhtRequestAdditionalData
    {
        public string MobileNumber { get; set; }

        public string PaymentId { get; set; }

        public string AdditionalData { get; set; }

        public List<AsanPardakhtSettlementPortionModel> SettlementPortions { get; set; } = new();
    }
}
