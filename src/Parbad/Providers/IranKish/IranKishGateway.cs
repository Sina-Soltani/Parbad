using System;
using System.Threading.Tasks;
using Parbad.Configurations;
using Parbad.Core;
using Parbad.Utilities;

namespace Parbad.Providers.IranKish
{
    /// <summary>
    /// IranKish gateway. (Warning: It's not tested yet)
    /// </summary>
    internal class IranKishGateway : GatewayBase
    {
        public IranKishGateway() : base(Gateway.IranKish.ToString())
        {
        }

        protected IranKishGatewayConfiguration Configuration => ParbadConfiguration.Gateways.GetIranKishGatewayConfiguration();

        public override RequestResult Request(Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var data = IranKishHelper.CreateRequestData(invoice, Configuration);

            var response = WebHelper.SendXmlWebRequest(IranKishHelper.TokenWebServiceUrl, data, IranKishHelper.HttpRequestHeader);

            return IranKishHelper.CreateRequestResult(response, invoice, Configuration);
        }

        public override async Task<RequestResult> RequestAsync(Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var data = IranKishHelper.CreateRequestData(invoice, Configuration);

            var response = await WebHelper.SendXmlWebRequestAsync(IranKishHelper.TokenWebServiceUrl, data, IranKishHelper.HttpRequestHeader);

            return IranKishHelper.CreateRequestResult(response, invoice, Configuration);
        }

        public override VerifyResult Verify(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            if (verifyPaymentContext == null) throw new ArgumentNullException(nameof(verifyPaymentContext));

            var callbackResult = IranKishHelper.CreateCallbackResult(verifyPaymentContext, Configuration);

            //  Verify

            var data = IranKishHelper.CreateVerifyData(verifyPaymentContext, callbackResult, Configuration);

            var response = WebHelper.SendXmlWebRequest(IranKishHelper.VerifyWebServiceUrl, data, IranKishHelper.HttpVerifyHeaders);

            return IranKishHelper.CreateVerifyResult(response, verifyPaymentContext, callbackResult);
        }

        public override async Task<VerifyResult> VerifyAsync(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            if (verifyPaymentContext == null) throw new ArgumentNullException(nameof(verifyPaymentContext));

            var callbackResult = IranKishHelper.CreateCallbackResult(verifyPaymentContext, Configuration);

            //  Verify

            var data = IranKishHelper.CreateVerifyData(verifyPaymentContext, callbackResult, Configuration);

            var response = await WebHelper.SendXmlWebRequestAsync(IranKishHelper.VerifyWebServiceUrl, data, IranKishHelper.HttpVerifyHeaders);

            return IranKishHelper.CreateVerifyResult(response, verifyPaymentContext, callbackResult);
        }

        public override RefundResult Refund(GatewayRefundPaymentContext refundPaymentContext)
        {
            return new RefundResult(Gateway.IranKish, 0, RefundResultStatus.Failed, "Gateway IranKish does not have Refund operation.");
        }

        public override Task<RefundResult> RefundAsync(GatewayRefundPaymentContext refundPaymentContext)
        {
            return Task.FromResult(Refund(refundPaymentContext));
        }
    }
}
