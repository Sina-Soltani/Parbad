using System.Text.Json.Serialization;

namespace Parbad.Gateway.FanAva.Internal.Models
{
    internal class FanAvaRequestModel
    {
        public WSContextModel WSContext { get; set; }
        public string TransType { get; set; }
        public string ReserveNum { get; set; }
        public Money Amount { get; set; }
        public string RedirectUrl { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string UserId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string MobileNo { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Email { get; set; }


        internal class WSContextModel
        {
            public string UserId { get; set; }
            public string Password { get; set; }

        }
    }




}