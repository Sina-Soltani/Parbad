// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Parbad.Abstraction;
using Parbad.Exceptions;
using Parbad.TrackingNumberProviders;

namespace Parbad.InvoiceBuilder
{
    /// <summary>
    /// A builder which helps to build an invoice.
    /// </summary>
    public interface IInvoiceBuilder
    {
        /// <summary>
        /// Gets or sets the tracking number of invoice.
        /// </summary>
        long TrackingNumber { get; set; }

        /// <summary>
        /// Gets os sets the amount of the invoice.
        /// <para>Note: You can also enter long and decimal numbers.</para>
        /// </summary>
        Money Amount { get; set; }

        /// <summary>
        /// A complete URL of your website. It will be used by the gateway for redirecting
        /// the client again to your website.
        /// <para>Note: A complete URL would be like: "http://www.mywebsite.com/foo/bar/"</para>
        /// </summary>
        CallbackUrl CallbackUrl { get; set; }

        /// <summary>
        /// Gets or sets the type of the gateway which the invoice must be paid in.
        /// </summary>
        string GatewayName { get; set; }

        /// <summary>
        /// Additional data for this invoice.
        /// </summary>
        IDictionary<string, object> AdditionalData { get; set; }

        /// <summary>
        /// Defines a mechanism for retrieving a service object; that is, an object that
        /// provides custom support to other objects.
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// Sets an <see cref="ITrackingNumberProvider"/> which generates a new tracking numbers for each payment requests.
        /// </summary>
        /// <param name="provider">An implementation of <see cref="ITrackingNumberProvider"/>.</param>
        /// <exception cref="ArgumentNullException"></exception>
        IInvoiceBuilder SetTrackingNumberProvider(ITrackingNumberProvider provider);

        /// <summary>
        /// Sets the amount of the invoice.
        /// </summary>
        /// <param name="amount">The amount of invoice.</param>
        IInvoiceBuilder SetAmount(Money amount);

        /// <summary>
        /// Sets the callback URL. It will be used by the gateway for redirecting the client
        /// again to your website.
        /// </summary>
        /// <param name="url">
        /// A complete URL of your website. It will be used by the gateway for redirecting the
        /// client again to your website.
        /// <para>Note: A complete URL would be like: "http://www.mywebsite.com/foo/bar/"</para>
        /// </param>
        IInvoiceBuilder SetCallbackUrl(CallbackUrl url);

        /// <summary>
        /// Sets the gateway name.
        /// </summary>
        /// <param name="gatewayName"></param>
        /// <exception cref="GatewayNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        IInvoiceBuilder SetGateway(string gatewayName);

        /// <summary>
        /// Adds additional data to the invoice.
        /// </summary>
        /// <param name="key">Key of the data</param>
        /// <param name="value">Value of the data</param>
        IInvoiceBuilder AddAdditionalData(string key, object value);

        /// <summary>
        /// Builds an invoice with the given data.
        /// </summary>
        Task<Invoice> BuildAsync(CancellationToken cancellationToken = default);
    }
}
