﻿// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.Gateway.ZarinPal;
using Parbad.Gateway.ZarinPal.Internal;
using Parbad.InvoiceBuilder;

namespace Parbad
{
    public static class ZarinPalGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to ZarinPal gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="zarinPalInvoice">Describes an invoice for ZarinPal gateway.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IInvoiceBuilder UseZarinPal(this IInvoiceBuilder builder, ZarinPalInvoice zarinPalInvoice)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.AddFormatter(invoice =>
            {
                if (invoice.AdditionalData.ContainsKey(ZarinPalHelper.ZarinPalRequestAdditionalKeyName))
                {
                    invoice.AdditionalData.Remove(ZarinPalHelper.ZarinPalRequestAdditionalKeyName);
                }

                invoice.AdditionalData.Add(ZarinPalHelper.ZarinPalRequestAdditionalKeyName, zarinPalInvoice);
            });

            return builder.SetGateway(ZarinPalGateway.Name);
        }

        /// <summary>
        /// The invoice will be sent to ZarinPal gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="description">A short description about this invoice which is required by ZarinPal gateway.</param>
        /// <param name="email">Buyer's email.</param>
        /// <param name="mobile">Buyer's mobile.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IInvoiceBuilder UseZarinPal(this IInvoiceBuilder builder, string description, string email = null, string mobile = null)
            => UseZarinPal(builder, new ZarinPalInvoice(description, email, mobile));
    }
}
