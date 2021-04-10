// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Parbad.Abstraction;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Gateway.PayIr.Internal
{
    internal static class PayIrHelper
    {
        public const string OkResult = "1";

        public static PayIrRequestModel CreateRequestData(PayIrGatewayAccount account, Invoice invoice)
        {
            var api = account.IsTestAccount ? "test" : account.Api;

            return new PayIrRequestModel
            {
                Api = api,
                Amount = invoice.Amount,
                Redirect = invoice.CallbackUrl
            };
        }

        public static PaymentRequestResult CreateRequestResult(string response, HttpContext httpContext, PayIrGatewayAccount account, PayIrGatewayOptions gatewayOptions)
        {
            var result = JsonConvert.DeserializeObject<PayIrRequestResponseModel>(response);

            if (!result.IsSucceed)
            {
                return PaymentRequestResult.Failed(result.ErrorMessage, account.Name);
            }

            var paymentPageUrl = $"{gatewayOptions.PaymentPageUrl}{result.Token}";

            return PaymentRequestResult.SucceedWithRedirect(account.Name, httpContext, paymentPageUrl);
        }

        public static async Task<PayIrCallbackResult> CreateCallbackResultAsync(HttpRequest httpRequest, CancellationToken cancellationToken)
        {
            var token = await httpRequest.TryGetParamAsync("Token", cancellationToken).ConfigureAwaitFalse();
            var status = await httpRequest.TryGetParamAsync("Status", cancellationToken).ConfigureAwaitFalse();

            var isSucceed = status.Exists && string.Equals(status.Value, OkResult, StringComparison.InvariantCultureIgnoreCase);
            string message = null;

            if (!isSucceed)
            {
                message = $"Error {status}";
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

        public static PaymentVerifyResult CreateVerifyResult(string response, MessagesOptions messagesOptions)
        {
            var result = JsonConvert.DeserializeObject<PayIrVerifyResponseModel>(response);

            PaymentVerifyResult verifyResult;

            if (result.IsSucceed)
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
    }
}
