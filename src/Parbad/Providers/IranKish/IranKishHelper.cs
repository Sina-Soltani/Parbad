using System;
using System.Collections.Generic;
using Parbad.Core;
using Parbad.Infrastructure.Translating;
using Parbad.Providers.IranKish.ResultTranslators;
using Parbad.Utilities;

namespace Parbad.Providers.IranKish
{
    internal static class IranKishHelper
    {
        public const string PaymentPageUrl = "https://ikc.shaparak.ir/TPayment/Payment/index";
        public const string TokenWebServiceUrl = "https://ikc.shaparak.ir/TToken/Tokens.svc";
        public const string VerifyWebServiceUrl = "https://ikc.shaparak.ir/TVerify/Verify.svc";

        public static Dictionary<string, string> HttpRequestHeader => new Dictionary<string, string> { { "SOAPAction", "http://tempuri.org/ITokens/MakeToken" } };
        public static Dictionary<string, string> HttpVerifyHeaders => new Dictionary<string, string> { { "SOAPAction", "http://tempuri.org/IVerify/KicccPaymentsVerification" } };

        private const string OkResult = "100";

        public static string CreateRequestData(Invoice invoice, IranKishGatewayConfiguration configuration)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<tem:MakeToken>" +
                $"<tem:amount>{invoice.Amount}</tem:amount>" +
                $"<tem:merchantId>{configuration.MerchantId}</tem:merchantId>" +
                $"<tem:invoiceNo>{invoice.OrderNumber}</tem:invoiceNo>" +
                "<tem:paymentId></tem:paymentId>" +
                "<tem:specialPaymentId></tem:specialPaymentId>" +
                $"<tem:revertURL>{invoice.CallbackUrl}</tem:revertURL>" +
                "<tem:description></tem:description>" +
                "</tem:MakeToken>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static RequestResult CreateRequestResult(string webServiceResponse, Invoice invoice, IranKishGatewayConfiguration configuration)
        {
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "result", "http://schemas.datacontract.org/2004/07/Token");
            var message = XmlHelper.GetNodeValueFromXml(webServiceResponse, "message", "http://schemas.datacontract.org/2004/07/Token");
            var token = XmlHelper.GetNodeValueFromXml(webServiceResponse, "token", "http://schemas.datacontract.org/2004/07/Token");

            var isSucceed = result.Equals("true", StringComparison.OrdinalIgnoreCase) && !token.IsNullOrEmpty();

            var status = isSucceed ? RequestResultStatus.Success : RequestResultStatus.Failed;

            string htmlForm = null;

            if (isSucceed)
            {
                htmlForm = CreateRequestHtmlForm(PaymentPageUrl, configuration.MerchantId, token);
            }

            return new RequestResult(status, message, invoice.OrderNumber.ToString())
            {
                BehaviorMode = GatewayRequestBehaviorMode.Post,
                Content = htmlForm
            };
        }

        public static IranKishCallbackResult CreateCallbackResult(GatewayVerifyPaymentContext context, IranKishGatewayConfiguration configuration)
        {
            var resultCode = context.RequestParameters.GetAs<string>("ResultCode");
            var token = context.RequestParameters.GetAs<string>("Token");
            var merchantId = context.RequestParameters.GetAs<string>("MerchantId");

            // Equals to ReferenceID in Parbad system.
            var invoiceNumber = context.RequestParameters.GetAs<string>("InvoiceNumber");

            // Equals to TransactionID in Parbad system.
            var referenceId = context.RequestParameters.GetAs<string>("ReferenceId");

            var isSucceed = false;
            VerifyResult verifyResult = null;

            if (merchantId != configuration.MerchantId ||
                invoiceNumber != context.OrderNumber.ToString() ||
                token.IsNullOrWhiteSpace())
            {
                verifyResult = new VerifyResult(Gateway.IranKish, invoiceNumber, referenceId, VerifyResultStatus.NotValid, "Invalid data is received from the gateway");
            }
            else
            {
                IGatewayResultTranslator translator = new IranKishGatewayResultTranslator();
                var translatedMessage = translator.Translate(resultCode);

                isSucceed = resultCode == OkResult;

                if (!isSucceed)
                {
                    verifyResult = new VerifyResult(Gateway.IranKish, invoiceNumber, referenceId, VerifyResultStatus.Failed, translatedMessage);
                }
            }

            return new IranKishCallbackResult
            {
                IsSucceed = isSucceed,
                Token = token,
                InvoiceNumber = invoiceNumber,
                ReferenceId = referenceId,
                Result = verifyResult
            };
        }

        public static string CreateVerifyData(GatewayVerifyPaymentContext context, IranKishCallbackResult callbackResult, IranKishGatewayConfiguration configuration)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<tem:KicccPaymentsVerification>" +
                $"<tem:token>{callbackResult.Token}</tem:token>" +
                $"<tem:merchantId>{configuration.MerchantId}</tem:merchantId>" +
                $"<tem:referenceNumber>{callbackResult.ReferenceId}</tem:referenceNumber>" +
                "<tem:sha1Key></tem:sha1Key>" +
                "</tem:KicccPaymentsVerification>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static VerifyResult CreateVerifyResult(string webServiceResponse, GatewayVerifyPaymentContext context, IranKishCallbackResult callbackResult)
        {
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "KicccPaymentsVerificationResult");

            // The result object is actually the amount of invoice . It must equal to invoice's amount.
            if (!long.TryParse(result, out var integerResult))
            {
                return new VerifyResult(Gateway.IranKish, callbackResult.InvoiceNumber, callbackResult.ReferenceId, VerifyResultStatus.Failed, "پاسخ دریافت شده از درگاه پرداخت قابل بررسی نیست.");
            }

            var isSuccess = integerResult != context.Amount;

            var status = isSuccess ? VerifyResultStatus.Success : VerifyResultStatus.Failed;

            IGatewayResultTranslator translator = new IranKishGatewayResultTranslator();

            var translatedMessage = isSuccess ? "تراکنش با موفقیت انجام گردید." : translator.Translate(result);

            return new VerifyResult(Gateway.IranKish, callbackResult.InvoiceNumber, callbackResult.ReferenceId, status, translatedMessage);
        }

        private static string CreateRequestHtmlForm(string paymentPageUrl, string merchantId, string token)
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
