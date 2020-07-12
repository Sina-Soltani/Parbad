// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Storage.EntityFrameworkCore.Builder;
using Parbad.Storage.EntityFrameworkCore.Initializers;

namespace Parbad.Storage.EntityFrameworkCore.Internal
{
    [Obsolete("Database Initializers are not supported anymore and will be removed in a future version.")]
    internal class DatabaseInitializerBuilder : IDatabaseInitializerBuilder
    {
        public DatabaseInitializerBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public IDatabaseInitializerBuilder AddInitializer<TInitializer>(
            ServiceLifetime initializerLifetime = ServiceLifetime.Transient)
            where TInitializer : class, IDatabaseInitializer
        {
            Services.Add<IDatabaseInitializer, TInitializer>(initializerLifetime);

            return this;
        }

        public IDatabaseInitializerBuilder AddInitializer(IDatabaseInitializer initializer)
        {
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));

            Services.AddSingleton(initializer);

            return this;
        }

        public IDatabaseInitializerBuilder AddInitializer(
            Func<IServiceProvider, IDatabaseInitializer> implementationFactory,
            ServiceLifetime initializerLifetime = ServiceLifetime.Transient)
        {
            if (implementationFactory == null) throw new ArgumentNullException(nameof(implementationFactory));

            Services.Add(implementationFactory, initializerLifetime);

            return this;
        }
    }
}
