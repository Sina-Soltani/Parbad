using System;
using System.Collections.Generic;
using System.Text;

namespace Parbad.Gateway.AsanPardakht.Internal.Models
{
    public class AsanPardakhtSettlementPortionModel
    {
        public string Iban { get; set; }

        public long AmountInRials { get; set; }

        public string PaymentId { get; set; }
    }
}
