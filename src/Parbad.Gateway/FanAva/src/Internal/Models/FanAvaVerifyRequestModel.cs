using System.Text.Json.Serialization;

namespace Parbad.Gateway.FanAva.Internal.Models
{
    internal class FanAvaVerifyRequestModel
    {
        public FanAvaRequestModel.WSContextModel WSContext { get; set; }
        public string Token { get; set; }
        [JsonPropertyName("RefNum")]
        public string InvoiceNumber { get; set; }
        public Money Amount { get; set; }
    }




}