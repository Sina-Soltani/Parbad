using Microsoft.EntityFrameworkCore;
using Parbad.Storage.Abstractions;
using Parbad.Storage.Abstractions.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Storage.EntityFrameworkCore
{
    /// <summary>
    /// EntityFramework Core implementation of <see cref="IStorageManager"/>.
    /// </summary>
    public class EntityFrameworkCoreStorageManager : IStorageManager
    {
        /// <summary>
        /// Initializes an instance of <see cref="EntityFrameworkCoreStorageManager"/>.
        /// </summary>
        /// <param name="storage"></param>
        public EntityFrameworkCoreStorageManager(IStorage storage)
        {
            Storage = storage;
        }

        protected readonly IStorage Storage;

        /// <inheritdoc />
        public virtual Task CreatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            return Storage.CreatePaymentAsync(payment, cancellationToken);
        }

        /// <inheritdoc />
        public virtual Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            return Storage.UpdatePaymentAsync(payment, cancellationToken);
        }

        /// <inheritdoc />
        public virtual Task CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
        {
            return Storage.CreateTransactionAsync(transaction, cancellationToken);
        }

        /// <inheritdoc />
        public virtual Task<Payment> GetPaymentByTrackingNumberAsync(long trackingNumber, CancellationToken cancellationToken = default)
        {
            return Storage.Payments
                .AsNoTracking()
                .SingleOrDefaultAsync(payment => payment.TrackingNumber == trackingNumber, cancellationToken);
        }

        /// <inheritdoc />
        public virtual Task<Payment> GetPaymentByTokenAsync(string paymentToken, CancellationToken cancellationToken = default)
        {
            return Storage.Payments
                .AsNoTracking()
                .SingleOrDefaultAsync(payment => payment.Token == paymentToken, cancellationToken);
        }

        /// <inheritdoc />
        public virtual Task<bool> DoesPaymentExistAsync(long trackingNumber, CancellationToken cancellationToken = default)
        {
            return Storage.Payments.AnyAsync(payment => payment.TrackingNumber == trackingNumber, cancellationToken);
        }

        /// <inheritdoc />
        public virtual Task<bool> DoesPaymentExistAsync(string paymentToken, CancellationToken cancellationToken = default)
        {
            return Storage.Payments.AnyAsync(payment => payment.Token == paymentToken, cancellationToken);
        }

        /// <inheritdoc />
        public virtual Task<List<Transaction>> GetTransactionsAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            return Storage.Transactions
                .Where(transaction => transaction.PaymentId == payment.Id)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
