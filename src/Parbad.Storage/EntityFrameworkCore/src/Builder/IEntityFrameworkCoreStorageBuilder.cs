// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Parbad.Storage.EntityFrameworkCore.Builder
{
    [Obsolete("This is obsolete and will be removed in a future version.")]
    public interface IEntityFrameworkCoreStorageBuilder
    {
        /// <summary>
        /// Provides solution to perform payment request, verify the requested payment and
        /// refund a payment.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Configures Parbad database initializer.
        /// Initializer is useful for creating, deleting, migrating and seeding the Parbad database.
        /// <para>Note: Use ConfigureDatabase method before using this method.</para>
        /// <para>Note: Use one of predefined database creators depends on the database
        /// provider (In-Memory, SQL Server, MySQL, Sqlite, etc.) that you chose.</para>
        /// <para>Providers examples:</para>
        /// <para>For In-Memory, use the CreateDatabase method.</para>
        /// <para>For SQL Server, use the CreateAndMigrateDatabase method.</para>
        /// <para>For Sqlite, use DeleteAndCreateDatabase.
        /// Because Sqlite has some limitations with migrations.
        /// In this case, one way to fix this is to first delete the old Parbad database and then create it again.
        /// </para>
        /// <para>If you prefer to initialize the Parbad database yourself, then use the UseInitializer method
        /// and define how the database must be initialized.
        /// </para>
        /// <para>Note: Initializer will be called in order that you specified.</para>
        /// </summary>
        /// <param name="configureInitializer"></param>
        [Obsolete("Database Initializers are not supported anymore and will be removed in a future version.")]
        IEntityFrameworkCoreStorageBuilder ConfigureDatabaseInitializer(Action<IDatabaseInitializerBuilder> configureInitializer);
    }
}
