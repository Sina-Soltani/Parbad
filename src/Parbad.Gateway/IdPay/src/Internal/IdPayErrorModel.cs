using Newtonsoft.Json;

namespace Parbad.Gateway.IdPay.Internal
{
    internal class IdPayErrorModel
    {
        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }

        [JsonProperty("error_message")]
        public string ErrorMessage { get; set; }

        public override string ToString()
        {
            return $"Error Code: {ErrorCode}, Error Message: {ErrorMessage}";
        }
    }
}