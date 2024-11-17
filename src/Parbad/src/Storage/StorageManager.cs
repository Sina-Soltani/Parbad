// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.Storage.Abstractions;
using Parbad.Storage.Abstractions.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Storage;

/// <inheritdoc />
[Obsolete("This interface will be removed in a future release.")]
public class StorageManager : IStorageManager
{
    /// <summary>
    /// Initializes an instance of <see cref="StorageManager"/>.
    /// </summary>
    /// <param name="storage"></param>
    [Obsolete("This interface will be removed in a future release. All methods are moved to IStorage interface.")]
    public StorageManager(IStorage storage)
    {
        Storage = storage;
    }

    /// <summary>
    /// Gets an instance of <see cref="IStorage"/>.
    /// </summary>
    protected IStorage Storage { get; }

    /// <inheritdoc />
    public IQueryable<Payment> Payments => Storage.Payments;

    /// <inheritdoc />
    public IQueryable<Transaction> Transactions => Storage.Transactions;

    /// <inheritdoc />
    public virtual Task CreatePaymentAsync(Payment payment, CancellationToken cancellationToken = default) =>
        Storage.CreatePaymentAsync(payment, cancellationToken);

    /// <inheritdoc />
    public virtual Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default) =>
        Storage.UpdatePaymentAsync(payment, cancellationToken);

    public Task DeletePaymentAsync(Payment payment, CancellationToken cancellationToken = default) =>
        Storage.DeletePaymentAsync(payment, cancellationToken);

    /// <inheritdoc />
    public virtual Task CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default) =>
        Storage.CreateTransactionAsync(transaction, cancellationToken);

    public Task UpdateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default) =>
        Storage.UpdateTransactionAsync(transaction, cancellationToken);

    public Task DeleteTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default) =>
        Storage.DeleteTransactionAsync(transaction, cancellationToken);

    /// <inheritdoc />
    public virtual Task<Payment> GetPaymentByTrackingNumberAsync(long trackingNumber, CancellationToken cancellationToken = default) =>
        Storage.GetPaymentByTrackingNumberAsync(trackingNumber, cancellationToken);

    /// <inheritdoc />
    public virtual Task<Payment> GetPaymentByTokenAsync(string paymentToken, CancellationToken cancellationToken = default) =>
        Storage.GetPaymentByTokenAsync(paymentToken, cancellationToken);

    /// <inheritdoc />
    public virtual Task<bool> DoesPaymentExistAsync(long trackingNumber, CancellationToken cancellationToken = default) =>
        Storage.DoesPaymentExistAsync(trackingNumber, cancellationToken);

    /// <inheritdoc />
    public virtual Task<bool> DoesPaymentExistAsync(string paymentToken, CancellationToken cancellationToken = default) =>
        Storage.DoesPaymentExistAsync(paymentToken, cancellationToken);

    /// <inheritdoc />
    public virtual Task<List<Transaction>> GetTransactionsAsync(Payment payment, CancellationToken cancellationToken = default) =>
        Storage.GetTransactionsAsync(payment, cancellationToken);
}
