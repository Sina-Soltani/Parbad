// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Data.Context;

namespace Parbad.Builder
{
    public static class StorageServiceBuilderExtensions
    {
        /// <summary>
        /// Configures the storage required by Parbad to save and load the data.
        /// Note: It uses Microsoft.EntityFrameworkCore as storage, which means you can save your data
        /// in different database providers such as SQL Server, MySql, Sqlite, PostgreSQL, Oracle, InMemory, etc.
        /// <para>For more information see: https://docs.microsoft.com/en-us/ef/core/providers/.</para>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="storageBuilder"></param>
        public static IParbadBuilder ConfigureStorage(this IParbadBuilder builder, Action<DbContextOptionsBuilder> storageBuilder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (storageBuilder == null) throw new ArgumentNullException(nameof(storageBuilder));

            builder.Services.AddDbContext<ParbadDataContext>(storageBuilder);

            var serviceProvider = builder.Services.BuildServiceProvider();

            var databaseCreator = serviceProvider.GetRequiredService<IParbadDatabaseCreator>();

            databaseCreator.Create();

            return builder;
        }

        /// <summary>
        /// Configures the context to connect to a Microsoft SQL Server database.
        /// It also creates and migrates the database.
        /// </summary>
        /// <param name="builder">The builder being used to configure the context.</param>
        /// <param name="connectionString">The connection string of the database to connect to.</param>
        public static DbContextOptionsBuilder UseParbadSqlServer(this DbContextOptionsBuilder builder,
            string connectionString)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            builder.UseSqlServer(connectionString, optionsBuilder => optionsBuilder.MigrationsAssembly("Parbad"));

            return builder;
        }
    }
}
