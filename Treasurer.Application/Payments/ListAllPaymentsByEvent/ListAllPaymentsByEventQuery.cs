using Microsoft.EntityFrameworkCore;
using Treasurer.Application.Common.AppRequests;
using Treasurer.Application.Common.Interfaces;
using Treasurer.Core.Entities;

namespace Treasurer.Application.Payments.ListAllPaymentsByEvent;

public sealed record ListAllPaymentsByEventQuery(string EventId)
{
    public sealed record PaymentItem(
        Guid PaymentId,
        DateTime PaymentDate,
        string BookingReference,
        string PaidFor,
        bool IsGuardianPayment,
        decimal Value);

    public sealed record Result(
        string EventId,
        string EventName,
        IReadOnlyList<PaymentItem> Payments,
        decimal TotalValue);

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

            List<BookingEntity> bookings = await dbContext.Bookings
                .AsNoTracking()
                .Where(booking =>
                    booking.BookingEvents.Any(
                        bookingEvent =>
                            bookingEvent.EventId == eventEntity.Id))
                .Include(booking => booking.Booker)
                .Include(booking => booking.Payments)
                .Include(booking => booking.BookingEvents)
                    .ThenInclude(bookingEvent => bookingEvent.Event)
                .Include(booking => booking.BookingEvents)
                    .ThenInclude(bookingEvent => bookingEvent.Attendee)
                .AsSplitQuery()
                .ToListAsync(cancellationToken);

            List<PaymentItem> payments = new();

            foreach (BookingEntity booking in bookings)
            {
                List<string> attendeeNames = booking.BookingEvents
                    .Where(x => x.EventId == eventEntity.Id)
                    .Select(x => x.Attendee.AttendeeCustomerName)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(name => name)
                    .ToList();

                string paidFor = attendeeNames.Count > 0
                    ? string.Join(", ", attendeeNames)
                    : booking.Booker.ContactName;

                bool isGuardianPayment = attendeeNames.Any(
                    attendeeName =>
                        !string.Equals(
                            attendeeName.Trim(),
                            booking.Booker.ContactName.Trim(),
                            StringComparison.OrdinalIgnoreCase));

                List<string> bookingEventIds = booking.BookingEvents
                    .Select(x => x.Event.BookwhenEventId)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(id => id, StringComparer.Ordinal)
                    .ToList();

                foreach (PaymentEntity payment in booking.Payments)
                {
                    decimal allocatedValue = AllocatePaymentToEvent(
                        payment.PaymentValue,
                        bookingEventIds,
                        eventEntity.BookwhenEventId);

                    payments.Add(new PaymentItem(
                        payment.Id,
                        payment.PaymentDate,
                        payment.BookingReference,
                        paidFor,
                        isGuardianPayment,
                        allocatedValue));
                }
            }

            List<PaymentItem> orderedPayments = payments
                .OrderByDescending(x => x.PaymentDate)
                .ThenBy(x => x.PaidFor)
                .ToList();

            return AppResponse<Result>.Ok(
                new Result(
                    eventEntity.BookwhenEventId,
                    eventEntity.EventName,
                    orderedPayments,
                    orderedPayments.Sum(x => x.Value)));
        }

        private static decimal AllocatePaymentToEvent(decimal paymentValue, IReadOnlyList<string> orderedEventIds, string targetEventId)
        {
            if (orderedEventIds.Count == 0)
            {
                return 0;
            }

            int targetIndex = orderedEventIds
                .ToList()
                .FindIndex(id =>
                    string.Equals(
                        id,
                        targetEventId,
                        StringComparison.Ordinal));

            if (targetIndex < 0)
            {
                return 0;
            }

            long totalPence = decimal.ToInt64(
                decimal.Round(
                    paymentValue * 100m,
                    0,
                    MidpointRounding.AwayFromZero));

            long basePence =
                totalPence / orderedEventIds.Count;

            long remainder =
                totalPence % orderedEventIds.Count;

            long allocatedPence = basePence;

            if (targetIndex < Math.Abs(remainder))
            {
                allocatedPence += Math.Sign(remainder);
            }

            return allocatedPence / 100m;
        }
    }
}
