// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Abstraction;
using Parbad.Gateway.Saman.Internal;
using Parbad.InvoiceBuilder;
using System;

namespace Parbad.Gateway.Saman;

public static class SamanGatewayInvoiceBuilderExtensions
{
    /// <summary>
    /// The invoice will be sent to Saman gateway.
    /// </summary>
    /// <param name="builder"></param>
    public static IInvoiceBuilder UseSaman(this IInvoiceBuilder builder)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        return builder.SetGateway(SamanGateway.Name);
    }

    /// <summary>
    /// Sets additional data which will be sent to Saman Gateway.
    /// </summary>
    public static IInvoiceBuilder SetSamanData(this IInvoiceBuilder builder, string cellNumber)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        if (!string.IsNullOrWhiteSpace(cellNumber))
        {
            builder.AddOrUpdateProperty(SamanHelper.CellNumberPropertyKey, cellNumber);
        }

        return builder;
    }

    internal static string GetSamanCellNumber(this Invoice invoice)
    {
        if (invoice.Properties.TryGetValue(SamanHelper.CellNumberPropertyKey, out var cellNumber))
        {
            return cellNumber.ToString();
        }

        return null;
    }
}
