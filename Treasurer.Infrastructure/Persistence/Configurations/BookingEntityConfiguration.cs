using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Treasurer.Core.Entities;

namespace Treasurer.Infrastructure.Persistence.Configurations;

internal sealed class BookingEntityConfiguration : IEntityTypeConfiguration<BookingEntity>
{
    public void Configure(EntityTypeBuilder<BookingEntity> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BookwhenBookingId)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.BookwhenBookingId)
            .IsUnique();

        builder.Property(x => x.BookingReference)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.BookingReference)
            .IsUnique();

        builder.Property(x => x.BookingPayment)
            .HasPrecision(18, 2);

        builder.Property(x => x.IsGuardianBooking);

        builder.HasIndex(x => x.BookerId);

        builder.HasOne(x => x.Booker)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.BookerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.BookingEvents)
            .WithOne(x => x.Booking)
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Payments)
            .WithOne(x => x.Booking)
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
