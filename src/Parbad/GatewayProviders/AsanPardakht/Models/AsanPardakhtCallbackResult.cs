using Parbad.Internal;

namespace Parbad.GatewayProviders.AsanPardakht.Models
{
    internal class AsanPardakhtCallbackResult
    {
        public bool IsSucceed { get; set; }

        public string PayGateTranId { get; set; }

        public string Rrn { get; set; }

        public string LastFourDigitOfPAN { get; set; }

        public PaymentVerifyResult Result { get; set; }
    }
}
