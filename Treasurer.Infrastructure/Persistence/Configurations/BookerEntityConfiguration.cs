using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Treasurer.Core.Entities;

namespace Treasurer.Infrastructure.Persistence.Configurations;

internal sealed class BookerEntityConfiguration : IEntityTypeConfiguration<BookerEntity>
{
    public void Configure(EntityTypeBuilder<BookerEntity> builder)
    {
        builder.ToTable("Bookers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BookerEmail)
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(x => x.BookerEmail)
            .IsUnique();

        builder.Property(x => x.ContactName)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasMany(x => x.Bookings)
            .WithOne(x => x.Booker)
            .HasForeignKey(x => x.BookerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
