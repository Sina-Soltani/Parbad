// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Storage.Abstractions;
using Parbad.Storage.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Storage.Cache.Abstractions;

/// <summary>
/// Abstract cache implementation of Parbad storage.
/// </summary>
public abstract class CacheStorage : IStorage
{
    /// <inheritdoc />
    public virtual IQueryable<Payment> Payments => Collection.Payments.AsQueryable();

    /// <inheritdoc />
    public virtual IQueryable<Transaction> Transactions => Collection.Transactions.AsQueryable();

    /// <summary>
    /// A collection for holding the data.
    /// </summary>
    protected abstract ICacheStorageCollection Collection { get; }

    /// <inheritdoc />
    public virtual Task CreatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        if (payment == null) throw new ArgumentNullException(nameof(payment));
        cancellationToken.ThrowIfCancellationRequested();

        payment.Id = GenerateNewPaymentId();

        var record = FindPayment(payment);

        if (record != null) throw new InvalidOperationException($"There is already a payment record in database with id {payment.Id}");

        Collection.Payments.Add(payment);

        return SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        if (payment == null) throw new ArgumentNullException(nameof(payment));
        cancellationToken.ThrowIfCancellationRequested();

        var record = FindPayment(payment);

        if (record == null) throw new InvalidOperationException($"No payment records found in database with id {payment.Id}");

        record.Token = payment.Token;
        record.TrackingNumber = payment.TrackingNumber;
        record.TransactionCode = payment.TransactionCode;

        return SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task DeletePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        if (payment == null) throw new ArgumentNullException(nameof(payment));
        cancellationToken.ThrowIfCancellationRequested();

        var record = FindPayment(payment);

        if (record == null) throw new InvalidOperationException($"No payment records found in database with id {payment.Id}");

        Collection.Payments.Remove(record);

        Collection.Transactions.RemoveAll(model => model.PaymentId == record.Id);

        return SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));
        cancellationToken.ThrowIfCancellationRequested();

        transaction.Id = GenerateNewTransactionId();

        var record = FindTransaction(transaction);

        if (record != null) throw new InvalidOperationException($"There is already a transaction record in database with id {transaction.Id}");

        Collection.Transactions.Add(transaction);

        return SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task UpdateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));
        cancellationToken.ThrowIfCancellationRequested();

        var record = FindTransaction(transaction);

        if (record == null) throw new InvalidOperationException($"No payment records found in database with id {transaction.Id}");

        record.IsSucceed = transaction.IsSucceed;

        return SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task DeleteTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));
        cancellationToken.ThrowIfCancellationRequested();

        var record = FindTransaction(transaction);

        if (record == null) throw new InvalidOperationException($"No payment records found in database with id {transaction.Id}");

        Collection.Transactions.Remove(record);

        return SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task<Payment?> GetPaymentByTrackingNumberAsync(long trackingNumber, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var record = Collection.Payments.SingleOrDefault(payment => payment.TrackingNumber == trackingNumber);

        return Task.FromResult<Payment?>(record);
    }

    /// <inheritdoc />
    public virtual Task<Payment?> GetPaymentByTokenAsync(string paymentToken, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var record = Collection.Payments.SingleOrDefault(payment => payment.Token == paymentToken);

        return Task.FromResult<Payment?>(record);
    }

    /// <inheritdoc />
    public virtual Task<bool> DoesPaymentExistAsync(long trackingNumber, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var exists = Collection.Payments.Any(payment => payment.TrackingNumber == trackingNumber);

        return Task.FromResult(exists);
    }

    /// <inheritdoc />
    public virtual Task<bool> DoesPaymentExistAsync(string paymentToken, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var exists = Collection.Payments.Any(payment => payment.Token == paymentToken);

        return Task.FromResult(exists);
    }

    /// <inheritdoc />
    public virtual Task<List<Transaction>> GetTransactionsAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        var transactions = Collection.Transactions
                                     .Where(transaction => transaction.PaymentId == payment.Id)
                                     .ToList();

        return Task.FromResult(transactions);
    }

    /// <summary>
    /// Finds a payment in storage.
    /// </summary>
    /// <param name="payment"></param>
    protected virtual Payment? FindPayment(Payment payment)
    {
        return Collection.Payments.Contains(payment)
            ? payment
            : Collection.Payments.SingleOrDefault(model => model.Id == payment.Id);
    }

    /// <summary>
    /// Finds a transaction in storage.
    /// </summary>
    /// <param name="transaction"></param>
    protected virtual Transaction? FindTransaction(Transaction transaction)
    {
        return Collection.Transactions.Contains(transaction)
            ? transaction
            : Collection.Transactions.SingleOrDefault(model => model.Id == transaction.Id);
    }

    /// <summary>
    /// Generates a unique id for a new payment record.
    /// </summary>
    protected virtual long GenerateNewPaymentId()
    {
        return Collection.Payments.Count == 0
            ? 1
            : Collection.Payments.Max(model => model.Id) + 1;
    }

    /// <summary>
    /// Generates a unique id for a new transaction record.
    /// </summary>
    protected virtual long GenerateNewTransactionId()
    {
        return Collection.Transactions.Count == 0
            ? 1
            : Collection.Transactions.Max(model => model.Id) + 1;
    }

    /// <summary>
    /// Saves the current data in storage.
    /// </summary>
    /// <param name="cancellationToken"></param>
    protected abstract Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
