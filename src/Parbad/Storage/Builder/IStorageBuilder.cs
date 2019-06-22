// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Storage.Abstractions;

namespace Parbad.Storage.Builder
{
    /// <summary>
    /// A builder for building the Parbad storage.
    /// </summary>
    public interface IStorageBuilder
    {
        /// <summary>
        /// Specifies the contract for a collection of service descriptors.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Adds the given storage to Parbad services.
        /// </summary>
        /// <typeparam name="TStorage"></typeparam>
        /// <param name="lifetime">Lifetime of the given storage.</param>
        IStorageBuilder AddStorage<TStorage>(ServiceLifetime lifetime) where TStorage : class, IStorage;

        /// <summary>
        /// Adds the given storage to Parbad services as singleton.
        /// </summary>
        /// <param name="storage"></param>
        IStorageBuilder AddStorage(IStorage storage);

        /// <summary>
        /// Adds the given storage to Parbad services.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="lifetime">Lifetime of the given storage.</param>
        IStorageBuilder AddStorage(Func<IServiceProvider, IStorage> factory, ServiceLifetime lifetime);
    }
}
