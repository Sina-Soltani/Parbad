// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Parbad.Abstraction;
using Parbad.Gateway.Saman.Internal.Models;
using Parbad.Gateway.Saman.Internal.ResultTranslators;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using Parbad.Storage.Abstractions;
using Parbad.Utilities;

namespace Parbad.Gateway.Saman.Internal
{
    internal static class SamanHelper
    {
        public const string MobileGatewayKey = "UseMobileGateway";
        public const string AdditionalVerificationDataKey = "SamanAdditionalVerificationData";
        public const string BaseServiceUrl = "https://sep.shaparak.ir/";
        public static string PaymentPageUrl => $"{BaseServiceUrl}payment.aspx";
        public const string WebServiceUrl = "/payments/referencepayment.asmx";
        public const string TokenWebServiceUrl = "/payments/initpayment.asmx";
        public const string MobilePaymentTokenUrl = "/MobilePG/MobilePayment";
        public static string MobilePaymentPageUrl => $"{BaseServiceUrl}OnlinePG/OnlinePG";
        public const string MobileVerifyPaymentUrl = "https://verify.sep.ir/Payments/ReferencePayment.asmx";

        public static Task<PaymentRequestResult> CreateRequest(
            Invoice invoice,
            HttpContext httpContext,
            SamanGatewayAccount account,
            HttpClient httpClient,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken)
        {
            if (invoice.IsSamanMobileGatewayEnabled())
            {
                return CreateMobilePaymentRequest(invoice, httpContext, account, httpClient, messagesOptions, cancellationToken);
            }

            return CreateWebPaymentRequest(invoice, httpContext, account, httpClient, messagesOptions, cancellationToken);
        }
        public static async Task<SamanCallbackResult> CreateCallbackResultAsync(
            HttpRequest httpRequest,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken)
        {
            var isSuccess = false;
            PaymentVerifyResult verifyResult = null;
            StringValues referenceId = "";
            StringValues transactionId = "";

            var securePan = await httpRequest.TryGetParamAsync("SecurePan", cancellationToken).ConfigureAwaitFalse();
            var cid = await httpRequest.TryGetParamAsync("CID", cancellationToken).ConfigureAwaitFalse();
            var traceNo = await httpRequest.TryGetParamAsync("TraceNo", cancellationToken).ConfigureAwaitFalse();
            var rrn = await httpRequest.TryGetParamAsync("RRN", cancellationToken).ConfigureAwaitFalse();

            var state = await httpRequest.TryGetParamAsync("state", cancellationToken).ConfigureAwaitFalse();

            if (!state.Exists || state.Value.IsNullOrEmpty())
            {
                verifyResult = PaymentVerifyResult.Failed(messagesOptions.InvalidDataReceivedFromGateway);
            }
            else
            {
                var referenceIdResult = await httpRequest.TryGetParamAsync("ResNum", cancellationToken).ConfigureAwaitFalse();
                if (referenceIdResult.Exists) referenceId = referenceIdResult.Value;

                var transactionIdResult = await httpRequest.TryGetParamAsync("RefNum", cancellationToken).ConfigureAwaitFalse();
                if (transactionIdResult.Exists) transactionId = transactionIdResult.Value;

                isSuccess = state.Value.Equals("OK", StringComparison.OrdinalIgnoreCase);

                if (!isSuccess)
                {
                    var message = SamanStateTranslator.Translate(state.Value, messagesOptions);

                    verifyResult = PaymentVerifyResult.Failed(message);
                }
            }

            return new SamanCallbackResult
            {
                IsSucceed = isSuccess,
                ReferenceId = referenceId,
                TransactionId = transactionId,
                SecurePan = securePan.Value,
                Cid = cid.Value,
                TraceNo = traceNo.Value,
                Rrn = rrn.Value,
                Result = verifyResult
            };
        }

        public static string CreateVerifyData(SamanCallbackResult callbackResult, SamanGatewayAccount account)
        {
            return
                "<soapenv:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:urn=\"urn:Foo\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<urn:verifyTransaction soapenv:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
                $"<String_1 xsi:type=\"xsd:string\">{callbackResult.TransactionId}</String_1>" +
                $"<String_2 xsi:type=\"xsd:string\">{account.MerchantId}</String_2>" +
                "</urn:verifyTransaction>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static PaymentVerifyResult CreateVerifyResult(string webServiceResponse, InvoiceContext context, SamanCallbackResult callbackResult, MessagesOptions messagesOptions)
        {
            var stringResult = XmlHelper.GetNodeValueFromXml(webServiceResponse, "result");

            //  This result is actually: TotalAmount
            //  it must be equals to TotalAmount in database.
            var numericResult = Convert.ToInt64(stringResult);

            var isSuccess = numericResult > 0 && numericResult == (long)context.Payment.Amount;

            var message = isSuccess
                ? messagesOptions.PaymentSucceed
                : SamanResultTranslator.Translate(numericResult, messagesOptions);

            var result = new PaymentVerifyResult
            {
                Status = isSuccess ? PaymentVerifyResultStatus.Succeed : PaymentVerifyResultStatus.Failed,
                TransactionCode = callbackResult.TransactionId,
                Message = message
            };

            result.AdditionalData.Add(AdditionalVerificationDataKey, new SamanAdditionalVerificationData
            {
                Cid = callbackResult.Cid,
                TraceNo = callbackResult.TraceNo,
                SecurePan = callbackResult.SecurePan,
                Rrn = callbackResult.Rrn
            });

            return result;
        }

        public static string CreateRefundData(InvoiceContext context, Money amount, SamanGatewayAccount account)
        {
            return
                "<soapenv:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:urn=\"urn:Foo\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<urn:reverseTransaction soapenv:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
                $"<String_1 xsi:type=\"xsd:string\">{context.Payment.TransactionCode}</String_1>" +
                $"<String_2 xsi:type=\"xsd:string\">{(long)amount}</String_2>" +
                $"<Username xsi:type=\"xsd:string\">{account.MerchantId}</Username>" +
                $"<Password xsi:type=\"xsd:string\">{account.Password}</Password>" +
                "</urn:reverseTransaction>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static PaymentRefundResult CreateRefundResult(string webServiceResponse, MessagesOptions messagesOptions)
        {
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "result");

            var integerResult = Convert.ToInt32(result);

            var isSucceed = integerResult > 0;

            var message = SamanResultTranslator.Translate(integerResult, messagesOptions);

            return new PaymentRefundResult
            {
                Status = isSucceed ? PaymentRefundResultStatus.Succeed : PaymentRefundResultStatus.Failed,
                Message = message
            };
        }

        private static async Task<PaymentRequestResult> CreateWebPaymentRequest(
            Invoice invoice,
            HttpContext httpContext,
            SamanGatewayAccount account,
            HttpClient httpClient,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken)
        {
            var data = CreateSoapRequest(invoice, account);

            var responseMessage = await httpClient.PostXmlAsync(TokenWebServiceUrl, data, cancellationToken);

            var response = await responseMessage.Content.ReadAsStringAsync();

            var token = XmlHelper.GetNodeValueFromXml(response, "result");

            string message = null;
            var isSucceed = true;

            if (token.IsNullOrEmpty())
            {
                message = $"{messagesOptions.InvalidDataReceivedFromGateway} Token is null or empty.";
                isSucceed = false;
            }
            else if (long.TryParse(token, out var longToken) && longToken < 0)
            {
                message = SamanResultTranslator.Translate(longToken, messagesOptions);
                isSucceed = false;
            }

            if (!isSucceed)
            {
                return PaymentRequestResult.Failed(message, account.Name);
            }

            return PaymentRequestResult.SucceedWithPost(
                account.Name,
                httpContext,
                PaymentPageUrl,
                new Dictionary<string, string>
                {
                    {"Token", token},
                    {"RedirectURL", invoice.CallbackUrl}
                });
        }

        private static async Task<PaymentRequestResult> CreateMobilePaymentRequest(
            Invoice invoice,
            HttpContext httpContext,
            SamanGatewayAccount account,
            HttpClient httpClient,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken)
        {
            var data = new SamanMobilePaymentTokenRequest
            {
                TerminalId = account.MerchantId,
                ResNum = invoice.TrackingNumber.ToString(),
                Amount = invoice.Amount,
                RedirectUrl = invoice.CallbackUrl,
                Action = "Token"
            };

            var responseMessage = await httpClient.PostJsonAsync(MobilePaymentTokenUrl, data, cancellationToken);

            var response = await responseMessage.Content.ReadAsStringAsync();

            var tokenResponse = JsonConvert.DeserializeObject<SamanMobilePaymentTokenResponse>(response);

            if (tokenResponse == null)
            {
                var message = $"{messagesOptions.InvalidDataReceivedFromGateway} Serialized token response is null.";
                return PaymentRequestResult.Failed(message, account.Name);
            }

            if (tokenResponse.Status == -1)
            {
                return PaymentRequestResult.Failed(tokenResponse.GetError(), account.Name);
            }

            var result = PaymentRequestResult.SucceedWithPost(
                account.Name,
                httpContext,
                MobilePaymentPageUrl,
                new Dictionary<string, string>
                {
                    {"Token", tokenResponse.Token}
                });

            result.DatabaseAdditionalData.Add(MobileGatewayKey, true.ToString());

            return result;
        }

        private static string CreateSoapRequest(Invoice invoice, SamanGatewayAccount account)
        {
            return
                "<soapenv:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:urn=\"urn:Foo\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<urn:RequestToken soapenv:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
                $"<TermID xsi:type=\"xsd:string\">{account.MerchantId}</TermID>" +
                $"<ResNum xsi:type=\"xsd:string\">{invoice.TrackingNumber}</ResNum>" +
                $"<TotalAmount xsi:type=\"xsd:long\">{(long)invoice.Amount}</TotalAmount>" +
                "</urn:RequestToken>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static string GetVerificationUrl(InvoiceContext invoiceContext)
        {
            var record = invoiceContext
                .Transactions
                .SingleOrDefault(transaction => transaction.Type == TransactionType.Request);

            if (record == null || record.AdditionalData.IsNullOrEmpty()) return WebServiceUrl;

            if (AdditionalDataConverter.ToDictionary(record).TryGetValue(MobileGatewayKey, out var isMobileGatewayEnabled) && bool.Parse(isMobileGatewayEnabled))
            {
                return MobileVerifyPaymentUrl;
            }

            return WebServiceUrl;
        }
    }
}
