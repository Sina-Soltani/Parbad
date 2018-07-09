using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using Parbad.Utilities;

namespace Parbad.Infrastructure.Caching
{
    internal class MemoryCacheManager : ICacheManager
    {
        private static MemoryCache Cache => MemoryCache.Default;
        private static readonly object LockObject = new object();

        public IEnumerable<KeyValuePair<string, object>> GetAll()
        {
            return Cache;
        }

        public virtual object Get(string key)
        {
            if (key.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(key));
            }

            lock (LockObject)
            {
                return Cache.Get(key);
            }
        }

        public virtual void AddOrUpdate(string key, object value, TimeSpan cacheTime)
        {
            if (key.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (cacheTime.TotalMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cacheTime), "Cache time must be a positive number");
            }

            lock (LockObject)
            {
                var cacheItemPolicy = new CacheItemPolicy
                {
                    Priority = CacheItemPriority.Default,
                    AbsoluteExpiration = DateTimeOffset.UtcNow.Add(cacheTime)
                };

                if (!Cache.Contains(key))
                {
                    Cache.Add(key, value, cacheItemPolicy);
                }
                else
                {
                    Cache.Set(key, value, cacheItemPolicy);
                }
            }
        }

        public virtual bool DoesExists(string key)
        {
            if (key.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(key));
            }

            lock (LockObject)
            {
                return Cache.Contains(key);
            }
        }

        public virtual void Remove(string key)
        {
            if (key.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(key));
            }

            lock (LockObject)
            {
                Cache.Remove(key);
            }
        }
    }
}