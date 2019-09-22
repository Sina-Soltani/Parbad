// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Parbad.Abstraction;

namespace Parbad.GatewayBuilders
{
    /// <summary>
    /// A source which used to add the accounts of the specified gateway.
    /// </summary>
    /// <typeparam name="TAccount"></typeparam>
    public interface IGatewayAccountSource<TAccount> where TAccount : GatewayAccount
    {
        /// <summary>
        /// Adds the accounts for specified gateway.
        /// </summary>
        /// <param name="accounts"></param>
        Task AddAccountsAsync(IGatewayAccountCollection<TAccount> accounts);
    }
}
