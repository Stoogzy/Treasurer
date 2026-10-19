using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Treasurer.Core.Entities;

namespace Treasurer.Infrastructure.Persistence.Configurations;

internal sealed class AttendeeEntityConfiguration : IEntityTypeConfiguration<AttendeeEntity>
{
    public void Configure(EntityTypeBuilder<AttendeeEntity> builder)
    {
        builder.ToTable("Attendees");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AttendeeCustomerId)
            .HasMaxLength(100);

        builder.HasIndex(x => x.AttendeeCustomerId)
            .IsUnique()
            .HasFilter("[AttendeeCustomerId] IS NOT NULL");

        builder.Property(x => x.AttendeeCustomerName)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasMany(x => x.BookingEvents)
            .WithOne(x => x.Attendee)
            .HasForeignKey(x => x.AttendeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
