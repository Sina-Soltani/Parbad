using Newtonsoft.Json;

namespace Parbad.Gateway.FanAva.Internal.Models
{
    internal class FanAvaRefundResultModel
    {
        public string Result { get; set; }
        
        [JsonProperty("RefNum")]
        public string InvoiceNumber { get; set; }
    }
}
