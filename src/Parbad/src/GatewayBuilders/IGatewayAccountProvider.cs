// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Parbad.Abstraction;

namespace Parbad.GatewayBuilders
{
    public interface IGatewayAccountProvider<TAccount> where TAccount : GatewayAccount
    {
        Task<IGatewayAccountCollection<TAccount>> LoadAccountsAsync();
    }
}
