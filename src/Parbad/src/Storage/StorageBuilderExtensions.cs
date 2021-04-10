// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Parbad.Internal;
using Parbad.Storage;
using Parbad.Storage.Abstractions;
using System;

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

            var storageBuilder = new StorageBuilder(builder.Services);
            storageBuilder.UseDefaultStorageManager();

            configureStorage(storageBuilder);

            return builder;
        }

        /// <summary>
        /// Uses the default implementation of <see cref="IStorageManager"/>.
        /// </summary>
        /// <param name="builder"></param>
        public static IStorageBuilder UseDefaultStorageManager(this IStorageBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddStorageManager<StorageManager>(ServiceLifetime.Transient);
        }
    }
}
