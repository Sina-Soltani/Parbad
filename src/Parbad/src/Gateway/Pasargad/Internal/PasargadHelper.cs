// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Parbad.Gateway.Pasargad.Internal.Models;
using Parbad.Http;
using Parbad.Internal;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Gateway.Pasargad.Internal;

internal static class PasargadHelper
{
    public static async Task<PasargadCallbackResultModel> BindCallbackResultModel(HttpRequest httpRequest,
                                                                                  CancellationToken cancellationToken)
    {
        var invoiceNumber = await httpRequest.TryGetParamAsync("iN", cancellationToken).ConfigureAwaitFalse();

        var invoiceDate = await httpRequest.TryGetParamAsync("iD", cancellationToken).ConfigureAwaitFalse();

        var transactionReferenceId = await httpRequest.TryGetParamAsync("tref", cancellationToken).ConfigureAwaitFalse();

        return new PasargadCallbackResultModel
               {
                   InvoiceNumber = invoiceNumber.Value,
                   InvoiceDate = invoiceDate.Value,
                   TransactionReferenceId = transactionReferenceId.Value
               };
    }

    public static string GetTimeStamp()
    {
        return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
    }
}
