namespace Parbad.Gateway.Tara.Internal.Models
{
    internal class TaraCallbackResult
    {
        public bool IsSucceed { get; set; }
        public string Token { get; set; }
        public string Result { get; set; }
        public string Description { get; set; }
        public string ChannelRefNumber { get; set; }
        public string AdditionalData { get; set; }
        public long OrderId { get; set; }
        public string Message { get; set; }
    }
}

