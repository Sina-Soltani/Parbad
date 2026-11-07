// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Storage.Abstractions;
using Parbad.Storage.EntityFrameworkCore.Context;
using Parbad.Storage.EntityFrameworkCore.Internal;
using Parbad.Storage.EntityFrameworkCore.Options;
using System;

namespace Parbad.Storage.EntityFrameworkCore.Builder;

public static class EntityFrameworkStorageBuilderExtensions
{
    /// <summary>
    /// Stores Parbad's data with EF Core in the application's own <typeparamref name="TContext"/>, which must contain
    /// Parbad's entities (<c>modelBuilder.UseParbad()</c> in its <c>OnModelCreating</c>). Parbad's tables are then part
    /// of the application's database and of its migrations.
    /// <para>
    /// Register the context with <c>AddDbContextFactory&lt;TContext&gt;</c> (or <c>AddPooledDbContextFactory</c>).
    /// Parbad creates a short-lived context from the factory for every operation, so it never saves changes that the
    /// application has pending on its own context instance. <c>AddDbContextFactory</c> also registers
    /// <typeparamref name="TContext"/> itself, so the application keeps injecting its context as before.
    /// </para>
    /// </summary>
    /// <typeparam name="TContext">The application's context.</typeparam>
    public static IStorageBuilder UseEfCore<TContext>(this IStorageBuilder builder) where TContext : DbContext
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        builder.AddStorage(services =>
                           {
                               var factory = services.GetService<IDbContextFactory<TContext>>() ??
                                             throw new InvalidOperationException($"Parbad's EF Core storage needs IDbContextFactory<{typeof(TContext).Name}>. " +
                                                                                 $"Register the context with services.AddDbContextFactory<{typeof(TContext).Name}>(...) " +
                                                                                 "or AddPooledDbContextFactory.");

                               return new EntityFrameworkCoreStorage<TContext>(factory);
                           },
                           ServiceLifetime.Transient);

        return builder;
    }

    /// <summary>
    /// Stores Parbad's data with EF Core in Parbad's own <see cref="ParbadDataContext"/>, i.e. in a database of its own
    /// with its own migrations. To keep Parbad's tables in the application's database instead, use
    /// <see cref="UseEfCore{TContext}"/>.
    /// <para>
    /// EF Core supports many database providers, such as SQL Server, PostgreSQL, MySQL, SQLite and Oracle; see
    /// https://learn.microsoft.com/ef/core/providers/.
    /// </para>
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configureEfCoreOptions">Configures the database of <see cref="ParbadDataContext"/> and its tables.</param>
    public static IStorageBuilder UseEfCore(this IStorageBuilder builder, Action<EntityFrameworkCoreOptions> configureEfCoreOptions)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (configureEfCoreOptions == null) throw new ArgumentNullException(nameof(configureEfCoreOptions));

        var options = new EntityFrameworkCoreOptions();
        configureEfCoreOptions(options);

        builder.Services.AddDbContextFactory<ParbadDataContext>(dbBuilder =>
        {
            options.ConfigureDbContext?.Invoke(dbBuilder);

            ((IDbContextOptionsBuilderInfrastructure)dbBuilder).AddOrUpdateExtension(new ParbadModelOptionsExtension(options));
        });

        return builder.UseEfCore<ParbadDataContext>();
    }
}
