// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
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
