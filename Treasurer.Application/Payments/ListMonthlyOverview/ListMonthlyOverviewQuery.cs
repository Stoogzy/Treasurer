using Microsoft.EntityFrameworkCore;
using Treasurer.Application.Common.AppRequests;
using Treasurer.Application.Common.Interfaces;
using Treasurer.Core.Entities;
using Treasurer.Core.Enums;

namespace Treasurer.Application.Payments.ListMonthlyOverview;

public sealed record ListMonthlyOverviewQuery(
    DateOnly StartDate,
    DateOnly EndDate)
{
    public sealed record AllocationSummary(
        bool IsComplete,
        int UnresolvedBookingCount,
        int UnresolvedEventCount,
        int UnallocatedPaymentCount,
        decimal UnallocatedPaymentValue);

    public sealed record Result(
        DateOnly StartDate,
        DateOnly EndDate,
        decimal TotalPaymentsReceived,
        int PaymentCount,
        decimal MembershipIncome,
        int MembershipPaymentCount,
        decimal AllocatedEventIncome,
        int EventsWithPayments,
        AllocationSummary AllocationSummary);

    public sealed class Handler(IApplicationDbContext dbContext) : IRequestHandler<ListMonthlyOverviewQuery, Result>
    {
        public async Task<AppResponse<Result>> Handle(ListMonthlyOverviewQuery query, CancellationToken cancellationToken)
        {
            DateTime startDate = query.StartDate.ToDateTime(TimeOnly.MinValue);
            DateTime endDateExclusive = query.EndDate
                .AddDays(1)
                .ToDateTime(TimeOnly.MinValue);

            IQueryable<PaymentEntity> payments = dbContext.Payments
                .AsNoTracking()
                .Where(payment =>
                    payment.PaymentDate >= startDate &&
                    payment.PaymentDate < endDateExclusive);

            int paymentCount = await payments.CountAsync(cancellationToken);
            decimal totalPaymentsReceived = await payments.SumAsync(payment => payment.PaymentValue, cancellationToken);

            IQueryable<PaymentEntity> membershipPayments =
                payments.Where(payment =>
                    payment.PaymentCategory ==
                    PaymentCategory.Membership);

            int membershipPaymentCount = await membershipPayments.CountAsync(cancellationToken);

            decimal membershipIncome = await membershipPayments.SumAsync(
                    payment => payment.PaymentValue,
                    cancellationToken);

            IQueryable<BookingEventEntity> mixedMembershipComponents =
                dbContext.BookingEvents
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
                                endDateExclusive));

            int mixedMembershipPaymentCount =
                await mixedMembershipComponents.CountAsync(
                    cancellationToken);

            decimal mixedMembershipIncome =
                await mixedMembershipComponents.SumAsync(
                    bookingEvent =>
                        bookingEvent.EventValue!.Value,
                    cancellationToken);

            membershipPaymentCount += mixedMembershipPaymentCount;
            membershipIncome += mixedMembershipIncome;

            IQueryable<BookingEventEntity> bookingEventsInPeriod = dbContext.BookingEvents
                .AsNoTracking()
                .Where(bookingEvent =>
                    bookingEvent.Booking.Payments.Any(payment =>
                        payment.PaymentDate >= startDate &&
                        payment.PaymentDate < endDateExclusive));

            decimal allocatedEventIncome =
                await bookingEventsInPeriod
                    .Where(bookingEvent =>
                        bookingEvent.EventValue.HasValue &&
                        bookingEvent.EventValueSource !=
                            EventValueSource.MembershipIncluded &&
                        bookingEvent.EventValueSource !=
                            EventValueSource.InitialMembershipPayment)
                    .SumAsync(
                        bookingEvent =>
                            bookingEvent.EventValue!.Value,
                        cancellationToken);

            int eventsWithPayments =
                await bookingEventsInPeriod
                    .Where(bookingEvent =>
                        bookingEvent.EventValue.HasValue &&
                        bookingEvent.EventValueSource !=
                            EventValueSource.MembershipIncluded &&
                        bookingEvent.EventValueSource !=
                            EventValueSource.InitialMembershipPayment)
                    .Select(bookingEvent => bookingEvent.EventId)
                    .Distinct()
                    .CountAsync(cancellationToken);

            List<Guid> unresolvedBookingIds =
                await bookingEventsInPeriod
                    .Where(bookingEvent =>
                        bookingEvent.EventValueSource ==
                        EventValueSource.Unresolved)
                    .Select(bookingEvent => bookingEvent.BookingId)
                    .ToListAsync(cancellationToken);

            List<PaymentEntity> allocatablePayments =
                await payments
                    .Where(payment =>
                        payment.PaymentCategory !=
                            PaymentCategory.Membership)
                    .Include(payment => payment.Booking)
                        .ThenInclude(booking =>
                            booking!.BookingEvents)
                    .ToListAsync(cancellationToken);

            List<decimal> unallocatedPaymentValues =
                allocatablePayments
                    .Select(payment =>
                    {
                        decimal allocatedValue =
                            payment.Booking?.BookingEvents
                                .Where(bookingEvent =>
                                    bookingEvent.EventValue.HasValue)
                                .Sum(bookingEvent =>
                                    bookingEvent.EventValue!.Value)
                            ?? 0m;

                        return payment.PaymentValue -
                            allocatedValue;
                    })
                    .Where(remainingValue =>
                        remainingValue > 0m)
                    .ToList();

            AllocationSummary allocationSummary = new(
                IsComplete:
                    unresolvedBookingIds.Count == 0 &&
                    unallocatedPaymentValues.Count == 0,
                UnresolvedBookingCount:
                    unresolvedBookingIds.Distinct().Count(),
                UnresolvedEventCount: unresolvedBookingIds.Count,
                UnallocatedPaymentCount:
                    unallocatedPaymentValues.Count,
                UnallocatedPaymentValue:
                    unallocatedPaymentValues.Sum());

            return AppResponse<Result>.Ok(
                new Result(
                    query.StartDate,
                    query.EndDate,
                    totalPaymentsReceived,
                    paymentCount,
                    membershipIncome,
                    membershipPaymentCount,
                    allocatedEventIncome,
                    eventsWithPayments,
                    allocationSummary));
        }
    }
}
