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
        public static IParbadBuilder ConfigureStorage(this IParbadBuilder builder, Action<IStorageBuilder> configureStorage)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configureStorage == null) throw new ArgumentNullException(nameof(configureStorage));

            configureStorage(new StorageBuilder(builder.Services).UseDefaultStorageManager());

            return builder;
        }

        public static IStorageBuilder UseDefaultStorageManager(this IStorageBuilder builder)
        {
            return AddStorageManager<StorageManager>(builder, ServiceLifetime.Transient);
        }

        public static IStorageBuilder AddStorageManager<TManager>(this IStorageBuilder builder, ServiceLifetime lifetime) where TManager : class, IStorageManager
        {
            builder.Services.AddOrUpdate<IStorageManager, TManager>(lifetime);

            return builder;
        }

        public static IStorageBuilder AddStorageManager(this IStorageBuilder builder, IStorageManager storageManager)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (storageManager == null) throw new ArgumentNullException(nameof(storageManager));

            builder.Services
                .RemoveAll<IStorageManager>()
                .AddSingleton(storageManager);

            return builder;
        }

        public static IStorageBuilder AddStorageManager(this IStorageBuilder builder, Func<IServiceProvider, IStorageManager> factory, ServiceLifetime lifetime)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            builder.Services.AddOrUpdate(factory, lifetime);

            return builder;
        }
    }
}
