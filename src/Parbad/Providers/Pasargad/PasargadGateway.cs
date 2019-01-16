using System;
using System.Threading.Tasks;
using Parbad.Configurations;
using Parbad.Core;
using Parbad.Utilities;

namespace Parbad.Providers.Pasargad
{
    internal class PasargadGateway : GatewayBase
    {
        protected PasargadGatewayConfiguration PasargadConfiguration => ParbadConfiguration.Gateways.GetPasargadGatewayConfiguration();

        public PasargadGateway() : base(Gateway.Pasargad.ToString())
        {
        }

        public override RequestResult Request(Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            return PasargadHelper.CreateRequestResult(PasargadConfiguration, invoice);
        }

        public override Task<RequestResult> RequestAsync(Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            return Task.FromResult(Request(invoice));
        }

        public override VerifyResult Verify(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            if (verifyPaymentContext == null) throw new ArgumentNullException(nameof(verifyPaymentContext));

            //  Process the received data

            var callbackResult = PasargadHelper.CreateCallbackResult(verifyPaymentContext);

            if (!callbackResult.IsSucceed)
            {
                return callbackResult.Result;
            }

            //  Check and compare the received data

            var response = WebHelper.SendWebRequest(PasargadHelper.CheckPaymentPageUrl, callbackResult.CallbackCheckData, "POST", "application/x-www-form-urlencoded");

            var checkCallbackResult = PasargadHelper.CreateCheckCallbackResult(response, PasargadConfiguration, callbackResult);

            if (!checkCallbackResult.IsSucceed)
            {
                return checkCallbackResult.Result;
            }

            //  Verify

            var data = PasargadHelper.CreateVerifyData(PasargadConfiguration, verifyPaymentContext, callbackResult);

            response = WebHelper.SendWebRequest(PasargadHelper.VerifyPaymentPageUrl, data, "POST", "application/x-www-form-urlencoded");

            return PasargadHelper.CreateVerifyResult(response, callbackResult);
        }

        public override async Task<VerifyResult> VerifyAsync(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            if (verifyPaymentContext == null) throw new ArgumentNullException(nameof(verifyPaymentContext));

            //  Process the received data

            var callbackResult = PasargadHelper.CreateCallbackResult(verifyPaymentContext);

            if (!callbackResult.IsSucceed)
            {
                return callbackResult.Result;
            }

            //  Check and compare the received data

            var response = await WebHelper.SendWebRequestAsync(PasargadHelper.CheckPaymentPageUrl, callbackResult.CallbackCheckData, "POST", "application/x-www-form-urlencoded");

            var checkCallbackResult = PasargadHelper.CreateCheckCallbackResult(response, PasargadConfiguration, callbackResult);

            if (!checkCallbackResult.IsSucceed)
            {
                return checkCallbackResult.Result;
            }

            //  Verify

            var data = PasargadHelper.CreateVerifyData(PasargadConfiguration, verifyPaymentContext, callbackResult);

            response = await WebHelper.SendWebRequestAsync(PasargadHelper.VerifyPaymentPageUrl, data, "POST", "application/x-www-form-urlencoded");

            return PasargadHelper.CreateVerifyResult(response, callbackResult);
        }

        public override RefundResult Refund(GatewayRefundPaymentContext refundPaymentContext)
        {
            if (refundPaymentContext == null) throw new ArgumentNullException(nameof(refundPaymentContext));

            var data = PasargadHelper.CreateRefundData(refundPaymentContext, PasargadConfiguration);

            var response = WebHelper.SendWebRequest(PasargadHelper.RefundPaymentPageUrl, data, "POST", "application/x-www-form-urlencoded");

            return PasargadHelper.CreateRefundResult(response, refundPaymentContext);
        }

        public override async Task<RefundResult> RefundAsync(GatewayRefundPaymentContext refundPaymentContext)
        {
            if (refundPaymentContext == null) throw new ArgumentNullException(nameof(refundPaymentContext));

            var data = PasargadHelper.CreateRefundData(refundPaymentContext, PasargadConfiguration);

            var response = await WebHelper.SendWebRequestAsync(PasargadHelper.RefundPaymentPageUrl, data, "POST", "application/x-www-form-urlencoded");

            return PasargadHelper.CreateRefundResult(response, refundPaymentContext);
        }
    }
}
