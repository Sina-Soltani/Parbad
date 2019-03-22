// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Parbad.Data.Domain.Payments;
using Parbad.Data.Domain.Transactions;

namespace Parbad.Data.Context
{
    public class ParbadDataContext : DbContext
    {
        public ParbadDataContext(DbContextOptions<ParbadDataContext> options) : base(options)
        {
        }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .ApplyConfiguration(new PaymentConfiguration())
                .ApplyConfiguration(new TransactionConfiguration());
        }
    }
}
