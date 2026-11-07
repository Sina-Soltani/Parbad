using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Parbad.Storage.Abstractions;
using Parbad.Storage.Abstractions.Models;

namespace Parbad.PaymentFlow.Tests.Infrastructure;

/// <summary>
/// A thread-safe <see cref="IStorage"/> that stores copies of the records, like a real database.
/// It enforces unique tracking numbers and tokens, so the tests observe Parbad's own behaviour and are
/// not affected by the cache storages.
/// </summary>
public sealed class InMemoryStorage : IStorage
{
    private readonly object _sync = new();
    private readonly List<Payment> _payments = new();
    private readonly List<Transaction> _transactions = new();
    private long _lastPaymentId;
    private long _lastTransactionId;

    /// <summary>
    /// When set, <see cref="UpdatePaymentAsync"/> or <see cref="TryCompletePaymentAsync"/> throws this exception once,
    /// to simulate a database failure.
    /// </summary>
    public Exception? FailNextPaymentUpdate { get; set; }

    public IQueryable<Payment> Payments
    {
        get
        {
            lock (_sync) return _payments.Select(Copy).ToList().AsQueryable();
        }
    }

    public IQueryable<Transaction> Transactions
    {
        get
        {
            lock (_sync) return _transactions.Select(Copy).ToList().AsQueryable();
        }
    }

    public Task CreatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            if (_payments.Any(p => p.TrackingNumber == payment.TrackingNumber))
                throw new InvalidOperationException($"Duplicate tracking number {payment.TrackingNumber}.");

            if (_payments.Any(p => p.Token == payment.Token))
                throw new InvalidOperationException($"Duplicate payment token {payment.Token}.");

            payment.Id = ++_lastPaymentId;
            _payments.Add(Copy(payment));
        }

        return Task.CompletedTask;
    }

    public Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            if (FailNextPaymentUpdate is { } failure)
            {
                FailNextPaymentUpdate = null;
                throw failure;
            }

            var index = _payments.FindIndex(p => p.Id == payment.Id);

            if (index < 0) throw new InvalidOperationException($"No payment with id {payment.Id}.");

            _payments[index] = Copy(payment);
        }

        return Task.CompletedTask;
    }

    public Task<bool> TryCompletePaymentAsync(long paymentId,
                                              PaymentStatus status,
                                              string? transactionCode,
                                              string? bankReference,
                                              CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            if (FailNextPaymentUpdate is { } failure)
            {
                FailNextPaymentUpdate = null;
                throw failure;
            }

            var record = _payments.SingleOrDefault(p => p.Id == paymentId) ?? throw new InvalidOperationException($"No payment with id {paymentId}.");

            if (record.IsPaid || (record.IsCompleted && status != PaymentStatus.Paid)) return Task.FromResult(false);

            record.Status = status;
            record.TransactionCode = transactionCode;
            record.BankReference = bankReference;

            return Task.FromResult(true);
        }
    }

    public Task DeletePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        lock (_sync) _payments.RemoveAll(p => p.Id == payment.Id);

        return Task.CompletedTask;
    }

    public Task CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            transaction.Id = ++_lastTransactionId;
            _transactions.Add(Copy(transaction));
        }

        return Task.CompletedTask;
    }

    public Task UpdateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            var index = _transactions.FindIndex(t => t.Id == transaction.Id);

            if (index < 0) throw new InvalidOperationException($"No transaction with id {transaction.Id}.");

            _transactions[index] = Copy(transaction);
        }

        return Task.CompletedTask;
    }

    public Task DeleteTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        lock (_sync) _transactions.RemoveAll(t => t.Id == transaction.Id);

        return Task.CompletedTask;
    }

    public Task<Payment?> GetPaymentByTrackingNumberAsync(long trackingNumber, CancellationToken cancellationToken = default)
    {
        lock (_sync) return Task.FromResult(_payments.Where(p => p.TrackingNumber == trackingNumber).Select(Copy).SingleOrDefault());
    }

    public Task<Payment?> GetPaymentByTokenAsync(string paymentToken, CancellationToken cancellationToken = default)
    {
        lock (_sync) return Task.FromResult(_payments.Where(p => p.Token == paymentToken).Select(Copy).SingleOrDefault());
    }

    public Task<Payment?> FindPaidPaymentByBankReferenceAsync(string gatewayName,
                                                             string bankReference,
                                                               long exceptTrackingNumber,
                                                               CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            return Task.FromResult(_payments.Where(p => p.IsPaid &&
                                                        p.TrackingNumber != exceptTrackingNumber &&
                                                        (p.BankReference == bankReference ||
                                                         (p.BankReference == null && p.TransactionCode == bankReference)) &&
                                                        string.Equals(p.GatewayName, gatewayName, StringComparison.OrdinalIgnoreCase))
                                            .Select(Copy)
                                            .FirstOrDefault());
        }
    }

    public Task<bool> DoesPaymentExistAsync(long trackingNumber, CancellationToken cancellationToken = default)
    {
        lock (_sync) return Task.FromResult(_payments.Any(p => p.TrackingNumber == trackingNumber));
    }

    public Task<bool> DoesPaymentExistAsync(string paymentToken, CancellationToken cancellationToken = default)
    {
        lock (_sync) return Task.FromResult(_payments.Any(p => p.Token == paymentToken));
    }

    public Task<long?> GetLargestTrackingNumberAsync(CancellationToken cancellationToken = default)
    {
        lock (_sync) return Task.FromResult(_payments.Count == 0 ? (long?)null : _payments.Max(p => p.TrackingNumber));
    }

    public Task<List<Transaction>> GetTransactionsAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        lock (_sync) return Task.FromResult(_transactions.Where(t => t.PaymentId == payment.Id).Select(Copy).ToList());
    }

    private static Payment Copy(Payment payment) => new()
    {
        Id = payment.Id,
        TrackingNumber = payment.TrackingNumber,
        Amount = payment.Amount,
        Token = payment.Token,
        TransactionCode = payment.TransactionCode,
        GatewayName = payment.GatewayName,
        GatewayAccountName = payment.GatewayAccountName,
        BankReference = payment.BankReference,
        Status = payment.Status,
        RefundedAmount = payment.RefundedAmount
    };

    private static Transaction Copy(Transaction transaction) => new()
    {
        Id = transaction.Id,
        Amount = transaction.Amount,
        Type = transaction.Type,
        IsSucceed = transaction.IsSucceed,
        Message = transaction.Message,
        AdditionalData = transaction.AdditionalData,
        PaymentId = transaction.PaymentId
    };
}
