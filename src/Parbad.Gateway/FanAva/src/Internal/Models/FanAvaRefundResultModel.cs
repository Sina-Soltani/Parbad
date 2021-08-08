using System.Text.Json.Serialization;

namespace Parbad.Gateway.FanAva.Internal.Models
{
    internal class FanAvaRefundResultModel
    {
        public string Result { get; set; }
        [JsonPropertyName("RefNum")]
        public string InvoiceNumber { get; set; }

    }
}