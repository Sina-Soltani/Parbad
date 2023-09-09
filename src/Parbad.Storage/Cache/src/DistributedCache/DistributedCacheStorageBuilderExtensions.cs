// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Storage.Abstractions;
using Parbad.Storage.Cache.DistributedCache;
using System;

namespace Parbad.Builder;

public static class DistributedCacheStorageBuilderExtensions
{
    /// <summary>
    /// Uses <see cref="IDistributedCache"/> for saving and loading data.
    /// </summary>
    /// <param name="builder"></param>
    public static IStorageBuilder UseDistributedCache(this IStorageBuilder builder)
        => UseDistributedCache(builder, options => { });

    /// <summary>
    /// Uses <see cref="IDistributedCache"/> for saving and loading data.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configureOptions"></param>
    public static IStorageBuilder UseDistributedCache(this IStorageBuilder builder, Action<DistributedCacheStorageOptions> configureOptions)
    {
        builder.Services.Configure(configureOptions);
        builder.AddStorage<DistributedCacheStorage>(ServiceLifetime.Transient);

        return builder;
    }
}
