// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Storage.Abstractions.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Storage.Abstractions
{
    /// <summary>
    /// A storage which used by Parbad for saving and loading data.
    /// </summary>
    public interface IStorage
    {
        /// <summary>
        /// Gets a list of <see cref="Payment"/>.
        /// </summary>
        IQueryable<Payment> Payments { get; }

        /// <summary>
        /// Gets a list of <see cref="Transaction"/>.
        /// </summary>
        IQueryable<Transaction> Transactions { get; }

        /// <summary>
        /// Creates a new payment.
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="cancellationToken"></param>
        Task CreatePaymentAsync(Payment payment, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the given <paramref name="payment"/>.
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="cancellationToken"></param>
        Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the given <paramref name="payment"/>.
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="cancellationToken"></param>
        Task DeletePaymentAsync(Payment payment, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new transaction.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="cancellationToken"></param>
        Task CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the given <paramref name="transaction"/>.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="cancellationToken"></param>
        Task UpdateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the given <paramref name="transaction"/>.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="cancellationToken"></param>
        Task DeleteTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);
    }
}
