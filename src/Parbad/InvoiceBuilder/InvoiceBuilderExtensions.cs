// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Parbad.Abstraction;
using Parbad.Exceptions;
using Parbad.InvoiceBuilder;

namespace Parbad
{
    /// <summary>
    /// Extension methods for <see cref="IInvoiceBuilder"/>.
    /// </summary>
    public static class InvoiceBuilderExtensions
    {
        /// <summary>
        /// Sets the tracking number of invoice.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="trackingNumber">
        /// The tracking number of invoice.
        /// <para>Note: It must be unique for each payment requests.</para>
        /// </param>
        public static IInvoiceBuilder SetTrackingNumber(this IInvoiceBuilder builder, long trackingNumber)
            => AddFormatter(builder, invoice => invoice.TrackingNumber = trackingNumber);

        /// <summary>
        /// Sets the amount of the invoice.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="amount">The amount of invoice.</param>
        public static IInvoiceBuilder SetAmount(this IInvoiceBuilder builder, Money amount)
            => AddFormatter(builder, invoice => invoice.Amount = amount);

        /// <summary>
        /// Sets the callback URL. It will be used by the gateway for redirecting the
        /// client again to your website.
        /// <para>Note: A complete URL would be like: "http://www.mywebsite.com/foo/bar/"</para>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="callbackUrl">
        /// A complete URL of your website. It will be used by the gateway for redirecting the
        /// client again to your website.
        /// <para>Note: A complete URL would be like: "http://www.mywebsite.com/foo/bar/"</para>
        /// </param>
        public static IInvoiceBuilder SetCallbackUrl(this IInvoiceBuilder builder, string callbackUrl)
            => AddFormatter(builder, invoice => invoice.CallbackUrl = new CallbackUrl(callbackUrl));

        /// <summary>
        /// Sets the callback URL. It will be used by the gateway for redirecting the
        /// client again to your website.
        /// <para>Note: A complete URL would be like: "http://www.mywebsite.com/foo/bar/"</para>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="callbackUrl">
        /// A complete URL of your website. It will be used by the gateway for redirecting the
        /// client again to your website.
        /// <para>Note: A complete URL would be like: "http://www.mywebsite.com/foo/bar/"</para>
        /// </param>
        public static IInvoiceBuilder SetCallbackUrl(this IInvoiceBuilder builder, CallbackUrl callbackUrl)
            => AddFormatter(builder, invoice => invoice.CallbackUrl = callbackUrl);

        /// <summary>
        /// Sets the gateway name.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="gatewayName"></param>
        /// <exception cref="GatewayNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static IInvoiceBuilder SetGateway(this IInvoiceBuilder builder, string gatewayName)
        {
            if (gatewayName == null) throw new ArgumentNullException(nameof(gatewayName));

            return AddFormatter(builder, invoice => invoice.GatewayName = gatewayName);
        }

        /// <summary>
        /// Adds the given key and value to the invoice.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="key">Key of the data</param>
        /// <param name="value">Value of the data</param>
        public static IInvoiceBuilder AddAdditionalData(this IInvoiceBuilder builder, string key, object value)
            => ChangeAdditionalData(builder, additionalData => additionalData.Add(key, value));

        /// <summary>
        /// Appends the given dictionary to the additional data of the invoice.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="additionalData"></param>
        public static IInvoiceBuilder AddAdditionalData(this IInvoiceBuilder builder, IDictionary<string, object> additionalData)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (additionalData == null) throw new ArgumentNullException(nameof(additionalData));

            ChangeAdditionalData(builder, invoiceAdditionalData =>
            {
                foreach (var data in additionalData)
                {
                    invoiceAdditionalData.Add(data.Key, data.Value);
                }
            });

            return builder;
        }

        /// <summary>
        /// Adds or updates the given key and value to the invoice.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="key">Key of the data</param>
        /// <param name="value">Value of the data</param>
        public static IInvoiceBuilder AddOrUpdateAdditionalData(this IInvoiceBuilder builder, string key, object value)
            => AddOrUpdateAdditionalData(builder, new Dictionary<string, object> { { key, value } });

        /// <summary>
        /// Adds or updates the given dictionary to the additional data of the invoice.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="additionalData"></param>
        public static IInvoiceBuilder AddOrUpdateAdditionalData(this IInvoiceBuilder builder, IDictionary<string, object> additionalData)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (additionalData == null) throw new ArgumentNullException(nameof(additionalData));

            ChangeAdditionalData(builder, invoiceAdditionalData =>
            {
                foreach (var data in additionalData)
                {
                    if (invoiceAdditionalData.ContainsKey(data.Key))
                    {
                        invoiceAdditionalData[data.Key] = data.Value;
                    }
                    else
                    {
                        invoiceAdditionalData.Add(data.Key, data.Value);
                    }
                }
            });

            return builder;
        }

        /// <summary>
        /// Changes the additional data.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="onChange">An action to perform on the additional data.</param>
        public static IInvoiceBuilder ChangeAdditionalData(this IInvoiceBuilder builder, Action<IDictionary<string, object>> onChange)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return AddFormatter(builder, invoice => onChange(invoice.AdditionalData));
        }

        /// <summary>
        /// Adds the given formatter to the list of formatters.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="formatInvoice"></param>
        public static IInvoiceBuilder AddFormatter(this IInvoiceBuilder builder, Action<Invoice> formatInvoice)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddFormatter(new DefaultInvoiceFormatter(formatInvoice));
        }

        /// <summary>
        /// Adds the given formatter to the list of formatters.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="formatInvoice"></param>
        public static IInvoiceBuilder AddFormatter(this IInvoiceBuilder builder, Func<Invoice, Task> formatInvoice)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddFormatter(new DefaultInvoiceFormatter(formatInvoice));
        }
    }
}
