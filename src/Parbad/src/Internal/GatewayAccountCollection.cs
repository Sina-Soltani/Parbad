// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Parbad.Abstraction;
using Parbad.Exceptions;
using Parbad.GatewayBuilders;

namespace Parbad.Internal
{
    public class GatewayAccountCollection<TAccount> : IGatewayAccountCollection<TAccount>
        where TAccount : GatewayAccount
    {
        private readonly ICollection<TAccount> _accounts;

        public GatewayAccountCollection()
        {
            _accounts = new List<TAccount>();
        }

        public virtual TAccount Get(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return _accounts.SingleOrDefault(item => item.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public virtual TAccount GetDefaultAccount() => _accounts.FirstOrDefault();

        public virtual IEnumerator<TAccount> GetEnumerator() => _accounts.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public virtual void Add(TAccount item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            if (_accounts.Any(account => account.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new DuplicateAccountException(item);
            }

            _accounts.Add(item);
        }

        public virtual void Clear() => _accounts.Clear();

        public virtual bool Contains(TAccount item) => _accounts.Contains(item);

        public virtual void CopyTo(TAccount[] array, int arrayIndex) => _accounts.CopyTo(array, arrayIndex);

        public virtual bool Remove(TAccount item) => _accounts.Remove(item);

        public virtual int Count => _accounts.Count;

        public virtual bool IsReadOnly => _accounts.IsReadOnly;
    }
}
