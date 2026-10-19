using Microsoft.EntityFrameworkCore;
using Treasurer.Application.Common.Interfaces;
using Treasurer.Core.Entities;

namespace Treasurer.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<EventEntity> Events => Set<EventEntity>();
    public DbSet<BookingEntity> Bookings => Set<BookingEntity>();
    public DbSet<BookingEventEntity> BookingEvents => Set<BookingEventEntity>();
    public DbSet<BookerEntity> Bookers => Set<BookerEntity>();
    public DbSet<AttendeeEntity> Attendees => Set<AttendeeEntity>();
    public DbSet<PaymentEntity> Payments => Set<PaymentEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
