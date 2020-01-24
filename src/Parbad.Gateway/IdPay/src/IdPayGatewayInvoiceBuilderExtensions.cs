// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.Gateway.IdPay;
using Parbad.InvoiceBuilder;

namespace Parbad
{
    public static class IdPayGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to IDPay.ir gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UseIdPay(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(IdPayGateway.Name);
        }
    }
}
