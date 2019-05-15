using Parbad.Internal;

namespace Parbad.GatewayProviders.AsanPardakht
{
    internal class AsanPardakhtVerifyResult
    {
        public bool IsSucceed { get; internal set; }
        public PaymentVerifyResult Result { get; internal set; }
    }
}
