// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Parbad.Storage.EntityFrameworkCore.Context;
using Parbad.Storage.EntityFrameworkCore.Initializers;

namespace Parbad.Storage.EntityFrameworkCore.Internal
{
    public class CustomDatabaseInitializer : IDatabaseInitializer
    {
        private readonly Func<ParbadDataContext, Task> _configureInitializer;

        public CustomDatabaseInitializer(Func<ParbadDataContext, Task> configureInitializer)
        {
            _configureInitializer = configureInitializer;
        }

        public Task InitializeAsync(ParbadDataContext context, CancellationToken cancellationToken = default)
        {
            return _configureInitializer(context);
        }
    }
}
