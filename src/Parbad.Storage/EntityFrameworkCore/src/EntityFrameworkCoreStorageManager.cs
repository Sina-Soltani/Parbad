using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Parbad.Storage.Abstractions;

namespace Parbad.Storage.EntityFrameworkCore
{
    /// <summary>
    /// EntityFramework Core implementation of <see cref="IStorageManager"/>.
    /// </summary>
    public class EntityFrameworkCoreStorageManager : IStorageManager
    {
        private readonly IStorage _storage;

        /// <summary>
        /// Initializes an instance of <see cref="EntityFrameworkCoreStorageManager"/>.
        /// </summary>
        /// <param name="storage"></param>
        public EntityFrameworkCoreStorageManager(IStorage storage)
        {
            _storage = storage;
        }

        /// <inheritdoc />
        public Task CreatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            return _storage.CreatePaymentAsync(payment, cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            return _storage.UpdatePaymentAsync(payment, cancellationToken);
        }

        /// <inheritdoc />
        public Task CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
        {
            return _storage.CreateTransactionAsync(transaction, cancellationToken);
        }

        /// <inheritdoc />
        public Task<Payment> GetPaymentByTrackingNumberAsync(long trackingNumber, CancellationToken cancellationToken = default)
        {
            return _storage.Payments
                .AsNoTracking()
                .SingleOrDefaultAsync(payment => payment.TrackingNumber == trackingNumber, cancellationToken);
        }

        /// <inheritdoc />
        public Task<Payment> GetPaymentByTokenAsync(string paymentToken, CancellationToken cancellationToken = default)
        {
            return _storage.Payments
                .AsNoTracking()
                .SingleOrDefaultAsync(payment => payment.Token == paymentToken, cancellationToken);
        }

        /// <inheritdoc />
        public Task<bool> DoesPaymentExistAsync(long trackingNumber, CancellationToken cancellationToken = default)
        {
            return _storage.Payments.AnyAsync(payment => payment.TrackingNumber == trackingNumber, cancellationToken);
        }

        /// <inheritdoc />
        public Task<bool> DoesPaymentExistAsync(string paymentToken, CancellationToken cancellationToken = default)
        {
            return _storage.Payments.AnyAsync(payment => payment.Token == paymentToken, cancellationToken);
        }

        /// <inheritdoc />
        public Task<List<Transaction>> GetTransactionsAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            return _storage.Transactions
                .Where(transaction => transaction.PaymentId == payment.Id)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
