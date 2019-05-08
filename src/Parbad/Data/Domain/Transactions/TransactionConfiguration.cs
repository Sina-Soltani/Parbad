// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Parbad.Data.Domain.Transactions
{
    /// <summary>
    /// Transaction entity configuration.
    /// </summary>
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("transaction", "Parbad");

            builder
                .HasKey(entity => entity.Id)
                .HasName("transaction_id");
            builder.Property(entity => entity.Id)
                .HasColumnName("transaction_id")
                .ValueGeneratedOnAdd();

            builder.Property(entity => entity.Amount)
                .HasColumnName(nameof(Transaction.Amount).ToLower())
                .IsRequired(required: true);

            builder.Property(entity => entity.Type)
                .HasColumnName(nameof(Transaction.Type).ToLower())
                .IsRequired(required: true);

            builder.Property(entity => entity.IsSucceed)
                .HasColumnName("is_succeed")
                .IsRequired(required: true);

            builder.Property(entity => entity.Message)
                .HasColumnName(nameof(Transaction.Message).ToLower())
                .IsRequired(required: false);

            builder.Property(entity => entity.AdditionalData)
                .HasColumnName("additional_data")
                .IsRequired(required: false);

            builder.Property(entity => entity.CreatedOn)
                .HasColumnName("created_on")
                .IsRequired(required: true);

            builder.Property(entity => entity.UpdatedOn)
                .HasColumnName("updated_on")
                .IsRequired(required: false);
        }
    }
}
