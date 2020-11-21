// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Parbad.Internal;
using Parbad.Storage;
using Parbad.Storage.Abstractions;
using Parbad.Storage.Builder;

namespace Parbad.Builder
{
    public static class StorageBuilderExtensions
    {
        /// <summary>
        /// Configures the storage which required by Parbad for saving and loading data.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureStorage"></param>
        public static IParbadBuilder ConfigureStorage(this IParbadBuilder builder, Action<IStorageBuilder> configureStorage)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configureStorage == null) throw new ArgumentNullException(nameof(configureStorage));

            configureStorage(new StorageBuilder(builder.Services).UseDefaultStorageManager());

            return builder;
        }

        /// <summary>
        /// Uses the default implementation of <see cref="IStorageManager"/>.
        /// </summary>
        /// <param name="builder"></param>
        public static IStorageBuilder UseDefaultStorageManager(this IStorageBuilder builder)
        {
            return AddStorageManager<StorageManager>(builder, ServiceLifetime.Transient);
        }

        /// <summary>
        /// Adds an implementation of <see cref="IStorageManager"/> which required by Parbad for managing the storage operations.
        /// </summary>
        /// <typeparam name="TManager"></typeparam>
        /// <param name="builder"></param>
        /// <param name="lifetime">The lifetime of given StorageManager.</param>
        public static IStorageBuilder AddStorageManager<TManager>(this IStorageBuilder builder, ServiceLifetime lifetime) where TManager : class, IStorageManager
        {
            builder.Services.AddOrUpdate<IStorageManager, TManager>(lifetime);

            return builder;
        }

        /// <summary>
        /// Adds an implementation of <see cref="IStorageManager"/> which required by Parbad for managing the storage operations.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="storageManager"></param>
        public static IStorageBuilder AddStorageManager(this IStorageBuilder builder, IStorageManager storageManager)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (storageManager == null) throw new ArgumentNullException(nameof(storageManager));

            builder.Services
                .RemoveAll<IStorageManager>()
                .AddSingleton(storageManager);

            return builder;
        }

        /// <summary>
        /// Adds an implementation of <see cref="IStorageManager"/> which required by Parbad for managing the storage operations.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="factory"></param>
        /// <param name="lifetime">The lifetime of given StorageManager.</param>
        public static IStorageBuilder AddStorageManager(this IStorageBuilder builder, Func<IServiceProvider, IStorageManager> factory, ServiceLifetime lifetime)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            builder.Services.AddOrUpdate(factory, lifetime);

            return builder;
        }
    }
}
