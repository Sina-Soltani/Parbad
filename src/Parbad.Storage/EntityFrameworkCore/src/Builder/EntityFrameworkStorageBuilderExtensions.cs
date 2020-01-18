// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Storage.Builder;
using Parbad.Storage.EntityFrameworkCore;
using Parbad.Storage.EntityFrameworkCore.Builder;
using Parbad.Storage.EntityFrameworkCore.Context;
using Parbad.Storage.EntityFrameworkCore.Internal;

namespace Parbad.Builder
{
    public static class EntityFrameworkStorageBuilderExtensions
    {
        /// <summary>
        /// Uses the EntityFramework Core as a storage for saving and loading data.
        /// <para>
        /// Note: It means Parbad can save and load the data in different database providers
        /// such as SQL Server, MySql, Sqlite, PostgreSQL, Oracle, InMemory, etc.
        /// For more information see: https://docs.microsoft.com/en-us/ef/core/providers/.
        /// </para>
        /// <para>Note: This database is only for internal usages such as saving and loading payment information.
        /// You don't need to think about merging and using this database with your own database.
        /// The important payment information such as Tracking Number, Transaction Code, etc. will you get from the result of
        /// all payment requests.</para>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureDatabaseBuilder">Configure what database must be used and how it must be created.</param>
        public static IEntityFrameworkCoreStorageBuilder UseEntityFrameworkCore(this IStorageBuilder builder,
            Action<DbContextOptionsBuilder> configureDatabaseBuilder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configureDatabaseBuilder == null) throw new ArgumentNullException(nameof(configureDatabaseBuilder));

            builder.Services.AddDbContext<ParbadDataContext>(configureDatabaseBuilder);

            builder.AddStorage<EntityFrameworkCoreStorage>(ServiceLifetime.Transient);

            builder.AddStorageManager<EntityFrameworkCoreStorageManager>(ServiceLifetime.Transient);

            return new EntityFrameworkCoreStorageBuilder(builder.Services);
        }

        /// <summary>
        /// Uses the MigrationsAssembly method internally and passes the Parbad assembly name
        /// as parameter for configuring the migrations.
        /// </summary>
        /// <typeparam name="TBuilder"></typeparam>
        /// <typeparam name="TExtension"></typeparam>
        /// <param name="builder"></param>
        public static RelationalDbContextOptionsBuilder<TBuilder, TExtension> UseParbadMigrations<TBuilder, TExtension>(
            this RelationalDbContextOptionsBuilder<TBuilder, TExtension> builder)
            where TBuilder : RelationalDbContextOptionsBuilder<TBuilder, TExtension>
            where TExtension : RelationalOptionsExtension, new()
        {
            return builder.MigrationsAssembly(typeof(ParbadDataContext).Assembly.GetName().Name);
        }
    }
}
