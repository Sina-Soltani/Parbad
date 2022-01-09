// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.InvoiceBuilder;
using System;

namespace Parbad.Gateway.AsanPardakht
{
    public static class AsanPardakhtGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to Asan Pardakht gateway.
        /// </summary>
        public static IInvoiceBuilder UseAsanPardakht(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(AsanPardakhtGateway.Name);
        }
    }
}
