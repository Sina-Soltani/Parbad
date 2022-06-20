using System;
using System.Collections.Generic;
using System.Text;

namespace Parbad.Gateway.AsanPardakht.Internal.Models
{
    internal class AsanPardakhtPaymentCompletionModel
    {
        public long MerchantConfigurationId { get; set; }

        public long PayGateTranId { get; set; }
    }
}
