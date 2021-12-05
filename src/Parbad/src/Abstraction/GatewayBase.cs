// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Storage.Abstractions.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Abstraction
{
    /// <inheritdoc />
    public abstract class GatewayBase<TAccount> : IGateway where TAccount : GatewayAccount
    {
        protected GatewayBase(IGatewayAccountProvider<TAccount> accountProvider)
        {
            AccountProvider = accountProvider;
        }

        protected IGatewayAccountProvider<TAccount> AccountProvider { get; }

        /// <inheritdoc />
        public abstract Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public abstract Task<IPaymentFetchResult> FetchAsync(InvoiceContext context, CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public abstract Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public abstract Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a gateway account using the specified account name in the given invoice. If no account is given on the invoice, the first account will be retrieved.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        protected virtual async Task<TAccount> GetAccountAsync(Invoice invoice)
        {
            var accountName = invoice.GetAccountName();

            var accounts = await AccountProvider.LoadAccountsAsync();

            TAccount account;

            if (accountName.IsNullOrEmpty())
            {
                account = accounts.GetDefaultAccount();

                if (account == null) throw new Exception($"No accounts of type {typeof(TAccount).Name} exist.");
            }
            else
            {
                account = accounts.Get(accountName);

                if (account == null) throw new Exception($"Account {accountName} does not exist.");
            }

            return account;
        }

        /// <summary>
        /// Gets a gateway account using the specified account name in the given <paramref name="payment"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        protected virtual async Task<TAccount> GetAccountAsync(Payment payment)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            var accounts = await AccountProvider.LoadAccountsAsync();

            var account = accounts.Get(payment.GatewayAccountName);

            if (account == null) throw new Exception($"The account \"{payment.GatewayAccountName}\" of type {typeof(TAccount).Name} does not exist.");

            return account;
        }
    }
}
