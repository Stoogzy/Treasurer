using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Treasurer.Core.Entities;

namespace Treasurer.Infrastructure.Persistence.Configurations;

internal sealed class BookingEventEntityConfiguration : IEntityTypeConfiguration<BookingEventEntity>
{
    public void Configure(EntityTypeBuilder<BookingEventEntity> builder)
    {
        builder.ToTable("BookingEvents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BookwhenEntryId)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.BookwhenEntryId)
            .IsUnique();

        builder.Property(x => x.TicketName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.TicketType)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => x.BookingId);

        builder.HasIndex(x => x.EventId);

        builder.HasIndex(x => x.AttendeeId);

        builder.HasIndex(x => new
        {
            x.BookingId,
            x.EventId
        });

        builder.HasOne(x => x.Booking)
            .WithMany(x => x.BookingEvents)
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Event)
            .WithMany(x => x.BookingEvents)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Attendee)
            .WithMany(x => x.BookingEvents)
            .HasForeignKey(x => x.AttendeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
