using System;
using System.Collections.Generic;

namespace Parbad.Infrastructure.Caching
{
    internal interface ICacheManager
    {
        IEnumerable<KeyValuePair<string, object>> GetAll();

        object Get(string key);

        void AddOrUpdate(string key, object value, TimeSpan cacheTime);

        bool DoesExists(string key);

        void Remove(string key);
    }
}