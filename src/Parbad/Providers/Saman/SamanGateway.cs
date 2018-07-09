using System;
using System.Threading.Tasks;
using Parbad.Configurations;
using Parbad.Core;
using Parbad.Infrastructure.Translating;
using Parbad.Providers.Saman.ResultTranslators;
using Parbad.Utilities;

namespace Parbad.Providers.Saman
{
    internal class SamanGateway : GatewayBase
    {
        private const string PaymentPageUrl = "https://sep.shaparak.ir/payment.aspx";
        private const string WebServiceUrl = "https://sep.shaparak.ir/payments/referencepayment.asmx";

        protected SamanGatewayConfiguration SamanConfiguration => ParbadConfiguration.Gateways.GetSamanGatewayConfiguration();

        public SamanGateway() : base(Gateway.Saman.ToString())
        {
        }

        public override RequestResult Request(Invoice invoice)
        {
            if (invoice == null)
            {
                throw new ArgumentNullException(nameof(invoice));
            }

            string htmlForm = CreateRequestHtmlForm(
                SamanConfiguration.MerchantId,
                PaymentPageUrl,
                invoice.Amount,
                invoice.OrderNumber,
                invoice.CallbackUrl);

            return new RequestResult(RequestResultStatus.Success, string.Empty, invoice.OrderNumber.ToString())
            {
                BehaviorMode = GatewayRequestBehaviorMode.Post,
                Content = htmlForm
            };
        }

        public override Task<RequestResult> RequestAsync(Invoice invoice)
        {
            return Task.FromResult(Request(invoice));
        }

        public override VerifyResult Verify(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            if (verifyPaymentContext == null)
            {
                throw new ArgumentNullException(nameof(verifyPaymentContext));
            }

            string state = verifyPaymentContext.RequestParameters.GetAs<string>("state");

            if (state.IsNullOrWhiteSpace())
            {
                return new VerifyResult(Gateway.Saman, string.Empty, string.Empty, VerifyResultStatus.Failed, "Invalid data is received from the gateway.");
            }

            string referenceId = verifyPaymentContext.RequestParameters.GetAs<string>("ResNum");

            string transactionId = verifyPaymentContext.RequestParameters.GetAs<string>("RefNum");

            IGatewayResultTranslator resultTranslator = new SamanStateTranslator();

            var translatedMessage = resultTranslator.Translate(state);

            var isSuccess = state.Equals("OK", StringComparison.InvariantCultureIgnoreCase);

            var status = isSuccess ? VerifyResultStatus.Success : VerifyResultStatus.Failed;

            if (!isSuccess)
            {
                return new VerifyResult(Gateway.Saman, referenceId, transactionId, status, translatedMessage);
            }

            string verifyXml = CreateVerifyXml(SamanConfiguration.MerchantId, transactionId);

            var result = WebHelper.SendXmlWebRequest(WebServiceUrl, verifyXml);

            result = XmlHelper.GetNodeValueFromXml(result, "result");

            //  This result is actually: TotalAmount
            //  it must be equals to TotalAmount in database.
            int integerResult = Convert.ToInt32(result);

            isSuccess = integerResult > 0 && integerResult == verifyPaymentContext.Amount;

            status = isSuccess ? VerifyResultStatus.Success : VerifyResultStatus.Failed;

            resultTranslator = new SamanResultTranslator();

            translatedMessage = resultTranslator.Translate(integerResult);

            return new VerifyResult(Gateway.Saman, referenceId, transactionId, status, translatedMessage);
        }

        public override async Task<VerifyResult> VerifyAsync(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            if (verifyPaymentContext == null)
            {
                throw new ArgumentNullException(nameof(verifyPaymentContext));
            }

            string state = verifyPaymentContext.RequestParameters.GetAs<string>("state");

            if (state.IsNullOrWhiteSpace())
            {
                return new VerifyResult(Gateway.Saman, string.Empty, string.Empty, VerifyResultStatus.Failed, "Invalid data is received from the gateway.");
            }

            string referenceId = verifyPaymentContext.RequestParameters.GetAs<string>("ResNum");

            string transactionId = verifyPaymentContext.RequestParameters.GetAs<string>("RefNum");

            IGatewayResultTranslator resultTranslator = new SamanStateTranslator();

            var translatedMessage = resultTranslator.Translate(state);

            var isSuccess = state.Equals("OK", StringComparison.InvariantCultureIgnoreCase);

            var status = isSuccess ? VerifyResultStatus.Success : VerifyResultStatus.Failed;

            if (!isSuccess)
            {
                return new VerifyResult(Gateway.Saman, referenceId, transactionId, status, translatedMessage);
            }

            string verifyXml = CreateVerifyXml(SamanConfiguration.MerchantId, transactionId);

            var result = await WebHelper.SendXmlWebRequestAsync(WebServiceUrl, verifyXml);

            result = XmlHelper.GetNodeValueFromXml(result, "result");

            //  This result is actually: TotalAmount
            //  it must be equals to TotalAmount in database.
            int integerResult = Convert.ToInt32(result);

            isSuccess = integerResult > 0 && integerResult == verifyPaymentContext.Amount;

            status = isSuccess ? VerifyResultStatus.Success : VerifyResultStatus.Failed;

            resultTranslator = new SamanResultTranslator();

            translatedMessage = resultTranslator.Translate(integerResult);

            return new VerifyResult(Gateway.Saman, referenceId, transactionId, status, translatedMessage);
        }

        public override RefundResult Refund(GatewayRefundPaymentContext refundPaymentContext)
        {
            string reverseXml = CreateReverseWebServiceXml(
                SamanConfiguration.MerchantId,
                SamanConfiguration.Password,
                refundPaymentContext.TransactionId,
                refundPaymentContext.Amount);

            var result = WebHelper.SendXmlWebRequest(WebServiceUrl, reverseXml);

            result = XmlHelper.GetNodeValueFromXml(result, "result");

            int integerResult = Convert.ToInt32(result);

            IGatewayResultTranslator gatewayResultTranslator = new SamanResultTranslator();

            var translatedMessage = gatewayResultTranslator.Translate(integerResult);

            var isSuccess = integerResult > 0;

            var status = isSuccess ? RefundResultStatus.Success : RefundResultStatus.Failed;

            return new RefundResult(Gateway.Saman, refundPaymentContext.Amount, status, translatedMessage);
        }

        public override async Task<RefundResult> RefundAsync(GatewayRefundPaymentContext refundPaymentContext)
        {
            string reverseXml = CreateReverseWebServiceXml(
                SamanConfiguration.MerchantId,
                SamanConfiguration.Password,
                refundPaymentContext.TransactionId,
                refundPaymentContext.Amount);

            var result = await WebHelper.SendXmlWebRequestAsync(WebServiceUrl, reverseXml);

            result = XmlHelper.GetNodeValueFromXml(result, "result");

            int integerResult = Convert.ToInt32(result);

            IGatewayResultTranslator gatewayResultTranslator = new SamanResultTranslator();

            var translatedMessage = gatewayResultTranslator.Translate(integerResult);

            var isSuccess = integerResult > 0;

            var status = isSuccess ? RefundResultStatus.Success : RefundResultStatus.Failed;

            return new RefundResult(Gateway.Saman, refundPaymentContext.Amount, status, translatedMessage);
        }

        private static string CreateRequestHtmlForm(string merchantId, string paymentPageUrl, long totalAmount, long reservationNumber, string redirectUrl)
        {
            return
                "<html>" +
                "<body>" +
                $"<form id=\"paymentForm\" action=\"{paymentPageUrl}\" method=\"post\" />" +
                $"<input type=\"hidden\" name=\"Amount\" value=\"{totalAmount}\" />" +
                $"<input type=\"hidden\" name=\"MID\" value=\"{merchantId}\" />" +
                $"<input type=\"hidden\" name=\"ResNum\" value=\"{reservationNumber}\" />" +
                $"<input type=\"hidden\" name=\"RedirectURL\" value=\"{redirectUrl}\" />" +
                "</form>" +
                "<script type=\"text/javascript\">" +
                "document.getElementById('paymentForm').submit();" +
                "</script>" +
                "</body>" +
                "</html>";
        }

        private static string CreateVerifyXml(string merchantId, object transactionId)
        {
            return
                "<soapenv:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:urn=\"urn:Foo\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<urn:verifyTransaction soapenv:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
                $"<String_1 xsi:type=\"xsd:string\">{transactionId}</String_1>" +
                $"<String_2 xsi:type=\"xsd:string\">{merchantId}</String_2>" +
                "</urn:verifyTransaction>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        private static string CreateReverseWebServiceXml(string merchantId, string password, object transactionId, long amount)
        {
            return
                "<soapenv:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:urn=\"urn:Foo\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<urn:reverseTransaction soapenv:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
                $"<String_1 xsi:type=\"xsd:string\">{transactionId}</String_1>" +
                $"<String_2 xsi:type=\"xsd:string\">{amount}</String_2>" +
                $"<Username xsi:type=\"xsd:string\">{merchantId}</Username>" +
                $"<Password xsi:type=\"xsd:string\">{password}</Password>" +
                "</urn:reverseTransaction>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }
    }
}