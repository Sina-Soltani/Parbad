using System;
using System.Threading.Tasks;
using Parbad.Configurations;
using Parbad.Core;
using Parbad.Infrastructure.Translating;
using Parbad.Providers.Saman.ResultTranslators;
using Parbad.Utilities;

namespace Parbad.Providers.Saman
{
    internal class SamanGateway : GatewayBase
    {
        protected SamanGatewayConfiguration SamanConfiguration => ParbadConfiguration.Gateways.GetSamanGatewayConfiguration();

        public SamanGateway() : base(Gateway.Saman.ToString())
        {
        }

        public override RequestResult Request(Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            return SamanHelper.CreateRequestResult(invoice, SamanConfiguration);
        }

        public override Task<RequestResult> RequestAsync(Invoice invoice)
        {
            return Task.FromResult(Request(invoice));
        }

        public override VerifyResult Verify(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            if (verifyPaymentContext == null) throw new ArgumentNullException(nameof(verifyPaymentContext));

            var callbackResult = SamanHelper.CreateCallbackResult(verifyPaymentContext);

            if (!callbackResult.IsSucceed)
            {
                return callbackResult.Result;
            }

            var data = SamanHelper.CreateVerifyData(callbackResult, SamanConfiguration);

            var response = WebHelper.SendXmlWebRequest(SamanHelper.WebServiceUrl, data);

            return SamanHelper.CreateVerifyResult(response, verifyPaymentContext, callbackResult);
        }

        public override async Task<VerifyResult> VerifyAsync(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            if (verifyPaymentContext == null) throw new ArgumentNullException(nameof(verifyPaymentContext));

            var callbackResult = SamanHelper.CreateCallbackResult(verifyPaymentContext);

            if (!callbackResult.IsSucceed)
            {
                return callbackResult.Result;
            }

            var data = SamanHelper.CreateVerifyData(callbackResult, SamanConfiguration);

            var response = await WebHelper.SendXmlWebRequestAsync(SamanHelper.WebServiceUrl, data);

            return SamanHelper.CreateVerifyResult(response, verifyPaymentContext, callbackResult);
        }

        public override RefundResult Refund(GatewayRefundPaymentContext refundPaymentContext)
        {
            if (refundPaymentContext == null) throw new ArgumentNullException(nameof(refundPaymentContext));

            var data = SamanHelper.CreateRefundData(refundPaymentContext, SamanConfiguration);

            var response = WebHelper.SendXmlWebRequest(SamanHelper.WebServiceUrl, data);

            return SamanHelper.CreateRefundResult(response, refundPaymentContext);
        }

        public override async Task<RefundResult> RefundAsync(GatewayRefundPaymentContext refundPaymentContext)
        {
            if (refundPaymentContext == null) throw new ArgumentNullException(nameof(refundPaymentContext));

            var data = SamanHelper.CreateRefundData(refundPaymentContext, SamanConfiguration);

            var response = await WebHelper.SendXmlWebRequestAsync(SamanHelper.WebServiceUrl, data);

            return SamanHelper.CreateRefundResult(response, refundPaymentContext);
        }
    }
}
