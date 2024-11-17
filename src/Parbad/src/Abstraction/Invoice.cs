// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Parbad.Abstraction;

/// <summary>
/// Describes an invoice which must be requested.
/// </summary>
public class Invoice
{
    /// <summary>
    /// Describes an invoice which must be requested.
    /// </summary>
    public Invoice()
    {
    }

    /// <summary>
    /// Gets or sets the Tracking number of the invoice.
    /// </summary>
    public long TrackingNumber { get; set; }

    /// <summary>
    /// Gets or sets the amount of the invoice.
    /// <para>Note: You can also enter long and decimal numbers. It can also be parsed to long and decimal.</para>
    /// <para>Examples:</para>
    /// <para>long a = invoice.Amount;</para>
    /// <para>decimal a = invoice.Amount;</para>
    /// </summary>
    public Money Amount { get; set; }

    /// <summary>
    /// A complete URL of your website. It will be used by the gateway for redirecting
    /// the client again to your website.
    /// <para>Note: A complete URL would be like: "http://www.mywebsite.com/foo/bar/"</para>
    /// </summary>
    public CallbackUrl CallbackUrl { get; set; }

    /// <summary>
    /// Gets or sets the name of the gateway which the invoice must be paid in.
    /// </summary>
    public string GatewayName { get; set; }

    /// <summary>
    /// Gets or sets the properties of the invoice.
    /// </summary>
    public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
}
