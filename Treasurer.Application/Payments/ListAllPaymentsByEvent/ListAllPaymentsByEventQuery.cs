using Microsoft.EntityFrameworkCore;
using Treasurer.Application.Common.AppRequests;
using Treasurer.Application.Common.Interfaces;
using Treasurer.Core.Entities;
using Treasurer.Core.Enums;

namespace Treasurer.Application.Payments.ListAllPaymentsByEvent;

public sealed record ListAllPaymentsByEventQuery(
    string EventId,
    DateOnly StartDate,
    DateOnly EndDate)
{
    public sealed record PaymentItem(
        Guid BookingEventId,
        DateTime PaymentDate,
        string BookingReference,
        string PaidFor,
        bool? IsGuardianPayment,
        decimal Value,
        EventValueSource ValueSource);

    public sealed record Result(
        string EventId,
        string EventName,
        DateOnly StartDate,
        DateOnly EndDate,
        IReadOnlyList<PaymentItem> Payments,
        decimal TotalValue);

    public sealed class Handler(IApplicationDbContext dbContext) : IRequestHandler<ListAllPaymentsByEventQuery, Result>
    {
        public async Task<AppResponse<Result>> Handle(ListAllPaymentsByEventQuery query, CancellationToken cancellationToken)
        {
            DateTime startDate = query.StartDate.ToDateTime(TimeOnly.MinValue);
            DateTime endDateExclusive = query.EndDate
                .AddDays(1)
                .ToDateTime(TimeOnly.MinValue);

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
                    bookingEvent.Booking.Payments.Any(payment =>
                        payment.PaymentDate >= startDate &&
                        payment.PaymentDate < endDateExclusive))
                .Select(bookingEvent => new
                {
                    BookingEventId = bookingEvent.Id,
                    PaymentDate = bookingEvent.Booking.Payments
                        .Where(payment =>
                            payment.PaymentDate >= startDate &&
                            payment.PaymentDate < endDateExclusive)
                        .OrderByDescending(payment =>
                            payment.PaymentDate)
                        .Select(payment => payment.PaymentDate)
                        .First(),
                    bookingEvent.Booking.BookingReference,
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
                    payment.PaidFor,
                    payment.IsGuardianPayment,
                    payment.Value,
                    payment.ValueSource))
                .ToList();

            return AppResponse<Result>.Ok(
                new Result(
                    eventEntity.BookwhenEventId,
                    eventEntity.EventName,
                    query.StartDate,
                    query.EndDate,
                    payments,
                    payments.Sum(x => x.Value)));
        }
    }
}
