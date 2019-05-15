// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Parbad.Data.Domain.Payments
{
    /// <summary>
    /// Payment entity configuration.
    /// </summary>
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("payment", "Parbad");

            builder
                .HasKey(entity => entity.Id)
                .HasName("payment_id");
            builder.Property(entity => entity.Id)
                .HasColumnName("payment_id")
                .ValueGeneratedOnAdd();

            builder.Property(entity => entity.TrackingNumber)
                .HasColumnName("tracking_number")
                .IsRequired(required: true);
            builder.HasIndex(entity => entity.TrackingNumber).IsUnique(unique: true);

            builder.Property(entity => entity.Token)
                .HasColumnName(nameof(Payment.Token).ToLower())
                .IsRequired(required: true);
            builder.HasIndex(entity => entity.Token).IsUnique(unique: true);

            builder.Property(entity => entity.Amount)
                .HasColumnName(nameof(Payment.Amount).ToLower())
                .IsRequired(required: true);

            builder.Property(entity => entity.TransactionCode)
                .HasColumnName("transaction_code")
                .IsRequired(required: false);

            builder.Property(entity => entity.GatewayName)
                .HasColumnName("gateway_name")
                .HasMaxLength(20)
                .IsRequired(required: true);

            builder.Property(entity => entity.IsCompleted)
                .HasColumnName("is_completed")
                .IsRequired(required: true);

            builder.Property(entity => entity.IsPaid)
                .HasColumnName("is_paid")
                .IsRequired(required: true);

            builder.Property(entity => entity.CreatedOn)
                .HasColumnName("created_on")
                .IsRequired(required: true);

            builder.Property(entity => entity.UpdatedOn)
                .HasColumnName("updated_on")
                .IsRequired(required: false);

            builder
                .HasMany(entity => entity.Transactions)
                .WithOne(entity => entity.Payment)
                .HasForeignKey(entity => entity.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}