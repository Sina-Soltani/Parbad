// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.Abstraction;
using Parbad.InvoiceBuilder;

namespace Parbad.Gateway.AsanPardakht
{
    public static class AsanPardakhtGatewayInvoiceBuilderExtensions
    {
        private const string DataKey = "AsanPardakhtDataKey";

        /// <summary>
        /// The invoice will be sent to Asan Pardakht gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UseAsanPardakht(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(AsanPardakhtGateway.Name);
        }

        /// <summary>
        /// Sets some additional data to the invoice for <see cref="AsanPardakhtGateway"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static IInvoiceBuilder SetAsanPardakhtData(this IInvoiceBuilder builder, AsanPardakhtRequestAdditionalData data)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (data == null) throw new ArgumentNullException(nameof(data));

            return builder.AddOrUpdateProperty(DataKey, data);
        }

        public static AsanPardakhtRequestAdditionalData GetAsanPardakhtData(this Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            if (invoice.Properties.ContainsKey(DataKey))
            {
                return (AsanPardakhtRequestAdditionalData)invoice.Properties[DataKey];
            }

            return null;
        }
    }
}
