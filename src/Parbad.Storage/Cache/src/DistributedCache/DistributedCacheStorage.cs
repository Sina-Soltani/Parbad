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
/// Distributed cache implementation of Parbad storage.
/// </summary>
public class DistributedCacheStorage : CacheStorage
{
    private readonly IDistributedCache _distributedCache;
    private readonly DistributedCacheStorageOptions _options;

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
    protected override Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var data = ObjectSerializer.SerializeObject(Collection);

        return _distributedCache.SetAsync(_options.CacheKey, data, _options.CacheEntryOptions, cancellationToken);
    }

    private ICacheStorageCollection BuildCollection()
    {
        var buffer = _distributedCache.Get(_options.CacheKey);

        return buffer == null
            ? new CacheStorageCollection()
            : ObjectSerializer.DeserializeObject<CacheStorageCollection>(buffer);
    }
}
