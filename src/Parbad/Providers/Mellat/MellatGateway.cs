using System;
using System.Threading.Tasks;
using Parbad.Configurations;
using Parbad.Core;
using Parbad.Utilities;

namespace Parbad.Providers.Mellat
{
    internal class MellatGateway : GatewayBase
    {
        public MellatGateway() : base(Gateway.Mellat.ToString())
        {
        }

        protected MellatGatewayConfiguration Configuration => ParbadConfiguration.Gateways.GetMellatGatewayConfiguration();

        public override RequestResult Request(Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var data = MellatHelper.CreateRequestData(invoice, Configuration);

            var response = WebHelper.SendXmlWebRequest(GetWebServiceUrl(), data);

            return MellatHelper.CreateRequestResult(response);
        }

        public override async Task<RequestResult> RequestAsync(Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var data = MellatHelper.CreateRequestData(invoice, Configuration);

            var response = await WebHelper.SendXmlWebRequestAsync(GetWebServiceUrl(), data);

            return MellatHelper.CreateRequestResult(response);
        }

        public override VerifyResult Verify(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            if (verifyPaymentContext == null) throw new ArgumentNullException(nameof(verifyPaymentContext));

            var callbackResult = MellatHelper.CrateCallbackResult(verifyPaymentContext);

            if (!callbackResult.IsSucceed)
            {
                return callbackResult.Result;
            }

            //  Verify

            var data = MellatHelper.CreateVerifyData(verifyPaymentContext, Configuration, callbackResult);

            var response = WebHelper.SendXmlWebRequest(GetWebServiceUrl(), data);

            //  Check the verifying request.
            var verifyResult = MellatHelper.CheckVerifyResult(response, callbackResult);

            if (!verifyResult.IsSucceed)
            {
                return verifyResult.Result;
            }

            //  Settle
            data = MellatHelper.CreateSettleData(Configuration, verifyPaymentContext, callbackResult);

            response = WebHelper.SendXmlWebRequest(GetWebServiceUrl(), data);

            return MellatHelper.CreateSettleResult(response, callbackResult);
        }

        public override async Task<VerifyResult> VerifyAsync(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            if (verifyPaymentContext == null) throw new ArgumentNullException(nameof(verifyPaymentContext));

            var callbackResult = MellatHelper.CrateCallbackResult(verifyPaymentContext);

            if (!callbackResult.IsSucceed)
            {
                return callbackResult.Result;
            }

            //  Verify

            var data = MellatHelper.CreateVerifyData(verifyPaymentContext, Configuration, callbackResult);

            var response = await WebHelper.SendXmlWebRequestAsync(GetWebServiceUrl(), data);

            //  Check the verifying request.
            var verifyResult = MellatHelper.CheckVerifyResult(response, callbackResult);

            if (!verifyResult.IsSucceed)
            {
                return verifyResult.Result;
            }

            //  Settle
            data = MellatHelper.CreateSettleData(Configuration, verifyPaymentContext, callbackResult);

            response = await WebHelper.SendXmlWebRequestAsync(GetWebServiceUrl(), data);

            return MellatHelper.CreateSettleResult(response, callbackResult);
        }

        public override RefundResult Refund(GatewayRefundPaymentContext refundPaymentContext)
        {
            if (refundPaymentContext == null) throw new ArgumentNullException(nameof(refundPaymentContext));

            var data = MellatHelper.CreateRefundData(Configuration, refundPaymentContext);

            var response = WebHelper.SendXmlWebRequest(GetWebServiceUrl(), data);

            return MellatHelper.CreateRefundResult(response, refundPaymentContext);
        }

        public override async Task<RefundResult> RefundAsync(GatewayRefundPaymentContext refundPaymentContext)
        {
            if (refundPaymentContext == null) throw new ArgumentNullException(nameof(refundPaymentContext));

            var data = MellatHelper.CreateRefundData(Configuration, refundPaymentContext);

            var response = await WebHelper.SendXmlWebRequestAsync(GetWebServiceUrl(), data);

            return MellatHelper.CreateRefundResult(response, refundPaymentContext);
        }

        private string GetWebServiceUrl()
        {
            return Configuration.IsTestModeEnabled ? MellatHelper.TestWebServiceUrl : MellatHelper.WebServiceUrl;
        }
    }
}