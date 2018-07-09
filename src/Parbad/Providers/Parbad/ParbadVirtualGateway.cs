using System.Threading.Tasks;
using System.Web.Hosting;
using Parbad.Configurations;
using Parbad.Core;
using Parbad.Infrastructure.Translating;
using Parbad.Providers.Parbad.ResultTranslators;
using Parbad.Utilities;

namespace Parbad.Providers.Parbad
{
    internal class ParbadVirtualGateway : GatewayBase
    {
        protected ParbadVirtualGatewayConfiguration GatewayConfiguration => ParbadConfiguration.Gateways.GetParbadVirtualGatewayConfiguration();

        public ParbadVirtualGateway() : base(Gateway.ParbadVirtualGateway.ToString())
        {
        }

        public override RequestResult Request(Invoice invoice)
        {
            var paymentHttpPostForm = CreateRequestHttpPostForm(
                paymentPageUrl: GetPaymentPageUrl(),
                orderNumber: invoice.OrderNumber,
                amount: invoice.Amount,
                redirectUrl: invoice.CallbackUrl);

            return new RequestResult(RequestResultStatus.Success, string.Empty, invoice.OrderNumber.ToString())
            {
                BehaviorMode = GatewayRequestBehaviorMode.Post,
                Content = paymentHttpPostForm
            };
        }

        public override Task<RequestResult> RequestAsync(Invoice invoice)
        {
            return Task.FromResult(Request(invoice));
        }

        public override VerifyResult Verify(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            var result = verifyPaymentContext.RequestParameters.GetAs<string>("Result");

            if (string.IsNullOrWhiteSpace(result))
            {
                return new VerifyResult(Gateway.ParbadVirtualGateway, string.Empty, string.Empty, VerifyResultStatus.Failed, "Invalid data is received from the gateway");
            }

            var referenceId = verifyPaymentContext.RequestParameters.GetAs<string>("ReferenceId");

            var transactionId = verifyPaymentContext.RequestParameters.GetAs<string>("TransactionId");

            IGatewayResultTranslator gatewayResultTranslator = new ParbadVirtualGatewayResultTranslator();

            var translatedMessage = gatewayResultTranslator.Translate(result);

            var isSuccess = result == "true";

            var status = isSuccess ? VerifyResultStatus.Success : VerifyResultStatus.Failed;

            return new VerifyResult(Gateway.ParbadVirtualGateway, referenceId, transactionId, status, translatedMessage);
        }

        public override Task<VerifyResult> VerifyAsync(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            return Task.FromResult(Verify(verifyPaymentContext));
        }

        public override RefundResult Refund(GatewayRefundPaymentContext refundPaymentContext)
        {
            return new RefundResult(Gateway.ParbadVirtualGateway, refundPaymentContext.Amount, RefundResultStatus.Success, "Transaction refund successfully.");
        }

        public override Task<RefundResult> RefundAsync(GatewayRefundPaymentContext refundPaymentContext)
        {
            return Task.FromResult(Refund(refundPaymentContext));
        }

        private string GetPaymentPageUrl()
        {
            return HostingEnvironment.ApplicationVirtualPath.ToggleStringAtEnd("/", true) + GatewayConfiguration.GatewayHandlerPath;
        }

        private static string CreateRequestHttpPostForm(string paymentPageUrl, long orderNumber, Money amount, string redirectUrl)
        {
            return
                "<html>" +
                "<body>" +
                $"<form id=\"paymentForm\" action=\"{paymentPageUrl}\" method=\"post\" />" +
                $"<input type=\"hidden\" name=\"commandType\" value=\"Request\" />" +
                $"<input type=\"hidden\" name=\"orderNumber\" value=\"{orderNumber}\" />" +
                $"<input type=\"hidden\" name=\"amount\" value=\"{amount}\" />" +
                $"<input type=\"hidden\" name=\"redirectUrl\" value=\"{redirectUrl}\" />" +
                "</form>" +
                "<script type=\"text/javascript\">" +
                "document.getElementById('paymentForm').submit();" +
                "</script>" +
                "</body>" +
                "</html>";
        }
    }
}