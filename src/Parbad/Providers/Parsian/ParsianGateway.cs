using System;
using System.Threading.Tasks;
using Parbad.Configurations;
using Parbad.Core;
using Parbad.Infrastructure.Translating;
using Parbad.Providers.Parsian.ResultTranslators;
using Parbad.Utilities;

namespace Parbad.Providers.Parsian
{
    internal class ParsianGateway : GatewayBase
    {
        private const string PaymentPageUrl = "https://pec.shaparak.ir/pecpaymentgateway/";
        private const string WebServiceUrl = "https://pec.shaparak.ir/pecpaymentgateway/eshopservice.asmx";

        protected ParsianGatewayConfiguration ParsianConfiguration => ParbadConfiguration.Gateways.GetParsianGatewayConfiguration();

        public ParsianGateway() : base(Gateway.Parsian.ToString())
        {
        }

        public override RequestResult Request(Invoice invoice)
        {
            string webServiceXml = CreateRequestWebServiceXml(
                pin: ParsianConfiguration.Pin,
                orderId: invoice.OrderNumber,
                amount: invoice.Amount,
                callBackUrl: invoice.CallbackUrl);

            var result = WebHelper.SendXmlWebRequest(WebServiceUrl, webServiceXml);

            long authority = Convert.ToInt64(XmlHelper.GetNodeValueFromXml(result, "authority", "http://tempuri.org/"));

            byte status = Convert.ToByte(XmlHelper.GetNodeValueFromXml(result, "status", "http://tempuri.org/"));

            var isSucceed = status == 0;

            IGatewayResultTranslator gatewayResultTranslator = new ParsianStatusTranslator();

            var translatedResult = gatewayResultTranslator.Translate(status);

            if (!isSucceed)
            {
                return new RequestResult(RequestResultStatus.Failed, translatedResult, authority.ToString());
            }

            var paymentPageUrl = CreatePaymentPageUrl(PaymentPageUrl, authority);

            return new RequestResult(RequestResultStatus.Success, translatedResult, authority.ToString())
            {
                BehaviorMode = GatewayRequestBehaviorMode.Redirect,
                Content = paymentPageUrl
            };
        }

        public override async Task<RequestResult> RequestAsync(Invoice invoice)
        {
            string webServiceXml = CreateRequestWebServiceXml(
                pin: ParsianConfiguration.Pin,
                orderId: invoice.OrderNumber,
                amount: invoice.Amount,
                callBackUrl: invoice.CallbackUrl);

            var result = await WebHelper.SendXmlWebRequestAsync(WebServiceUrl, webServiceXml);

            long authority = Convert.ToInt64(XmlHelper.GetNodeValueFromXml(result, "authority", "http://tempuri.org/"));

            byte status = Convert.ToByte(XmlHelper.GetNodeValueFromXml(result, "status", "http://tempuri.org/"));

            var isSucceed = status == 0;

            IGatewayResultTranslator gatewayResultTranslator = new ParsianStatusTranslator();

            var translatedResult = gatewayResultTranslator.Translate(status);

            if (!isSucceed)
            {
                return new RequestResult(RequestResultStatus.Failed, translatedResult, authority.ToString());
            }

            var paymentPageUrl = CreatePaymentPageUrl(PaymentPageUrl, authority);

            return new RequestResult(RequestResultStatus.Success, translatedResult, authority.ToString())
            {
                BehaviorMode = GatewayRequestBehaviorMode.Redirect,
                Content = paymentPageUrl
            };
        }

        public override VerifyResult Verify(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            //  Callback

            var referenceId = verifyPaymentContext.RequestParameters.GetAs<string>("au");

            var response = verifyPaymentContext.RequestParameters.GetAs<string>("rs");

            if (referenceId.IsNullOrWhiteSpace() || response.IsNullOrWhiteSpace())
            {
                return new VerifyResult(Gateway.Parsian, referenceId, string.Empty, VerifyResultStatus.Failed, "Invalid data is received from the gateway.");
            }

            IGatewayResultTranslator gatewayResultTranslator = new ParsianStatusTranslator();

            var isSuccess = response != "0";

            if (!isSuccess)
            {
                var callbackTranslatedMessage = gatewayResultTranslator.Translate(Convert.ToByte(response));

                return new VerifyResult(Gateway.Parsian, referenceId, string.Empty, VerifyResultStatus.Failed, callbackTranslatedMessage);
            }

            //  Now verify

            string verifyWebServiceXml = CreateVerifyWebServiceXml(
                pin: ParsianConfiguration.Pin,
                authority: Convert.ToInt64(referenceId));

            var verifyResult = WebHelper.SendXmlWebRequest(WebServiceUrl, verifyWebServiceXml);

            byte status = Convert.ToByte(XmlHelper.GetNodeValueFromXml(verifyResult, "status", "http://tempuri.org/"));

            //  invoiceNumber is TransactionId
            var transactionId = XmlHelper.GetNodeValueFromXml(verifyResult, "invoiceNumber", "http://tempuri.org/");

            var translatedMessage = gatewayResultTranslator.Translate(status);

            isSuccess = status == 0;

            var verifyResultStatus = isSuccess ? VerifyResultStatus.Success : VerifyResultStatus.Failed;

            return new VerifyResult(Gateway.Parsian, referenceId, transactionId, verifyResultStatus, translatedMessage);
        }

        public override async Task<VerifyResult> VerifyAsync(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            //  Callback

            var referenceId = verifyPaymentContext.RequestParameters.GetAs<string>("au");

            var response = verifyPaymentContext.RequestParameters.GetAs<string>("rs");

            if (referenceId.IsNullOrWhiteSpace() || response.IsNullOrWhiteSpace())
            {
                return new VerifyResult(Gateway.Parsian, referenceId, string.Empty, VerifyResultStatus.Failed, "Invalid data is received from the gateway.");
            }

            IGatewayResultTranslator gatewayResultTranslator = new ParsianStatusTranslator();

            var isSuccess = response != "0";

            if (!isSuccess)
            {
                var callbackTranslatedMessage = gatewayResultTranslator.Translate(Convert.ToByte(response));

                return new VerifyResult(Gateway.Parsian, referenceId, string.Empty, VerifyResultStatus.Failed, callbackTranslatedMessage);
            }

            //  Now verify

            string verifyWebServiceXml = CreateVerifyWebServiceXml(
                pin: ParsianConfiguration.Pin,
                authority: Convert.ToInt64(referenceId));

            var verifyResult = await WebHelper.SendXmlWebRequestAsync(WebServiceUrl, verifyWebServiceXml);

            byte status = Convert.ToByte(XmlHelper.GetNodeValueFromXml(verifyResult, "status", "http://tempuri.org/"));

            //  invoiceNumber is TransactionId
            var transactionId = XmlHelper.GetNodeValueFromXml(verifyResult, "invoiceNumber", "http://tempuri.org/");

            var translatedMessage = gatewayResultTranslator.Translate(status);

            isSuccess = status == 0;

            var verifyResultStatus = isSuccess ? VerifyResultStatus.Success : VerifyResultStatus.Failed;

            return new VerifyResult(Gateway.Parsian, referenceId, transactionId, verifyResultStatus, translatedMessage);
        }

        public override RefundResult Refund(GatewayRefundPaymentContext refundPaymentContext)
        {
            string webServiceXml = CreateReverseWebServiceXml(ParsianConfiguration.Pin,
                refundPaymentContext.OrderNumber,
                refundPaymentContext.Amount);

            var result = WebHelper.SendXmlWebRequest(WebServiceUrl, webServiceXml);

            byte status = Convert.ToByte(XmlHelper.GetNodeValueFromXml(result, "status", "http://tempuri.org/"));

            IGatewayResultTranslator gatewayResultTranslator = new ParsianStatusTranslator();

            var translatedMessage = gatewayResultTranslator.Translate(status);

            var isSuccess = status == 0;

            var reverseResultStatus = isSuccess ? RefundResultStatus.Success : RefundResultStatus.Failed;

            return new RefundResult(Gateway.Parsian, refundPaymentContext.Amount, reverseResultStatus, translatedMessage);
        }

        public override async Task<RefundResult> RefundAsync(GatewayRefundPaymentContext refundPaymentContext)
        {
            string webServiceXml = CreateReverseWebServiceXml(ParsianConfiguration.Pin,
                refundPaymentContext.OrderNumber,
                refundPaymentContext.Amount);

            var result = await WebHelper.SendXmlWebRequestAsync(WebServiceUrl, webServiceXml);

            byte status = Convert.ToByte(XmlHelper.GetNodeValueFromXml(result, "status", "http://tempuri.org/"));

            IGatewayResultTranslator gatewayResultTranslator = new ParsianStatusTranslator();

            var translatedMessage = gatewayResultTranslator.Translate(status);

            var isSuccess = status == 0;

            var reverseResultStatus = isSuccess ? RefundResultStatus.Success : RefundResultStatus.Failed;

            return new RefundResult(Gateway.Parsian, refundPaymentContext.Amount, reverseResultStatus, translatedMessage);
        }

        private static string CreateRequestWebServiceXml(string pin, long orderId, long amount, string callBackUrl)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<tem:PinPaymentRequest>" +
                "<!--Optional:-->" +
                $"<tem:pin>{pin}</tem:pin>" +
                $"<tem:amount>{amount}</tem:amount>" +
                $"<tem:orderId>{orderId}</tem:orderId>" +
                "<!--Optional:-->" +
                $"<tem:callbackUrl>{callBackUrl}</tem:callbackUrl>" +
                "<tem:authority>0</tem:authority>" +
                "<tem:status>0</tem:status>" +
                "</tem:PinPaymentRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        private static string CreatePaymentPageUrl(string paymentPageUrl, long authority)
        {
            return $"{paymentPageUrl}?au={authority}";
        }

        private static string CreateVerifyWebServiceXml(string pin, long authority)
        {
            return
                "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:tem=\"http://tempuri.org/\">" +
                "<soap:Header/>" +
                "<soap:Body>" +
                "<tem:PaymentEnquiry>" +
                "<!--Optional:-->" +
                $"<tem:pin>{pin}</tem:pin>" +
                $"<tem:authority>{authority}</tem:authority>" +
                "<tem:status>0</tem:status>" +
                "<tem:invoiceNumber>0</tem:invoiceNumber>" +
                "</tem:PaymentEnquiry>" +
                "</soap:Body>" +
                "</soap:Envelope>";
        }

        private static string CreateReverseWebServiceXml(string pin, long orderId, long amount)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<tem:PinRefundPayment>" +
                "<!--Optional:-->" +
                $"<tem:pin>{pin}</tem:pin>" +
                $"<tem:orderId>{orderId}</tem:orderId>" +
                $"<tem:orderToRefund>{orderId}</tem:orderToRefund>" +
                $"<tem:amount>{amount}</tem:amount>" +
                "<tem:status>0</tem:status>" +
                "</tem:PinRefundPayment>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }
    }
}