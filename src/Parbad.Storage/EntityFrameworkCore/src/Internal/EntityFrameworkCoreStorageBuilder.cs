// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using System;
using Parbad.Internal;
using Parbad.Storage.EntityFrameworkCore.Builder;
using Parbad.Storage.EntityFrameworkCore.Context;
using Parbad.Storage.EntityFrameworkCore.Initializers;

namespace Parbad.Storage.EntityFrameworkCore.Internal
{
    internal class EntityFrameworkCoreStorageBuilder : IEntityFrameworkCoreStorageBuilder
    {
        public EntityFrameworkCoreStorageBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public IEntityFrameworkCoreStorageBuilder ConfigureDatabaseInitializer(Action<IDatabaseInitializerBuilder> configureInitializer)
        {
            if (configureInitializer == null) throw new ArgumentNullException(nameof(configureInitializer));

            configureInitializer(new DatabaseInitializerBuilder(Services));

            var services = Services.BuildServiceProvider();

            var database = services.GetRequiredService<ParbadDataContext>();
            var initializers = services.GetServices<IDatabaseInitializer>();

            foreach (var initializer in initializers)
            {
                initializer
                    .InitializeAsync(database)
                    .ConfigureAwaitFalse()
                    .GetAwaiter()
                    .GetResult();
            }

            return this;
        }
    }
}
