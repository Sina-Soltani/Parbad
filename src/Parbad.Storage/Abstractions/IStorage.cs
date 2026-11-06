// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Parbad.Storage.Abstractions.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Storage.Abstractions;

/// <summary>
/// A storage which used by Parbad for saving and loading data.
/// </summary>
public interface IStorage
{
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

    /// <summary>
    /// Gets a payment by its tracking number.
    /// </summary>
    /// <param name="trackingNumber"></param>
    /// <param name="cancellationToken"></param>
    Task<Payment?> GetPaymentByTrackingNumberAsync(long trackingNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a payment by its token.
    /// </summary>
    /// <param name="paymentToken"></param>
    /// <param name="cancellationToken"></param>
    Task<Payment?> GetPaymentByTokenAsync(string paymentToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a payment exists with the given <paramref name="trackingNumber"/>.
    /// </summary>
    /// <param name="trackingNumber"></param>
    /// <param name="cancellationToken"></param>
    Task<bool> DoesPaymentExistAsync(long trackingNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a payment exists with the given <paramref name="paymentToken"/>.
    /// </summary>
    /// <param name="paymentToken"></param>
    /// <param name="cancellationToken"></param>
    Task<bool> DoesPaymentExistAsync(string paymentToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the largest tracking number of all payments, or <see langword="null"/> when there are no payments.
    /// </summary>
    /// <param name="cancellationToken"></param>
    Task<long?> GetLargestTrackingNumberAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of transactions of the given <paramref name="payment"/>.
    /// </summary>
    /// <param name="payment"></param>
    /// <param name="cancellationToken"></param>
    Task<List<Transaction>> GetTransactionsAsync(Payment payment, CancellationToken cancellationToken = default);
}
