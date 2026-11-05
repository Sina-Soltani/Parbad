using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Parbad.Storage.Abstractions;
using Parbad.Storage.Abstractions.Models;

namespace Parbad.PaymentFlow.Tests.Infrastructure;

/// <summary>
/// A storage that implements only <see cref="IStorage"/>, like a custom storage written before the optional
/// capabilities (<see cref="IPaidPaymentLookup"/>, <see cref="IPaymentCompletion"/>) existed. It forwards
/// everything to an <see cref="InMemoryStorage"/>.
/// </summary>
public sealed class StorageWithoutOptionalCapabilities : IStorage
{
    private readonly InMemoryStorage _inner;

    public StorageWithoutOptionalCapabilities(InMemoryStorage inner) => _inner = inner;

    public IQueryable<Payment> Payments => _inner.Payments;

    public IQueryable<Transaction> Transactions => _inner.Transactions;

    public Task CreatePaymentAsync(Payment payment, CancellationToken cancellationToken = default) =>
        _inner.CreatePaymentAsync(payment, cancellationToken);

    public Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default) =>
        _inner.UpdatePaymentAsync(payment, cancellationToken);

    public Task DeletePaymentAsync(Payment payment, CancellationToken cancellationToken = default) =>
        _inner.DeletePaymentAsync(payment, cancellationToken);

    public Task CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default) =>
        _inner.CreateTransactionAsync(transaction, cancellationToken);

    public Task UpdateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default) =>
        _inner.UpdateTransactionAsync(transaction, cancellationToken);

    public Task DeleteTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default) =>
        _inner.DeleteTransactionAsync(transaction, cancellationToken);

    public Task<Payment?> GetPaymentByTrackingNumberAsync(long trackingNumber, CancellationToken cancellationToken = default) =>
        _inner.GetPaymentByTrackingNumberAsync(trackingNumber, cancellationToken);

    public Task<Payment?> GetPaymentByTokenAsync(string paymentToken, CancellationToken cancellationToken = default) =>
        _inner.GetPaymentByTokenAsync(paymentToken, cancellationToken);

    public Task<bool> DoesPaymentExistAsync(long trackingNumber, CancellationToken cancellationToken = default) =>
        _inner.DoesPaymentExistAsync(trackingNumber, cancellationToken);

    public Task<bool> DoesPaymentExistAsync(string paymentToken, CancellationToken cancellationToken = default) =>
        _inner.DoesPaymentExistAsync(paymentToken, cancellationToken);

    public Task<List<Transaction>> GetTransactionsAsync(Payment payment, CancellationToken cancellationToken = default) =>
        _inner.GetTransactionsAsync(payment, cancellationToken);
}
