using Microsoft.EntityFrameworkCore;
using Treasurer.Application.Common.AppRequests;
using Treasurer.Application.Common.Interfaces;
using Treasurer.Core.Entities;
using Treasurer.Core.Enums;

namespace Treasurer.Application.Payments.ListUnresolvedBookingPayments;

public sealed record ListUnresolvedBookingPaymentsQuery
{
    public sealed record UnresolvedEventItem(
        Guid BookingEventId,
        string EventId,
        string EventName,
        DateTime? StartsAt,
        string AttendeeName,
        string TicketName,
        string TicketType);

    public sealed record BookingItem(
        Guid BookingId,
        string BookingReference,
        decimal? BookingPayment,
        bool? IsGuardianBooking,
        string BookerName,
        string BookerEmail,
        IReadOnlyList<UnresolvedEventItem> Events);

    public sealed record Result(IReadOnlyList<BookingItem> Bookings, int UnresolvedEventCount);

    public sealed class Handler(IApplicationDbContext dbContext) : IRequestHandler<ListUnresolvedBookingPaymentsQuery, Result>
    {
        public async Task<AppResponse<Result>> Handle(ListUnresolvedBookingPaymentsQuery query, CancellationToken cancellationToken)
        {
            List<BookingEntity> bookings = await dbContext.Bookings
                .AsNoTracking()
                .Where(booking => booking.BookingEvents.Any(
                    bookingEvent =>
                        bookingEvent.EventValueSource ==
                        EventValueSource.Unresolved))
                .Include(booking => booking.Booker)
                .Include(booking => booking.BookingEvents.Where(
                    bookingEvent =>
                        bookingEvent.EventValueSource ==
                        EventValueSource.Unresolved))
                    .ThenInclude(bookingEvent => bookingEvent.Event)
                .Include(booking => booking.BookingEvents.Where(
                    bookingEvent =>
                        bookingEvent.EventValueSource ==
                        EventValueSource.Unresolved))
                    .ThenInclude(bookingEvent => bookingEvent.Attendee)
                .OrderBy(booking => booking.BookingReference)
                .ToListAsync(cancellationToken);

            List<BookingItem> results = bookings
                .Select(booking => new BookingItem(
                    booking.Id,
                    booking.BookingReference,
                    booking.BookingPayment,
                    booking.IsGuardianBooking,
                    booking.Booker.ContactName,
                    booking.Booker.BookerEmail,
                    booking.BookingEvents
                        .OrderBy(bookingEvent =>
                            bookingEvent.Event.StartsAt)
                        .ThenBy(bookingEvent =>
                            bookingEvent.Attendee.AttendeeCustomerName)
                        .Select(bookingEvent =>
                            new UnresolvedEventItem(
                                bookingEvent.Id,
                                bookingEvent.Event.BookwhenEventId,
                                bookingEvent.Event.EventName,
                                bookingEvent.Event.StartsAt,
                                bookingEvent.Attendee
                                    .AttendeeCustomerName,
                                bookingEvent.TicketName,
                                bookingEvent.TicketType))
                        .ToList()))
                .ToList();

            return AppResponse<Result>.Ok(new Result(results, results.Sum(booking => booking.Events.Count)));
        }
    }
}
