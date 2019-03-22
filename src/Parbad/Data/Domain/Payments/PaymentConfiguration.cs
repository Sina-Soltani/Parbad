// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Parbad.Data.Domain.Payments
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("TbPayment", "parbad");

            builder.Property(entity => entity.Id)
                .HasColumnName("PaymentID")
                .ValueGeneratedOnAdd();
            builder.HasKey(entity => new { entity.Id, entity.TrackingNumber, entity.Token });

            builder.Property(entity => entity.TrackingNumber).IsRequired();

            builder.Property(entity => entity.Amount).IsRequired();

            builder.Property(entity => entity.IsCompleted).IsRequired();

            builder.Property(entity => entity.IsPaid).IsRequired();

            builder.Property(entity => entity.CreatedOn).IsRequired();

            builder
                .HasMany(entity => entity.Transactions)
                .WithOne(entity => entity.Payment)
                .HasForeignKey(entity => new { entity.PaymentId, TrackingNumber = entity.PaymentTrackingNumber, entity.PaymentToken })
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
