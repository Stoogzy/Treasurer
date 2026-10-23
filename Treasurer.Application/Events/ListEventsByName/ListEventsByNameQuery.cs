using Microsoft.EntityFrameworkCore;
using Treasurer.Application.Common.AppRequests;
using Treasurer.Application.Common.Interfaces;
using Treasurer.Core.Entities;
using Treasurer.Core.Enums;

namespace Treasurer.Application.Events.ListEventsByName;

public sealed record ListEventsByNameQuery(
    string EventName,
    DateOnly StartDate,
    DateOnly EndDate)
{
    public sealed record PaymentItem(
        Guid BookingEventId,
        DateTime PaymentDate,
        string BookingReference,
        string PaidBy,
        string PaidFor,
        bool? IsGuardianPayment,
        decimal Value,
        EventValueSource ValueSource);

    public sealed record AttendeeItem(
        Guid BookingEventId,
        string AttendeeName,
        string BookingReference,
        string TicketName,
        string TicketType,
        bool? Attended,
        bool? IsGuardianBooking);

    public sealed record AllocationSummary(
        bool IsComplete,
        int UnresolvedBookingCount,
        int UnresolvedEventCount);

    public sealed record OccurrenceItem(
        string EventId,
        DateTime StartsAt,
        DateTime? EndsAt,
        string? Location,
        IReadOnlyList<PaymentItem> Payments,
        IReadOnlyList<AttendeeItem> Attendees,
        decimal TotalValue,
        AllocationSummary AllocationSummary);

    public sealed record Result(
        string EventName,
        DateOnly StartDate,
        DateOnly EndDate,
        IReadOnlyList<OccurrenceItem> Occurrences,
        decimal TotalValue);

    public sealed class Handler(IApplicationDbContext dbContext)
        : IRequestHandler<ListEventsByNameQuery, Result>
    {
        public async Task<AppResponse<Result>> Handle(
            ListEventsByNameQuery query,
            CancellationToken cancellationToken)
        {
            DateTime startDate =
                query.StartDate.ToDateTime(TimeOnly.MinValue);
            DateTime endDateExclusive = query.EndDate
                .AddDays(1)
                .ToDateTime(TimeOnly.MinValue);

            List<EventEntity> events = await dbContext.Events
                .AsNoTracking()
                .Where(eventEntity =>
                    eventEntity.EventName == query.EventName &&
                    eventEntity.StartsAt.HasValue &&
                    eventEntity.StartsAt >= startDate &&
                    eventEntity.StartsAt < endDateExclusive)
                .OrderBy(eventEntity => eventEntity.StartsAt)
                .ToListAsync(cancellationToken);

            if (events.Count == 0)
            {
                return new AppResponse<Result>(
                    statusCode: 404,
                    message:
                        $"No '{query.EventName}' events were found in the selected period.");
            }

            List<Guid> eventIds = events
                .Select(eventEntity => eventEntity.Id)
                .ToList();

            var paymentRows = await dbContext.BookingEvents
                .AsNoTracking()
                .Where(bookingEvent =>
                    eventIds.Contains(bookingEvent.EventId) &&
                    bookingEvent.EventValue.HasValue &&
                    bookingEvent.EventValueSource !=
                        EventValueSource.MembershipIncluded &&
                    bookingEvent.EventValueSource !=
                        EventValueSource.InitialMembershipPayment &&
                    bookingEvent.Booking.Payments.Any())
                .Select(bookingEvent => new
                {
                    bookingEvent.EventId,
                    BookingEventId = bookingEvent.Id,
                    PaymentDate = bookingEvent.Booking.Payments
                        .OrderByDescending(payment =>
                            payment.PaymentDate)
                        .Select(payment => payment.PaymentDate)
                        .First(),
                    bookingEvent.Booking.BookingReference,
                    PaymentCustomerName =
                        bookingEvent.Booking.Payments
                            .OrderByDescending(payment =>
                                payment.PaymentDate)
                            .Select(payment =>
                                payment.CustomerName)
                            .FirstOrDefault(),
                    bookingEvent.Booking.Booker.ContactName,
                    PaidFor =
                        bookingEvent.Attendee.AttendeeCustomerName,
                    IsGuardianPayment =
                        bookingEvent.Booking.IsGuardianBooking,
                    Value = bookingEvent.EventValue!.Value,
                    ValueSource = bookingEvent.EventValueSource
                })
                .ToListAsync(cancellationToken);

            var attendeeRows = await dbContext.BookingEvents
                .AsNoTracking()
                .Where(bookingEvent =>
                    eventIds.Contains(bookingEvent.EventId))
                .Select(bookingEvent => new
                {
                    bookingEvent.EventId,
                    BookingEventId = bookingEvent.Id,
                    AttendeeName =
                        bookingEvent.Attendee.AttendeeCustomerName,
                    bookingEvent.Booking.BookingReference,
                    bookingEvent.TicketName,
                    bookingEvent.TicketType,
                    bookingEvent.Attended,
                    IsGuardianBooking =
                        bookingEvent.Booking.IsGuardianBooking
                })
                .ToListAsync(cancellationToken);

            var unresolvedRows = await dbContext.BookingEvents
                .AsNoTracking()
                .Where(bookingEvent =>
                    eventIds.Contains(bookingEvent.EventId) &&
                    bookingEvent.EventValueSource ==
                        EventValueSource.Unresolved &&
                    bookingEvent.Booking.Payments.Any())
                .Select(bookingEvent => new
                {
                    bookingEvent.EventId,
                    bookingEvent.BookingId
                })
                .ToListAsync(cancellationToken);

            List<OccurrenceItem> occurrences = events
                .Select(eventEntity =>
                {
                    List<PaymentItem> payments = paymentRows
                        .Where(row =>
                            row.EventId == eventEntity.Id)
                        .OrderByDescending(row =>
                            row.PaymentDate)
                        .ThenBy(row => row.PaidFor)
                        .Select(row => new PaymentItem(
                            row.BookingEventId,
                            row.PaymentDate,
                            row.BookingReference,
                            row.PaymentCustomerName ??
                                row.ContactName,
                            row.PaidFor,
                            row.IsGuardianPayment,
                            row.Value,
                            row.ValueSource))
                        .ToList();

                    List<AttendeeItem> attendees = attendeeRows
                        .Where(row =>
                            row.EventId == eventEntity.Id)
                        .OrderBy(row => row.AttendeeName)
                        .ThenBy(row => row.BookingReference)
                        .Select(row => new AttendeeItem(
                            row.BookingEventId,
                            row.AttendeeName,
                            row.BookingReference,
                            row.TicketName,
                            row.TicketType,
                            row.Attended,
                            row.IsGuardianBooking))
                        .ToList();

                    List<Guid> unresolvedBookingIds =
                        unresolvedRows
                            .Where(row =>
                                row.EventId == eventEntity.Id)
                            .Select(row => row.BookingId)
                            .ToList();

                    return new OccurrenceItem(
                        eventEntity.BookwhenEventId,
                        eventEntity.StartsAt!.Value,
                        eventEntity.EndsAt,
                        eventEntity.Location,
                        payments,
                        attendees,
                        payments.Sum(payment => payment.Value),
                        new AllocationSummary(
                            unresolvedBookingIds.Count == 0,
                            unresolvedBookingIds
                                .Distinct()
                                .Count(),
                            unresolvedBookingIds.Count));
                })
                .ToList();

            return AppResponse<Result>.Ok(
                new Result(
                    query.EventName,
                    query.StartDate,
                    query.EndDate,
                    occurrences,
                    occurrences.Sum(occurrence =>
                        occurrence.TotalValue)));
        }
    }
}
