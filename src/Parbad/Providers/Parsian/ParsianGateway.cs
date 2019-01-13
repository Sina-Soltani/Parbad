using System;
using System.Threading.Tasks;
using Parbad.Configurations;
using Parbad.Core;
using Parbad.Utilities;

namespace Parbad.Providers.Parsian
{
    internal class ParsianGateway : GatewayBase
    {
        protected ParsianGatewayConfiguration ParsianConfiguration => ParbadConfiguration.Gateways.GetParsianGatewayConfiguration();

        public ParsianGateway() : base(Gateway.Parsian.ToString())
        {
        }

        public override RequestResult Request(Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var data = ParsianHelper.CreateRequestData(ParsianConfiguration, invoice);

            var response = WebHelper.SendXmlWebRequest(ParsianHelper.WebServiceUrl, data);

            return ParsianHelper.CreateRequestResult(response);
        }

        public override async Task<RequestResult> RequestAsync(Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var data = ParsianHelper.CreateRequestData(ParsianConfiguration, invoice);

            var response = await WebHelper.SendXmlWebRequestAsync(ParsianHelper.WebServiceUrl, data);

            return ParsianHelper.CreateRequestResult(response);
        }

        public override VerifyResult Verify(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            if (verifyPaymentContext == null) throw new ArgumentNullException(nameof(verifyPaymentContext));

            var callbackResult = ParsianHelper.CreateCallbackResult(verifyPaymentContext);

            if (!callbackResult.IsSucceed)
            {
                return callbackResult.Result;
            }

            //  Verify

            var data = ParsianHelper.CreateVerifyData(ParsianConfiguration, callbackResult);

            var response = WebHelper.SendXmlWebRequest(ParsianHelper.WebServiceUrl, data);

            return ParsianHelper.CreateVerifyResult(response, callbackResult);
        }

        public override async Task<VerifyResult> VerifyAsync(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            if (verifyPaymentContext == null) throw new ArgumentNullException(nameof(verifyPaymentContext));

            var callbackResult = ParsianHelper.CreateCallbackResult(verifyPaymentContext);

            if (!callbackResult.IsSucceed)
            {
                return callbackResult.Result;
            }

            //  Verify

            var data = ParsianHelper.CreateVerifyData(ParsianConfiguration, callbackResult);

            var response = await WebHelper.SendXmlWebRequestAsync(ParsianHelper.WebServiceUrl, data);

            return ParsianHelper.CreateVerifyResult(response, callbackResult);
        }

        public override RefundResult Refund(GatewayRefundPaymentContext refundPaymentContext)
        {
            if (refundPaymentContext == null) throw new ArgumentNullException(nameof(refundPaymentContext));

            var data = ParsianHelper.CreateRefundData(ParsianConfiguration, refundPaymentContext);

            var response = WebHelper.SendXmlWebRequest(ParsianHelper.WebServiceUrl, data);

            return ParsianHelper.CreateRefundResult(response, refundPaymentContext);
        }

        public override async Task<RefundResult> RefundAsync(GatewayRefundPaymentContext refundPaymentContext)
        {
            if (refundPaymentContext == null) throw new ArgumentNullException(nameof(refundPaymentContext));

            var data = ParsianHelper.CreateRefundData(ParsianConfiguration, refundPaymentContext);

            var response = await WebHelper.SendXmlWebRequestAsync(ParsianHelper.WebServiceUrl, data);

            return ParsianHelper.CreateRefundResult(response, refundPaymentContext);
        }
    }
}