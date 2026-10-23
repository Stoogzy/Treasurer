using Microsoft.EntityFrameworkCore;
using Treasurer.Application.Common.AppRequests;
using Treasurer.Application.Common.Interfaces;
using Treasurer.Core.Entities;
using Treasurer.Core.Enums;

namespace Treasurer.Application.Payments.ListAllPaymentsByEvent;

public sealed record ListAllPaymentsByEventQuery(string EventId)
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

    public sealed record Result(
        string EventId,
        string EventName,
        DateTime? EventStartsAt,
        DateTime? EventEndsAt,
        IReadOnlyList<PaymentItem> Payments,
        IReadOnlyList<AttendeeItem> Attendees,
        decimal TotalValue,
        AllocationSummary AllocationSummary);

    public sealed class Handler(IApplicationDbContext dbContext) : IRequestHandler<ListAllPaymentsByEventQuery, Result>
    {
        public async Task<AppResponse<Result>> Handle(ListAllPaymentsByEventQuery query, CancellationToken cancellationToken)
        {
            EventEntity? eventEntity = await dbContext.Events
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.BookwhenEventId == query.EventId,
                    cancellationToken);

            if (eventEntity is null)
            {
                return new AppResponse<Result>(statusCode: 404, message: $"Event '{query.EventId}' was not found.");
            }

            var paymentRows = await dbContext.BookingEvents
                .AsNoTracking()
                .Where(bookingEvent =>
                    bookingEvent.EventId == eventEntity.Id &&
                    bookingEvent.EventValue.HasValue &&
                    bookingEvent.EventValueSource !=
                        EventValueSource.MembershipIncluded &&
                    bookingEvent.EventValueSource !=
                        EventValueSource.InitialMembershipPayment &&
                    bookingEvent.Booking.Payments.Any())
                .Select(bookingEvent => new
                {
                    BookingEventId = bookingEvent.Id,
                    PaymentDate = bookingEvent.Booking.Payments
                        .OrderByDescending(payment =>
                            payment.PaymentDate)
                        .Select(payment => payment.PaymentDate)
                        .First(),
                    bookingEvent.Booking.BookingReference,
                    PaymentCustomerName = bookingEvent.Booking.Payments
                        .OrderByDescending(payment =>
                            payment.PaymentDate)
                        .Select(payment => payment.CustomerName)
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

            List<PaymentItem> payments = paymentRows
                .OrderByDescending(payment => payment.PaymentDate)
                .ThenBy(payment => payment.PaidFor)
                .Select(payment => new PaymentItem(
                    payment.BookingEventId,
                    payment.PaymentDate,
                    payment.BookingReference,
                    payment.PaymentCustomerName ??
                        payment.ContactName,
                    payment.PaidFor,
                    payment.IsGuardianPayment,
                    payment.Value,
                    payment.ValueSource))
                .ToList();

            List<AttendeeItem> attendees = await dbContext.BookingEvents
                .AsNoTracking()
                .Where(bookingEvent =>
                    bookingEvent.EventId == eventEntity.Id)
                .OrderBy(bookingEvent =>
                    bookingEvent.Attendee.AttendeeCustomerName)
                .ThenBy(bookingEvent =>
                    bookingEvent.Booking.BookingReference)
                .Select(bookingEvent => new AttendeeItem(
                    bookingEvent.Id,
                    bookingEvent.Attendee.AttendeeCustomerName,
                    bookingEvent.Booking.BookingReference,
                    bookingEvent.TicketName,
                    bookingEvent.TicketType,
                    bookingEvent.Attended,
                    bookingEvent.Booking.IsGuardianBooking))
                .ToListAsync(cancellationToken);

            List<Guid> unresolvedBookingIds = await dbContext.BookingEvents
                .AsNoTracking()
                .Where(bookingEvent =>
                    bookingEvent.EventId == eventEntity.Id &&
                    bookingEvent.EventValueSource ==
                        EventValueSource.Unresolved &&
                    bookingEvent.Booking.Payments.Any())
                .Select(bookingEvent => bookingEvent.BookingId)
                .ToListAsync(cancellationToken);

            AllocationSummary allocationSummary = new(
                IsComplete: unresolvedBookingIds.Count == 0,
                UnresolvedBookingCount:
                    unresolvedBookingIds.Distinct().Count(),
                UnresolvedEventCount: unresolvedBookingIds.Count);

            return AppResponse<Result>.Ok(
                new Result(
                    eventEntity.BookwhenEventId,
                    eventEntity.EventName,
                    eventEntity.StartsAt,
                    eventEntity.EndsAt,
                    payments,
                    attendees,
                    payments.Sum(x => x.Value),
                    allocationSummary));
        }
    }
}
