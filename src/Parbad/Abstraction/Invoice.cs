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
        /// <param name="gatewayType">The type of the gateway which the invoice must be paid in.</param>
        /// <param name="additionalData">Additional data for this invoice.</param>
        public Invoice(long trackingNumber, Money amount, CallbackUrl callbackUrl, Type gatewayType, IDictionary<string, object> additionalData = null)
        {
            if (trackingNumber <= 0)
            {
                throw new InvalidTrackingNumberException(trackingNumber);
            }

            TrackingNumber = trackingNumber;
            Amount = amount;
            CallbackUrl = callbackUrl;
            GatewayType = gatewayType;
            AdditionalData = additionalData ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Tracking number of invoice.
        /// </summary>
        public long TrackingNumber { get; }

        /// <summary>
        /// The amount of the invoice.
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
        /// The type of the gateway which the invoice must be paid in.
        /// </summary>
        public Type GatewayType { get; }

        /// <summary>
        /// Additional data for this invoice.
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; }
    }
}
