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
        var invoiceId = await httpRequest.TryGetParamAsync("invoiceId", cancellationToken).ConfigureAwaitFalse();
        var status = await httpRequest.TryGetParamAsync("status", cancellationToken).ConfigureAwaitFalse();
        var referenceNumber = await httpRequest.TryGetParamAsync("referenceNumber", cancellationToken).ConfigureAwaitFalse();
        var trackId = await httpRequest.TryGetParamAsync("trackId", cancellationToken).ConfigureAwaitFalse();

        return new PasargadCallbackResultModel
        {
            InvoiceId = invoiceId.Value,
            Status = (PasargadCallbackStatusResult)Enum.Parse(typeof(PasargadCallbackStatusResult), status.Value),
            ReferenceNumber = referenceNumber.Value,
            TrackId = trackId.Value,
        };
    }

    public static string GetTimeStamp()
    {
        return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
    }
}
