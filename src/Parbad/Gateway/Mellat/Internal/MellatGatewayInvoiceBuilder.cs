// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Parbad.InvoiceBuilder;

namespace Parbad.Gateway.Mellat.Internal
{
    internal class MellatGatewayInvoiceBuilder : IMellatGatewayInvoiceBuilder
    {
        public MellatGatewayInvoiceBuilder(IInvoiceBuilder invoiceBuilder)
        {
            InvoiceBuilder = invoiceBuilder;
        }

        public IInvoiceBuilder InvoiceBuilder { get; }

        public IMellatGatewayInvoiceBuilder AddMellatCumulativeAccount(long subServiceId, long amount)
        {
            return AddMellatCumulativeAccount(subServiceId, amount, 0);
        }

        public IMellatGatewayInvoiceBuilder AddMellatCumulativeAccount(long subServiceId, long amount, long payerId)
        {
            return AddMellatCumulativeAccounts(new List<MellatCumulativeDynamicAccount>
            {
                new MellatCumulativeDynamicAccount(subServiceId, amount, payerId)
            });
        }

        public IMellatGatewayInvoiceBuilder AddMellatCumulativeAccounts(IList<MellatCumulativeDynamicAccount> accounts)
        {
            if (accounts == null) throw new ArgumentNullException(nameof(accounts));
            if (accounts.Count == 0) throw new ArgumentException("Accounts cannot be an empty collection.", nameof(accounts));

            IList<MellatCumulativeDynamicAccount> existingAccounts = new List<MellatCumulativeDynamicAccount>();

            foreach (var account in accounts)
            {
                existingAccounts.Add(account);
            }

            InvoiceBuilder.AddAdditionalData(MellatHelper.CumulativeAccountsKey, existingAccounts);

            return this;
        }
    }
}
