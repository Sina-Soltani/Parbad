using System;
using System.Threading.Tasks;
using Parbad.Utilities;

namespace Parbad.Core
{
    internal abstract class GatewayBase
    {
        protected GatewayBase(string name)
        {
            if (name.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
        }

        public string Name { get; }

        public abstract RequestResult Request(Invoice invoice);
        public abstract Task<RequestResult> RequestAsync(Invoice invoice);

        public abstract VerifyResult Verify(GatewayVerifyPaymentContext verifyPaymentContext);
        public abstract Task<VerifyResult> VerifyAsync(GatewayVerifyPaymentContext verifyPaymentContext);

        public abstract RefundResult Refund(GatewayRefundPaymentContext refundPaymentContext);
        public abstract Task<RefundResult> RefundAsync(GatewayRefundPaymentContext refundPaymentContext);
    }
}