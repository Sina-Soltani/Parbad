// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.Abstraction;
using Parbad.InvoiceBuilder;

namespace Parbad.Gateway.YekPay
{
    public static class YekPayGatewayInvoiceBuilderExtensions
    {
        private const string YekPayRequestKey = "YekPayRequest";

        /// <summary>
        /// The invoice will be sent to YekPay gateway.
        /// </summary>
        public static IInvoiceBuilder UseYekPay(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(YekPayGateway.Name);
        }

        /// <summary>
        /// The invoice will be sent to YekPay gateway.
        /// </summary>
        public static IInvoiceBuilder SetYekPayData(this IInvoiceBuilder builder, Action<YekPayRequest> configureYekPay)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configureYekPay == null) throw new ArgumentNullException(nameof(configureYekPay));

            var yekPayRequest = new YekPayRequest();
            configureYekPay(yekPayRequest);

            return SetYekPayData(builder, yekPayRequest);
        }

        /// <summary>
        /// The invoice will be sent to YekPay gateway.
        /// </summary>
        public static IInvoiceBuilder SetYekPayData(this IInvoiceBuilder builder, YekPayRequest yekPayRequest)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (yekPayRequest == null) throw new ArgumentNullException(nameof(yekPayRequest));

            SetYekPayRequest(builder, yekPayRequest);

            return builder;
        }

        internal static void SetYekPayRequest(IInvoiceBuilder builder, YekPayRequest yekPayRequest)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (yekPayRequest == null) throw new ArgumentNullException(nameof(yekPayRequest));

            builder.AddOrUpdateAdditionalData(YekPayRequestKey, yekPayRequest);
        }

        internal static YekPayRequest GetYekPayRequest(this Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            if (invoice.Properties.ContainsKey(YekPayRequestKey))
            {
                return (YekPayRequest)invoice.Properties[YekPayRequestKey];
            }

            return null;
        }
    }
}
