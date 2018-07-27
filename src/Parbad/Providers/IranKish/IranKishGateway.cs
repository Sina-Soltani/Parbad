using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Parbad.Configurations;
using Parbad.Core;
using Parbad.Infrastructure.Translating;
using Parbad.Providers.IranKish.ResultTranslators;
using Parbad.Utilities;

namespace Parbad.Providers.IranKish
{
    internal class IranKishGateway : GatewayBase
    {
        private const string PaymentPageUrl = "https://ikc.shaparak.ir/TPayment/Payment/index";
        private const string TokenWebServiceUrl = "https://ikc.shaparak.ir/TToken/Tokens.svc";
        private const string VerifyWebServiceUrl = "https://ikc.shaparak.ir/TVerify/Verify.svc";

        private const string OkResult = "100";

        public IranKishGateway() : base(Gateway.IranKish.ToString())
        {
        }

        protected IranKishGatewayConfiguration Configuration => ParbadConfiguration.Gateways.GetIranKishGatewayConfiguration();

        public override RequestResult Request(Invoice invoice)
        {
            if (invoice == null)
            {
                throw new ArgumentNullException(nameof(invoice));
            }

            var webService = CreateTokenWebService(
                merchantId: Configuration.MerchantId,
                amount: invoice.Amount,
                invoiceNo: invoice.OrderNumber.ToString(),
                revertUrl: invoice.CallbackUrl);

            var headers = new Dictionary<string, string> { { "SOAPAction", "http://tempuri.org/ITokens/MakeToken" } };
            var xmlResult = WebHelper.SendXmlWebRequest(TokenWebServiceUrl, webService, headers);

            var result = XmlHelper.GetNodeValueFromXml(xmlResult, "result", "http://schemas.datacontract.org/2004/07/Token");
            var message = XmlHelper.GetNodeValueFromXml(xmlResult, "message", "http://schemas.datacontract.org/2004/07/Token");
            var token = XmlHelper.GetNodeValueFromXml(xmlResult, "token", "http://schemas.datacontract.org/2004/07/Token");

            if (!result.Equals("true", StringComparison.InvariantCultureIgnoreCase) || token.IsNullOrEmpty())
            {
                return new RequestResult(RequestResultStatus.Failed, message, invoice.OrderNumber.ToString());
            }

            var postHtmlForm = CreatePayRequestHtmlForm(PaymentPageUrl, Configuration.MerchantId, token);

            return new RequestResult(RequestResultStatus.Success, string.Empty, invoice.OrderNumber.ToString())
            {
                BehaviorMode = GatewayRequestBehaviorMode.Post,
                Content = postHtmlForm
            };
        }

        public override async Task<RequestResult> RequestAsync(Invoice invoice)
        {
            if (invoice == null)
            {
                throw new ArgumentNullException(nameof(invoice));
            }

            var webService = CreateTokenWebService(
                merchantId: Configuration.MerchantId,
                amount: invoice.Amount,
                invoiceNo: invoice.OrderNumber.ToString(),
                revertUrl: invoice.CallbackUrl);

            var headers = new Dictionary<string, string> { { "SOAPAction", "http://tempuri.org/ITokens/MakeToken" } };
            var xmlResult = await WebHelper.SendXmlWebRequestAsync(TokenWebServiceUrl, webService, headers);

            var result = XmlHelper.GetNodeValueFromXml(xmlResult, "result", "http://schemas.datacontract.org/2004/07/Token");
            var message = XmlHelper.GetNodeValueFromXml(xmlResult, "message", "http://schemas.datacontract.org/2004/07/Token");
            var token = XmlHelper.GetNodeValueFromXml(xmlResult, "token", "http://schemas.datacontract.org/2004/07/Token");

            if (!result.Equals("true", StringComparison.InvariantCultureIgnoreCase) || token.IsNullOrEmpty())
            {
                return new RequestResult(RequestResultStatus.Failed, message, invoice.OrderNumber.ToString());
            }

            var postHtmlForm = CreatePayRequestHtmlForm(PaymentPageUrl, Configuration.MerchantId, token);

            return new RequestResult(RequestResultStatus.Success, string.Empty, invoice.OrderNumber.ToString())
            {
                BehaviorMode = GatewayRequestBehaviorMode.Post,
                Content = postHtmlForm
            };
        }

        public override VerifyResult Verify(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            if (verifyPaymentContext == null)
            {
                throw new ArgumentNullException(nameof(verifyPaymentContext));
            }

            var resultCode = verifyPaymentContext.RequestParameters.GetAs<string>("ResultCode");
            var token = verifyPaymentContext.RequestParameters.GetAs<string>("Token");
            var merchantId = verifyPaymentContext.RequestParameters.GetAs<string>("MerchantId");

            // Reference ID
            var referenceId = verifyPaymentContext.RequestParameters.GetAs<string>("InvoiceNumber");

            // Transaction ID (IranKish's Reference ID is Transaction ID in our system)
            var transactionId = verifyPaymentContext.RequestParameters.GetAs<string>("ReferenceId");

            // Some data cheking for sure.
            if (merchantId != Configuration.MerchantId ||
                referenceId != verifyPaymentContext.OrderNumber.ToString() ||
                token.IsNullOrWhiteSpace())
            {
                return new VerifyResult(Gateway.IranKish, referenceId, transactionId, VerifyResultStatus.NotValid, "Invalid data is received from the gateway");
            }

            //  To translate gateway's result
            IGatewayResultTranslator gatewayResultTranslator = new IranKishGatewayResultTranslator();
            var translatedResult = gatewayResultTranslator.Translate(resultCode);

            if (resultCode != OkResult)
            {
                return new VerifyResult(Gateway.IranKish, referenceId, transactionId, VerifyResultStatus.Failed, translatedResult);
            }


            //  Verify
            var webServiceVerify = CreateVerifyWebService(
                merchantId: Configuration.MerchantId,
                token: token,
                referenceId: transactionId);

            var headers = new Dictionary<string, string> { { "SOAPAction", "http://tempuri.org/IVerify/KicccPaymentsVerification" } };
            var serviceResult = WebHelper.SendXmlWebRequest(VerifyWebServiceUrl, webServiceVerify, headers);

            var result = XmlHelper.GetNodeValueFromXml(serviceResult, "KicccPaymentsVerificationResult");

            // The 'result' is actually the invoice's amount. It must equal to invoice's amount.
            if (!long.TryParse(result, out var integerResult))
            {
                return new VerifyResult(Gateway.IranKish, referenceId, transactionId, VerifyResultStatus.Failed, "پاسخ دریافت شده از درگاه پرداخت قابل بررسی نیست.");
            }

            var isSuccess = integerResult != verifyPaymentContext.Amount;
            var status = isSuccess ? VerifyResultStatus.Success : VerifyResultStatus.Failed;
            translatedResult = isSuccess ? "تراکنش با موفقیت انجام گردید." : gatewayResultTranslator.Translate(result);

            return new VerifyResult(Gateway.IranKish, referenceId, transactionId, status, translatedResult);
        }

        public override async Task<VerifyResult> VerifyAsync(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            if (verifyPaymentContext == null)
            {
                throw new ArgumentNullException(nameof(verifyPaymentContext));
            }

            var resultCode = verifyPaymentContext.RequestParameters.GetAs<string>("ResultCode");
            var token = verifyPaymentContext.RequestParameters.GetAs<string>("Token");
            var merchantId = verifyPaymentContext.RequestParameters.GetAs<string>("MerchantId");

            // Reference ID
            var referenceId = verifyPaymentContext.RequestParameters.GetAs<string>("InvoiceNumber");

            // Transaction ID (IranKish's Reference ID is Transaction ID in our system)
            var transactionId = verifyPaymentContext.RequestParameters.GetAs<string>("ReferenceId");

            // Some data cheking for sure.
            if (merchantId != Configuration.MerchantId ||
                referenceId != verifyPaymentContext.OrderNumber.ToString() ||
                token.IsNullOrWhiteSpace())
            {
                return new VerifyResult(Gateway.IranKish, referenceId, transactionId, VerifyResultStatus.NotValid, "Invalid data is received from the gateway");
            }

            //  To translate gateway's result
            IGatewayResultTranslator gatewayResultTranslator = new IranKishGatewayResultTranslator();
            var translatedResult = gatewayResultTranslator.Translate(resultCode);

            if (resultCode != OkResult)
            {
                return new VerifyResult(Gateway.IranKish, referenceId, transactionId, VerifyResultStatus.Failed, translatedResult);
            }


            //  Verify
            var webServiceVerify = CreateVerifyWebService(
                merchantId: Configuration.MerchantId,
                token: token,
                referenceId: transactionId);

            var headers = new Dictionary<string, string> { { "SOAPAction", "http://tempuri.org/IVerify/KicccPaymentsVerification" } };
            var serviceResult = await WebHelper.SendXmlWebRequestAsync(VerifyWebServiceUrl, webServiceVerify, headers);

            var result = XmlHelper.GetNodeValueFromXml(serviceResult, "KicccPaymentsVerificationResult");

            // The 'result' is actually the invoice's amount. It must equal to invoice's amount.
            if (!long.TryParse(result, out var integerResult))
            {
                return new VerifyResult(Gateway.IranKish, referenceId, transactionId, VerifyResultStatus.Failed, "پاسخ دریافت شده از درگاه پرداخت قابل بررسی نیست.");
            }

            var isSuccess = integerResult != verifyPaymentContext.Amount;
            var status = isSuccess ? VerifyResultStatus.Success : VerifyResultStatus.Failed;
            translatedResult = isSuccess ? "تراکنش با موفقیت انجام گردید." : gatewayResultTranslator.Translate(result);

            return new VerifyResult(Gateway.IranKish, referenceId, transactionId, status, translatedResult);
        }

        public override RefundResult Refund(GatewayRefundPaymentContext refundPaymentContext)
        {
            return new RefundResult(Gateway.IranKish, 0, RefundResultStatus.Failed, "Gateway IranKish does not have Refund operation.");
        }

        public override Task<RefundResult> RefundAsync(GatewayRefundPaymentContext refundPaymentContext)
        {
            return Task.FromResult(Refund(refundPaymentContext));
        }

        private static string CreateTokenWebService(string merchantId, long amount, string invoiceNo, string revertUrl)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<tem:MakeToken>" +
                $"<tem:amount>{amount}</tem:amount>" +
                $"<tem:merchantId>{merchantId}</tem:merchantId>" +
                $"<tem:invoiceNo>{invoiceNo}</tem:invoiceNo>" +
                "<tem:paymentId></tem:paymentId>" +
                "<tem:specialPaymentId></tem:specialPaymentId>" +
                $"<tem:revertURL>{revertUrl}</tem:revertURL>" +
                "<tem:description></tem:description>" +
                "</tem:MakeToken>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        private static string CreateVerifyWebService(string merchantId, string token, string referenceId)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<tem:KicccPaymentsVerification>" +
                $"<tem:token>{token}</tem:token>" +
                $"<tem:merchantId>{merchantId}</tem:merchantId>" +
                $"<tem:referenceNumber>{referenceId}</tem:referenceNumber>" +
                "<tem:sha1Key></tem:sha1Key>" +
                "</tem:KicccPaymentsVerification>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        private static string CreatePayRequestHtmlForm(string paymentPageUrl, string merchantId, string token)
        {
            return
                "<html>" +
                "<body>" +
                $"<form id=\"paymentForm\" action=\"{paymentPageUrl}\" method=\"post\" />" +
                $"<input type=\"hidden\" name=\"merchantid\" value=\"{merchantId}\" />" +
                $"<input type=\"hidden\" name=\"token\" value=\"{token}\" />" +
                "</form>" +
                "<script type=\"text/javascript\">" +
                "document.getElementById('paymentForm').submit();" +
                "</script>" +
                "</body>" +
                "</html>";
        }
    }
}