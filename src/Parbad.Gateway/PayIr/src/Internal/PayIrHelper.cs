// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Parbad.Abstraction;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Options;

namespace Parbad.Gateway.PayIr.Internal
{
    internal static class PayIrHelper
    {
        public const string WebServiceUrl = "https://pay.ir";
        public const string WebServiceRequestUrl = "pg/send";
        public const string WebServiceVerifyUrl = "pg/verify";
        public const string PaymentPageUrl = WebServiceUrl + "/pg/";
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

        public static PaymentRequestResult CreateRequestResult(string response, IHttpContextAccessor httpContextAccessor, PayIrGatewayAccount account)
        {
            var result = JsonConvert.DeserializeObject<PayIrRequestResponseModel>(response);

            if (!result.IsSucceed)
            {
                return PaymentRequestResult.Failed(result.ErrorMessage, account.Name);
            }

            var paymentPageUrl = $"{PaymentPageUrl}{result.Token}";

            return PaymentRequestResult.Succeed(new GatewayRedirect(httpContextAccessor, paymentPageUrl), account.Name);
        }

        public static PayIrCallbackResult CreateCallbackResult(HttpRequest httpRequest)
        {
            httpRequest.TryGetParam("Token", out var token);
            httpRequest.TryGetParam("Status", out var status);

            IPaymentVerifyResult verifyResult = null;

            var isSucceed = string.Equals(status, OkResult, StringComparison.InvariantCultureIgnoreCase);

            if (!isSucceed)
            {
                var message = $"Error {status}";

                verifyResult = PaymentVerifyResult.Failed(message);
            }

            return new PayIrCallbackResult
            {
                Token = token,
                IsSucceed = isSucceed,
                Result = verifyResult
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

            if (!result.IsSucceed)
            {
                var message = $"ErrorCode: {result.ErrorCode}, ErrorMessage: {result.ErrorMessage}";

                return PaymentVerifyResult.Failed(message);
            }

            return PaymentVerifyResult.Succeed(result.TransId, messagesOptions.PaymentSucceed);
        }
    }
}
