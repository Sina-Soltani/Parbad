using Parbad.Internal;

namespace Parbad.Gateway.AsanPardakht.Internal.Models
{
    public class AsanPardakhtVerifyResult
    {
        public bool IsSucceed { get; internal set; }
        public PaymentVerifyResult Result { get; internal set; }
    }
}