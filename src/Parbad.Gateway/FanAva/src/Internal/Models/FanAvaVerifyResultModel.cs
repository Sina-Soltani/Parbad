using System.Text.Json.Serialization;

namespace Parbad.Gateway.FanAva.Internal.Models
{
    internal class FanAvaVerifyResultModel
    {
        public string Result { get; set; }
        public string Amount { get; set; }
        [JsonPropertyName("RefNum")]
        public string InvoiceNumber { get; set; }

    }
}