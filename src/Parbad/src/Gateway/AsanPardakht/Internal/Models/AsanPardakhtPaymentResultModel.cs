using System;
using System.Collections.Generic;
using System.Text;

namespace Parbad.Gateway.AsanPardakht.Internal.Models
{
    public class AsanPardakhtPaymentResultModel
    {
        public string CardNumber { get; set; }
        public string Rrn { get; set; }
        public string RefID { get; set; }
        public string Amount { get; set; }
        public long PayGateTranID { get; set; }
        public string SalesOrderID { get; set; }
        public string Hash { get; set; }
        public long ServiceTypeId { get; set; }
        public string ServiceStatusCode { get; set; }
        public DateTime PayGateTranDate { get; set; }
        public long PayGateTranDateEpoch { get; set; }
    }
}
