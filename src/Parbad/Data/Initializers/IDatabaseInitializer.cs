// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Parbad.Data.Context;

namespace Parbad.Data.Initializers
{
    /// <summary>
    /// An initializer to initialize the database required by Parbad.
    /// </summary>
    public interface IDatabaseInitializer
    {
        /// <summary>
        /// Initializes the database.
        /// </summary>
        /// <param name="context">Parbad database.</param>
        /// <param name="cancellationToken"></param>
        Task InitializeAsync(ParbadDataContext context, CancellationToken cancellationToken = default);
    }
}
