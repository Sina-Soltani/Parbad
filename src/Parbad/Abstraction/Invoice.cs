// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Parbad.Exceptions;

namespace Parbad.Abstraction
{
    /// <summary>
    /// Describes an invoice which must be requested.
    /// </summary>
    public class Invoice
    {
        /// <summary>
        /// Initializes an instance of <see cref="Invoice"/>.
        /// </summary>
        /// <param name="trackingNumber">Tracking number of invoice.</param>
        /// <param name="amount">The amount of invoice.
        /// <para>Note: You can also enter long and decimal numbers.</para>
        /// </param>
        /// <param name="callbackUrl">A complete URL of your website. It will be used by the gateway for redirecting the client again to your website.
        /// <para>Note: A complete URL would be like: "http://www.mywebsite.com/foo/bar/"</para>
        /// </param>
        /// <param name="gatewayName">Name of the gateway which the invoice must be paid in.</param>
        /// <param name="additionalData">Additional data for this invoice.</param>
        public Invoice(long trackingNumber, Money amount, CallbackUrl callbackUrl, string gatewayName, IDictionary<string, object> additionalData = null)
        {
            if (trackingNumber <= 0) throw new InvalidTrackingNumberException(trackingNumber);
            TrackingNumber = trackingNumber;
            Amount = amount ?? throw new ArgumentNullException(nameof(amount));
            CallbackUrl = callbackUrl ?? throw new ArgumentNullException(nameof(callbackUrl));
            GatewayName = gatewayName ?? throw new ArgumentNullException(nameof(gatewayName));
            AdditionalData = additionalData ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets the Tracking number of the invoice.
        /// </summary>
        public long TrackingNumber { get; }

        /// <summary>
        /// Gets the amount of the invoice.
        /// <para>Note: You can also enter long and decimal numbers. It can also be parsed to long and decimal.</para>
        /// <para>Examples:</para>
        /// <para>long a = invoice.Amount;</para>
        /// <para>decimal a = invoice.Amount;</para>
        /// </summary>
        public Money Amount { get; }

        /// <summary>
        /// A complete URL of your website. It will be used by the gateway for redirecting
        /// the client again to your website.
        /// <para>Note: A complete URL would be like: "http://www.mywebsite.com/foo/bar/"</para>
        /// </summary>
        public CallbackUrl CallbackUrl { get; }

        /// <summary>
        /// Gets the name of the gateway which the invoice must be paid in.
        /// </summary>
        public string GatewayName { get; }

        /// <summary>
        /// Gets the additional data of the invoice.
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; }
    }
}
