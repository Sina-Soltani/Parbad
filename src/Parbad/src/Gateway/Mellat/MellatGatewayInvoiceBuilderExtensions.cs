﻿using System;
using System.Collections.Generic;
using System.Linq;
using Parbad.Gateway.Mellat.Internal;
using Parbad.InvoiceBuilder;

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

            builder.ChangeAdditionalData(data =>
            {
                if (data.ContainsKey(MellatHelper.CumulativeAccountsKey))
                {
                    allAccounts = (List<MellatCumulativeDynamicAccount>)data[MellatHelper.CumulativeAccountsKey];
                }
                else
                {
                    allAccounts = new List<MellatCumulativeDynamicAccount>();
                }

                allAccounts.AddRange(accounts);
            });

            builder.AddAdditionalData(MellatHelper.CumulativeAccountsKey, allAccounts);

            return builder;
        }
    }
}
