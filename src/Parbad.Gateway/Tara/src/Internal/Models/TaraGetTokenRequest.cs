using System.Collections.Generic;

namespace Parbad.Gateway.Tara.Internal.Models
{
    internal class TaraGetTokenRequest
    {
        public string ip { get; set; }
        public List<TaraServiceAmount> serviceAmountList { get; set; }
        public List<TaraInvoiceItem> taraInvoiceItemList { get; set; }
        public string additionalData { get; set; }
        public string callBackUrl { get; set; }
        public string amount { get; set; }
        public string mobile { get; set; }
        public long orderId { get; set; }
        public long vat { get; set; }
    }
}

