// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Parbad.Abstraction;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Utilities;

namespace Parbad.Gateway.IranKish
{
    internal static class IranKishHelper
    {
        public const string PaymentPageUrl = "https://ikc.shaparak.ir/TPayment/Payment/index";
        public const string BaseServiceUrl = "https://ikc.shaparak.ir/";
        public const string TokenWebServiceUrl = "/TToken/Tokens.svc";
        public const string VerifyWebServiceUrl = "/TVerify/Verify.svc";

        public static KeyValuePair<string, string> HttpRequestHeader => new KeyValuePair<string, string>("SOAPAction", "http://tempuri.org/ITokens/MakeToken");
        public static KeyValuePair<string, string> HttpVerifyHeader => new KeyValuePair<string, string>("SOAPAction", "http://tempuri.org/IVerify/KicccPaymentsVerification");

        private const string OkResult = "100";

        public static string CreateRequestData(Invoice invoice, IranKishGatewayAccount account)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<tem:MakeToken>" +
                $"<tem:amount>{(long)invoice.Amount}</tem:amount>" +
                $"<tem:merchantId>{account.MerchantId}</tem:merchantId>" +
                $"<tem:invoiceNo>{invoice.TrackingNumber}</tem:invoiceNo>" +
                "<tem:paymentId></tem:paymentId>" +
                "<tem:specialPaymentId></tem:specialPaymentId>" +
                $"<tem:revertURL>{invoice.CallbackUrl}</tem:revertURL>" +
                "<tem:description></tem:description>" +
                "</tem:MakeToken>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static PaymentRequestResult CreateRequestResult(
            string webServiceResponse,
            Invoice invoice,
            IranKishGatewayAccount account,
            IHttpContextAccessor httpContextAccessor,
            MessagesOptions messagesOptions)
        {
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "result", "http://schemas.datacontract.org/2004/07/Token");
            var message = XmlHelper.GetNodeValueFromXml(webServiceResponse, "message", "http://schemas.datacontract.org/2004/07/Token");
            var token = XmlHelper.GetNodeValueFromXml(webServiceResponse, "token", "http://schemas.datacontract.org/2004/07/Token");

            var isSucceed = result.Equals("true", StringComparison.OrdinalIgnoreCase) && !token.IsNullOrEmpty();

            if (!isSucceed)
            {
                if (message.IsNullOrEmpty())
                {
                    message = messagesOptions.InvalidDataReceivedFromGateway;
                }

                return PaymentRequestResult.Failed(message);
            }

            var transporter = new GatewayPost(
                httpContextAccessor,
                PaymentPageUrl,
                new Dictionary<string, string>
                {
                    {"merchantid", account.MerchantId},
                    {"token", token}
                });

            return PaymentRequestResult.Succeed(transporter, account.Name);
        }

        public static IranKishCallbackResult CreateCallbackResult(
            InvoiceContext context,
            IranKishGatewayAccount account,
            HttpRequest httpRequest,
            MessagesOptions messagesOptions)
        {
            httpRequest.TryGetParam("ResultCode", out var resultCode);
            httpRequest.Form.TryGetValue("Token", out var token);
            httpRequest.TryGetParam("MerchantId", out var merchantId);

            // Equals to TrackingNumber in Parbad system.
            httpRequest.TryGetParamAs<long>("InvoiceNumber", out var invoiceNumber);

            // Equals to TransactionCode in Parbad system.
            httpRequest.TryGetParam("ReferenceId", out var referenceId);

            var isSucceed = false;
            PaymentVerifyResult verifyResult = null;

            if (merchantId != account.MerchantId ||
                invoiceNumber != context.Payment.TrackingNumber ||
                token.IsNullOrEmpty())
            {
                verifyResult = new PaymentVerifyResult
                {
                    TrackingNumber = invoiceNumber,
                    TransactionCode = referenceId,
                    IsSucceed = false,
                    Message = messagesOptions.InvalidDataReceivedFromGateway
                };
            }
            else
            {
                var translatedMessage = IranKishGatewayResultTranslator.Translate(resultCode, messagesOptions);

                isSucceed = resultCode == OkResult;

                if (!isSucceed)
                {
                    verifyResult = new PaymentVerifyResult
                    {
                        TrackingNumber = invoiceNumber,
                        TransactionCode = referenceId,
                        IsSucceed = false,
                        Message = translatedMessage
                    };
                }
            }

            return new IranKishCallbackResult
            {
                IsSucceed = isSucceed,
                Token = token,
                InvoiceNumber = invoiceNumber,
                ReferenceId = referenceId,
                Result = verifyResult
            };
        }

        public static string CreateVerifyData(IranKishCallbackResult callbackResult, IranKishGatewayAccount account)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<tem:KicccPaymentsVerification>" +
                $"<tem:token>{callbackResult.Token}</tem:token>" +
                $"<tem:merchantId>{account.MerchantId}</tem:merchantId>" +
                $"<tem:referenceNumber>{callbackResult.ReferenceId}</tem:referenceNumber>" +
                "<tem:sha1Key></tem:sha1Key>" +
                "</tem:KicccPaymentsVerification>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static PaymentVerifyResult CreateVerifyResult(
            string webServiceResponse,
            InvoiceContext context,
            IranKishCallbackResult callbackResult,
            MessagesOptions messagesOptions)
        {
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "KicccPaymentsVerificationResult");

            // The result object is actually the amount of invoice . It must equal to invoice's amount.
            if (!long.TryParse(result, out var numericResult))
            {
                return new PaymentVerifyResult
                {
                    TrackingNumber = callbackResult.InvoiceNumber,
                    TransactionCode = callbackResult.ReferenceId,
                    IsSucceed = false,
                    Message = messagesOptions.InvalidDataReceivedFromGateway
                };
            }

            var isSuccess = numericResult != (long)context.Payment.Amount;

            var translatedMessage = isSuccess
                ? messagesOptions.PaymentSucceed
                : IranKishGatewayResultTranslator.Translate(result, messagesOptions);

            return new PaymentVerifyResult
            {
                TrackingNumber = callbackResult.InvoiceNumber,
                TransactionCode = callbackResult.ReferenceId,
                IsSucceed = true,
                Message = translatedMessage
            };
        }
    }
}
