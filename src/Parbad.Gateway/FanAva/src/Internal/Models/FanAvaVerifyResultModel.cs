using Newtonsoft.Json;

namespace Parbad.Gateway.FanAva.Internal.Models
{
    internal class FanAvaVerifyResultModel
    {
        public string Result { get; set; }
        
        public string Amount { get; set; }
        
        [JsonProperty("RefNum")]
        public string InvoiceNumber { get; set; }
    }
}
