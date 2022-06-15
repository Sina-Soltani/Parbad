// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Abstraction;
using Parbad.Gateway.AsanPardakht.Internal.Models;
using Parbad.InvoiceBuilder;
using System;
using System.Collections.Generic;

namespace Parbad.Gateway.AsanPardakht
{
    public static class AsanPardakhtGatewayInvoiceBuilderExtensions
    {
        private const string DataKey = "AsanPardakhtDataKey";

        /// <summary>
        /// The invoice will be sent to Asan Pardakht gateway.
        /// </summary>
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

        /// <summary>
        /// Sets some additional data to the invoice for <see cref="AsanPardakhtGateway"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public static IInvoiceBuilder SetAsanPardakhtData(this IInvoiceBuilder builder, string mobile, string additionalData = "", string paymentId = "", List<AsanPardakhtSettlementPortionModel> settlementPortions = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            if (settlementPortions != null && settlementPortions.Count > 7)
                throw new Exception("You can set max 7 Settlement portion.");

            var data = new AsanPardakhtRequestAdditionalData()
            {
                MobileNumber = mobile,
                PaymentId = paymentId,
                AdditionalData = additionalData,
                SettlementPortions = settlementPortions
            };

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
