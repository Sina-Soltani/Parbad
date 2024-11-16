// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parbad.Storage.EntityFrameworkCore.Domain;
using Parbad.Storage.EntityFrameworkCore.Options;

namespace Parbad.Storage.EntityFrameworkCore.Configuration;

/// <summary>
/// Transaction entity configuration.
/// </summary>
public class TransactionConfiguration : EntityTypeConfiguration<TransactionEntity>
{
    /// <summary>
    /// Initializes an instance of <see cref="TransactionConfiguration"/>.
    /// </summary>
    public TransactionConfiguration(EntityFrameworkCoreOptions efCoreOptions) : base(efCoreOptions)
    {
    }

    /// <inheritdoc />
    public override void Configure(EntityTypeBuilder<TransactionEntity> builder, EntityFrameworkCoreOptions efCoreOptions)
    {
        builder.ToTable(efCoreOptions.TransactionTableOptions, efCoreOptions.DefaultSchema);

        builder
           .HasKey(entity => entity.Id)
           .HasName("transaction_id");
        builder.Property(entity => entity.Id)
               .HasColumnName("transaction_id")
               .ValueGeneratedOnAdd();

        builder.Property(entity => entity.Amount)
               .HasColumnName(nameof(TransactionEntity.Amount).ToLower())
               .HasColumnType("decimal(18,2)")
               .IsRequired(required: true);

        builder.Property(entity => entity.Type)
               .HasColumnName(nameof(TransactionEntity.Type).ToLower())
               .IsRequired(required: true);

        builder.Property(entity => entity.IsSucceed)
               .HasColumnName("is_succeed")
               .IsRequired(required: true);

        builder.Property(entity => entity.Message)
               .HasColumnName(nameof(TransactionEntity.Message).ToLower())
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