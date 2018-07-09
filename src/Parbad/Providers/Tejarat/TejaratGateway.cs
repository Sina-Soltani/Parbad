using System;
using System.Threading.Tasks;
using Parbad.Configurations;
using Parbad.Core;
using Parbad.Infrastructure.Translating;
using Parbad.Providers.Tejarat.ResultTranslators;
using Parbad.Utilities;

namespace Parbad.Providers.Tejarat
{
    internal class TejaratGateway : GatewayBase
    {
        private const string PaymentPageUrl = "https://ikc.shaparak.ir/Tpayment/Payment";
        private const string TokenServiceUrl = "https://ikc.shaparak.ir/TToken/Tokens.svc?wsdl";
        private const string VerifyServiceUrl = "https://ikc.shaparak.ir/TVerify/Verify.svc?wsdl";

        protected TejaratGatewayConfiguration TejaratConfiguration => ParbadConfiguration.Gateways.GetTejaratGatewayConfiguration();

        public TejaratGateway() : base(Gateway.Tejarat.ToString())
        {
        }

        public override RequestResult Request(Invoice invoice)
        {
            string webServiceXml = CreateRequestWebServiceXml(
                TejaratConfiguration.Merchant,
                amount: invoice.Amount,
                invoiceNo: invoice.OrderNumber,
                revertUrl: invoice.CallbackUrl);

            var webServiceResponse = WebHelper.SendXmlWebRequest(TokenServiceUrl, webServiceXml);

            //  true|false
            var result = Convert.ToBoolean(XmlHelper.GetNodeValueFromXml(webServiceResponse, "result", "http://schemas.datacontract.org/2004/07/Token"));

            var message = XmlHelper.GetNodeValueFromXml(webServiceResponse, "message", "http://schemas.datacontract.org/2004/07/Token");

            if (!result)
            {
                return new RequestResult(RequestResultStatus.Failed, message, string.Empty);
            }

            var token = XmlHelper.GetNodeValueFromXml(webServiceResponse, "token", "http://schemas.datacontract.org/2004/07/Token");

            var postHtmlForm = CreatePostHtmlForm(PaymentPageUrl, token, TejaratConfiguration.Merchant);

            return new RequestResult(RequestResultStatus.Success, string.Empty, invoice.OrderNumber.ToString())
            {
                BehaviorMode = GatewayRequestBehaviorMode.Post,
                Content = postHtmlForm
            };
        }

        public override async Task<RequestResult> RequestAsync(Invoice invoice)
        {
            string webServiceXml = CreateRequestWebServiceXml(
                TejaratConfiguration.Merchant,
                amount: invoice.Amount,
                invoiceNo: invoice.OrderNumber,
                revertUrl: invoice.CallbackUrl);

            var webServiceResponse = await WebHelper.SendXmlWebRequestAsync(TokenServiceUrl, webServiceXml);

            //  true|false
            var result = Convert.ToBoolean(XmlHelper.GetNodeValueFromXml(webServiceResponse, "result", "http://schemas.datacontract.org/2004/07/Token"));

            var message = XmlHelper.GetNodeValueFromXml(webServiceResponse, "message", "http://schemas.datacontract.org/2004/07/Token");

            if (!result)
            {
                return new RequestResult(RequestResultStatus.Failed, message, string.Empty);
            }

            var token = XmlHelper.GetNodeValueFromXml(webServiceResponse, "token", "http://schemas.datacontract.org/2004/07/Token");

            var postHtmlForm = CreatePostHtmlForm(PaymentPageUrl, token, TejaratConfiguration.Merchant);

            return new RequestResult(RequestResultStatus.Success, string.Empty, invoice.OrderNumber.ToString())
            {
                BehaviorMode = GatewayRequestBehaviorMode.Post,
                Content = postHtmlForm
            };
        }

        public override VerifyResult Verify(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            //  Callback

            var resultCode = verifyPaymentContext.RequestParameters.GetAs<string>("ResultCode");

            string referenceId = verifyPaymentContext.RequestParameters.GetAs<string>("InvoiceNumber");

            //  This is TransactionId (in my system)
            string transactionId = verifyPaymentContext.RequestParameters.GetAs<string>("referenceId");

            bool isSuccess = resultCode == "100";

            IGatewayResultTranslator gatewayResultTranslator = new TejaratPayRequestResultTranslator();

            if (!isSuccess)
            {
                var callbackTranslatedMessage = gatewayResultTranslator.Translate(resultCode);

                return new VerifyResult(Gateway.Tejarat, referenceId, transactionId, VerifyResultStatus.Failed, callbackTranslatedMessage);
            }

            //  Now verify

            string webServiceXml = CreateVerifyWebServiceXml(
                merchantId: TejaratConfiguration.Merchant,
                referenceNumber: transactionId,
                token: "",
                sha1Key: TejaratConfiguration.Sha1Key);

            var webServiceResponse = WebHelper.SendXmlWebRequest(VerifyServiceUrl, webServiceXml);

            string result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "verifyResponse", "http://tejarat/paymentGateway/definitions");

            //  This result is actually: TotalAmount
            //  it must be equals to TotalAmount in database.
            int integerResult = Convert.ToInt32(result);

            gatewayResultTranslator = new TejaratVerifyResultTranslator();

            var verifyTranslatedMessage = gatewayResultTranslator.Translate(resultCode);

            isSuccess = integerResult > 0 && integerResult == verifyPaymentContext.Amount;

            var status = isSuccess ? VerifyResultStatus.Success : VerifyResultStatus.Failed;

            return new VerifyResult(Gateway.Tejarat, referenceId, transactionId, status, verifyTranslatedMessage);
        }

        public override async Task<VerifyResult> VerifyAsync(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            //  Callback

            var resultCode = verifyPaymentContext.RequestParameters.GetAs<string>("ResultCode");

            string referenceId = verifyPaymentContext.RequestParameters.GetAs<string>("InvoiceNumber");

            //  This is TransactionId (in my system)
            string transactionId = verifyPaymentContext.RequestParameters.GetAs<string>("referenceId");

            bool isSuccess = resultCode == "100";

            IGatewayResultTranslator gatewayResultTranslator = new TejaratPayRequestResultTranslator();

            if (!isSuccess)
            {
                var callbackTranslatedMessage = gatewayResultTranslator.Translate(resultCode);

                return new VerifyResult(Gateway.Tejarat, referenceId, transactionId, VerifyResultStatus.Failed, callbackTranslatedMessage);
            }

            //  Now verify

            string webServiceXml = CreateVerifyWebServiceXml(
                merchantId: TejaratConfiguration.Merchant,
                referenceNumber: transactionId,
                token: "",
                sha1Key: TejaratConfiguration.Sha1Key);

            var webServiceResponse = await WebHelper.SendXmlWebRequestAsync(VerifyServiceUrl, webServiceXml);

            string result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "verifyResponse", "http://tejarat/paymentGateway/definitions");

            //  This result is actually: TotalAmount
            //  it must be equals to TotalAmount in database.
            int integerResult = Convert.ToInt32(result);

            gatewayResultTranslator = new TejaratVerifyResultTranslator();

            var verifyTranslatedMessage = gatewayResultTranslator.Translate(resultCode);

            isSuccess = integerResult > 0 && integerResult == verifyPaymentContext.Amount;

            var status = isSuccess ? VerifyResultStatus.Success : VerifyResultStatus.Failed;

            return new VerifyResult(Gateway.Tejarat, referenceId, transactionId, status, verifyTranslatedMessage);
        }

        public override RefundResult Refund(GatewayRefundPaymentContext refundPaymentContext)
        {
            return new RefundResult(Gateway.Tejarat, 0, RefundResultStatus.Failed, "Gateway Tejarat does not have Refund operation.");
        }

        public override Task<RefundResult> RefundAsync(GatewayRefundPaymentContext refundPaymentContext)
        {
            return Task.FromResult(Refund(refundPaymentContext));
        }

        private static string CreateRequestWebServiceXml(string merchantId, long amount, long invoiceNo, string revertUrl)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<tem:MakeToken>" +
                "<!--Optional:-->" +
                $"<tem:amount>{amount}</tem:amount>" +
                "<!--Optional:-->" +
                $"<tem:merchantId>{merchantId}</tem:merchantId>" +
                "<!--Optional:-->" +
                $"<tem:invoiceNo>{invoiceNo}</tem:invoiceNo>" +
                "<!--Optional:-->" +
                "<tem:paymentId></tem:paymentId>" +
                "<!--Optional:-->" +
                "<tem:specialPaymentId></tem:specialPaymentId>" +
                "<!--Optional:-->" +
                $"<tem:revertURL>{revertUrl}</tem:revertURL>" +
                "<!--Optional:-->" +
                "<tem:description>?</tem:description>" +
                "</tem:MakeToken>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        private static string CreatePostHtmlForm(string paymentPageUrl, string token, string merchantId)
        {
            return
                "<html>" +
                "<body>" +
                $"<form id=\"paymentForm\" action=\"{paymentPageUrl}\" method=\"post\" />" +
                $"<input type=\"hidden\" name=\"MerchantId\" value=\"{merchantId}\" />" +
                $"<input type=\"hidden\" name=\"Token\" value=\"{token}\" />" +
                "</form>" +
                "<script type=\"text/javascript\">" +
                "document.getElementById('paymentForm').submit();" +
                "</script>" +
                "</body>" +
                "</html>";
        }

        private static string CreateVerifyWebServiceXml(string merchantId, string token, string referenceNumber, string sha1Key)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<tem:KicccPaymentsVerification>" +
                "<!--Optional:-->" +
                $"<tem:token>{token}</tem:token>" +
                "<!--Optional:-->" +
                $"<tem:merchantId>{merchantId}</tem:merchantId>" +
                "<!--Optional:-->" +
                $"<tem:referenceNumber>{referenceNumber}</tem:referenceNumber>" +
                "<!--Optional:-->" +
                $"<tem:sha1Key>{sha1Key}</tem:sha1Key>" +
                "</tem:KicccPaymentsVerification>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }
    }
}