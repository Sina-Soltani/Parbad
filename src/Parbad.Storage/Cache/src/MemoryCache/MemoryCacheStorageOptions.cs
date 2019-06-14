// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Caching.Memory;

namespace Parbad.Storage.Cache.MemoryCache
{
    /// <summary>
    /// Memory cache storage options.
    /// </summary>
    public class MemoryCacheStorageOptions
    {
        /// <summary>
        /// Gets or sets the key name which will be used by <see cref="IMemoryCache"/>.
        /// The default value is "parbad.storage.cache"
        /// </summary>
        public string CacheKey { get; set; } = "parbad.storage.cache";

        /// <summary>
        /// Represents the cache options applied to an entry of the <see cref="IMemoryCache"/> instance.
        /// </summary>
        public MemoryCacheEntryOptions CacheEntryOptions { get; set; } = new MemoryCacheEntryOptions
        {
            Priority = CacheItemPriority.NeverRemove
        };
    }
}
