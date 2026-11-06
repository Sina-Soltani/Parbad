// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Parbad.Storage.Abstractions.Models;
using Parbad.Storage.Cache.Abstractions;
using Parbad.Storage.Cache.Internal;

namespace Parbad.Storage.Cache.DistributedCache;

/// <summary>
/// Distributed cache implementation of Parbad storage, which lets several instances of an application share
/// the same payment records.
/// <para>
/// Note: all records are kept in a single cache entry, which an instance loads once and then rewrites
/// completely on every save. Writes of instances that work at the same time can therefore overwrite each
/// other's payments, and the data is lost when the cache entry is evicted. Consider the Entity Framework Core
/// storage (<c>Parbad.Storage.EntityFrameworkCore</c>) where payments must not be lost.
/// </para>
/// </summary>
public class DistributedCacheStorage : CacheStorage
{
    private readonly IDistributedCache _distributedCache;
    private readonly DistributedCacheStorageOptions _options;
    private readonly SemaphoreSlim _saveLock = new(1, 1);

    /// <summary>
    /// Initializes an instance of <see cref="DistributedCacheStorage"/>.
    /// </summary>
    /// <param name="distributedCache"></param>
    /// <param name="options"></param>
    public DistributedCacheStorage(IDistributedCache distributedCache, IOptions<DistributedCacheStorageOptions> options)
    {
        _distributedCache = distributedCache;
        _options = options.Value;
        Collection = BuildCollection();
    }

    /// <inheritdoc />
    protected override ICacheStorageCollection Collection { get; }

    /// <inheritdoc />
    /// <remarks>
    /// The payment's state is refreshed from the cache first, because another instance may have completed it since
    /// this instance loaded the records (a verification takes seconds). That narrows the window to the time between
    /// reading and writing the cache entry, but it is not atomic across instances: a distributed cache has no
    /// compare-and-set. Use the Entity Framework Core storage where several servers verify payments.
    /// </remarks>
    public override async Task<bool> TryCompletePaymentAsync(long paymentId,
                                                             PaymentStatus status,
                                                             string? transactionCode,
                                                             string? bankReference,
                                                             CancellationToken cancellationToken = default)
    {
        var buffer = await _distributedCache.GetAsync(_options.CacheKey, cancellationToken).ConfigureAwait(false);

        var cached = buffer == null
            ? null
            : ObjectSerializer.DeserializeObject<CacheStorageCollection>(buffer).Payments.SingleOrDefault(payment => payment.Id == paymentId);

        if (cached != null)
        {
            lock (SyncRoot)
            {
                var record = Collection.Payments.SingleOrDefault(payment => payment.Id == paymentId);

                // Only take a state that is further along. The cache may not yet have a completion this instance
                // made a moment ago, and that must not be undone.
                if (record != null && ((cached.IsPaid && !record.IsPaid) || (cached.IsCompleted && !record.IsCompleted)))
                {
                    record.Status = cached.Status;
                    record.TransactionCode = cached.TransactionCode;
                    record.BankReference = cached.BankReference;
                }
            }
        }

        return await base.TryCompletePaymentAsync(paymentId, status, transactionCode, bankReference, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Saves are serialized so that an older snapshot can never reach the cache after a newer one.
        await _saveLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            byte[] data;

            lock (SyncRoot)
            {
                data = ObjectSerializer.SerializeObject(Collection);
            }

            await _distributedCache.SetAsync(_options.CacheKey, data, _options.CacheEntryOptions, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _saveLock.Release();
        }
    }

    private ICacheStorageCollection BuildCollection()
    {
        var buffer = _distributedCache.Get(_options.CacheKey);

        return buffer == null
            ? new CacheStorageCollection()
            : ObjectSerializer.DeserializeObject<CacheStorageCollection>(buffer);
    }
}
