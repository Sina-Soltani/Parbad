// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Parbad.GatewayProviders.Mellat;
using Parbad.InvoiceBuilder;

namespace Parbad
{
    public static class MellatGatewayExtraFunctionsExtensions
    {
        public static IInvoiceBuilder AddMellatCumulativeAccount(this IInvoiceBuilder builder, long subServiceId, long amount)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddMellatCumulativeAccount(subServiceId, amount, 0);
        }

        public static IInvoiceBuilder AddMellatCumulativeAccount(this IInvoiceBuilder builder, long subServiceId, long amount, long payerId)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddMellatCumulativeAccounts(new List<MellatCumulativeDynamicAccount>
            {
                new MellatCumulativeDynamicAccount(subServiceId, amount, payerId)
            });
        }

        public static IInvoiceBuilder AddMellatCumulativeAccounts(this IInvoiceBuilder builder, IList<MellatCumulativeDynamicAccount> accounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (accounts == null) throw new ArgumentNullException(nameof(accounts));
            if (accounts.Count == 0) throw new ArgumentException("accounts cannot be an empty collection.", nameof(accounts));

            IList<MellatCumulativeDynamicAccount> existingAccounts;

            if (builder.AdditionalData.ContainsKey(MellatHelper.CumulativeAccountsKey))
            {
                existingAccounts = (IList<MellatCumulativeDynamicAccount>)builder.AdditionalData[MellatHelper.CumulativeAccountsKey];
            }
            else
            {
                existingAccounts = new List<MellatCumulativeDynamicAccount>();
            }

            foreach (var account in accounts)
            {
                existingAccounts.Add(account);
            }

            builder.AdditionalData.Remove(MellatHelper.CumulativeAccountsKey);
            builder.AddAdditionalData(MellatHelper.CumulativeAccountsKey, existingAccounts);

            builder.UseGateway(Gateway.Mellat);

            return builder;
        }
    }
}
