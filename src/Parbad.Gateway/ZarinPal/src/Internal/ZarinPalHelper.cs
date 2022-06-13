// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Parbad.Abstraction;
using Parbad.Gateway.ZarinPal.Models;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Storage.Abstractions.Models;

namespace Parbad.Gateway.ZarinPal.Internal
{
    internal static class ZarinPalHelper
    {
        private const int NumericOkResult = 100;
        private const int NumericAlreadyOkResult = 101;
        private const string AuthorityDatabaseKey = "Authority";

        public static string ZarinPalRequestAdditionalKeyName => "ZarinPalRequest";

        public static ZarinPalRequestModel CreateRequestModel(ZarinPalGatewayAccount account, Invoice invoice)
        {
            if (!invoice.Properties.ContainsKey(ZarinPalRequestAdditionalKeyName) ||
                invoice.Properties[ZarinPalRequestAdditionalKeyName] is not ZarinPalInvoice zarinPalInvoice)
            {
                throw new
                    InvalidOperationException("Request failed. ZarinPal Gateway needs invoice information. Please use the SetZarinPalData method of Invoice Builder to add the information.");
            }

            return new()
                   {
                       MerchantId = account.MerchantId,
                       Amount = invoice.Amount,
                       CallbackUrl = invoice.CallbackUrl,
                       Description = zarinPalInvoice.Description,
                       Email = zarinPalInvoice.Email,
                       Mobile = zarinPalInvoice.Mobile
                   };
        }

        public static PaymentRequestResult CreateRequestResult(ZarinPalRequestResultModel resultModel,
                                                               HttpContext httpContext,
                                                               ZarinPalGatewayAccount account,
                                                               ZarinPalGatewayOptions gatewayOptions,
                                                               MessagesOptions messagesOptions)
        {
            if (!IsSucceedResult(resultModel.Code))
            {
                var message = ZarinPalCodeTranslator.Translate(resultModel.Code, messagesOptions);

                return PaymentRequestResult.Failed(message, account.Name);
            }

            var paymentPageUrl = GetPaymentPageUrl(account.IsSandbox, gatewayOptions) + resultModel.Authority;

            var result = PaymentRequestResult.SucceedWithRedirect(account.Name, httpContext, paymentPageUrl);

            result.GatewayResponseCode = resultModel.Code.ToString();
            result.DatabaseAdditionalData.Add(AuthorityDatabaseKey, resultModel.Authority);

            return result;
        }

        public static async Task<ZarinPalCallbackResult> CreateCallbackResultAsync(
            HttpRequest httpRequest,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken)
        {
            var authority = await httpRequest.TryGetParamAsync("Authority", cancellationToken).ConfigureAwaitFalse();
            var status = await httpRequest.TryGetParamAsAsync<int>("Status", cancellationToken).ConfigureAwaitFalse();
            string message = null;

            var isSucceed = status.Exists && IsSucceedResult(status.Value);

            if (!isSucceed)
            {
                message = ZarinPalCodeTranslator.Translate(status.Value, messagesOptions);
            }

            return new ZarinPalCallbackResult
                   {
                       Authority = authority.Value,
                       Status = status.Value,
                       IsSucceed = isSucceed,
                       Message = message
                   };
        }

        public static ZarinPalVerificationModel CreateVerificationModel(ZarinPalGatewayAccount account, ZarinPalCallbackResult callbackResult, Money amount)
        {
            return new()
                   {
                       MerchantId = account.MerchantId,
                       Amount = amount,
                       Authority = callbackResult.Authority
                   };
        }

        public static PaymentVerifyResult CreateVerifyResult(ZarinPalOriginalVerificationResult verificationResult, ZarinPalGatewayAccount account, MessagesOptions messagesOptions)
        {
            PaymentVerifyResult result;

            if (IsSucceedResult(verificationResult.Code))
            {
                result = PaymentVerifyResult.Succeed(verificationResult.RefId, messagesOptions.PaymentSucceed);
            }
            else
            {
                var message = ZarinPalCodeTranslator.Translate(verificationResult.Code, messagesOptions);

                var verifyResultStatus = verificationResult.Code == NumericAlreadyOkResult
                    ? PaymentVerifyResultStatus.AlreadyVerified
                    : PaymentVerifyResultStatus.Failed;

                result = new PaymentVerifyResult
                         {
                             Status = verifyResultStatus,
                             Message = message
                         };
            }

            result.GatewayAccountName = account.Name;
            result.GatewayResponseCode = verificationResult.Code.ToString();
            result.SetZarinPalOriginalVerificationResult(verificationResult);

            return result;
        }

        public static ZarinPalRefundModel CreateRefundModel(string authority, ZarinPalGatewayAccount account)
        {
            return new()
                   {
                       MerchantId = account.MerchantId,
                       Authority = authority
                   };
        }

        public static PaymentRefundResult CreateRefundResult(ZarinPalRefundResultModel resultModel, ZarinPalGatewayAccount account, MessagesOptions messagesOptions)
        {
            var result = new PaymentRefundResult
                         {
                             GatewayResponseCode = resultModel.Code.ToString(),
                             GatewayAccountName = account.Name
                         };

            if (IsSucceedResult(resultModel.Code))
            {
                result.Status = PaymentRefundResultStatus.Succeed;

                result.Message = messagesOptions.PaymentSucceed;
            }
            else
            {
                result.Status = resultModel.Code == NumericAlreadyOkResult ? PaymentRefundResultStatus.AlreadyRefunded : PaymentRefundResultStatus.Failed;

                result.Message = ZarinPalCodeTranslator.Translate(resultModel.Code, messagesOptions);
            }

            return result;
        }

        public static string GetAuthorityFromAdditionalData(Transaction transaction)
        {
            var additionalData = JsonConvert.DeserializeObject<IDictionary<string, string>>(transaction.AdditionalData);

            if (!additionalData.ContainsKey(AuthorityDatabaseKey))
            {
                return null;
            }

            return additionalData[AuthorityDatabaseKey];
        }

        public static string GetApiRequestUrl(bool isSandbox, ZarinPalGatewayOptions gatewayOptions)
        {
            return isSandbox
                ? gatewayOptions.SandboxApiRequestUrl
                : gatewayOptions.ApiRequestUrl;
        }

        public static string GetApiVerificationUrl(bool isSandbox, ZarinPalGatewayOptions gatewayOptions)
        {
            return isSandbox
                ? gatewayOptions.SandboxApiVerificationUrl
                : gatewayOptions.ApiVerificationUrl;
        }

        public static string GetApiRefundUrl(bool isSandbox, ZarinPalGatewayOptions gatewayOptions)
        {
            return isSandbox
                ? gatewayOptions.SandboxApiRefundUrl
                : gatewayOptions.ApiRefundUrl;
        }

        private static string GetPaymentPageUrl(bool isSandbox, ZarinPalGatewayOptions gatewayOptions)
        {
            return isSandbox
                ? gatewayOptions.SandboxPaymentPageUrl
                : gatewayOptions.PaymentPageUrl;
        }

        public static async Task<(int? Code, string Message)> TryGetError(HttpResponseMessage httpResponseMessage, MessagesOptions messagesOptions)
        {
            var json = await httpResponseMessage.Content.ReadAsStringAsync();

            var failedResult = JsonConvert.DeserializeObject<ZarinPalFailedResult>(json);

            string message;

            if (failedResult?.Errors != null)
            {
                message = ZarinPalCodeTranslator.Translate(failedResult.Errors.Code, messagesOptions);
            }
            else
            {
                message = $"ZarinPal HTTP request failed. Status code {httpResponseMessage.StatusCode}";
            }

            return (failedResult?.Errors?.Code, message);
        }

        private static bool IsSucceedResult(int status) => status == NumericOkResult;
    }
}
