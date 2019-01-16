using System;
using Parbad.Core;
using Parbad.Infrastructure.Translating;
using Parbad.Providers.Mellat.Models;
using Parbad.Providers.Mellat.ResultTranslators;
using Parbad.Utilities;

namespace Parbad.Providers.Mellat
{
    internal static class MellatHelper
    {
        private const string OkResult = "0";
        private const string DuplicateOrderNumberResult = "41";
        private const string AlreadyVerifiedResult = "43";
        private const string SettleSuccess = "45";

        public const string PaymentPageUrl = "https://bpm.shaparak.ir/pgwchannel/startpay.mellat";
        public const string WebServiceUrl = "https://bpm.shaparak.ir/pgwchannel/services/pgw";
        public const string TestWebServiceUrl = "https://bpm.shaparak.ir/pgwchannel/services/pgwtest";

        public static string CreateRequestData(Invoice invoice, MellatGatewayConfiguration configuration)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:int=\"http://interfaces.core.sw.bps.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<int:bpPayRequest>" +
                $"<terminalId>{configuration.TerminalId}</terminalId>" +
                "<!--Optional:-->" +
                $"<userName>{configuration.UserName}</userName>" +
                "<!--Optional:-->" +
                $"<userPassword>{configuration.UserPassword}</userPassword>" +
                $"<orderId>{invoice.OrderNumber}</orderId>" +
                $"<amount>{invoice.Amount}</amount>" +
                "<!--Optional:-->" +
                $"<localDate>{DateTime.Now:yyyyMMdd}</localDate>" +
                "<!--Optional:-->" +
                $"<localTime>{DateTime.Now:HHmmss}</localTime>" +
                "<!--Optional:-->" +
                "<additionalData></additionalData>" +
                "<!--Optional:-->" +
                $"<callBackUrl>{invoice.CallbackUrl}</callBackUrl>" +
                "<payerId>0</payerId>" +
                "'</int:bpPayRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static RequestResult CreateRequestResult(string webServiceResponse)
        {
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "return");

            var arrayResult = result.Split(',');

            var resCode = arrayResult[0];
            var refId = arrayResult.Length > 1 ? arrayResult[1] : string.Empty;

            string message;

            var isSucceed = resCode == OkResult;

            if (!isSucceed)
            {
                IGatewayResultTranslator gatewayResultTranslator = new MellatGatewayResultTranslator();

                message = gatewayResultTranslator.Translate(resCode);

                var status = resCode == DuplicateOrderNumberResult ? RequestResultStatus.DuplicateOrderNumber : RequestResultStatus.Failed;

                return new RequestResult(status, message, refId);
            }

            message = "درخواست با موفقیت ارسال شد.";

            var postHtmlForm = CreatePayRequestHtmlForm(PaymentPageUrl, refId);

            return new RequestResult(RequestResultStatus.Success, message, refId)
            {
                BehaviorMode = GatewayRequestBehaviorMode.Post,
                Content = postHtmlForm
            };
        }

        public static MellatCallbackResult CrateCallbackResult(GatewayVerifyPaymentContext context)
        {
            var resCode = context.RequestParameters.GetAs<string>("ResCode", caseSensitive: true);

            if (resCode.IsNullOrWhiteSpace())
            {
                var verifyResult = new VerifyResult(Gateway.Mellat, string.Empty, string.Empty, VerifyResultStatus.NotValid, "Invalid data is received from the gateway");

                return new MellatCallbackResult
                {
                    IsSucceed = false,
                    Result = verifyResult
                };
            }

            //  Reference ID
            var refId = context.RequestParameters.GetAs<string>("RefId", caseSensitive: true);

            //  Transaction ID
            var saleReferenceId = context.RequestParameters.GetAs<string>("SaleReferenceId", caseSensitive: true);

            //  To translate gateway's result
            IGatewayResultTranslator gatewayResultTranslator = new MellatGatewayResultTranslator();

            var isSucceed = false;
            VerifyResult result = null;

            if (resCode == OkResult)
            {
                isSucceed = true;
            }
            else
            {
                var translatedResult = gatewayResultTranslator.Translate(resCode);

                result = new VerifyResult(Gateway.Mellat, refId, saleReferenceId, VerifyResultStatus.Failed, translatedResult);
            }

            return new MellatCallbackResult
            {
                IsSucceed = isSucceed,
                RefId = refId,
                SaleReferenceId = saleReferenceId,
                Result = result
            };
        }

        public static string CreateVerifyData(GatewayVerifyPaymentContext context, MellatGatewayConfiguration configuration, MellatCallbackResult callbackResult)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:int=\"http://interfaces.core.sw.bps.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<int:bpVerifyRequest>" +
                $"<terminalId>{configuration.TerminalId}</terminalId>" +
                "<!--Optional:-->" +
                $"<userName>{configuration.UserName}</userName>" +
                "<!--Optional:-->" +
                $"<userPassword>{configuration.UserPassword}</userPassword>" +
                $"<orderId>{context.OrderNumber}</orderId>" +
                $"<saleOrderId>{context.OrderNumber}</saleOrderId>" +
                $"<saleReferenceId>{callbackResult.SaleReferenceId}</saleReferenceId>" +
                "</int:bpVerifyRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static MellatVerifyResult CheckVerifyResult(string webServiceResponse, MellatCallbackResult callbackResult)
        {
            var serviceResult = XmlHelper.GetNodeValueFromXml(webServiceResponse, "return");

            var isSucceed = false;
            VerifyResult verifyResult = null;

            if (serviceResult == OkResult)
            {
                isSucceed = true;
            }
            else
            {
                IGatewayResultTranslator translator = new MellatGatewayResultTranslator();

                var translatedResult = translator.Translate(serviceResult);

                var status = serviceResult == AlreadyVerifiedResult
                    ? VerifyResultStatus.AlreadyVerified
                    : VerifyResultStatus.Failed;

                verifyResult = new VerifyResult(Gateway.Mellat, callbackResult.RefId, callbackResult.SaleReferenceId, status, translatedResult);
            }

            return new MellatVerifyResult
            {
                IsSucceed = isSucceed,
                Result = verifyResult
            };
        }

        public static string CreateSettleData(MellatGatewayConfiguration configuration, GatewayVerifyPaymentContext context, MellatCallbackResult callbackResult)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:int=\"http://interfaces.core.sw.bps.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<int:bpSettleRequest>" +
                $"<terminalId>{configuration.TerminalId}</terminalId>" +
                "<!--Optional:-->" +
                $"<userName>{configuration.UserName}</userName>" +
                "<!--Optional:-->" +
                $"<userPassword>{configuration.UserPassword}</userPassword>" +
                $"<orderId>{context.OrderNumber}</orderId>" +
                $"<saleOrderId>{context.OrderNumber}</saleOrderId>" +
                $"<saleReferenceId>{callbackResult.SaleReferenceId}</saleReferenceId>" +
                "</int:bpSettleRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static VerifyResult CreateSettleResult(string webServiceResponse, MellatCallbackResult callbackResult)
        {
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "return");

            IGatewayResultTranslator translator = new MellatGatewayResultTranslator();

            var translatedMessage = translator.Translate(result);

            var isSuccess = result == OkResult || result == SettleSuccess;

            var status = isSuccess ? VerifyResultStatus.Success : VerifyResultStatus.Failed;

            return new VerifyResult(Gateway.Mellat, callbackResult.RefId, callbackResult.SaleReferenceId, status, translatedMessage);
        }

        public static string CreateRefundData(MellatGatewayConfiguration configuration, GatewayRefundPaymentContext context)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:int=\"http://interfaces.core.sw.bps.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<int:bpReversalRequest>" +
                $"<terminalId>{configuration.TerminalId}</terminalId>" +
                "<!--Optional:-->" +
                $"<userName>{configuration.UserName}</userName>" +
                "<!--Optional:-->" +
                $"<userPassword>{configuration.UserPassword}</userPassword>" +
                $"<orderId>{context.OrderNumber}</orderId>" +
                $"<saleOrderId>{context.OrderNumber}</saleOrderId>" +
                $"<saleReferenceId>{Convert.ToInt64(context.TransactionId)}</saleReferenceId>" +
                "</int:bpReversalRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static RefundResult CreateRefundResult(string webServiceResponse, GatewayRefundPaymentContext context)
        {
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "return");

            var isSuccess = result == OkResult;

            var status = isSuccess ? RefundResultStatus.Success : RefundResultStatus.Failed;

            IGatewayResultTranslator translator = new MellatGatewayResultTranslator();

            var translatedMessage = translator.Translate(result);

            return new RefundResult(Gateway.Mellat, context.Amount, status, translatedMessage);
        }

        private static string CreatePayRequestHtmlForm(string paymentPageUrl, string refId)
        {
            return
                "<html>" +
                "<body>" +
                $"<form id=\"paymentForm\" action=\"{paymentPageUrl}\" method=\"post\" />" +
                $"<input type=\"hidden\" name=\"RefId\" value=\"{refId}\" />" +
                "</form>" +
                "<script type=\"text/javascript\">" +
                "document.getElementById('paymentForm').submit();" +
                "</script>" +
                "</body>" +
                "</html>";
        }
    }
}