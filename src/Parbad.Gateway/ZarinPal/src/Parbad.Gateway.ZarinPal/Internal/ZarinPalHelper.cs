// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Parbad.Abstraction;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Utilities;

namespace Parbad.Gateway.ZarinPal.Internal
{
    internal static class ZarinPalHelper
    {
        public const string WebServiceUrl = "https://#.zarinpal.com/pg/services/WebGate/wsdl";
        public const string PaymentPageUrl = "https://#.zarinpal.com/pg/StartPay/";
        public const string OkResult = "100";

        public static string CreateRequestData(ZarinPalGatewayAccount account, Invoice invoice)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:zar=\"http://zarinpal.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<zar:PaymentRequest>" +
                $"<zar:MerchantID>{account.MerchantId}</zar:MerchantID>" +
                $"<zar:Amount>{(long)invoice.Amount}</zar:Amount>" +
                "<zar:Description></zar:Description>" +
                "<!--Optional:-->" +
                "<zar:Email></zar:Email>" +
                "<!--Optional:-->" +
                "<zar:Mobile></zar:Mobile>" +
                $"<zar:CallbackURL>{invoice.CallbackUrl}</zar:CallbackURL>" +
                "</zar:PaymentRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static PaymentRequestResult CreateRequestResult(
            string response,
            IHttpContextAccessor httpContextAccessor,
            ZarinPalGatewayAccount account)
        {
            var status = XmlHelper.GetNodeValueFromXml(response, "Status");
            var authority = XmlHelper.GetNodeValueFromXml(response, "Authority");

            var isSucceed = string.Equals(status, OkResult, StringComparison.InvariantCultureIgnoreCase);

            if (!isSucceed)
            {
                var message = $"Error {status}";

                return PaymentRequestResult.Failed(message);
            }

            var paymentPageUrl = GetWebPageUrl(account.IsSandbox) + authority;

            return PaymentRequestResult.Succeed(new GatewayRedirect(httpContextAccessor, paymentPageUrl), account.Name);
        }

        public static ZarinPalCallbackResult CreateCallbackResult(HttpRequest httpRequest)
        {
            httpRequest.TryGetParam("Authority", out var authority);
            httpRequest.TryGetParam("Status", out var status);

            IPaymentVerifyResult verifyResult = null;

            var isSucceed = string.Equals(status, OkResult, StringComparison.InvariantCultureIgnoreCase);

            if (!isSucceed)
            {
                var message = $"Error {status}";

                verifyResult = PaymentVerifyResult.Failed(message);
            }

            return new ZarinPalCallbackResult
            {
                Authority = authority,
                IsSucceed = isSucceed,
                Result = verifyResult
            };
        }

        public static string CreateVerifyData(ZarinPalGatewayAccount account, ZarinPalCallbackResult callbackResult, Money amount)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:zar=\"http://zarinpal.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<zar:PaymentVerification>" +
                $"<zar:MerchantID>{account.MerchantId}</zar:MerchantID>" +
                $"<zar:Authority>{callbackResult.Authority}</zar:Authority>" +
                $"<zar:Amount>{(long)amount}</zar:Amount>" +
                "</zar:PaymentVerification>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static PaymentVerifyResult CreateVerifyResult(string response, MessagesOptions messagesOptions)
        {
            var status = XmlHelper.GetNodeValueFromXml(response, "Status");
            var refId = XmlHelper.GetNodeValueFromXml(response, "RefID");

            var isSucceed = string.Equals(status, OkResult, StringComparison.InvariantCultureIgnoreCase);

            if (!isSucceed)
            {
                var message = $"Error {status}";

                return PaymentVerifyResult.Failed(message);
            }

            return PaymentVerifyResult.Succeed(refId, messagesOptions.PaymentSucceed);
        }

        public static string GetWebServiceUrl(bool isSandbox)
        {
            var urlPrefix = isSandbox ? "sandbox" : "www";

            return WebServiceUrl.Replace("#", urlPrefix);
        }

        public static string GetWebPageUrl(bool isSandbox)
        {
            var urlPrefix = isSandbox ? "sandbox" : "www";

            return PaymentPageUrl.Replace("#", urlPrefix);
        }
    }
}