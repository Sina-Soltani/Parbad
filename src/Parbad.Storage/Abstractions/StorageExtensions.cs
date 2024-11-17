using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Parbad.Storage.Abstractions.Models;

namespace Parbad.Storage.Abstractions;

public static class StorageExtensions
{
    /// <inheritdoc />
    public static Task<Payment> GetPaymentByTrackingNumberAsync(this IStorage storage,
                                                                long trackingNumber,
                                                                CancellationToken cancellationToken = default)
    {
        if (storage == null) throw new ArgumentNullException(nameof(storage));

        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(storage.Payments.SingleOrDefault(model => model.TrackingNumber == trackingNumber));
    }

    /// <inheritdoc />
    public static Task<Payment> GetPaymentByTokenAsync(this IStorage storage,
                                                       string paymentToken,
                                                       CancellationToken cancellationToken = default)
    {
        if (storage == null) throw new ArgumentNullException(nameof(storage));

        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(storage.Payments.SingleOrDefault(model => model.Token == paymentToken));
    }

    /// <inheritdoc />
    public static Task<bool> DoesPaymentExistAsync(this IStorage storage,
                                                   long trackingNumber,
                                                   CancellationToken cancellationToken = default)
    {
        if (storage == null) throw new ArgumentNullException(nameof(storage));

        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(storage.Payments.Any(model => model.TrackingNumber == trackingNumber));
    }

    /// <inheritdoc />
    public static Task<bool> DoesPaymentExistAsync(this IStorage storage,
                                                   string paymentToken,
                                                   CancellationToken cancellationToken = default)
    {
        if (storage == null) throw new ArgumentNullException(nameof(storage));

        return Task.FromResult(storage.Payments.Any(model => model.Token == paymentToken));
    }

    /// <inheritdoc />
    public static Task<List<Transaction>> GetTransactionsAsync(this IStorage storage,
                                                               Payment payment,
                                                               CancellationToken cancellationToken = default)
    {
        if (storage == null) throw new ArgumentNullException(nameof(storage));

        return Task.FromResult(storage.Transactions.Where(model => model.PaymentId == payment.Id).ToList());
    }
}
