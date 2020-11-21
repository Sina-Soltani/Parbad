// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace Parbad.Internal
{
    internal static class CollectionExtensions
    {
        public static bool TryGetValue(this NameValueCollection collection, string key, out StringValues value)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            value = collection[key];

            return collection.HasKey(key);
        }

        public static bool HasKey(this NameValueCollection collection, string key)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            return collection.AllKeys.Any(itemKey => string.Equals(itemKey, key, StringComparison.OrdinalIgnoreCase));
        }

        public static ICollection<StringValues> GetValues(this NameValueCollection collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            var result = new Collection<StringValues>();

            for (var index = 0; index < collection.Count; index++)
            {
                result.Add(collection[index]);
            }

            return result;
        }
    }
}
