// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Parbad.Http;
using Parbad.Options;

namespace Parbad.Gateway.PayPing.Internal
{
    internal static class PayPingGatewayHelper
    {
        public static async Task<PayPingCallbackResult> GetCallbackResult(
            HttpRequest request,
            InvoiceContext context,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken)
        {
            var refId = await request.TryGetParamAsync("refid", cancellationToken);
            var amount = await request.TryGetParamAsAsync<long>("amount", cancellationToken);
            var clientRefId = await request.TryGetParamAsync("clientrefid", cancellationToken);
            var isValid = true;
            var message = "";

            if (!refId.Exists)
            {
                isValid = false;
                message += "RefId isn't received.";
            }

            if (!amount.Exists)
            {
                isValid = false;
                message += "Amount isn't received.";
            }

            if (!clientRefId.Exists)
            {
                isValid = false;
                message += "ClientRefId isn't received.";
            }
            else
            {
                if (clientRefId.Value != context.Payment.TrackingNumber.ToString())
                {
                    isValid = false;
                    message += "ClientRefId isn't valid.";
                }
            }

            if (!isValid)
            {
                message = $"{messagesOptions.InvalidDataReceivedFromGateway}{message}";
            }

            return new PayPingCallbackResult
            {
                IsSucceed = isValid,
                Message = message,
                RefId = refId.Value
            };
        }

        public static VerifyPayRequestModel CreateVerificationModel(InvoiceContext context, PayPingCallbackResult callbackResult)
        {
            return new VerifyPayRequestModel
            {
                Amount = (long)context.Payment.Amount,
                RefId = callbackResult.RefId
            };
        }
    }
}
