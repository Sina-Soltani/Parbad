// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Gateway.PayIr.Internal;
using Parbad.InvoiceBuilder;
using System;
using Parbad.Abstraction;

namespace Parbad.Gateway.PayIr
{
    public static class PayIrGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to Pay.ir gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UsePayIr(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(PayIrGateway.Name);
        }

        /// <summary>
        /// Sets additional data to the Invoice for <see cref="PayIrGateway"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="data"></param>
        public static IInvoiceBuilder SetPayIrAdditionalData(this IInvoiceBuilder builder, PayIrRequestAdditionalData data)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (data == null) throw new ArgumentNullException(nameof(data));

            return builder.AddOrUpdateProperty(PayIrHelper.RequestAdditionalDataKey, data);
        }

        internal static PayIrRequestAdditionalData GetPayIrAdditionalData(this Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            if (invoice.Properties.ContainsKey(PayIrHelper.RequestAdditionalDataKey))
            {
                return (PayIrRequestAdditionalData)invoice.Properties[PayIrHelper.RequestAdditionalDataKey];
            }

            return null;
        }
    }
}
