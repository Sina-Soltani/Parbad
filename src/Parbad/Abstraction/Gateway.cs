using System;
using System.Threading;
using System.Threading.Tasks;
using Parbad.Data.Domain.Payments;
using Parbad.GatewayBuilders;
using Parbad.Internal;

namespace Parbad.Abstraction
{
    /// <inheritdoc />
    public abstract class Gateway<TAccount> : IGateway where TAccount : GatewayAccount
    {
        protected Gateway(IGatewayAccountProvider<TAccount> accountProvider)
        {
            AccountProvider = accountProvider;
        }

        protected IGatewayAccountProvider<TAccount> AccountProvider { get; }

        /// <inheritdoc />
        public abstract Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public abstract Task<IPaymentVerifyResult> VerifyAsync(Payment payment, CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public abstract Task<IPaymentRefundResult> RefundAsync(Payment payment, Money amount, CancellationToken cancellationToken = default);

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
