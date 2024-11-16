using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Parbad.Storage.Abstractions;

public static class StorageBuilderExtensions
{
    /// <summary>
    /// Adds an implementation of <see cref="IStorageManager"/> which required by Parbad for managing the storage operations.
    /// </summary>
    /// <typeparam name="TManager"></typeparam>
    /// <param name="builder"></param>
    /// <param name="lifetime">The lifetime of given StorageManager.</param>
    [Obsolete("StorageManager will be removed in a future release. The implementations are moved to the IStorage interface.")]
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
    [Obsolete("StorageManager will be removed in a future release. The implementations are moved to the IStorage interface.")]
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
    [Obsolete("StorageManager will be removed in a future release. The implementations are moved to the IStorage interface.")]
    public static IStorageBuilder AddStorageManager(this IStorageBuilder builder, Func<IServiceProvider, IStorageManager> factory, ServiceLifetime lifetime)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (factory == null) throw new ArgumentNullException(nameof(factory));

        builder.Services.AddOrUpdate(factory, lifetime);

        return builder;
    }
}
