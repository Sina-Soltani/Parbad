using System;
using Parbad.Core;
using Parbad.Infrastructure.Translating;
using Parbad.Providers.Saman.Models;
using Parbad.Providers.Saman.ResultTranslators;
using Parbad.Utilities;

namespace Parbad.Providers.Saman
{
    internal static class SamanHelper
    {
        public const string PaymentPageUrl = "https://sep.shaparak.ir/payment.aspx";
        public const string WebServiceUrl = "https://sep.shaparak.ir/payments/referencepayment.asmx";

        public static RequestResult CreateRequestResult(Invoice invoice, SamanGatewayConfiguration configuration)
        {
            var htmlContent =
                "<html>" +
                "<body>" +
                $"<form id=\"paymentForm\" action=\"{PaymentPageUrl}\" method=\"post\" />" +
                $"<input type=\"hidden\" name=\"Amount\" value=\"{invoice.Amount}\" />" +
                $"<input type=\"hidden\" name=\"MID\" value=\"{configuration.MerchantId}\" />" +
                $"<input type=\"hidden\" name=\"ResNum\" value=\"{invoice.OrderNumber}\" />" +
                $"<input type=\"hidden\" name=\"RedirectURL\" value=\"{invoice.CallbackUrl}\" />" +
                "</form>" +
                "<script type=\"text/javascript\">" +
                "document.getElementById('paymentForm').submit();" +
                "</script>" +
                "</body>" +
                "</html>";

            return new RequestResult(RequestResultStatus.Success, string.Empty, invoice.OrderNumber.ToString())
            {
                BehaviorMode = GatewayRequestBehaviorMode.Post,
                Content = htmlContent
            };
        }

        public static SamanCallbackResult CreateCallbackResult(GatewayVerifyPaymentContext context)
        {
            var isSuccess = false;
            VerifyResult verifyResult = null;
            string referenceId = null;
            string transactionId = null;

            var state = context.RequestParameters.GetAs<string>("state");

            if (state.IsNullOrWhiteSpace())
            {
                verifyResult = new VerifyResult(Gateway.Saman, string.Empty, string.Empty, VerifyResultStatus.Failed, "Invalid data is received from the gateway.");
            }
            else
            {
                referenceId = context.RequestParameters.GetAs<string>("ResNum");

                transactionId = context.RequestParameters.GetAs<string>("RefNum");

                IGatewayResultTranslator translator = new SamanStateTranslator();

                var translatedMessage = translator.Translate(state);

                isSuccess = state.Equals("OK", StringComparison.OrdinalIgnoreCase);

                if (!isSuccess)
                {
                    verifyResult = new VerifyResult(Gateway.Saman, referenceId, transactionId, VerifyResultStatus.Failed, translatedMessage);
                }
            }

            return new SamanCallbackResult
            {
                IsSucceed = isSuccess,
                ReferenceId = referenceId,
                TransactionId = transactionId,
                Result = verifyResult
            };
        }

        public static string CreateVerifyData(SamanCallbackResult callbackResult, SamanGatewayConfiguration configuration)
        {
            return
                "<soapenv:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:urn=\"urn:Foo\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<urn:verifyTransaction soapenv:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
                $"<String_1 xsi:type=\"xsd:string\">{callbackResult.TransactionId}</String_1>" +
                $"<String_2 xsi:type=\"xsd:string\">{configuration.MerchantId}</String_2>" +
                "</urn:verifyTransaction>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static VerifyResult CreateVerifyResult(string webServiceResponse, GatewayVerifyPaymentContext context, SamanCallbackResult callbackResult)
        {
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "result");

            //  This result is actually: TotalAmount
            //  it must be equals to TotalAmount in database.
            var integerResult = Convert.ToInt32(result);

            var isSuccess = integerResult > 0 && integerResult == context.Amount;

            var status = isSuccess ? VerifyResultStatus.Success : VerifyResultStatus.Failed;

            IGatewayResultTranslator translator = new SamanResultTranslator();

            var translatedMessage = translator.Translate(integerResult);

            return new VerifyResult(Gateway.Saman, callbackResult.ReferenceId, callbackResult.TransactionId, status, translatedMessage);
        }

        public static string CreateRefundData(GatewayRefundPaymentContext context, SamanGatewayConfiguration configuration)
        {
            return
                "<soapenv:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:urn=\"urn:Foo\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<urn:reverseTransaction soapenv:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
                $"<String_1 xsi:type=\"xsd:string\">{context.TransactionId}</String_1>" +
                $"<String_2 xsi:type=\"xsd:string\">{context.Amount}</String_2>" +
                $"<Username xsi:type=\"xsd:string\">{configuration.MerchantId}</Username>" +
                $"<Password xsi:type=\"xsd:string\">{configuration.Password}</Password>" +
                "</urn:reverseTransaction>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static RefundResult CreateRefundResult(string webServiceResponse, GatewayRefundPaymentContext context)
        {
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "result");

            var integerResult = Convert.ToInt32(result);

            IGatewayResultTranslator translator = new SamanResultTranslator();

            var translatedMessage = translator.Translate(integerResult);

            var status = integerResult > 0 ? RefundResultStatus.Success : RefundResultStatus.Failed;

            return new RefundResult(Gateway.Saman, context.Amount, status, translatedMessage);
        }
    }
}
