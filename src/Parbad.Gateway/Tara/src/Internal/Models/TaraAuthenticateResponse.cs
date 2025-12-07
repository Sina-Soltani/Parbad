namespace Parbad.Gateway.Tara.Internal.Models
{
    internal class TaraAuthenticateResponse
    {
        public string accessToken { get; set; }
        public string result { get; set; }
        public string description { get; set; }
        public string doTime { get; set; }
        public long expireTime { get; set; }
    }
}

