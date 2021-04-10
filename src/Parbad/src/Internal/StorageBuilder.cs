// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Parbad.Storage.Abstractions;
using System;

namespace Parbad.Internal
{
    internal class StorageBuilder : IStorageBuilder
    {
        public StorageBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public IStorageBuilder AddStorage<TStorage>(ServiceLifetime lifetime) where TStorage : class, IStorage
        {
            Services.AddOrUpdate<IStorage, TStorage>(lifetime);

            return this;
        }

        public IStorageBuilder AddStorage(IStorage storage)
        {
            Services.TryAddSingleton(storage);

            return this;
        }

        public IStorageBuilder AddStorage(Func<IServiceProvider, IStorage> factory, ServiceLifetime lifetime)
        {
            Services.AddOrUpdate(factory, lifetime);

            return this;
        }
    }
}
