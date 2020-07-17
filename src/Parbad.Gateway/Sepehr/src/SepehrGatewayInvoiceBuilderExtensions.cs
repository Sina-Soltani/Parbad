// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.Gateway.Sepehr;
using Parbad.InvoiceBuilder;

namespace Parbad
{
    public static class SepehrGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to Sepehr gateway.
        /// </summary>
        public static IInvoiceBuilder UseSepehr(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(SepehrGateway.Name);
        }
    }
}
