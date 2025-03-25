// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Parbad.Gateway.Melli.Internal.Models;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Options;

namespace Parbad.Gateway.Melli.Internal;

internal static class MelliHelper
{
    public static async Task<MelliCallbackResult> BindCallbackResult(HttpRequest httpRequest,
                                                                     InvoiceContext context,
                                                                     MessagesOptions messagesOptions,
                                                                     CancellationToken cancellationToken)
    {
        var resCode = await httpRequest.TryGetParamAsAsync<int>("ResCode", cancellationToken).ConfigureAwaitFalse();
        var token = await httpRequest.TryGetParamAsync("Token", cancellationToken).ConfigureAwaitFalse();
        var orderId = await httpRequest.TryGetParamAsAsync<long>("OrderId", cancellationToken).ConfigureAwaitFalse();

        var callbackResult = new MelliCallbackResult
                             {
                                 ResCode = resCode.Value,
                                 Token = token.Value,
                                 OrderId = orderId.Value,
                             };

        if (resCode.Value != Constants.SuccessCode)
        {
            callbackResult.IsSucceeded = false;
            callbackResult.Message = messagesOptions.PaymentFailed;
        }
        else if (orderId.Value != context.Payment.TrackingNumber || string.IsNullOrEmpty(token.Value))
        {
            callbackResult.IsSucceeded = false;
            callbackResult.Message = messagesOptions.InvalidDataReceivedFromGateway;
        }
        else
        {
            callbackResult.IsSucceeded = true;
        }

        return callbackResult;
    }
}
