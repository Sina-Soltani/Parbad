// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Internal
{
    /// <summary>
    /// An async lock per key. Entries exist only while someone holds or waits for the key, so the lock does not
    /// grow with the number of keys ever used.
    /// </summary>
    internal sealed class KeyedAsyncLock<TKey>
    {
        private readonly Dictionary<TKey, Entry> _entries = new Dictionary<TKey, Entry>();

        /// <summary>
        /// Waits until no one else holds <paramref name="key"/>. Dispose the result to release it.
        /// </summary>
        public async Task<IDisposable> AcquireAsync(TKey key, CancellationToken cancellationToken)
        {
            Entry entry;

            lock (_entries)
            {
                if (!_entries.TryGetValue(key, out entry))
                {
                    entry = new Entry();
                    _entries.Add(key, entry);
                }

                entry.References++;
            }

            try
            {
                await entry.Semaphore.WaitAsync(cancellationToken).ConfigureAwaitFalse();
            }
            catch
            {
                Release(key, entry, acquired: false);
                throw;
            }

            return new Releaser(this, key, entry);
        }

        private void Release(TKey key, Entry entry, bool acquired)
        {
            if (acquired) entry.Semaphore.Release();

            lock (_entries)
            {
                if (--entry.References == 0)
                {
                    _entries.Remove(key);
                    entry.Semaphore.Dispose();
                }
            }
        }

        private sealed class Entry
        {
            public readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

            public int References;
        }

        private sealed class Releaser : IDisposable
        {
            private readonly KeyedAsyncLock<TKey> _owner;
            private readonly TKey _key;
            private Entry _entry;

            public Releaser(KeyedAsyncLock<TKey> owner, TKey key, Entry entry)
            {
                _owner = owner;
                _key = key;
                _entry = entry;
            }

            public void Dispose()
            {
                var entry = Interlocked.Exchange(ref _entry, null);

                if (entry != null) _owner.Release(_key, entry, acquired: true);
            }
        }
    }
}
