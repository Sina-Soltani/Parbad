// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.Abstraction;
using Parbad.Gateway.IdPay.Internal;

namespace Parbad.Gateway.IdPay;

public static class IdPayGatewayInvoiceExtensions
{
    internal static IdPayRequestAdditionalData GetIdPayAdditionalData(this Invoice invoice)
    {
        if (invoice == null) throw new ArgumentNullException(nameof(invoice));

        if (!invoice.Properties.ContainsKey(IdPayHelper.RequestAdditionalDataKey))
        {
            return null;
        }

        return (IdPayRequestAdditionalData)invoice.Properties[IdPayHelper.RequestAdditionalDataKey];
    }
}
