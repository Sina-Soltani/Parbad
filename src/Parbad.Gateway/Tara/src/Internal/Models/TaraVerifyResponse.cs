using System.Collections.Generic;

namespace Parbad.Gateway.Tara.Internal.Models
{
    internal class TaraVerifyResponse
    {
        public string token { get; set; }
        public string result { get; set; }
        public string description { get; set; }
        public string doTime { get; set; }
        public List<TaraServiceAmount> serviceAmountList { get; set; }
        public string amount { get; set; }
        public string rrn { get; set; }
        public string type { get; set; }
    }
}

