using Newtonsoft.Json;

namespace Parbad.Gateway.FanAva.Internal.Models
{
    internal class FanAvaCheckResultModel
    {
        [JsonIgnore]
        public bool IsSucceed { get; set; }

        [JsonIgnore]
        public IPaymentVerifyResult VerifyResult { get; set; }

        [JsonProperty("Result")]
        public string Result { get; set; }

        [JsonProperty("ReferenceNum")]
        public string InvoiceNumber { get; set; }

        [JsonProperty("Amount")]
        public decimal Amount { get; set; }
    }
}
