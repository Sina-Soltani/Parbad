// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Abstraction;
using Parbad.Gateway.Mellat.Internal;
using Parbad.InvoiceBuilder;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parbad.Gateway.Mellat
{
    public static class MellatGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to Mellat gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UseMellat(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(MellatGateway.Name);
        }

        public static IInvoiceBuilder AddMellatCumulativeAccount(this IInvoiceBuilder builder, long subServiceId, long amount)
        {
            return AddMellatCumulativeAccount(builder, subServiceId, amount, 0);
        }

        public static IInvoiceBuilder AddMellatCumulativeAccount(this IInvoiceBuilder builder, long subServiceId, long amount, long payerId)
        {
            return AddMellatCumulativeAccounts(builder, new List<MellatCumulativeDynamicAccount>
            {
                new MellatCumulativeDynamicAccount(subServiceId, amount, payerId)
            });
        }

        public static IInvoiceBuilder AddMellatCumulativeAccounts(this IInvoiceBuilder builder, IEnumerable<MellatCumulativeDynamicAccount> accounts)
        {
            if (accounts == null) throw new ArgumentNullException(nameof(accounts));
            if (!accounts.Any()) throw new ArgumentException("Accounts cannot be an empty collection.", nameof(accounts));

            List<MellatCumulativeDynamicAccount> allAccounts = null;

            builder.ChangeProperties(properties =>
            {
                if (properties.ContainsKey(MellatHelper.CumulativeAccountsKey))
                {
                    allAccounts = (List<MellatCumulativeDynamicAccount>)properties[MellatHelper.CumulativeAccountsKey];
                }
                else
                {
                    allAccounts = new List<MellatCumulativeDynamicAccount>();

                    properties.Add(MellatHelper.CumulativeAccountsKey, allAccounts);
                }

                allAccounts.AddRange(accounts);
            });

            return builder;
        }

        /// <summary>
        /// Sets additional data for <see cref="MellatGateway"/>.
        /// </summary>
        public static IInvoiceBuilder SetMellatAdditionalData(this IInvoiceBuilder builder, MellatGatewayAdditionalDataRequest request)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (request == null) throw new ArgumentNullException(nameof(request));

            builder.AddOrUpdateProperty(MellatHelper.AdditionalDataKey, request);

            return builder;
        }

        internal static MellatGatewayAdditionalDataRequest GetMellatAdditionalData(this Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            if (invoice.Properties.ContainsKey(MellatHelper.AdditionalDataKey))
            {
                return (MellatGatewayAdditionalDataRequest)invoice.Properties[MellatHelper.AdditionalDataKey];
            }

            return null;
        }
    }
}
