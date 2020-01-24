using Newtonsoft.Json;

namespace Parbad.Gateway.IdPay.Internal
{
    internal class IdPayRequestModel
    {
        [JsonProperty("order_id")]
        public long OrderId { get; set; }

        public long Amount { get; set; }

        public string Callback { get; set; }
    }
}