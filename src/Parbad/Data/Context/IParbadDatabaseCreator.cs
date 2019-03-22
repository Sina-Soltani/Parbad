// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Data.Context
{
    public interface IParbadDatabaseCreator
    {
        void Create();

        Task CreateAsync(CancellationToken cancellationToken = default);
    }
}
