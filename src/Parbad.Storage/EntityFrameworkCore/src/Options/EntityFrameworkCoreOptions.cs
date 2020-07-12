// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;
using Parbad.Storage.EntityFrameworkCore.Context;

namespace Parbad.Storage.EntityFrameworkCore.Options
{
    /// <summary>
    /// Contains the options for configuring the EntityFrameworkCore for Parbad storage.
    /// </summary>
    public class EntityFrameworkCoreOptions
    {
        /// <summary>
        /// Initializes an instance of <see cref="EntityFrameworkCoreOptions"/>.
        /// </summary>
        public EntityFrameworkCoreOptions()
        {
            DefaultSchema = "parbad";

            PaymentTableOptions = new TableOptions { Name = "payment" };

            TransactionTableOptions = new TableOptions { Name = "transaction" };
        }

        /// <summary>
        /// Configures the <see cref="ParbadDataContext"/>.
        /// </summary>
        public Action<DbContextOptionsBuilder> ConfigureDbContext { get; set; }

        /// <summary>
        /// Gets or sets the default schema for all tables. The default value is "parbad".
        /// </summary>
        public string DefaultSchema { get; set; }

        /// <summary>
        /// Contains the options for configuring the Payment table.
        /// </summary>
        public TableOptions PaymentTableOptions { get; set; }

        /// <summary>
        /// Contains the options for configuring the Transaction table.
        /// </summary>
        public TableOptions TransactionTableOptions { get; set; }
    }
}
