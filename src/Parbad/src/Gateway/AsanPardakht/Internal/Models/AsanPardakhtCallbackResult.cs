namespace Parbad.Gateway.AsanPardakht.Internal.Models
{
    public class AsanPardakhtCallbackResult
    {
        public bool IsSucceed { get; set; }

        public string PayGateTranId { get; set; }

        public string Rrn { get; set; }

        public string LastFourDigitOfPAN { get; set; }

        public string Message { get; set; }
    }
}