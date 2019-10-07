// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Parbad.Abstraction;
using Parbad.Exceptions;
using Parbad.Internal;
using Parbad.InvoiceBuilder;

namespace Parbad
{
    public static class InvoiceBuilderExtensions
    {
        /// <summary>
        /// Sets amount of the invoice.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="amount">The amount of invoice.</param>
        public static IInvoiceBuilder SetAmount(this IInvoiceBuilder builder, decimal amount)
        {
            return builder.SetAmount(amount);
        }

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
        /// <returns></returns>
        public static IInvoiceBuilder SetCallbackUrl(this IInvoiceBuilder builder, string callbackUrl)
        {
            return builder.SetCallbackUrl(new CallbackUrl(callbackUrl));
        }

        /// <summary>
        /// Sets the type of the gateway which the invoice must be paid in.
        /// </summary>
        /// <typeparam name="TGateway">Type of the gateway.</typeparam>
        /// <param name="builder"></param>
        /// <exception cref="InvalidGatewayTypeException"></exception>
        public static IInvoiceBuilder SetGatewayType<TGateway>(this IInvoiceBuilder builder) where TGateway : IGateway
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGatewayType(typeof(TGateway));
        }

        /// <summary>
        /// Sets the gateway using the given name. The given name must match the values of
        /// <see cref="Gateway"/> enum.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="gatewayName"></param>
        /// <exception cref="GatewayNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static IInvoiceBuilder UseGateway(this IInvoiceBuilder builder, string gatewayName)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (gatewayName == null) throw new ArgumentNullException(nameof(gatewayName));

            var gatewayType = GatewayHelper.FindGatewayTypeByName(gatewayName, throwException: true);

            return builder.SetGatewayType(gatewayType);
        }

        /// <summary>
        /// Appends the given dictionary to the additional data of the invoice.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="additionalData"></param>
        public static IInvoiceBuilder AddAdditionalData(this IInvoiceBuilder builder, IDictionary<string, object> additionalData)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (additionalData == null) throw new ArgumentNullException(nameof(additionalData));

            foreach (var data in additionalData)
            {
                builder.AddAdditionalData(data.Key, data.Value);
            }

            return builder;
        }
    }
}
