// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.Abstraction;
using Parbad.Gateway.Zibal.Internal;
using Parbad.InvoiceBuilder;

namespace Parbad.Gateway.Zibal
{
    public static class ZibalGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to Zibal Gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IInvoiceBuilder UseZibal(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(ZibalGateway.Name);
        }

        /// <summary>
        /// Sets Zibal Gateway data.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="zarinPalInvoice">Describes an invoice for ZarinPal gateway.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IInvoiceBuilder SetZibalData(this IInvoiceBuilder builder, ZibalRequest zibalRequest)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddOrUpdateProperty(ZibalHelper.ZibalRequestAdditionalKeyName, zibalRequest);
        }

        internal static ZibalRequestModel GetZibalRequest(this Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            if (invoice.Properties.ContainsKey(ZibalHelper.ZibalRequestAdditionalKeyName))
            {
                var model = (ZibalRequest)invoice.Properties[ZibalHelper.ZibalRequestAdditionalKeyName];
                return new ZibalRequestModel()
                {
                    CustomerMobile = model.CustomerMobile,
                    Description = model.Description,
                    FeeMode = model.FeeMode,
                    SendSms = model.SendSms,
                    AllowedCards = model.AllowedCards,
                    LinkToPay = model.LinkToPay,
                };
            }

            return null;
        }
    }
}
