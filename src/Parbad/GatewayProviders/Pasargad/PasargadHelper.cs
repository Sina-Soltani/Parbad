// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Parbad.Abstraction;
using Parbad.Data.Domain.Payments;
using Parbad.Data.Domain.Transactions;
using Parbad.GatewayProviders.Pasargad.Models;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Utilities;

namespace Parbad.GatewayProviders.Pasargad
{
    internal static class PasargadHelper
    {
        public const string PaymentPageUrl = "https://pep.shaparak.ir/gateway.aspx";
        public const string BaseServiceUrl = "https://pep.shaparak.ir/";
        public const string CheckPaymentPageUrl = "/CheckTransactionResult.aspx";
        public const string VerifyPaymentPageUrl = "/VerifyPayment.aspx";
        public const string RefundPaymentPageUrl = "/DoRefund.aspx";

        private const string ActionNumber = "1003";
        private const string RefundNumber = "1004";

        public static PaymentRequestResult CreateRequestResult(Invoice invoice, IHttpContextAccessor httpContextAccessor, PasargadGatewayOptions options)
        {
            var invoiceDate = GetTimeStamp(DateTime.Now);

            var timeStamp = invoiceDate;

            var dataToSign = string.Format("#{0}#{1}#{2}#{3}#{4}#{5}#{6}#{7}#",
                options.MerchantCode,
                options.TerminalCode,
                invoice.TrackingNumber,
                invoiceDate,
                (long)invoice.Amount,
                invoice.CallbackUrl,
                ActionNumber,
                timeStamp);

            var signedData = SignData(options.PrivateKey, dataToSign);

            var transporter = new GatewayPost(
                httpContextAccessor,
                PaymentPageUrl,
                new Dictionary<string, string>
                {
                    {"merchantCode", options.MerchantCode },
                    {"terminalCode",  options.TerminalCode},
                    {"invoiceNumber",  invoice.TrackingNumber.ToString()},
                    {"invoiceDate", invoiceDate },
                    {"amount",  invoice.Amount.ToLongString()},
                    {"redirectAddress", invoice.CallbackUrl},
                    {"action",  ActionNumber},
                    {"timeStamp", timeStamp },
                    {"sign",  signedData}
                });

            var result = PaymentRequestResult.Succeed(transporter);

            result.DatabaseAdditionalData.Add("timeStamp", timeStamp);

            return result;
        }

        public static PasargadCallbackResult CreateCallbackResult(HttpRequest httpRequest, MessagesOptions messagesOptions)
        {
            //  Reference ID
            httpRequest.TryGetParam("iN", out var invoiceNumber);

            //  Invoice Date
            httpRequest.TryGetParam("iD", out var invoiceDate);

            //  Transaction Code
            httpRequest.TryGetParam("tref", out var transactionId);

            var isSucceed = true;
            PaymentVerifyResult verifyResult = null;

            if (string.IsNullOrWhiteSpace(invoiceNumber) ||
                string.IsNullOrWhiteSpace(invoiceDate) ||
                string.IsNullOrWhiteSpace(transactionId))
            {
                isSucceed = false;

                verifyResult = PaymentVerifyResult.Failed(messagesOptions.InvalidDataReceivedFromGateway);
            }

            var data = new[] { new KeyValuePair<string, string>("invoiceUID", transactionId) };

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

        public static PasargadCheckCallbackResult CreateCheckCallbackResult(string webServiceResponse, PasargadGatewayOptions options, PasargadCallbackResult callbackResult, MessagesOptions messagesOptions)
        {
            var compareReferenceId = XmlHelper.GetNodeValueFromXml(webServiceResponse, "invoiceNumber");
            var compareAction = XmlHelper.GetNodeValueFromXml(webServiceResponse, "action");
            var compareMerchantCode = XmlHelper.GetNodeValueFromXml(webServiceResponse, "merchantCode");
            var compareTerminalCode = XmlHelper.GetNodeValueFromXml(webServiceResponse, "terminalCode");

            bool isSucceed;
            PaymentVerifyResult verifyResult = null;

            if (compareReferenceId.IsNullOrWhiteSpace() ||
                compareAction.IsNullOrWhiteSpace() ||
                compareMerchantCode.IsNullOrWhiteSpace() ||
                compareTerminalCode.IsNullOrWhiteSpace())
            {
                isSucceed = false;

                verifyResult = PaymentVerifyResult.Failed(messagesOptions.InvalidDataReceivedFromGateway);
            }
            else
            {
                var responseResult = XmlHelper.GetNodeValueFromXml(webServiceResponse, "result");

                isSucceed = responseResult.Equals("true", StringComparison.OrdinalIgnoreCase) &&
                            compareReferenceId == callbackResult.InvoiceNumber &&
                            compareAction == ActionNumber &&
                            compareMerchantCode == options.MerchantCode &&
                            compareTerminalCode == options.TerminalCode;

                if (!isSucceed)
                {
                    verifyResult = PaymentVerifyResult.Failed("پرداخت موفقيت آميز نبود و يا توسط خريدار کنسل شده است");
                }
            }

            return new PasargadCheckCallbackResult
            {
                IsSucceed = isSucceed,
                Result = verifyResult
            };
        }

        public static IEnumerable<KeyValuePair<string, string>> CreateVerifyData(Payment payment, PasargadGatewayOptions options, PasargadCallbackResult callbackResult)
        {
            var timeStamp = GetTimeStamp(DateTime.Now);

            var dataToSign = string.Format("#{0}#{1}#{2}#{3}#{4}#{5}#",
                options.MerchantCode,
                options.TerminalCode,
                payment.TrackingNumber,
                callbackResult.InvoiceDate,
                (long)payment.Amount,
                timeStamp);

            var signData = SignData(options.PrivateKey, dataToSign);

            return new[]
            {
                new KeyValuePair<string, string>("InvoiceNumber", payment.TrackingNumber.ToString()),
                new KeyValuePair<string, string>("InvoiceDate", callbackResult.InvoiceDate),
                new KeyValuePair<string, string>("MerchantCode", options.MerchantCode),
                new KeyValuePair<string, string>("TerminalCode", options.TerminalCode),
                new KeyValuePair<string, string>("Amount", ((long)payment.Amount).ToString()),
                new KeyValuePair<string, string>("TimeStamp", timeStamp),
                new KeyValuePair<string, string>("Sign", signData)
            };
        }

        public static PaymentVerifyResult CreateVerifyResult(string webServiceResponse, PasargadCallbackResult callbackResult, MessagesOptions messagesOptions)
        {
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "result");

            var isSucceed = result.Equals("true", StringComparison.OrdinalIgnoreCase);

            var message = isSucceed
                ? messagesOptions.PaymentSucceed
                : XmlHelper.GetNodeValueFromXml(webServiceResponse, "resultMessage");

            return new PaymentVerifyResult
            {
                IsSucceed = isSucceed,
                TransactionCode = callbackResult.TransactionId,
                Message = message
            };
        }

        public static IEnumerable<KeyValuePair<string, string>> CreateRefundData(Payment payment, Money amount, PasargadGatewayOptions options)
        {
            var transactionRecord = payment.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.Request);

            if (transactionRecord == null)
            {
                throw new Exception($"Cannot find transaction record for Payment-{payment.TrackingNumber}");
            }

            if (!AdditionalDataConverter.ToDictionary(transactionRecord).TryGetValue("invoiceDate", out var invoiceDate))
            {
                throw new Exception("Cannot get the invoiceDate from database.");
            }

            var timeStamp = GetTimeStamp(DateTime.Now);

            var dataToSign = string.Format("#{0}#{1}#{2}#{3}#{4}#{5}#{6}#",
                options.MerchantCode,
                options.TerminalCode,
                payment.TrackingNumber,
                invoiceDate,
                (long)amount,
                RefundNumber,
                timeStamp);

            var signedData = SignData(options.PrivateKey, dataToSign);

            return new[]
            {
                new KeyValuePair<string, string>("InvoiceNumber", payment.TrackingNumber.ToString()),
                new KeyValuePair<string, string>("InvoiceDate", invoiceDate),
                new KeyValuePair<string, string>("MerchantCode", options.MerchantCode),
                new KeyValuePair<string, string>("TerminalCode", options.TerminalCode),
                new KeyValuePair<string, string>("Amount", amount.ToLongString()),
                new KeyValuePair<string, string>("action", RefundNumber),
                new KeyValuePair<string, string>("TimeStamp", timeStamp),
                new KeyValuePair<string, string>("Sign", signedData)
            };
        }

        public static PaymentRefundResult CreateRefundResult(string webServiceResponse, MessagesOptions messagesOptions)
        {
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "result");

            var isSucceed = result.Equals("true", StringComparison.OrdinalIgnoreCase);

            var message = isSucceed
                ? messagesOptions.PaymentSucceed
                : XmlHelper.GetNodeValueFromXml(webServiceResponse, "resultMessage");

            return new PaymentRefundResult
            {
                IsSucceed = isSucceed,
                Message = message
            };
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
