using Newtonsoft.Json;

namespace Parbad.Gateway.FanAva.Internal.Models
{
    internal class FanAvaRefundRequestModel
    {
        public FanAvaRequestModel.WSContextModel WSContext { get; set; }

        public string Token { get; set; }

        [JsonProperty("RefNum")]
        public string InvoiceNumber { get; set; }
    }
}
