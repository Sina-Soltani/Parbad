using System;
using System.Net;
using System.Threading.Tasks;
using Parbad.Configurations;
using Parbad.Core;
using Parbad.Providers.Melli.Models;
using Parbad.Utilities;

namespace Parbad.Providers.Melli
{
    internal class MelliGateway : GatewayBase
    {
        protected MelliGatewayConfiguration Configuration => ParbadConfiguration.Gateways.GetMelliGatewayConfiguration();

        public MelliGateway() : base(Gateway.Melli.ToString())
        {
        }

        public override RequestResult Request(Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var data = MelliHelper.CreateRequestData(invoice, Configuration);

            var result = PostJson<MelliApiRequestResult>(MelliHelper.PaymentRequestUrl, data);

            return MelliHelper.CreateRequestResult(result);
        }

        public override async Task<RequestResult> RequestAsync(Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var data = MelliHelper.CreateRequestData(invoice, Configuration);

            var result = await PostJsonAsync<MelliApiRequestResult>(MelliHelper.PaymentRequestUrl, data);

            return MelliHelper.CreateRequestResult(result);
        }

        public override VerifyResult Verify(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            if (verifyPaymentContext == null) throw new ArgumentNullException(nameof(verifyPaymentContext));

            //  Check callback

            var callbackResult = MelliHelper.CreateCallbackResult(verifyPaymentContext, Configuration);

            if (!callbackResult.IsSucceed)
            {
                return callbackResult.Result;
            }

            //  Verify

            var result = PostJson<MelliApiVerifyResult>(MelliHelper.PaymentVerifyUrl, callbackResult.JsonDataToVerify);

            return MelliHelper.CreateVerifyResult(callbackResult.Token, result);
        }

        public override async Task<VerifyResult> VerifyAsync(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            if (verifyPaymentContext == null) throw new ArgumentNullException(nameof(verifyPaymentContext));

            //  Check callback

            var data = MelliHelper.CreateCallbackResult(verifyPaymentContext, Configuration);

            if (!data.IsSucceed)
            {
                return data.Result;
            }

            //  Verify

            var result = await PostJsonAsync<MelliApiVerifyResult>(MelliHelper.PaymentVerifyUrl, data.JsonDataToVerify);

            return MelliHelper.CreateVerifyResult(data.Token, result);
        }

        public override RefundResult Refund(GatewayRefundPaymentContext refundPaymentContext)
        {
            return new RefundResult(Gateway.Melli, 0, RefundResultStatus.Failed, "Gateway Melli does not have Refund operation.");
        }

        public override Task<RefundResult> RefundAsync(GatewayRefundPaymentContext refundPaymentContext)
        {
            return Task.FromResult(Refund(refundPaymentContext));
        }

        private static T PostJson<T>(string url, object data)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;

            return WebHelper.SendAsJson<T>(url, data, "POST");
        }

        private static Task<T> PostJsonAsync<T>(string url, object data)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;

            return WebHelper.SendAsJsonAsync<T>(url, data, "POST");
        }
    }
}
