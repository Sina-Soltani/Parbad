// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Parbad.Storage.EntityFrameworkCore.Domain.Payments;
using Parbad.Storage.EntityFrameworkCore.Domain.Transactions;

namespace Parbad.Storage.EntityFrameworkCore.Context
{
    public class ParbadDataContext : DbContext
    {
        public ParbadDataContext(DbContextOptions<ParbadDataContext> options) : base(options)
        {
        }

        public DbSet<PaymentEntity> Payments { get; set; }

        public DbSet<TransactionEntity> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .ApplyConfiguration(new PaymentConfiguration())
                .ApplyConfiguration(new TransactionConfiguration());
        }
    }
}
