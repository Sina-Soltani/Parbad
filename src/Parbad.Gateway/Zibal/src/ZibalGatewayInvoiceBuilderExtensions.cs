// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.Abstraction;
using Parbad.Gateway.Zibal.Internal;
using Parbad.InvoiceBuilder;

namespace Parbad.Gateway.Zibal
{
    public static class ZibalGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to Zibal Gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IInvoiceBuilder UseZibal(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(ZibalGateway.Name);
        }

        /// <summary>
        /// Sets the additional data for <see cref="ZibalGateway"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static IInvoiceBuilder SetZibalData(this IInvoiceBuilder builder, ZibalRequestAdditionalData zibalRequest)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddOrUpdateProperty(ZibalHelper.ZibalRequestAdditionalKeyName, zibalRequest);
        }

        internal static ZibalRequestAdditionalData? GetZibalRequestData(this Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            if (!invoice.Properties.ContainsKey(ZibalHelper.ZibalRequestAdditionalKeyName))
            {
                return null;
            }

            return (ZibalRequestAdditionalData)invoice.Properties[ZibalHelper.ZibalRequestAdditionalKeyName];
        }
    }
}
