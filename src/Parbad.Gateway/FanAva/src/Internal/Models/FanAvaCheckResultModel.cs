using System.Text.Json.Serialization;
using Parbad.Internal;

namespace Parbad.Gateway.FanAva.Internal.Models
{
    internal class FanAvaCheckResultModel
    {

        [JsonIgnore]
        public bool IsSucceed { get; set; }
        [JsonIgnore]
        public IPaymentVerifyResult VerifyResult { get; set; }



        [JsonPropertyName("Result")]
        public string Result { get; set; }
        [JsonPropertyName("ReferenceNum")]
        public string InvoiceNumber { get; set; }
        [JsonPropertyName("Amount")]
        public Money Amount { get; set; }
    }
}