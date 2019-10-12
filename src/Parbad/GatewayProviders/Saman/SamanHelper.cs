// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Parbad.Abstraction;
using Parbad.GatewayProviders.Saman.Models;
using Parbad.GatewayProviders.Saman.ResultTranslators;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Utilities;

namespace Parbad.GatewayProviders.Saman
{
    internal static class SamanHelper
    {
        public const string PaymentPageUrl = "https://sep.shaparak.ir/payment.aspx";
        public const string BaseServiceUrl = "https://sep.shaparak.ir/";
        public const string WebServiceUrl = "/payments/referencepayment.asmx";

        public static PaymentRequestResult CreateRequestResult(Invoice invoice, IHttpContextAccessor httpContextAccessor, SamanGatewayAccount account)
        {
            var transporter = new GatewayPost(
                httpContextAccessor,
                PaymentPageUrl,
                new Dictionary<string, string>
                {
                    {"Amount", invoice.Amount.ToLongString()},
                    {"MID", account.MerchantId},
                    {"ResNum", invoice.TrackingNumber.ToString()},
                    {"RedirectURL", invoice.CallbackUrl}
                });

            return PaymentRequestResult.Succeed(transporter, account.Name);
        }

        public static SamanCallbackResult CreateCallbackResult(HttpRequest httpRequest, MessagesOptions messagesOptions)
        {
            var isSuccess = false;
            PaymentVerifyResult verifyResult = null;
            StringValues referenceId;
            StringValues transactionId;

            httpRequest.TryGetParam("state", out var state);

            if (state.IsNullOrEmpty())
            {
                verifyResult = PaymentVerifyResult.Failed(messagesOptions.InvalidDataReceivedFromGateway);
            }
            else
            {
                httpRequest.TryGetParam("ResNum", out referenceId);

                httpRequest.TryGetParam("RefNum", out transactionId);

                isSuccess = state.Equals("OK", StringComparison.OrdinalIgnoreCase);

                if (!isSuccess)
                {
                    var message = SamanStateTranslator.Translate(state, messagesOptions);

                    verifyResult = PaymentVerifyResult.Failed(message);
                }
            }

            return new SamanCallbackResult
            {
                IsSucceed = isSuccess,
                ReferenceId = referenceId,
                TransactionId = transactionId,
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
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "result");

            //  This result is actually: TotalAmount
            //  it must be equals to TotalAmount in database.
            var numericResult = Convert.ToInt64(result);

            var isSuccess = numericResult > 0 && numericResult == (long)context.Payment.Amount;

            var message = isSuccess
                ? messagesOptions.PaymentSucceed
                : SamanResultTranslator.Translate(numericResult, messagesOptions);

            return new PaymentVerifyResult
            {
                IsSucceed = isSuccess,
                TransactionCode = callbackResult.TransactionId,
                Message = message
            };
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
                IsSucceed = isSucceed,
                Message = message
            };
        }
    }
}
