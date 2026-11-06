// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Storage.Abstractions;
using Parbad.Storage.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Storage.Cache.Abstractions;

/// <summary>
/// Abstract cache implementation of Parbad storage.
/// </summary>
public abstract class CacheStorage : IStorage
{
    // The storages are transient and all instances that use the same cache share one collection,
    // so the lock must belong to the collection, not to the storage instance.
    private static readonly ConditionalWeakTable<ICacheStorageCollection, object> CollectionLocks = new();

    /// <summary>
    /// A collection for holding the data.
    /// </summary>
    protected abstract ICacheStorageCollection Collection { get; }

    /// <summary>
    /// The lock that guards every read and write of <see cref="Collection"/>.
    /// </summary>
    private protected object SyncRoot => CollectionLocks.GetValue(Collection, _ => new object());

    /// <inheritdoc />
    public virtual Task CreatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        if (payment == null) throw new ArgumentNullException(nameof(payment));
        cancellationToken.ThrowIfCancellationRequested();

        lock (SyncRoot)
        {
            ThrowIfPaymentIsNotUnique(payment, existingRecord: null);

            payment.Id = GenerateNewPaymentId();

            var record = FindPayment(payment);

            if (record != null) throw new InvalidOperationException($"There is already a payment record in database with id {payment.Id}");

            Collection.Payments.Add(ClonePayment(payment));
        }

        return SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        if (payment == null) throw new ArgumentNullException(nameof(payment));
        cancellationToken.ThrowIfCancellationRequested();

        lock (SyncRoot)
        {
            var record = FindPayment(payment);

            if (record == null) throw new InvalidOperationException($"No payment records found in database with id {payment.Id}");

            ThrowIfPaymentIsNotUnique(payment, record);

            CopyPayment(payment, record);
        }

        return SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task DeletePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        if (payment == null) throw new ArgumentNullException(nameof(payment));
        cancellationToken.ThrowIfCancellationRequested();

        lock (SyncRoot)
        {
            var record = FindPayment(payment);

            if (record == null) throw new InvalidOperationException($"No payment records found in database with id {payment.Id}");

            Collection.Payments.Remove(record);

            Collection.Transactions.RemoveAll(model => model.PaymentId == record.Id);
        }

        return SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));
        cancellationToken.ThrowIfCancellationRequested();

        lock (SyncRoot)
        {
            transaction.Id = GenerateNewTransactionId();

            var record = FindTransaction(transaction);

            if (record != null) throw new InvalidOperationException($"There is already a transaction record in database with id {transaction.Id}");

            Collection.Transactions.Add(CloneTransaction(transaction));
        }

        return SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task UpdateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));
        cancellationToken.ThrowIfCancellationRequested();

        lock (SyncRoot)
        {
            var record = FindTransaction(transaction);

            if (record == null) throw new InvalidOperationException($"No payment records found in database with id {transaction.Id}");

            CopyTransaction(transaction, record);
        }

        return SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task DeleteTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));
        cancellationToken.ThrowIfCancellationRequested();

        lock (SyncRoot)
        {
            var record = FindTransaction(transaction);

            if (record == null) throw new InvalidOperationException($"No payment records found in database with id {transaction.Id}");

            Collection.Transactions.Remove(record);
        }

        return SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task<Payment?> GetPaymentByTrackingNumberAsync(long trackingNumber, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (SyncRoot)
        {
            var record = Collection.Payments.SingleOrDefault(payment => payment.TrackingNumber == trackingNumber);

            return Task.FromResult<Payment?>(record == null ? null : ClonePayment(record));
        }
    }

    /// <inheritdoc />
    public virtual Task<Payment?> GetPaymentByTokenAsync(string paymentToken, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (SyncRoot)
        {
            var record = Collection.Payments.SingleOrDefault(payment => payment.Token == paymentToken);

            return Task.FromResult<Payment?>(record == null ? null : ClonePayment(record));
        }
    }

    /// <inheritdoc />
    public virtual Task<Payment?> FindPaidPaymentByBankReferenceAsync(string gatewayName,
                                                                      string bankReference,
                                                                        long exceptTrackingNumber,
                                                                        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (SyncRoot)
        {
            // Payments stored before Parbad 4.0 have no bank reference; their transaction code is compared instead.
            var record = Collection.Payments.FirstOrDefault(payment => payment.IsPaid &&
                                                                       payment.TrackingNumber != exceptTrackingNumber &&
                                                                       string.Equals(payment.GatewayName, gatewayName, StringComparison.OrdinalIgnoreCase) &&
                                                                       (payment.BankReference == bankReference ||
                                                                        (payment.BankReference == null && payment.TransactionCode == bankReference)));

            return Task.FromResult<Payment?>(record == null ? null : ClonePayment(record));
        }
    }

    /// <inheritdoc />
    public virtual async Task<bool> TryCompletePaymentAsync(long paymentId,
                                                            PaymentStatus status,
                                                            string? transactionCode,
                                                            string? bankReference,
                                                            CancellationToken cancellationToken = default)
    {
        if (status is not (PaymentStatus.Paid or PaymentStatus.Failed or PaymentStatus.Cancelled))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "A payment is completed as paid, failed or cancelled.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        lock (SyncRoot)
        {
            var record = Collection.Payments.SingleOrDefault(payment => payment.Id == paymentId);

            if (record == null) throw new InvalidOperationException($"No payment records found in database with id {paymentId}");

            // A paid payment is never changed; a failed or cancelled one only becomes paid.
            if (record.IsPaid || (record.IsCompleted && status != PaymentStatus.Paid)) return false;

            record.Status = status;
            record.TransactionCode = transactionCode;
            record.BankReference = bankReference;
        }

        await SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }

    /// <inheritdoc />
    public virtual Task<bool> DoesPaymentExistAsync(long trackingNumber, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (SyncRoot)
        {
            var exists = Collection.Payments.Any(payment => payment.TrackingNumber == trackingNumber);

            return Task.FromResult(exists);
        }
    }

    /// <inheritdoc />
    public virtual Task<bool> DoesPaymentExistAsync(string paymentToken, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (SyncRoot)
        {
            var exists = Collection.Payments.Any(payment => payment.Token == paymentToken);

            return Task.FromResult(exists);
        }
    }

    /// <inheritdoc />
    public virtual Task<long?> GetLargestTrackingNumberAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (SyncRoot)
        {
            return Task.FromResult(Collection.Payments.Count == 0 ? (long?)null : Collection.Payments.Max(payment => payment.TrackingNumber));
        }
    }

    /// <inheritdoc />
    public virtual Task<List<Transaction>> GetTransactionsAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        lock (SyncRoot)
        {
            var transactions = Collection.Transactions
                                         .Where(transaction => transaction.PaymentId == payment.Id)
                                         .Select(CloneTransaction)
                                         .ToList();

            return Task.FromResult(transactions);
        }
    }

    /// <summary>
    /// Finds a payment in storage.
    /// </summary>
    /// <param name="payment"></param>
    protected virtual Payment? FindPayment(Payment payment)
    {
        return Collection.Payments.SingleOrDefault(model => model.Id == payment.Id);
    }

    /// <summary>
    /// Finds a transaction in storage.
    /// </summary>
    /// <param name="transaction"></param>
    protected virtual Transaction? FindTransaction(Transaction transaction)
    {
        return Collection.Transactions.SingleOrDefault(model => model.Id == transaction.Id);
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

    // Lookups by tracking number or token use SingleOrDefault, so a duplicate would make both payments unreachable.
    private void ThrowIfPaymentIsNotUnique(Payment payment, Payment? existingRecord)
    {
        foreach (var record in Collection.Payments)
        {
            if (ReferenceEquals(record, existingRecord)) continue;

            if (record.TrackingNumber == payment.TrackingNumber)
            {
                throw new InvalidOperationException($"There is already a payment record in database with tracking number {payment.TrackingNumber}");
            }

            if (payment.Token != null && record.Token == payment.Token)
            {
                throw new InvalidOperationException($"There is already a payment record in database with token {payment.Token}");
            }
        }
    }

    private static Payment ClonePayment(Payment payment)
    {
        var clone = new Payment { Id = payment.Id };

        CopyPayment(payment, clone);

        return clone;
    }

    private static void CopyPayment(Payment source, Payment target)
    {
        target.TrackingNumber = source.TrackingNumber;
        target.Amount = source.Amount;
        target.Token = source.Token;
        target.TransactionCode = source.TransactionCode;
        target.GatewayName = source.GatewayName;
        target.GatewayAccountName = source.GatewayAccountName;
        target.BankReference = source.BankReference;
        target.Status = source.Status;
        target.RefundedAmount = source.RefundedAmount;
    }

    private static Transaction CloneTransaction(Transaction transaction)
    {
        var clone = new Transaction { Id = transaction.Id };

        CopyTransaction(transaction, clone);

        return clone;
    }

    private static void CopyTransaction(Transaction source, Transaction target)
    {
        target.Amount = source.Amount;
        target.Type = source.Type;
        target.IsSucceed = source.IsSucceed;
        target.Message = source.Message;
        target.AdditionalData = source.AdditionalData;
        target.PaymentId = source.PaymentId;
    }
}
