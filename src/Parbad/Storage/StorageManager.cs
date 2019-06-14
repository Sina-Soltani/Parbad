// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Parbad.Storage.Abstractions;

namespace Parbad.Storage
{
    /// <inheritdoc />
    public class StorageManager : IStorageManager
    {
        /// <summary>
        /// Initializes in instance of <see cref="StorageManager"/>.
        /// </summary>
        /// <param name="storage"></param>
        public StorageManager(IStorage storage)
        {
            Storage = storage;
        }

        /// <summary>
        /// 
        /// </summary>
        protected IStorage Storage { get; }

        /// <inheritdoc />
        public virtual Task CreatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
            => Storage.CreatePaymentAsync(payment, cancellationToken);

        /// <inheritdoc />
        public virtual Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
            => Storage.UpdatePaymentAsync(payment, cancellationToken);

        /// <inheritdoc />
        public virtual Task CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
            => Storage.CreateTransactionAsync(transaction, cancellationToken);

        /// <inheritdoc />
        public virtual Task UpdateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
            => Storage.UpdateTransactionAsync(transaction, cancellationToken);

        /// <inheritdoc />
        public virtual Task<Payment> GetPaymentByTrackingNumberAsync(long trackingNumber, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Storage.Payments.SingleOrDefault(model => model.TrackingNumber == trackingNumber));
        }

        /// <inheritdoc />
        public virtual Task<Payment> GetPaymentByTokenAsync(string paymentToken, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Storage.Payments.SingleOrDefault(model => model.Token == paymentToken));
        }

        /// <inheritdoc />
        public virtual Task<bool> DoesPaymentExistAsync(long trackingNumber, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Storage.Payments.Any(model => model.TrackingNumber == trackingNumber));
        }

        /// <inheritdoc />
        public virtual Task<bool> DoesPaymentExistAsync(string paymentToken, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Storage.Payments.Any(model => model.Token == paymentToken));
        }

        /// <inheritdoc />
        public virtual Task<List<Transaction>> GetTransactionsAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Storage.Transactions.Where(model => model.PaymentId == payment.Id).ToList());
        }
    }
}
