using System;
using System.Threading.Tasks;
using Parbad.Configurations;
using Parbad.Core;
using Parbad.Utilities;

namespace Parbad.Providers.Pasargad
{
    internal class PasargadGateway : GatewayBase
    {
        private const string PaymentPageUrl = "https://pep.shaparak.ir/gateway.aspx";
        private const string CheckPaymentPageUrl = "https://pep.shaparak.ir/CheckTransactionResult.aspx";
        private const string VerifyPaymentPageUrl = "https://pep.shaparak.ir/VerifyPayment.aspx";
        private const string RefundPaymentPageUrl = "https://pep.shaparak.ir/DoRefund.aspx";
        private const string ActionNumber = "1003";
        private const string RefundNumber = "1004";

        protected PasargadGatewayConfiguration PasargadConfiguration => ParbadConfiguration.Gateways.GetPasargadGatewayConfiguration();

        public PasargadGateway() : base(Gateway.Pasargad.ToString())
        {
        }

        public override RequestResult Request(Invoice invoice)
        {
            var invoiceDate = PasargadHelper.GetTimeStamp(DateTime.Now);

            var timeStamp = invoiceDate;

            var dataToSign = string.Format("#{0}#{1}#{2}#{3}#{4}#{5}#{6}#{7}#",
                PasargadConfiguration.MerchantCode,
                PasargadConfiguration.TerminalCode,
                invoice.OrderNumber,
                invoiceDate,
                invoice.Amount,
                invoice.CallbackUrl,
                ActionNumber,
                timeStamp);

            var signData = PasargadHelper.SignData(PasargadConfiguration.PrivateKey, dataToSign);

            var htmlForm = CreateRequestHtmlForm(
                PasargadConfiguration.MerchantCode,
                PasargadConfiguration.TerminalCode,
                invoice.OrderNumber,
                invoiceDate,
                invoice.Amount,
                invoice.CallbackUrl,
                ActionNumber,
                timeStamp,
                signData,
                PaymentPageUrl);

            return new RequestResult(RequestResultStatus.Success, string.Empty, invoice.OrderNumber.ToString())
            {
                BehaviorMode = GatewayRequestBehaviorMode.Post,
                Content = htmlForm,
                AdditionalData = timeStamp
            };
        }

        public override Task<RequestResult> RequestAsync(Invoice invoice)
        {
            return Task.FromResult(Request(invoice));
        }

        public override VerifyResult Verify(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            //  Reference ID
            var referenceId = verifyPaymentContext.RequestParameters.GetAs<string>("iN");

            //  Invoice Date
            var invoiceDate = verifyPaymentContext.RequestParameters.GetAs<string>("iD");

            //  Transaction ID
            var transactionId = verifyPaymentContext.RequestParameters.GetAs<string>("tref");

            if (string.IsNullOrWhiteSpace(referenceId) ||
                string.IsNullOrWhiteSpace(invoiceDate) ||
                string.IsNullOrWhiteSpace(transactionId))
            {
                return new VerifyResult(Gateway.Pasargad, referenceId, transactionId, VerifyResultStatus.Failed, "اطلاعات دریافت شده از درگاه بانک نامعتبر است.");
            }

            var callbackRequestData = CreateCallbackWebRequestData(transactionId);

            var callBackResponse = WebHelper.SendWebRequest(CheckPaymentPageUrl, callbackRequestData, "POST", "application/x-www-form-urlencoded");

            var compareReferenceId = XmlHelper.GetNodeValueFromXml(callBackResponse, "invoiceNumber");
            var compareAction = XmlHelper.GetNodeValueFromXml(callBackResponse, "action");
            var compareMerchantCode = XmlHelper.GetNodeValueFromXml(callBackResponse, "merchantCode");
            var compareTerminalCode = XmlHelper.GetNodeValueFromXml(callBackResponse, "terminalCode");

            if (string.IsNullOrWhiteSpace(compareReferenceId) ||
                string.IsNullOrWhiteSpace(compareAction) ||
                string.IsNullOrWhiteSpace(compareMerchantCode) ||
                string.IsNullOrWhiteSpace(compareTerminalCode))
            {
                return new VerifyResult(Gateway.Pasargad, referenceId, transactionId, VerifyResultStatus.Failed, "اطلاعات دریافت شده از درگاه بانک نامعتبر است.");
            }

            var callbackResult = XmlHelper.GetNodeValueFromXml(callBackResponse, "result");

            var isCallbackSuccess = callbackResult.Equals("true", StringComparison.InvariantCultureIgnoreCase) &&
                            compareReferenceId == referenceId &&
                            compareAction == ActionNumber &&
                            compareMerchantCode == PasargadConfiguration.MerchantCode &&
                            compareTerminalCode == PasargadConfiguration.TerminalCode;

            if (!isCallbackSuccess)
            {
                return new VerifyResult(Gateway.Pasargad, referenceId, transactionId, VerifyResultStatus.Failed, "پرداخت موفقيت آميز نبود و يا توسط خريدار کنسل شده است");
            }

            var timeStamp = PasargadHelper.GetTimeStamp(DateTime.Now);

            var dataToSign = string.Format("#{0}#{1}#{2}#{3}#{4}#{5}#",
                PasargadConfiguration.MerchantCode,
                PasargadConfiguration.TerminalCode,
                verifyPaymentContext.ReferenceId,
                invoiceDate,
                (long)verifyPaymentContext.Amount,
                timeStamp);

            var signData = PasargadHelper.SignData(PasargadConfiguration.PrivateKey, dataToSign);

            var data = "InvoiceNumber=" + verifyPaymentContext.ReferenceId +
                       "&InvoiceDate=" + invoiceDate +
                       "&MerchantCode=" + PasargadConfiguration.MerchantCode +
                       "&TerminalCode=" + PasargadConfiguration.TerminalCode +
                       "&Amount=" + (long)verifyPaymentContext.Amount +
                       "&TimeStamp=" + timeStamp +
                       "&Sign=" + signData;

            var response = WebHelper.SendWebRequest(VerifyPaymentPageUrl, data, "POST", "application/x-www-form-urlencoded");

            var result = XmlHelper.GetNodeValueFromXml(response, "result");

            var resultMessage = XmlHelper.GetNodeValueFromXml(response, "resultMessage");

            var isSuccess = result.Equals("true", StringComparison.InvariantCultureIgnoreCase);

            var status = isSuccess ? VerifyResultStatus.Success : VerifyResultStatus.Failed;

            return new VerifyResult(Gateway.Pasargad, referenceId, transactionId, status, resultMessage);
        }

        public override async Task<VerifyResult> VerifyAsync(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            //  Reference ID
            var referenceId = verifyPaymentContext.RequestParameters.GetAs<string>("iN");

            //  Invoice Date
            var invoiceDate = verifyPaymentContext.RequestParameters.GetAs<string>("iD");

            //  Transaction ID
            var transactionId = verifyPaymentContext.RequestParameters.GetAs<string>("tref");

            if (string.IsNullOrWhiteSpace(referenceId) ||
                string.IsNullOrWhiteSpace(invoiceDate) ||
                string.IsNullOrWhiteSpace(transactionId))
            {
                return new VerifyResult(Gateway.Pasargad, referenceId, transactionId, VerifyResultStatus.Failed, "اطلاعات دریافت شده از درگاه بانک نامعتبر است.");
            }

            var callbackRequestData = CreateCallbackWebRequestData(transactionId);

            var callBackResponse = await WebHelper.SendWebRequestAsync(CheckPaymentPageUrl, callbackRequestData, "POST", "application/x-www-form-urlencoded");

            var compareReferenceId = XmlHelper.GetNodeValueFromXml(callBackResponse, "invoiceNumber");
            var compareAction = XmlHelper.GetNodeValueFromXml(callBackResponse, "action");
            var compareMerchantCode = XmlHelper.GetNodeValueFromXml(callBackResponse, "merchantCode");
            var compareTerminalCode = XmlHelper.GetNodeValueFromXml(callBackResponse, "terminalCode");

            if (string.IsNullOrWhiteSpace(compareReferenceId) ||
                string.IsNullOrWhiteSpace(compareAction) ||
                string.IsNullOrWhiteSpace(compareMerchantCode) ||
                string.IsNullOrWhiteSpace(compareTerminalCode))
            {
                return new VerifyResult(Gateway.Pasargad, referenceId, transactionId, VerifyResultStatus.Failed, "اطلاعات دریافت شده از درگاه بانک نامعتبر است.");
            }

            var callbackResult = XmlHelper.GetNodeValueFromXml(callBackResponse, "result");

            var isCallbackSuccess = callbackResult.Equals("true", StringComparison.InvariantCultureIgnoreCase) &&
                            compareReferenceId == referenceId &&
                            compareAction == ActionNumber &&
                            compareMerchantCode == PasargadConfiguration.MerchantCode &&
                            compareTerminalCode == PasargadConfiguration.TerminalCode;

            if (!isCallbackSuccess)
            {
                return new VerifyResult(Gateway.Pasargad, referenceId, transactionId, VerifyResultStatus.Failed, "پرداخت موفقيت آميز نبود و يا توسط خريدار کنسل شده است");
            }

            var timeStamp = PasargadHelper.GetTimeStamp(DateTime.Now);

            var dataToSign = string.Format("#{0}#{1}#{2}#{3}#{4}#{5}#",
                PasargadConfiguration.MerchantCode,
                PasargadConfiguration.TerminalCode,
                verifyPaymentContext.ReferenceId,
                invoiceDate,
                (long)verifyPaymentContext.Amount,
                timeStamp);

            var signData = PasargadHelper.SignData(PasargadConfiguration.PrivateKey, dataToSign);

            var data = "InvoiceNumber=" + verifyPaymentContext.ReferenceId +
                       "&InvoiceDate=" + invoiceDate +
                       "&MerchantCode=" + PasargadConfiguration.MerchantCode +
                       "&TerminalCode=" + PasargadConfiguration.TerminalCode +
                       "&Amount=" + (long)verifyPaymentContext.Amount +
                       "&TimeStamp=" + timeStamp +
                       "&Sign=" + signData;

            var response = await WebHelper.SendWebRequestAsync(VerifyPaymentPageUrl, data, "POST", "application/x-www-form-urlencoded");

            var result = XmlHelper.GetNodeValueFromXml(response, "result");

            var resultMessage = XmlHelper.GetNodeValueFromXml(response, "resultMessage");

            var isSuccess = result.Equals("true", StringComparison.InvariantCultureIgnoreCase);

            var status = isSuccess ? VerifyResultStatus.Success : VerifyResultStatus.Failed;

            return new VerifyResult(Gateway.Pasargad, referenceId, transactionId, status, resultMessage);
        }

        public override RefundResult Refund(GatewayRefundPaymentContext refundPaymentContext)
        {
            var invoiceDate = refundPaymentContext.AdditionalData;

            var timeStamp = PasargadHelper.GetTimeStamp(DateTime.Now);

            var dataToSign = string.Format("#{0}#{1}#{2}#{3}#{4}#{5}#{6}#",
                PasargadConfiguration.MerchantCode,
                PasargadConfiguration.TerminalCode,
                refundPaymentContext.OrderNumber,
                invoiceDate,
                refundPaymentContext.Amount,
                RefundNumber,
                timeStamp);

            var signedData = PasargadHelper.SignData(PasargadConfiguration.PrivateKey, dataToSign);

            var data = "InvoiceNumber=" + refundPaymentContext.OrderNumber +
                "&InvoiceDate=" + invoiceDate +
                "&MerchantCode=" + PasargadConfiguration.MerchantCode +
                "&TerminalCode=" + PasargadConfiguration.TerminalCode +
                "&Amount=" + refundPaymentContext.Amount +
                "&action=" + RefundNumber +
                "&TimeStamp=" + timeStamp +
                "&Sign=" + signedData;

            var response = WebHelper.SendWebRequest(RefundPaymentPageUrl, data, "POST", "application/x-www-form-urlencoded");

            var result = XmlHelper.GetNodeValueFromXml(response, "result");

            var resultMessage = XmlHelper.GetNodeValueFromXml(response, "resultMessage");

            var isSuccess = result.Equals("true", StringComparison.InvariantCultureIgnoreCase);

            var status = isSuccess ? RefundResultStatus.Success : RefundResultStatus.Failed;

            return new RefundResult(Gateway.Pasargad, refundPaymentContext.Amount, status, resultMessage);
        }

        public override async Task<RefundResult> RefundAsync(GatewayRefundPaymentContext refundPaymentContext)
        {
            var invoiceDate = refundPaymentContext.AdditionalData;

            var timeStamp = PasargadHelper.GetTimeStamp(DateTime.Now);

            var dataToSign = string.Format("#{0}#{1}#{2}#{3}#{4}#{5}#{6}#",
                PasargadConfiguration.MerchantCode,
                PasargadConfiguration.TerminalCode,
                refundPaymentContext.OrderNumber,
                invoiceDate,
                refundPaymentContext.Amount,
                RefundNumber,
                timeStamp);

            var signedData = PasargadHelper.SignData(PasargadConfiguration.PrivateKey, dataToSign);

            var data = "InvoiceNumber=" + refundPaymentContext.OrderNumber +
                       "&InvoiceDate=" + invoiceDate +
                       "&MerchantCode=" + PasargadConfiguration.MerchantCode +
                       "&TerminalCode=" + PasargadConfiguration.TerminalCode +
                       "&Amount=" + refundPaymentContext.Amount +
                       "&action=" + RefundNumber +
                       "&TimeStamp=" + timeStamp +
                       "&Sign=" + signedData;

            var response = await WebHelper.SendWebRequestAsync(RefundPaymentPageUrl, data, "POST", "application/x-www-form-urlencoded");

            var result = XmlHelper.GetNodeValueFromXml(response, "result");

            var resultMessage = XmlHelper.GetNodeValueFromXml(response, "resultMessage");

            var isSuccess = result.Equals("true", StringComparison.InvariantCultureIgnoreCase);

            var status = isSuccess ? RefundResultStatus.Success : RefundResultStatus.Failed;

            return new RefundResult(Gateway.Pasargad, refundPaymentContext.Amount, status, resultMessage);
        }

        private static string CreateRequestHtmlForm(
            string merchantCode,
            string terminalCode,
            long invoiceNumber,
            string invoiceDate,
            long amount,
            string redirectAddress,
            string action,
            string timeStamp,
            string sign,
            string paymentPageUrl)
        {
            return
                "<html>" +
                "<body>" +
                $"<form id=\"paymentForm\" action=\"{paymentPageUrl}\" method=\"post\" />" +
                $"<input type=\"hidden\" name=\"merchantCode\" value=\"{merchantCode}\" />" +
                $"<input type=\"hidden\" name=\"terminalCode\" value=\"{terminalCode}\" />" +
                $"<input type=\"hidden\" name=\"invoiceNumber\" value=\"{invoiceNumber}\" />" +
                $"<input type=\"hidden\" name=\"invoiceDate\" value=\"{invoiceDate}\" />" +
                $"<input type=\"hidden\" name=\"amount\" value=\"{amount}\" />" +
                $"<input type=\"hidden\" name=\"redirectAddress\" value=\"{redirectAddress}\" />" +
                $"<input type=\"hidden\" name=\"action\" value=\"{action}\" />" +
                $"<input type=\"hidden\" name=\"timeStamp\" value=\"{timeStamp}\" />" +
                $"<input type=\"hidden\" name=\"sign\" value=\"{sign}\" />" +
                "</form>" +
                "<script type=\"text/javascript\">" +
                "document.getElementById('paymentForm').submit();" +
                "</script>" +
                "</body>" +
                "</html>";
        }

        private static string CreateCallbackWebRequestData(string transactionReferenceId)
        {
            return $"invoiceUID={transactionReferenceId}";
        }
    }
}