using System;
using System.Threading.Tasks;
using Parbad.Configurations;
using Parbad.Core;
using Parbad.Infrastructure.Translating;
using Parbad.Providers.Mellat.ResultTranslators;
using Parbad.Utilities;

namespace Parbad.Providers.Mellat
{
    internal class MellatGateway : GatewayBase
    {
        private const string PaymentPageUrl = "https://bpm.shaparak.ir/pgwchannel/startpay.mellat";
        private const string WebServiceUrl = "https://bpm.shaparak.ir/pgwchannel/services/pgw";
        private const string TestWebServiceUrl = "https://bpm.shaparak.ir/pgwchannel/services/pgwtest";

        public MellatGateway() : base(Gateway.Mellat.ToString())
        {
        }

        protected MellatGatewayConfiguration Configuration => ParbadConfiguration.Gateways.GetMellatGatewayConfiguration();

        public override RequestResult Request(Invoice invoice)
        {
            string webServiceXml = CreatePayRequestWebService(
                terminalId: Configuration.TerminalId,
                userName: Configuration.UserName,
                userPassword: Configuration.UserPassword,
                orderId: invoice.OrderNumber,
                amount: invoice.Amount,
                localDate: DateTime.Now.ToString("yyyyMMdd"),
                localTime: DateTime.Now.ToString("HHmmss"),
                additionalData: string.Empty,
                callBackUrl: invoice.CallbackUrl,
                payerId: 0);

            var xmlResult = WebHelper.SendXmlWebRequest(GetWebServiceUrl(), webServiceXml);

            var result = XmlHelper.GetNodeValueFromXml(xmlResult, "return");

            var arrayResult = result.Split(',');

            string resCode = arrayResult[0];
            string refId = arrayResult.Length > 1 ? arrayResult[1] : string.Empty;

            IGatewayResultTranslator gatewayResultTranslator = new MellatGatewayResultTranslator();

            var translatedResult = gatewayResultTranslator.Translate(resCode);

            bool isSucceed = resCode == "0";

            if (!isSucceed)
            {
                return new RequestResult(RequestResultStatus.Failed, translatedResult, refId);
            }

            string postHtmlForm = CreatePayRequestHtmlForm(PaymentPageUrl, refId);

            return new RequestResult(RequestResultStatus.Success, translatedResult, refId)
            {
                BehaviorMode = GatewayRequestBehaviorMode.Post,
                Content = postHtmlForm
            };
        }

        public override async Task<RequestResult> RequestAsync(Invoice invoice)
        {
            var webServiceXml = CreatePayRequestWebService(
                terminalId: Configuration.TerminalId,
                userName: Configuration.UserName,
                userPassword: Configuration.UserPassword,
                orderId: invoice.OrderNumber,
                amount: invoice.Amount,
                localDate: DateTime.Now.ToString("yyyyMMdd"),
                localTime: DateTime.Now.ToString("HHmmss"),
                additionalData: string.Empty,
                callBackUrl: invoice.CallbackUrl,
                payerId: 0);

            var xmlResult = await WebHelper.SendXmlWebRequestAsync(GetWebServiceUrl(), webServiceXml);

            var result = XmlHelper.GetNodeValueFromXml(xmlResult, "return");

            var arrayResult = result.Split(',');

            string resCode = arrayResult[0];
            string refId = arrayResult.Length > 1 ? arrayResult[1] : string.Empty;

            IGatewayResultTranslator gatewayResultTranslator = new MellatGatewayResultTranslator();

            var translatedResult = gatewayResultTranslator.Translate(resCode);

            bool isSucceed = resCode == "0";

            if (!isSucceed)
            {
                return new RequestResult(RequestResultStatus.Failed, translatedResult, refId);
            }

            string postHtmlForm = CreatePayRequestHtmlForm(PaymentPageUrl, refId);

            return new RequestResult(RequestResultStatus.Success, translatedResult, refId)
            {
                BehaviorMode = GatewayRequestBehaviorMode.Post,
                Content = postHtmlForm
            };
        }

        public override VerifyResult Verify(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            string resCode = verifyPaymentContext.RequestParameters.GetAs<string>("ResCode", caseSensitive: true);

            if (resCode.IsNullOrWhiteSpace())
            {
                return new VerifyResult(Gateway.Mellat, string.Empty, string.Empty, VerifyResultStatus.Failed, "Invalid data is received from the gateway");
            }

            //  Reference ID
            string refId = verifyPaymentContext.RequestParameters.GetAs<string>("RefId", caseSensitive: true);

            //  Transaction ID
            var saleReferenceId = verifyPaymentContext.RequestParameters.GetAs<string>("SaleReferenceId", caseSensitive: true);

            IGatewayResultTranslator gatewayResultTranslator = new MellatGatewayResultTranslator();

            var translatedResult = gatewayResultTranslator.Translate(resCode);

            var isSuccess = resCode == "0";

            if (!isSuccess)
            {
                return new VerifyResult(Gateway.Mellat, refId, saleReferenceId, VerifyResultStatus.Failed, translatedResult);
            }

            string settleWebServiceXml = CreateSettleWebService(
                terminalId: Configuration.TerminalId,
                userName: Configuration.UserName,
                userPassword: Configuration.UserPassword,
                orderId: verifyPaymentContext.OrderNumber,
                saleOrderId: verifyPaymentContext.OrderNumber,
                saleReferenceId: Convert.ToInt64(saleReferenceId));

            string xmlResult = WebHelper.SendXmlWebRequest(GetWebServiceUrl(), settleWebServiceXml);

            var result = XmlHelper.GetNodeValueFromXml(xmlResult, "return");

            isSuccess = result == "0";

            var status = isSuccess ? VerifyResultStatus.Success : VerifyResultStatus.Failed;

            translatedResult = gatewayResultTranslator.Translate(result);

            return new VerifyResult(Gateway.Mellat, refId, saleReferenceId, status, translatedResult);
        }

        public override async Task<VerifyResult> VerifyAsync(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            string resCode = verifyPaymentContext.RequestParameters.GetAs<string>("ResCode", caseSensitive: true);

            if (resCode.IsNullOrWhiteSpace())
            {
                return new VerifyResult(Gateway.Mellat, string.Empty, string.Empty, VerifyResultStatus.Failed, "Invalid data is received from the gateway");
            }

            //  Reference ID
            string refId = verifyPaymentContext.RequestParameters.GetAs<string>("RefId", caseSensitive: true);

            //  Transaction ID
            var saleReferenceId = verifyPaymentContext.RequestParameters.GetAs<string>("SaleReferenceId", caseSensitive: true);

            IGatewayResultTranslator gatewayResultTranslator = new MellatGatewayResultTranslator();

            var translatedResult = gatewayResultTranslator.Translate(resCode);

            var isSuccess = resCode == "0";

            if (!isSuccess)
            {
                return new VerifyResult(Gateway.Mellat, refId, saleReferenceId, VerifyResultStatus.Failed, translatedResult);
            }

            string settleWebServiceXml = CreateSettleWebService(
                terminalId: Configuration.TerminalId,
                userName: Configuration.UserName,
                userPassword: Configuration.UserPassword,
                orderId: verifyPaymentContext.OrderNumber,
                saleOrderId: verifyPaymentContext.OrderNumber,
                saleReferenceId: Convert.ToInt64(saleReferenceId));

            string xmlResult = await WebHelper.SendXmlWebRequestAsync(GetWebServiceUrl(), settleWebServiceXml);

            var result = XmlHelper.GetNodeValueFromXml(xmlResult, "return");

            isSuccess = result == "0";

            var status = isSuccess ? VerifyResultStatus.Success : VerifyResultStatus.Failed;

            translatedResult = gatewayResultTranslator.Translate(result);

            return new VerifyResult(Gateway.Mellat, refId, saleReferenceId, status, translatedResult);
        }

        public override RefundResult Refund(GatewayRefundPaymentContext refundPaymentContext)
        {
            if (!long.TryParse(refundPaymentContext.TransactionId, out var longTransactionId))
            {
                throw new Exception($"Transaction ID format is not valid. Transaction ID must be Int64. Value: {refundPaymentContext.TransactionId}");
            }

            string webServiceXml = CreateReverseWebService(
                terminalId: Configuration.TerminalId,
                userName: Configuration.UserName,
                userPassword: Configuration.UserPassword,
                orderId: refundPaymentContext.OrderNumber,
                saleOrderId: refundPaymentContext.OrderNumber,
                saleReferenceId: longTransactionId);

            var xmlResult = WebHelper.SendXmlWebRequest(GetWebServiceUrl(), webServiceXml);

            var result = XmlHelper.GetNodeValueFromXml(xmlResult, "return");

            bool isSuccess = result == "0";

            var status = isSuccess ? RefundResultStatus.Success : RefundResultStatus.Failed;

            IGatewayResultTranslator gatewayResultTranslator = new MellatGatewayResultTranslator();

            var translatedResult = gatewayResultTranslator.Translate(result);

            return new RefundResult(Gateway.Mellat, refundPaymentContext.Amount, status, translatedResult);
        }

        public override async Task<RefundResult> RefundAsync(GatewayRefundPaymentContext refundPaymentContext)
        {
            if (!long.TryParse(refundPaymentContext.TransactionId, out var longTransactionId))
            {
                throw new Exception($"Transaction ID format is not valid. Transaction ID must be Int64. Value: {refundPaymentContext.TransactionId}");
            }

            string webServiceXml = CreateReverseWebService(
                terminalId: Configuration.TerminalId,
                userName: Configuration.UserName,
                userPassword: Configuration.UserPassword,
                orderId: refundPaymentContext.OrderNumber,
                saleOrderId: refundPaymentContext.OrderNumber,
                saleReferenceId: longTransactionId);

            var xmlResult = await WebHelper.SendXmlWebRequestAsync(GetWebServiceUrl(), webServiceXml);

            var result = XmlHelper.GetNodeValueFromXml(xmlResult, "return");

            bool isSuccess = result == "0";

            var status = isSuccess ? RefundResultStatus.Success : RefundResultStatus.Failed;

            IGatewayResultTranslator gatewayResultTranslator = new MellatGatewayResultTranslator();

            var translatedResult = gatewayResultTranslator.Translate(result);

            return new RefundResult(Gateway.Mellat, refundPaymentContext.Amount, status, translatedResult);
        }

        //private MellatGatewayConfiguration MellatConfiguration => (MellatGatewayConfiguration)Configuration;

        //public override string Name => Gateway.Mellat.ToString();

        //protected override IPaymentRequestCommand ProvidePaymentRequestCommand()
        //{
        //    return new MellatPaymentRequestCommand(
        //        MellatConfiguration.TerminalId,
        //        MellatConfiguration.UserName,
        //        MellatConfiguration.UserPassword,
        //        GetWebServiceUrl(),
        //        PaymentPageUrl);
        //}

        //protected override ICallbackCommand ProvideCallbackRequestCommand()
        //{
        //    return new MellatCallbackCommand();
        //}

        //protected override IVerifyCommand ProvideVerifyRequestCommand()
        //{
        //    return new MellatVerifyCommand(
        //        MellatConfiguration.TerminalId,
        //        MellatConfiguration.UserName,
        //        MellatConfiguration.UserPassword,
        //        GetWebServiceUrl());
        //}

        //protected override ISettleCommand ProvideSettleRequestCommand()
        //{
        //    return new MellatSettleCommand(
        //        MellatConfiguration.TerminalId,
        //        MellatConfiguration.UserName,
        //        MellatConfiguration.UserPassword,
        //        GetWebServiceUrl());
        //}

        //protected override IReverseCommand ProvideReverseRequestCommand()
        //{
        //    return new MellatReverseCommand(
        //        MellatConfiguration.TerminalId,
        //        MellatConfiguration.UserName,
        //        MellatConfiguration.UserPassword,
        //        GetWebServiceUrl());
        //}

        private static string CreatePayRequestWebService(long terminalId, string userName, string userPassword, long orderId, long amount, string localDate, string localTime, string additionalData, string callBackUrl, long payerId)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:int=\"http://interfaces.core.sw.bps.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<int:bpPayRequest>" +
                $"<terminalId>{terminalId}</terminalId>" +
                "<!--Optional:-->" +
                $"<userName>{userName}</userName>" +
                "<!--Optional:-->" +
                $"<userPassword>{userPassword}</userPassword>" +
                $"<orderId>{orderId}</orderId>" +
                $"<amount>{amount}</amount>" +
                "<!--Optional:-->" +
                $"<localDate>{localDate}</localDate>" +
                "<!--Optional:-->" +
                $"<localTime>{localTime}</localTime>" +
                "<!--Optional:-->" +
                $"<additionalData>{additionalData}</additionalData>" +
                "<!--Optional:-->" +
                $"<callBackUrl>{callBackUrl}</callBackUrl>" +
                $"<payerId>{payerId}</payerId>" +
                "'</int:bpPayRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
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

        private static string CreateSettleWebService(long terminalId, string userName, string userPassword, long orderId, long saleOrderId, long saleReferenceId)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:int=\"http://interfaces.core.sw.bps.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<int:bpSettleRequest>" +
                $"<terminalId>{terminalId}</terminalId>" +
                "<!--Optional:-->" +
                $"<userName>{userName}</userName>" +
                "<!--Optional:-->" +
                $"<userPassword>{userPassword}</userPassword>" +
                $"<orderId>{orderId}</orderId>" +
                $"<saleOrderId>{saleOrderId}</saleOrderId>" +
                $"<saleReferenceId>{saleReferenceId}</saleReferenceId>" +
                "</int:bpSettleRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        private static string CreateReverseWebService(long terminalId, string userName, string userPassword, long orderId, long saleOrderId, long saleReferenceId)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:int=\"http://interfaces.core.sw.bps.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<int:bpReversalRequest>" +
                $"<terminalId>{terminalId}</terminalId>" +
                "<!--Optional:-->" +
                $"<userName>{userName}</userName>" +
                "<!--Optional:-->" +
                $"<userPassword>{userPassword}</userPassword>" +
                $"<orderId>{orderId}</orderId>" +
                $"<saleOrderId>{saleOrderId}</saleOrderId>" +
                $"<saleReferenceId>{saleReferenceId}</saleReferenceId>" +
                "</int:bpReversalRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        private string GetWebServiceUrl()
        {
            return Configuration.IsTestModeEnabled ? TestWebServiceUrl : WebServiceUrl;
        }
    }
}