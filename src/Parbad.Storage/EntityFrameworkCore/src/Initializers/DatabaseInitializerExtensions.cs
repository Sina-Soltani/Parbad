// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Parbad.Storage.EntityFrameworkCore.Builder;
using Parbad.Storage.EntityFrameworkCore.Context;
using Parbad.Storage.EntityFrameworkCore.Internal;

namespace Parbad.Builder
{
    public static class DatabaseInitializerExtensions
    {
        /// <summary>
        /// Creates the Parbad database If it does not exist.
        /// </summary>
        /// <param name="builder"></param>
        public static IDatabaseInitializerBuilder CreateDatabase(this IDatabaseInitializerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddInitializer(context => context.Database.EnsureCreatedAsync());
        }

        /// <summary>
        /// Creates the Parbad database If it does not exist.
        /// Will migrate the database too.
        /// </summary>
        /// <param name="builder"></param>
        public static IDatabaseInitializerBuilder CreateAndMigrateDatabase(this IDatabaseInitializerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddInitializer(context => context.Database.MigrateAsync());
        }

        /// <summary>
        /// Deletes and creates the Parbad database.
        /// </summary>
        /// <param name="builder"></param>
        public static IDatabaseInitializerBuilder DeleteAndCreateDatabase(this IDatabaseInitializerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddInitializer(async context =>
            {
                await context.Database.EnsureDeletedAsync();

                await context.Database.EnsureCreatedAsync();
            });
        }

        /// <summary>
        /// Adds the specified Initializer to Parbad services.
        /// Initializers will be called in order that you specified.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureInitializer"></param>
        public static IDatabaseInitializerBuilder AddInitializer(
                this IDatabaseInitializerBuilder builder,
                Func<ParbadDataContext, Task> configureInitializer)
        {
            return builder.AddInitializer(provider =>
                new CustomDatabaseInitializer(configureInitializer),
                ServiceLifetime.Transient);
        }
    }
}
