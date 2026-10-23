using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Treasurer.Core.Entities;

namespace Treasurer.Infrastructure.Persistence.Configurations;

internal sealed class PaymentEntityConfiguration : IEntityTypeConfiguration<PaymentEntity>
{
    public void Configure(EntityTypeBuilder<PaymentEntity> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PaymentDate)
            .HasPrecision(0)
            .IsRequired();

        builder.HasIndex(x => x.PaymentDate);

        builder.Property(x => x.BookingReference)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.BookingReference);

        builder.Property(x => x.PaymentStatus)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PaymentMethod)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PaymentValue)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.PaymentCategory)
            .IsRequired();

        builder.HasIndex(x => x.PaymentCategory);

        builder.Property(x => x.CustomerName)
            .HasMaxLength(200);

        builder.Property(x => x.PayerEmail)
            .HasMaxLength(320);

        builder.HasIndex(x => x.BookingId);

        builder.HasOne(x => x.Booking)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
