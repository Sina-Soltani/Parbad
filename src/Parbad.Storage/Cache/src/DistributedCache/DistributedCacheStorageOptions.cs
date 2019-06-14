// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Caching.Distributed;

namespace Parbad.Storage.Cache.DistributedCache
{
    /// <summary>
    /// Distributed cache storage options.
    /// </summary>
    public class DistributedCacheStorageOptions
    {
        /// <summary>
        /// Gets or sets the key name which will be used by <see cref="IDistributedCache"/>.
        /// The default value is "parbad.storage.cache"
        /// </summary>
        public string CacheKey { get; set; } = "parbad.storage.cache";

        /// <summary>
        /// Provides the cache options for an entry in <see cref="IDistributedCache"/>.
        /// </summary>
        public DistributedCacheEntryOptions CacheEntryOptions { get; set; } = new DistributedCacheEntryOptions();
    }
}
