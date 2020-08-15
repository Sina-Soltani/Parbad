// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.Abstraction;
using Parbad.Gateway.YekPay;
using Parbad.InvoiceBuilder;

namespace Parbad
{
    public static class YekPayGatewayInvoiceBuilderExtensions
    {
        private const string YekPayRequestKey = "YekPayRequest";

        /// <summary>
        /// The invoice will be sent to YekPay gateway.
        /// </summary>
        public static IInvoiceBuilder UseYekPay(this IInvoiceBuilder builder, Action<YekPayRequest> configureYekPay)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configureYekPay == null) throw new ArgumentNullException(nameof(configureYekPay));

            var yekPayRequest = new YekPayRequest();
            configureYekPay(yekPayRequest);
            builder.SetYekPayRequest(yekPayRequest);

            builder.SetGateway(YekPayGateway.Name);

            return builder;
        }

        internal static void SetYekPayRequest(this IInvoiceBuilder builder, YekPayRequest yekPayRequest)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (yekPayRequest == null) throw new ArgumentNullException(nameof(yekPayRequest));

            builder.AddFormatter(invoice =>
            {
                if (invoice.AdditionalData.ContainsKey(YekPayRequestKey))
                {
                    invoice.AdditionalData.Remove(YekPayRequestKey);
                }

                invoice.AdditionalData.Add(YekPayRequestKey, yekPayRequest);
            });
        }

        internal static YekPayRequest GetYekPayRequest(this Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            if (invoice.AdditionalData.ContainsKey(YekPayRequestKey))
            {
                return (YekPayRequest)invoice.AdditionalData[YekPayRequestKey];
            }

            return null;
        }
    }
}
