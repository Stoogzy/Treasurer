using Microsoft.EntityFrameworkCore;
using Treasurer.Application.Common.AppRequests;
using Treasurer.Application.Common.Interfaces;
using Treasurer.Core.Entities;
using Treasurer.Core.Enums;

namespace Treasurer.Application.Payments.ListMembershipPayments;

public sealed record ListAllMembershipPaymentsQuery(
    DateOnly StartDate,
    DateOnly EndDate)
{
    public sealed record PaymentItem(
        Guid PaymentId,
        DateTime PaymentDate,
        string BookingReference,
        string PaidFor,
        decimal Value);

    public sealed record Result(
        DateOnly StartDate,
        DateOnly EndDate,
        IReadOnlyList<PaymentItem> Payments,
        decimal TotalValue);

    public sealed class Handler(IApplicationDbContext dbContext) : IRequestHandler<ListAllMembershipPaymentsQuery, Result>
    {
        public async Task<AppResponse<Result>> Handle(ListAllMembershipPaymentsQuery query, CancellationToken cancellationToken)
        {
            DateTime startDate = query.StartDate.ToDateTime(TimeOnly.MinValue);
            DateTime endDateExclusive = query.EndDate
                .AddDays(1)
                .ToDateTime(TimeOnly.MinValue);

            List<PaymentEntity> payments = await dbContext.Payments
                .AsNoTracking()
                .Where(payment =>
                    payment.PaymentCategory ==
                    PaymentCategory.Membership &&
                    payment.PaymentDate >= startDate &&
                    payment.PaymentDate < endDateExclusive)
                .Include(payment => payment.Booking)
                    .ThenInclude(booking => booking!.BookingEvents)
                        .ThenInclude(bookingEvent =>
                            bookingEvent.Attendee)
                .OrderByDescending(payment => payment.PaymentDate)
                .ToListAsync(cancellationToken);

            List<PaymentItem> results = payments
                .Select(payment =>
                {
                    List<string> attendeeNames =
                        payment.Booking?.BookingEvents
                            .Select(x =>
                                x.Attendee.AttendeeCustomerName)
                            .Where(name =>
                                !string.IsNullOrWhiteSpace(name))
                            .Distinct(
                                StringComparer.OrdinalIgnoreCase)
                            .OrderBy(name => name)
                            .ToList()
                        ?? [];

                    string paidFor = attendeeNames.Count > 0
                        ? string.Join(", ", attendeeNames)
                        : payment.CustomerName
                            ?? payment.PayerEmail
                            ?? "Unknown";

                    return new PaymentItem(
                        payment.Id,
                        payment.PaymentDate,
                        payment.BookingReference,
                        paidFor,
                        payment.PaymentValue);
                })
                .ToList();

            List<PaymentItem> mixedMembershipPayments =
                await dbContext.BookingEvents
                    .AsNoTracking()
                    .Where(bookingEvent =>
                        bookingEvent.EventValueSource ==
                            EventValueSource.InitialMembershipPayment &&
                        bookingEvent.EventValue > 0m &&
                        bookingEvent.Booking.Payments.Any(payment =>
                            payment.PaymentCategory ==
                                PaymentCategory.Mixed &&
                            payment.PaymentDate >= startDate &&
                            payment.PaymentDate <
                                endDateExclusive))
                    .Select(bookingEvent => new PaymentItem(
                        bookingEvent.Id,
                        bookingEvent.Booking.Payments
                            .Where(payment =>
                                payment.PaymentCategory ==
                                    PaymentCategory.Mixed &&
                                payment.PaymentDate >= startDate &&
                                payment.PaymentDate <
                                    endDateExclusive)
                            .OrderByDescending(payment =>
                                payment.PaymentDate)
                            .Select(payment =>
                                payment.PaymentDate)
                            .First(),
                        bookingEvent.Booking.BookingReference,
                        bookingEvent.Attendee
                            .AttendeeCustomerName,
                        bookingEvent.EventValue!.Value))
                    .ToListAsync(cancellationToken);

            results.AddRange(mixedMembershipPayments);
            results = results
                .OrderByDescending(payment =>
                    payment.PaymentDate)
                .ThenBy(payment => payment.PaidFor)
                .ToList();

            return AppResponse<Result>.Ok(
                new Result(
                    query.StartDate,
                    query.EndDate,
                    results,
                    results.Sum(x => x.Value)));
        }
    }
}
