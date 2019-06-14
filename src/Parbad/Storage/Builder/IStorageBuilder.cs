// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Storage.Abstractions;

namespace Parbad.Storage.Builder
{
    public interface IStorageBuilder
    {
        IServiceCollection Services { get; }

        IStorageBuilder AddStorage<TStorage>(ServiceLifetime lifetime) where TStorage : class, IStorage;

        IStorageBuilder AddStorage(IStorage storage);

        IStorageBuilder AddStorage(Func<IServiceProvider, IStorage> factory, ServiceLifetime lifetime);
    }
}
