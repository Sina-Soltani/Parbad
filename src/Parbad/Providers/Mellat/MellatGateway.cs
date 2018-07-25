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

        private const string OkResult = "0";
        private const string DuplicateOrderNumberResult = "41";
        private const string AlreadyVerifiedResult = "43";

        public MellatGateway() : base(Gateway.Mellat.ToString())
        {
        }

        protected MellatGatewayConfiguration Configuration => ParbadConfiguration.Gateways.GetMellatGatewayConfiguration();

        public override RequestResult Request(Invoice invoice)
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

            var xmlResult = WebHelper.SendXmlWebRequest(GetWebServiceUrl(), webServiceXml);

            var result = XmlHelper.GetNodeValueFromXml(xmlResult, "return");

            var arrayResult = result.Split(',');

            var resCode = arrayResult[0];
            var refId = arrayResult.Length > 1 ? arrayResult[1] : string.Empty;

            IGatewayResultTranslator gatewayResultTranslator = new MellatGatewayResultTranslator();

            var translatedResult = gatewayResultTranslator.Translate(resCode);

            var isSucceed = resCode == OkResult;

            if (!isSucceed)
            {
                var status = resCode == DuplicateOrderNumberResult ? RequestResultStatus.DuplicateOrderNumber : RequestResultStatus.Failed;

                return new RequestResult(status, translatedResult, refId);
            }

            var postHtmlForm = CreatePayRequestHtmlForm(PaymentPageUrl, refId);

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

            var resCode = arrayResult[0];
            var refId = arrayResult.Length > 1 ? arrayResult[1] : string.Empty;

            IGatewayResultTranslator gatewayResultTranslator = new MellatGatewayResultTranslator();

            var translatedResult = gatewayResultTranslator.Translate(resCode);

            var isSucceed = resCode == OkResult;

            if (!isSucceed)
            {
                var status = resCode == DuplicateOrderNumberResult ? RequestResultStatus.DuplicateOrderNumber : RequestResultStatus.Failed;

                return new RequestResult(status, translatedResult, refId);
            }

            var postHtmlForm = CreatePayRequestHtmlForm(PaymentPageUrl, refId);

            return new RequestResult(RequestResultStatus.Success, translatedResult, refId)
            {
                BehaviorMode = GatewayRequestBehaviorMode.Post,
                Content = postHtmlForm
            };
        }

        public override VerifyResult Verify(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            var resCode = verifyPaymentContext.RequestParameters.GetAs<string>("ResCode", caseSensitive: true);

            if (resCode.IsNullOrWhiteSpace())
            {
                return new VerifyResult(Gateway.Mellat, string.Empty, string.Empty, VerifyResultStatus.NotValid, "Invalid data is received from the gateway");
            }

            //  Reference ID
            string refId = verifyPaymentContext.RequestParameters.GetAs<string>("RefId", caseSensitive: true);

            //  Transaction ID
            var saleReferenceId = verifyPaymentContext.RequestParameters.GetAs<string>("SaleReferenceId", caseSensitive: true);

            //  To translate gateway's result
            IGatewayResultTranslator gatewayResultTranslator = new MellatGatewayResultTranslator();
            string translatedResult;

            if (resCode != OkResult)
            {
                translatedResult = gatewayResultTranslator.Translate(resCode);

                return new VerifyResult(Gateway.Mellat, refId, saleReferenceId, VerifyResultStatus.Failed, translatedResult);
            }

            //  Verify

            var webServiceVerifyXml = CreateVerifyWebService(
                terminalId: Configuration.TerminalId,
                userName: Configuration.UserName,
                userPassword: Configuration.UserPassword,
                orderId: verifyPaymentContext.OrderNumber,
                saleOrderId: verifyPaymentContext.OrderNumber,
                saleReferenceId: Convert.ToInt64(saleReferenceId));

            var xmlResult = WebHelper.SendXmlWebRequest(GetWebServiceUrl(), webServiceVerifyXml);

            var result = XmlHelper.GetNodeValueFromXml(xmlResult, "return");

            //  Check if verifing is failed.
            VerifyResultStatus status;

            if (result != OkResult)
            {
                translatedResult = gatewayResultTranslator.Translate(result);

                status = result == AlreadyVerifiedResult
                    ? VerifyResultStatus.AlreadyVerified
                    : VerifyResultStatus.Failed;

                return new VerifyResult(Gateway.Mellat, refId, saleReferenceId, status, translatedResult);
            }

            //  Settle
            var settleWebServiceXml = CreateSettleWebService(
                terminalId: Configuration.TerminalId,
                userName: Configuration.UserName,
                userPassword: Configuration.UserPassword,
                orderId: verifyPaymentContext.OrderNumber,
                saleOrderId: verifyPaymentContext.OrderNumber,
                saleReferenceId: Convert.ToInt64(saleReferenceId));

            xmlResult = WebHelper.SendXmlWebRequest(GetWebServiceUrl(), settleWebServiceXml);

            result = XmlHelper.GetNodeValueFromXml(xmlResult, "return");

            translatedResult = gatewayResultTranslator.Translate(result);

            var isSuccess = result == OkResult;

            status = isSuccess ? VerifyResultStatus.Success : VerifyResultStatus.Failed;

            return new VerifyResult(Gateway.Mellat, refId, saleReferenceId, status, translatedResult);
        }

        public override async Task<VerifyResult> VerifyAsync(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            var resCode = verifyPaymentContext.RequestParameters.GetAs<string>("ResCode", caseSensitive: true);

            if (resCode.IsNullOrWhiteSpace())
            {
                return new VerifyResult(Gateway.Mellat, string.Empty, string.Empty, VerifyResultStatus.NotValid, "Invalid data is received from the gateway");
            }

            //  Reference ID
            string refId = verifyPaymentContext.RequestParameters.GetAs<string>("RefId", caseSensitive: true);

            //  Transaction ID
            var saleReferenceId = verifyPaymentContext.RequestParameters.GetAs<string>("SaleReferenceId", caseSensitive: true);

            //  To translate gateway's result
            IGatewayResultTranslator gatewayResultTranslator = new MellatGatewayResultTranslator();
            string translatedResult;

            if (resCode != OkResult)
            {
                translatedResult = gatewayResultTranslator.Translate(resCode);

                return new VerifyResult(Gateway.Mellat, refId, saleReferenceId, VerifyResultStatus.Failed, translatedResult);
            }

            //  Verify

            var webServiceVerifyXml = CreateVerifyWebService(
                terminalId: Configuration.TerminalId,
                userName: Configuration.UserName,
                userPassword: Configuration.UserPassword,
                orderId: verifyPaymentContext.OrderNumber,
                saleOrderId: verifyPaymentContext.OrderNumber,
                saleReferenceId: Convert.ToInt64(saleReferenceId));

            var xmlResult = await WebHelper.SendXmlWebRequestAsync(GetWebServiceUrl(), webServiceVerifyXml);

            var result = XmlHelper.GetNodeValueFromXml(xmlResult, "return");

            //  Check if verifing is failed.
            VerifyResultStatus status;

            if (result != OkResult)
            {
                translatedResult = gatewayResultTranslator.Translate(result);

                status = result == AlreadyVerifiedResult
                    ? VerifyResultStatus.AlreadyVerified
                    : VerifyResultStatus.Failed;

                return new VerifyResult(Gateway.Mellat, refId, saleReferenceId, status, translatedResult);
            }

            //  Settle
            var settleWebServiceXml = CreateSettleWebService(
                terminalId: Configuration.TerminalId,
                userName: Configuration.UserName,
                userPassword: Configuration.UserPassword,
                orderId: verifyPaymentContext.OrderNumber,
                saleOrderId: verifyPaymentContext.OrderNumber,
                saleReferenceId: Convert.ToInt64(saleReferenceId));

            xmlResult = await WebHelper.SendXmlWebRequestAsync(GetWebServiceUrl(), settleWebServiceXml);

            result = XmlHelper.GetNodeValueFromXml(xmlResult, "return");

            translatedResult = gatewayResultTranslator.Translate(result);

            var isSuccess = result == OkResult;

            status = isSuccess ? VerifyResultStatus.Success : VerifyResultStatus.Failed;

            return new VerifyResult(Gateway.Mellat, refId, saleReferenceId, status, translatedResult);
        }

        public override RefundResult Refund(GatewayRefundPaymentContext refundPaymentContext)
        {
            if (!long.TryParse(refundPaymentContext.TransactionId, out var longTransactionId))
            {
                throw new Exception($"Transaction ID format is not valid. Transaction ID must be Int64. Value: {refundPaymentContext.TransactionId}");
            }

            var webServiceXml = CreateReverseWebService(
                terminalId: Configuration.TerminalId,
                userName: Configuration.UserName,
                userPassword: Configuration.UserPassword,
                orderId: refundPaymentContext.OrderNumber,
                saleOrderId: refundPaymentContext.OrderNumber,
                saleReferenceId: longTransactionId);

            var xmlResult = WebHelper.SendXmlWebRequest(GetWebServiceUrl(), webServiceXml);

            var result = XmlHelper.GetNodeValueFromXml(xmlResult, "return");

            var isSuccess = result == OkResult;

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

            var webServiceXml = CreateReverseWebService(
                terminalId: Configuration.TerminalId,
                userName: Configuration.UserName,
                userPassword: Configuration.UserPassword,
                orderId: refundPaymentContext.OrderNumber,
                saleOrderId: refundPaymentContext.OrderNumber,
                saleReferenceId: longTransactionId);

            var xmlResult = await WebHelper.SendXmlWebRequestAsync(GetWebServiceUrl(), webServiceXml);

            var result = XmlHelper.GetNodeValueFromXml(xmlResult, "return");

            var isSuccess = result == OkResult;

            var status = isSuccess ? RefundResultStatus.Success : RefundResultStatus.Failed;

            IGatewayResultTranslator gatewayResultTranslator = new MellatGatewayResultTranslator();

            var translatedResult = gatewayResultTranslator.Translate(result);

            return new RefundResult(Gateway.Mellat, refundPaymentContext.Amount, status, translatedResult);
        }

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

        private static string CreateVerifyWebService(long terminalId, string userName, string userPassword, long orderId, long saleOrderId, long saleReferenceId)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:int=\"http://interfaces.core.sw.bps.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<int:bpVerifyRequest>" +
                $"<terminalId>{terminalId}</terminalId>" +
                "<!--Optional:-->" +
                $"<userName>{userName}</userName>" +
                "<!--Optional:-->" +
                $"<userPassword>{userPassword}</userPassword>" +
                $"<orderId>{orderId}</orderId>" +
                $"<saleOrderId>{saleOrderId}</saleOrderId>" +
                $"<saleReferenceId>{saleReferenceId}</saleReferenceId>" +
                "</int:bpVerifyRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
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