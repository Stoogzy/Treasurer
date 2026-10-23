using Microsoft.EntityFrameworkCore;
using Treasurer.Application.Common.AppRequests;
using Treasurer.Application.Common.Interfaces;
using Treasurer.Core.Entities;
using Treasurer.Core.Enums;

namespace Treasurer.Application.Payments.ListUnresolvedBookingPayments;

public sealed record ListUnresolvedBookingPaymentsQuery(
    DateOnly StartDate,
    DateOnly EndDate)
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
        decimal AllocatedValue,
        decimal? RemainingUnallocatedValue,
        bool? IsGuardianBooking,
        string BookerName,
        string BookerEmail,
        IReadOnlyList<UnresolvedEventItem> Events);

    public sealed record UnallocatedPaymentItem(
        Guid PaymentId,
        DateTime PaymentDate,
        string BookingReference,
        string PaidBy,
        decimal PaymentValue,
        decimal AllocatedValue,
        decimal RemainingUnallocatedValue,
        string Reason);

    public sealed record Result(
        DateOnly StartDate,
        DateOnly EndDate,
        IReadOnlyList<BookingItem> Bookings,
        IReadOnlyList<UnallocatedPaymentItem> UnallocatedPayments,
        int UnresolvedEventCount,
        decimal TotalRemainingUnallocatedValue);

    public sealed class Handler(IApplicationDbContext dbContext) : IRequestHandler<ListUnresolvedBookingPaymentsQuery, Result>
    {
        public async Task<AppResponse<Result>> Handle(ListUnresolvedBookingPaymentsQuery query, CancellationToken cancellationToken)
        {
            DateTime startDate =
                query.StartDate.ToDateTime(TimeOnly.MinValue);
            DateTime endDateExclusive = query.EndDate
                .AddDays(1)
                .ToDateTime(TimeOnly.MinValue);

            List<BookingEntity> bookings = await dbContext.Bookings
                .AsNoTracking()
                .Where(booking =>
                    booking.BookingEvents.Any(bookingEvent =>
                        bookingEvent.EventValueSource ==
                        EventValueSource.Unresolved) &&
                    booking.Payments.Any(payment =>
                        payment.PaymentDate >= startDate &&
                        payment.PaymentDate < endDateExclusive))
                .Include(booking => booking.Booker)
                .Include(booking => booking.BookingEvents)
                    .ThenInclude(bookingEvent => bookingEvent.Event)
                .Include(booking => booking.BookingEvents)
                    .ThenInclude(bookingEvent => bookingEvent.Attendee)
                .OrderBy(booking => booking.BookingReference)
                .ToListAsync(cancellationToken);

            List<BookingItem> results = bookings
                .Select(booking =>
                {
                    decimal allocatedValue = booking.BookingEvents
                        .Where(bookingEvent =>
                            bookingEvent.EventValue.HasValue)
                        .Sum(bookingEvent =>
                            bookingEvent.EventValue!.Value);

                    decimal? remainingUnallocatedValue =
                        booking.BookingPayment - allocatedValue;

                    return new BookingItem(
                        booking.Id,
                        booking.BookingReference,
                        booking.BookingPayment,
                        allocatedValue,
                        remainingUnallocatedValue,
                        booking.IsGuardianBooking,
                        booking.Booker.ContactName,
                        booking.Booker.BookerEmail,
                        booking.BookingEvents
                            .Where(bookingEvent =>
                                bookingEvent.EventValueSource ==
                                EventValueSource.Unresolved)
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
                            .ToList());
                })
                .ToList();

            List<PaymentEntity> payments =
                await dbContext.Payments
                    .AsNoTracking()
                    .Where(payment =>
                        payment.PaymentCategory !=
                            PaymentCategory.Membership &&
                        payment.PaymentDate >= startDate &&
                        payment.PaymentDate < endDateExclusive)
                    .Include(payment => payment.Booking)
                        .ThenInclude(booking => booking!.Booker)
                    .Include(payment => payment.Booking)
                        .ThenInclude(booking =>
                            booking!.BookingEvents)
                    .OrderBy(payment => payment.PaymentDate)
                    .ToListAsync(cancellationToken);

            List<UnallocatedPaymentItem> unallocatedPayments =
                payments
                    .Select(payment =>
                    {
                        decimal allocatedValue =
                            payment.Booking?.BookingEvents
                                .Where(bookingEvent =>
                                    bookingEvent.EventValue.HasValue)
                                .Sum(bookingEvent =>
                                    bookingEvent.EventValue!.Value)
                            ?? 0m;

                        decimal remainingUnallocatedValue =
                            payment.PaymentValue -
                            allocatedValue;

                        string reason = payment.Booking is null
                            ? "No matching booking was found in the imported Events CSV."
                            : payment.Booking.BookingEvents.Any(
                                bookingEvent =>
                                    bookingEvent.EventValueSource ==
                                        EventValueSource.Unresolved)
                                ? "One or more imported event rows do not have a value."
                                : "The payment exceeds the value of the imported event rows and may include sessions outside the current Events CSV.";

                        return new UnallocatedPaymentItem(
                            payment.Id,
                            payment.PaymentDate,
                            payment.BookingReference,
                            payment.Booking?.Booker.ContactName
                                ?? payment.CustomerName
                                ?? payment.PayerEmail
                                ?? "Unknown",
                            payment.PaymentValue,
                            allocatedValue,
                            remainingUnallocatedValue,
                            reason);
                    })
                    .Where(payment =>
                        payment.RemainingUnallocatedValue > 0m)
                    .ToList();

            return AppResponse<Result>.Ok(
                new Result(
                    query.StartDate,
                    query.EndDate,
                    results,
                    unallocatedPayments,
                    results.Sum(booking => booking.Events.Count),
                    unallocatedPayments.Sum(payment =>
                        payment.RemainingUnallocatedValue)));
        }
    }
}
