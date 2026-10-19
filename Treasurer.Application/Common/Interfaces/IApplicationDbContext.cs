using Microsoft.EntityFrameworkCore;
using Treasurer.Core.Entities;

namespace Treasurer.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<EventEntity> Events { get; }
    DbSet<BookingEntity> Bookings { get; }
    DbSet<BookingEventEntity> BookingEvents { get; }
    DbSet<BookerEntity> Bookers { get; }
    DbSet<AttendeeEntity> Attendees { get; }
    DbSet<PaymentEntity> Payments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
