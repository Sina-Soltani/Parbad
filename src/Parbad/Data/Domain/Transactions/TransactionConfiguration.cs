// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Parbad.Data.Domain.Transactions
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("TbTransaction", "parbad");

            builder.Property(entity => entity.Id).ValueGeneratedOnAdd();
            builder
                .HasKey(entity => entity.Id)
                .HasName("TransactionID");

            builder.Property(entity => entity.Amount).IsRequired();

            builder.Property(entity => entity.Type).IsRequired();

            builder.Property(entity => entity.IsSucceed).IsRequired();

            builder.Property(entity => entity.CreatedOn).IsRequired();
        }
    }
}
