// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Parbad.Abstraction;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using Parbad.Storage.Abstractions.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Gateway.PayIr.Internal
{
    internal static class PayIrHelper
    {
        public const string OkResult = "1";
        private const string TokenDatabaseKey = "PayIrToken";
        public static string RequestAdditionalDataKey => nameof(RequestAdditionalDataKey);

        public static PayIrRequestModel CreateRequestData(PayIrGatewayAccount account, Invoice invoice)
        {
            var api = account.IsTestAccount ? "test" : account.Api;

            var additionalData = invoice.GetPayIrAdditionalData();

            return new PayIrRequestModel
            {
                Api = api,
                Amount = invoice.Amount,
                Redirect = invoice.CallbackUrl,
                NationalCode = additionalData?.NationalCode,
                Description = additionalData?.Description,
                FactorNumber = additionalData?.FactorNumber,
                Mobile = additionalData?.Mobile,
                ValidCardNumber = additionalData?.ValidCardNumber
            };
        }

        public static PaymentRequestResult CreateRequestResult(HttpResponseMessage responseMessage, string response, HttpContext httpContext, PayIrGatewayAccount account, PayIrGatewayOptions gatewayOptions)
        {
            var result = GatewayResponse.DeserializeJson<PayIrRequestResponseModel>(responseMessage, response);

            if (!result.IsSucceed)
            {
                return PaymentRequestResult.Failed(result.ErrorMessage, account.Name);
            }

            var paymentPageUrl = $"{gatewayOptions.PaymentPageUrl}{result.Token}";

            var requestResult = PaymentRequestResult.SucceedWithRedirect(account.Name, httpContext, paymentPageUrl);

            // Stored so that the callback can be bound to this invoice (see CreateCallbackResultAsync).
            requestResult.DatabaseAdditionalData.Add(TokenDatabaseKey, result.Token);

            return requestResult;
        }

        public static async Task<PayIrCallbackResult> CreateCallbackResultAsync(
            HttpRequest httpRequest,
            InvoiceContext context,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken)
        {
            var token = await httpRequest.TryGetParamAsync("Token", cancellationToken).ConfigureAwaitFalse();
            var status = await httpRequest.TryGetParamAsync("Status", cancellationToken).ConfigureAwaitFalse();

            var isSucceed = status.Exists && string.Equals(status.Value, OkResult, StringComparison.InvariantCultureIgnoreCase);
            string message = null;

            if (!isSucceed)
            {
                message = $"Error {status.Value}";
            }
            else if (!IsTokenIssuedForThisInvoice(token.Value, context))
            {
                // The callback carries the token of another invoice (e.g. a cheaper one that was paid).
                isSucceed = false;
                message = messagesOptions.InvalidDataReceivedFromGateway;
            }

            return new PayIrCallbackResult
            {
                Token = token.Value,
                IsSucceed = isSucceed,
                Message = message
            };
        }

        public static PayIrVerifyModel CreateVerifyData(PayIrGatewayAccount account, PayIrCallbackResult callbackResult)
        {
            var api = account.IsTestAccount ? "test" : account.Api;

            return new PayIrVerifyModel
            {
                Api = api,
                Token = callbackResult.Token
            };
        }

        public static PaymentVerifyResult CreateVerifyResult(HttpResponseMessage responseMessage, string response, InvoiceContext context, MessagesOptions messagesOptions)
        {
            var result = GatewayResponse.DeserializeJson<PayIrVerifyResponseModel>(responseMessage, response);

            PaymentVerifyResult verifyResult;

            if (result.IsSucceed && result.Amount > 0 && result.Amount != (long)context.Payment.Amount)
            {
                verifyResult = PaymentVerifyResult.Failed($"{messagesOptions.PaymentFailed} The amount reported by the gateway ({result.Amount}) does not match the invoice amount ({(long)context.Payment.Amount}).");
            }
            else if (result.IsSucceed)
            {
                verifyResult = PaymentVerifyResult.Succeed(result.TransId, messagesOptions.PaymentSucceed);
            }
            else
            {
                var message = $"ErrorCode: {result.ErrorCode}, ErrorMessage: {result.ErrorMessage}";

                verifyResult = PaymentVerifyResult.Failed(message);
            }

            var additionalData = new PayIrVerifyAdditionalData
            {
                CardNumber = result.CardNumber,
                FactorNumber = result.FactorNumber,
                Description = result.Description,
                Mobile = result.Mobile
            };

            verifyResult.SetPayIrAdditionalData(additionalData);

            return verifyResult;
        }

        /// <summary>
        /// Checks that the callback's token is the one Pay.ir issued for this invoice. Invoices requested before
        /// the token was stored have nothing to compare with and are accepted as before.
        /// </summary>
        private static bool IsTokenIssuedForThisInvoice(string callbackToken, InvoiceContext context)
        {
            var requestTransaction = context.Transactions?.SingleOrDefault(transaction => transaction.Type == TransactionType.Request);

            if (string.IsNullOrEmpty(requestTransaction?.AdditionalData) ||
                !requestTransaction.ToDictionary().TryGetValue(TokenDatabaseKey, out var issuedToken))
            {
                return true;
            }

            return string.Equals(issuedToken, callbackToken, StringComparison.Ordinal);
        }
    }
}
