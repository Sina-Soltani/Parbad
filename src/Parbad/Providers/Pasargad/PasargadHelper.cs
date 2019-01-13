using System;
using System.Security.Cryptography;
using System.Text;
using Parbad.Core;
using Parbad.Providers.Pasargad.Models;
using Parbad.Utilities;

namespace Parbad.Providers.Pasargad
{
    internal static class PasargadHelper
    {
        public const string PaymentPageUrl = "https://pep.shaparak.ir/gateway.aspx";
        public const string CheckPaymentPageUrl = "https://pep.shaparak.ir/CheckTransactionResult.aspx";
        public const string VerifyPaymentPageUrl = "https://pep.shaparak.ir/VerifyPayment.aspx";
        public const string RefundPaymentPageUrl = "https://pep.shaparak.ir/DoRefund.aspx";

        private const string ActionNumber = "1003";
        private const string RefundNumber = "1004";

        public static RequestResult CreateRequestResult(PasargadGatewayConfiguration configuration, Invoice invoice)
        {
            var invoiceDate = GetTimeStamp(DateTime.Now);

            var timeStamp = invoiceDate;

            var dataToSign = string.Format("#{0}#{1}#{2}#{3}#{4}#{5}#{6}#{7}#",
                configuration.MerchantCode,
                configuration.TerminalCode,
                invoice.OrderNumber,
                invoiceDate,
                invoice.Amount,
                invoice.CallbackUrl,
                ActionNumber,
                timeStamp);

            var signedData = SignData(configuration.PrivateKey, dataToSign);

            var htmlContent =
                "<html>" +
                "<body>" +
                $"<form id=\"paymentForm\" action=\"{PaymentPageUrl}\" method=\"post\" />" +
                $"<input type=\"hidden\" name=\"merchantCode\" value=\"{configuration.MerchantCode}\" />" +
                $"<input type=\"hidden\" name=\"terminalCode\" value=\"{configuration.TerminalCode}\" />" +
                $"<input type=\"hidden\" name=\"invoiceNumber\" value=\"{invoice.OrderNumber}\" />" +
                $"<input type=\"hidden\" name=\"invoiceDate\" value=\"{invoiceDate}\" />" +
                $"<input type=\"hidden\" name=\"amount\" value=\"{invoice.Amount}\" />" +
                $"<input type=\"hidden\" name=\"redirectAddress\" value=\"{invoice.CallbackUrl}\" />" +
                $"<input type=\"hidden\" name=\"action\" value=\"{ActionNumber}\" />" +
                $"<input type=\"hidden\" name=\"timeStamp\" value=\"{timeStamp}\" />" +
                $"<input type=\"hidden\" name=\"sign\" value=\"{signedData}\" />" +
                "</form>" +
                "<script type=\"text/javascript\">" +
                "document.getElementById('paymentForm').submit();" +
                "</script>" +
                "</body>" +
                "</html>";

            const string message = "درخواست آماده ارسال است.";

            return new RequestResult(RequestResultStatus.Success, message, invoice.OrderNumber.ToString())
            {
                BehaviorMode = GatewayRequestBehaviorMode.Post,
                Content = htmlContent,
                AdditionalData = timeStamp
            };
        }

        public static PasargadCallbackResult CreateCallbackResult(GatewayVerifyPaymentContext context)
        {
            //  Reference ID
            var invoiceNumber = context.RequestParameters.GetAs<string>("iN");

            //  Invoice Date
            var invoiceDate = context.RequestParameters.GetAs<string>("iD");

            //  Transaction ID
            var transactionId = context.RequestParameters.GetAs<string>("tref");

            var isSucceed = true;
            VerifyResult verifyResult = null;

            if (string.IsNullOrWhiteSpace(invoiceNumber) ||
                string.IsNullOrWhiteSpace(invoiceDate) ||
                string.IsNullOrWhiteSpace(transactionId))
            {
                isSucceed = false;

                verifyResult = new VerifyResult(Gateway.Pasargad, invoiceNumber, transactionId, VerifyResultStatus.Failed, "اطلاعات دریافت شده از درگاه بانک نامعتبر است.");
            }

            var data = $"invoiceUID={transactionId}";

            return new PasargadCallbackResult
            {
                IsSucceed = isSucceed,
                InvoiceNumber = invoiceNumber,
                InvoiceDate = invoiceDate,
                TransactionId = transactionId,
                CallbackCheckData = data,
                Result = verifyResult
            };
        }

        public static PasargadCheckCallbackResult CreateCheckCallbackResult(string webServiceResponse, PasargadGatewayConfiguration configuration, PasargadCallbackResult callbackResult)
        {
            var compareReferenceId = XmlHelper.GetNodeValueFromXml(webServiceResponse, "invoiceNumber");
            var compareAction = XmlHelper.GetNodeValueFromXml(webServiceResponse, "action");
            var compareMerchantCode = XmlHelper.GetNodeValueFromXml(webServiceResponse, "merchantCode");
            var compareTerminalCode = XmlHelper.GetNodeValueFromXml(webServiceResponse, "terminalCode");

            bool isSucceed;
            VerifyResult verifyResult = null;

            if (compareReferenceId.IsNullOrWhiteSpace() ||
                compareAction.IsNullOrWhiteSpace() ||
                compareMerchantCode.IsNullOrWhiteSpace() ||
                compareTerminalCode.IsNullOrWhiteSpace())
            {
                isSucceed = false;

                verifyResult = new VerifyResult(Gateway.Pasargad, callbackResult.InvoiceNumber, callbackResult.TransactionId, VerifyResultStatus.Failed, "اطلاعات دریافت شده از درگاه بانک نامعتبر است.");
            }
            else
            {
                var responseResult = XmlHelper.GetNodeValueFromXml(webServiceResponse, "result");

                isSucceed = responseResult.Equals("true", StringComparison.OrdinalIgnoreCase) &&
                            compareReferenceId == callbackResult.InvoiceNumber &&
                            compareAction == ActionNumber &&
                            compareMerchantCode == configuration.MerchantCode &&
                            compareTerminalCode == configuration.TerminalCode;

                if (!isSucceed)
                {
                    verifyResult = new VerifyResult(Gateway.Pasargad, callbackResult.InvoiceNumber, callbackResult.TransactionId, VerifyResultStatus.Failed, "پرداخت موفقيت آميز نبود و يا توسط خريدار کنسل شده است");
                }
            }

            return new PasargadCheckCallbackResult
            {
                IsSucceed = isSucceed,
                Result = verifyResult
            };
        }

        public static string CreateVerifyData(PasargadGatewayConfiguration configuration, GatewayVerifyPaymentContext context, PasargadCallbackResult callbackResult)
        {
            var timeStamp = GetTimeStamp(DateTime.Now);

            var dataToSign = string.Format("#{0}#{1}#{2}#{3}#{4}#{5}#",
                configuration.MerchantCode,
                configuration.TerminalCode,
                context.ReferenceId,
                callbackResult.InvoiceDate,
                (long)context.Amount,
                timeStamp);

            var signData = SignData(configuration.PrivateKey, dataToSign);

            return "InvoiceNumber=" + context.ReferenceId +
                       "&InvoiceDate=" + callbackResult.InvoiceDate +
                       "&MerchantCode=" + configuration.MerchantCode +
                       "&TerminalCode=" + configuration.TerminalCode +
                       "&Amount=" + (long)context.Amount +
                       "&TimeStamp=" + timeStamp +
                       "&Sign=" + signData;
        }

        public static VerifyResult CreateVerifyResult(string webServiceResponse, PasargadCallbackResult callbackResult)
        {
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "result");

            var resultMessage = XmlHelper.GetNodeValueFromXml(webServiceResponse, "resultMessage");

            var status = result.Equals("true", StringComparison.OrdinalIgnoreCase) ? VerifyResultStatus.Success : VerifyResultStatus.Failed;

            return new VerifyResult(Gateway.Pasargad, callbackResult.InvoiceNumber, callbackResult.TransactionId, status, resultMessage);
        }

        public static string CreateRefundData(GatewayRefundPaymentContext context, PasargadGatewayConfiguration configuration)
        {
            var invoiceDate = context.AdditionalData;

            var timeStamp = GetTimeStamp(DateTime.Now);

            var dataToSign = string.Format("#{0}#{1}#{2}#{3}#{4}#{5}#{6}#",
                configuration.MerchantCode,
                configuration.TerminalCode,
                context.OrderNumber,
                invoiceDate,
                context.Amount,
                RefundNumber,
                timeStamp);

            var signedData = SignData(configuration.PrivateKey, dataToSign);

            return "InvoiceNumber=" + context.OrderNumber +
                       "&InvoiceDate=" + invoiceDate +
                       "&MerchantCode=" + configuration.MerchantCode +
                       "&TerminalCode=" + configuration.TerminalCode +
                       "&Amount=" + context.Amount +
                       "&action=" + RefundNumber +
                       "&TimeStamp=" + timeStamp +
                       "&Sign=" + signedData;
        }

        public static RefundResult CreateRefundResult(string webServiceResponse, GatewayRefundPaymentContext context)
        {
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "result");

            var resultMessage = XmlHelper.GetNodeValueFromXml(webServiceResponse, "resultMessage");

            var status = result.Equals("true", StringComparison.OrdinalIgnoreCase) ? RefundResultStatus.Success : RefundResultStatus.Failed;

            return new RefundResult(Gateway.Pasargad, context.Amount, status, resultMessage);
        }

        public static bool IsPrivateKeyValid(string privateKey)
        {
            try
            {
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(privateKey);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string SignData(string privateKey, string dataToSign)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);

                var encryptedData = rsa.SignData(Encoding.UTF8.GetBytes(dataToSign), new SHA1CryptoServiceProvider());

                return Convert.ToBase64String(encryptedData);
            }
        }

        private static string GetTimeStamp(DateTime dateTime)
        {
            return dateTime.ToString("yyyy/MM/dd HH:mm:ss");
        }
    }
}