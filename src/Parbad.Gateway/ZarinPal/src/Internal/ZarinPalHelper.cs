// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
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
        public const string WebServiceUrl = "https://#.zarinpal.com/pg/services/WebGate/service";
        public const string PaymentPageUrl = "https://#.zarinpal.com/pg/StartPay/";
        public const string NumericOkResult = "100";
        public const string StringOkResult = "OK";
        public const string NumericAlreadyOkResult = "101";

        public static string ZarinPalRequestAdditionalKeyName => "ZarinPalRequest";

        public static string CreateRequestData(ZarinPalGatewayAccount account, Invoice invoice)
        {
            if (!invoice.AdditionalData.ContainsKey(ZarinPalRequestAdditionalKeyName) ||
                !(invoice.AdditionalData[ZarinPalRequestAdditionalKeyName] is ZarinPalInvoice zarinPalInvoice))
            {
                throw new InvalidOperationException("ZarinPal gateway error. For creating an invoice for ZarinPal gateway please use the UseZarinPal method instead of the SetGateway.");
            }

            var email = zarinPalInvoice.Email.IsNullOrEmpty() ? null : XmlHelper.EncodeXmlValue(zarinPalInvoice.Email);
            var mobile = zarinPalInvoice.Mobile.IsNullOrEmpty() ? null : XmlHelper.EncodeXmlValue(zarinPalInvoice.Mobile);

            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:zar=\"http://zarinpal.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<zar:PaymentRequest>" +
                $"<zar:MerchantID>{account.MerchantId}</zar:MerchantID>" +
                $"<zar:Amount>{(long)invoice.Amount}</zar:Amount>" +
                $"<zar:Description>{XmlHelper.EncodeXmlValue(zarinPalInvoice.Description)}</zar:Description>" +
                "<!--Optional:-->" +
                $"<zar:Email>{email}</zar:Email>" +
                "<!--Optional:-->" +
                $"<zar:Mobile>{mobile}</zar:Mobile>" +
                $"<zar:CallbackURL>{XmlHelper.EncodeXmlValue(invoice.CallbackUrl)}</zar:CallbackURL>" +
                "</zar:PaymentRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static PaymentRequestResult CreateRequestResult(string response,
            IHttpContextAccessor httpContextAccessor,
            ZarinPalGatewayAccount account,
            MessagesOptions messagesOptions)
        {
            var status = XmlHelper.GetNodeValueFromXml(response, "Status", "http://zarinpal.com/");
            var authority = XmlHelper.GetNodeValueFromXml(response, "Authority", "http://zarinpal.com/");

            var isSucceed = string.Equals(status, NumericOkResult, StringComparison.InvariantCultureIgnoreCase);

            if (!isSucceed)
            {
                var message = ZarinPalStatusTranslator.Translate(status, messagesOptions);

                return PaymentRequestResult.Failed(message, account.Name);
            }

            var paymentPageUrl = GetWebPageUrl(account.IsSandbox) + authority;

            return PaymentRequestResult.Succeed(new GatewayRedirect(httpContextAccessor, paymentPageUrl), account.Name);
        }

        public static async Task<ZarinPalCallbackResult> CreateCallbackResultAsync(HttpRequest httpRequest,
            CancellationToken cancellationToken)
        {
            var authority = await httpRequest.TryGetParamAsync("Authority", cancellationToken).ConfigureAwaitFalse();
            var status = await httpRequest.TryGetParamAsync("Status", cancellationToken).ConfigureAwaitFalse();

            IPaymentVerifyResult verifyResult = null;

            var isSucceed = status.Exists && string.Equals(status.Value, StringOkResult, StringComparison.InvariantCultureIgnoreCase);

            if (!isSucceed)
            {
                var message = $"Error {status}";

                verifyResult = PaymentVerifyResult.Failed(message);
            }

            return new ZarinPalCallbackResult
            {
                Authority = authority.Value,
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
            var status = XmlHelper.GetNodeValueFromXml(response, "Status", "http://zarinpal.com/");
            var refId = XmlHelper.GetNodeValueFromXml(response, "RefID", "http://zarinpal.com/");

            var isSucceed = string.Equals(status, NumericOkResult, StringComparison.OrdinalIgnoreCase);

            if (!isSucceed)
            {
                var message = ZarinPalStatusTranslator.Translate(status, messagesOptions);

                var verifyResultStatus = string.Equals(status, NumericAlreadyOkResult, StringComparison.OrdinalIgnoreCase)
                        ? PaymentVerifyResultStatus.AlreadyVerified
                        : PaymentVerifyResultStatus.Failed;

                return new PaymentVerifyResult
                {
                    Status = verifyResultStatus,
                    Message = message
                };
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
