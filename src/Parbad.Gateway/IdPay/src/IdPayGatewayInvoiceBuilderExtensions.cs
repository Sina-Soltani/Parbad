// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.Gateway.IdPay.Internal;
using Parbad.InvoiceBuilder;

namespace Parbad.Gateway.IdPay
{
    public static class IdPayGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// Sets some additional data that can be sent to the ID Pay gateway by requesting a payment.
        /// </summary>
        public static IInvoiceBuilder SetIdPayData(this IInvoiceBuilder builder, IdPayRequestAdditionalData data)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (data == null) throw new ArgumentNullException(nameof(data));

            return builder.AddOrUpdateProperty(IdPayHelper.RequestAdditionalDataKey, data);
        }

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
