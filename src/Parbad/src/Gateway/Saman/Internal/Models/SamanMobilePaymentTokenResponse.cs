namespace Parbad.Gateway.Saman.Internal.Models
{
    internal class SamanMobilePaymentTokenResponse
    {
        public int Status { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorDesc { get; set; }

        public string Token { get; set; }

        public string GetError()
        {
            return $"{nameof(ErrorCode)}: {ErrorCode}, {nameof(ErrorDesc)}: {ErrorDesc}";
        }
    }
}
