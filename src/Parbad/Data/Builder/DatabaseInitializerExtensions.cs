// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Internal;
using System;
using System.Threading.Tasks;
using Parbad.Data.Initializers;
using Parbad.Data.Context;

namespace Parbad.Builder
{
    public static class DatabaseInitializerExtensions
    {
        /// <summary>
        /// Adds the specified Initializer to Parbad services.
        /// Initializers will be called in order that you specified.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureInitializer"></param>
        public static IDatabaseInitializerBuilder UseInitializer(
            this IDatabaseInitializerBuilder builder,
            Func<ParbadDataContext, Task> configureInitializer)
        {
            return builder.AddInitializer(provider =>
                new CustomDatabaseInitializer(configureInitializer),
                ServiceLifetime.Transient);
        }

        /// <summary>
        /// Creates the Parbad database If it does not exist.
        /// </summary>
        /// <param name="builder"></param>
        public static IDatabaseInitializerBuilder CreateDatabase(this IDatabaseInitializerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.UseInitializer(context => context.Database.EnsureCreatedAsync());
        }

        /// <summary>
        /// Creates the Parbad database If it does not exist.
        /// Will migrate the database too.
        /// </summary>
        /// <param name="builder"></param>
        public static IDatabaseInitializerBuilder CreateAndMigrateDatabase(this IDatabaseInitializerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.UseInitializer(context => context.Database.MigrateAsync());
        }

        /// <summary>
        /// Deletes and creates the Parbad database.
        /// </summary>
        /// <param name="builder"></param>
        public static IDatabaseInitializerBuilder DeleteAndCreateDatabase(this IDatabaseInitializerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.UseInitializer(async context =>
            {
                await context.Database.EnsureDeletedAsync();

                await context.Database.EnsureCreatedAsync();
            });
        }
    }
}
