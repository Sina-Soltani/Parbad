using System;
using Parbad.Core;
using Parbad.Infrastructure.Translating;
using Parbad.Providers.Parsian.Models;
using Parbad.Providers.Parsian.ResultTranslators;
using Parbad.Utilities;

namespace Parbad.Providers.Parsian
{
    internal static class ParsianHelper
    {
        private const string PaymentPageUrl = "https://pec.shaparak.ir/pecpaymentgateway/";
        public const string WebServiceUrl = "https://pec.shaparak.ir/pecpaymentgateway/eshopservice.asmx";

        public static string CreateRequestData(ParsianGatewayConfiguration configuration, Invoice invoice)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<tem:PinPaymentRequest>" +
                "<!--Optional:-->" +
                $"<tem:pin>{configuration.Pin}</tem:pin>" +
                $"<tem:amount>{invoice.Amount}</tem:amount>" +
                $"<tem:orderId>{invoice.OrderNumber}</tem:orderId>" +
                "<!--Optional:-->" +
                $"<tem:callbackUrl>{invoice.CallbackUrl}</tem:callbackUrl>" +
                "<tem:authority>0</tem:authority>" +
                "<tem:status>0</tem:status>" +
                "</tem:PinPaymentRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static RequestResult CreateRequestResult(string webServiceResponse)
        {
            var authority = Convert.ToInt64(XmlHelper.GetNodeValueFromXml(webServiceResponse, "authority", "http://tempuri.org/"));

            var status = Convert.ToByte(XmlHelper.GetNodeValueFromXml(webServiceResponse, "status", "http://tempuri.org/"));

            var isSucceed = status == 0;

            IGatewayResultTranslator gatewayResultTranslator = new ParsianStatusTranslator();

            var translatedResult = gatewayResultTranslator.Translate(status);

            if (!isSucceed)
            {
                return new RequestResult(RequestResultStatus.Failed, translatedResult, authority.ToString());
            }

            var paymentPageUrl = $"{PaymentPageUrl}?au={authority}";

            return new RequestResult(RequestResultStatus.Success, translatedResult, authority.ToString())
            {
                BehaviorMode = GatewayRequestBehaviorMode.Redirect,
                Content = paymentPageUrl
            };
        }

        public static ParsianCallbackResult CreateCallbackResult(GatewayVerifyPaymentContext context)
        {
            var referenceId = context.RequestParameters.GetAs<string>("au");

            var response = context.RequestParameters.GetAs<string>("rs");

            var isSuccess = !referenceId.IsNullOrWhiteSpace() && !response.IsNullOrWhiteSpace() && response != "0";

            VerifyResult verifyResult = null;

            if (!isSuccess)
            {
                IGatewayResultTranslator translator = new ParsianStatusTranslator();

                var translatedMessage = translator.Translate(Convert.ToByte(response));

                verifyResult = new VerifyResult(Gateway.Parsian, referenceId, string.Empty, VerifyResultStatus.Failed, translatedMessage);
            }

            return new ParsianCallbackResult
            {
                IsSucceed = isSuccess,
                Authority = referenceId,
                Result = verifyResult
            };
        }

        public static string CreateVerifyData(ParsianGatewayConfiguration configuration, ParsianCallbackResult callbackResult)
        {
            return
                "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:tem=\"http://tempuri.org/\">" +
                "<soap:Header/>" +
                "<soap:Body>" +
                "<tem:PaymentEnquiry>" +
                "<!--Optional:-->" +
                $"<tem:pin>{configuration.Pin}</tem:pin>" +
                $"<tem:authority>{callbackResult.Authority}</tem:authority>" +
                "<tem:status>0</tem:status>" +
                "<tem:invoiceNumber>0</tem:invoiceNumber>" +
                "</tem:PaymentEnquiry>" +
                "</soap:Body>" +
                "</soap:Envelope>";
        }

        public static VerifyResult CreateVerifyResult(string webServiceResponse, ParsianCallbackResult callbackResult)
        {
            var status = Convert.ToByte(XmlHelper.GetNodeValueFromXml(webServiceResponse, "status", "http://tempuri.org/"));

            //  InvoiceNumber equals to TransactionId in Parbad system.
            var transactionId = XmlHelper.GetNodeValueFromXml(webServiceResponse, "invoiceNumber", "http://tempuri.org/");

            IGatewayResultTranslator translator = new ParsianStatusTranslator();

            var translatedMessage = translator.Translate(status);

            var isSuccess = status == 0;

            var verifyResultStatus = isSuccess ? VerifyResultStatus.Success : VerifyResultStatus.Failed;

            return new VerifyResult(Gateway.Parsian, callbackResult.Authority, transactionId, verifyResultStatus, translatedMessage);
        }

        public static string CreateRefundData(ParsianGatewayConfiguration configuration, GatewayRefundPaymentContext context)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<tem:PinRefundPayment>" +
                "<!--Optional:-->" +
                $"<tem:pin>{configuration.Pin}</tem:pin>" +
                $"<tem:orderId>{context.OrderNumber}</tem:orderId>" +
                $"<tem:orderToRefund>{context.OrderNumber}</tem:orderToRefund>" +
                $"<tem:amount>{context.Amount}</tem:amount>" +
                "<tem:status>0</tem:status>" +
                "</tem:PinRefundPayment>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static RefundResult CreateRefundResult(string webServiceResponse, GatewayRefundPaymentContext context)
        {
            var status = Convert.ToByte(XmlHelper.GetNodeValueFromXml(webServiceResponse, "status", "http://tempuri.org/"));

            IGatewayResultTranslator translator = new ParsianStatusTranslator();

            var translatedMessage = translator.Translate(status);

            var refundStatus = status == 0 ? RefundResultStatus.Success : RefundResultStatus.Failed;

            return new RefundResult(Gateway.Parsian, context.Amount, refundStatus, translatedMessage);
        }
    }
}