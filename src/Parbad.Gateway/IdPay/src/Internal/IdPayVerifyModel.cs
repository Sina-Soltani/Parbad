using Newtonsoft.Json;

namespace Parbad.Gateway.IdPay.Internal
{
    internal class IdPayVerifyModel
    {
        public string Id { get; set; }

        [JsonProperty("order_id")]
        public long OrderId { get; set; }
    }
}