using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Treasurer.Core.Entities;

namespace Treasurer.Infrastructure.Persistence.Configurations;

internal sealed class EventEntityConfiguration : IEntityTypeConfiguration<EventEntity>
{
    public void Configure(EntityTypeBuilder<EventEntity> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BookwhenEventId)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.BookwhenEventId)
            .IsUnique();

        builder.Property(x => x.EventName)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.EventCode)
            .HasMaxLength(100);

        builder.HasIndex(x => x.EventCode);

        builder.Property(x => x.Location)
            .HasMaxLength(500);

        builder.Property(x => x.StartsAt)
            .HasPrecision(0);

        builder.Property(x => x.EndsAt)
            .HasPrecision(0);

        builder.HasIndex(x => x.StartsAt);
    }
}
