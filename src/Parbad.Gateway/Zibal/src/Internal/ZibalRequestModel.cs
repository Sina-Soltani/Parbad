using Newtonsoft.Json;

namespace Parbad.Gateway.Zibal.Internal
{
    internal class ZibalRequestModel:ZibalRequest
    {
        /// <summary>
        /// Rial (ريال)
        /// </summary>
        public long Amount { get; set; }
        public string? Merchant { get; set; }
        [JsonProperty("callbackUrl")]
        public string CallBackUrl { get; set; }

        public string OrderId { get; set; }
    }
}