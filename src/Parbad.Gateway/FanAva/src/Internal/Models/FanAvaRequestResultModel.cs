using System.Text.Json.Serialization;

namespace Parbad.Gateway.FanAva.Internal.Models
{
    internal class FanAvaRequestResultModel
    {
        public string Result { get; set; }
        public string ExpirationDate { get; set; }
        public string Token { get; set; }
        public string ChannelId { get; set; }
        public string UserId { get; set; }
    }
}