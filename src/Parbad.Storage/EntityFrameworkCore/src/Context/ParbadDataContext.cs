// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Parbad.Storage.EntityFrameworkCore.Configuration;
using Parbad.Storage.EntityFrameworkCore.Domain;
using Parbad.Storage.EntityFrameworkCore.Options;

namespace Parbad.Storage.EntityFrameworkCore.Context
{
    public class ParbadDataContext : DbContext
    {
        public ParbadDataContext(DbContextOptions<ParbadDataContext> options, IOptions<EntityFrameworkCoreOptions> efCoreOptions) : base(options)
        {
            EntityFrameworkCoreOptions = efCoreOptions.Value;
        }

        public EntityFrameworkCoreOptions EntityFrameworkCoreOptions { get; }

        public DbSet<PaymentEntity> Payments { get; set; }

        public DbSet<TransactionEntity> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .ApplyConfiguration(new PaymentConfiguration(EntityFrameworkCoreOptions))
                .ApplyConfiguration(new TransactionConfiguration(EntityFrameworkCoreOptions));
        }
    }
}
