namespace Treasurer.Web.Models;

public sealed record EventItem(
    string EventId,
    string EventName,
    string? EventCode,
    string? Location,
    DateTime? StartsAt,
    DateTime? EndsAt);

public sealed record EventsResult(IReadOnlyList<EventItem> Events);

public sealed record EventNameItem(
    string EventName,
    int OccurrenceCount,
    DateTime FirstOccurrence,
    DateTime LastOccurrence);

public sealed record EventNamesResult(
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyList<EventNameItem> EventNames);

public sealed record EventPaymentItem(
    Guid BookingEventId,
    DateTime PaymentDate,
    string BookingReference,
    string PaidBy,
    string PaidFor,
    bool? IsGuardianPayment,
    decimal Value,
    int ValueSource);

public sealed record EventAttendeeItem(
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

public sealed record EventPaymentsResult(
    string EventId,
    string EventName,
    DateTime? EventStartsAt,
    DateTime? EventEndsAt,
    IReadOnlyList<EventPaymentItem> Payments,
    IReadOnlyList<EventAttendeeItem> Attendees,
    decimal TotalValue,
    AllocationSummary AllocationSummary);

public sealed record EventOccurrenceItem(
    string EventId,
    DateTime StartsAt,
    DateTime? EndsAt,
    string? Location,
    IReadOnlyList<EventPaymentItem> Payments,
    IReadOnlyList<EventAttendeeItem> Attendees,
    decimal TotalValue,
    AllocationSummary AllocationSummary);

public sealed record EventsByNameResult(
    string EventName,
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyList<EventOccurrenceItem> Occurrences,
    decimal TotalValue);

public sealed record MembershipPaymentItem(
    Guid PaymentId,
    DateTime PaymentDate,
    string BookingReference,
    string PaidFor,
    decimal Value);

public sealed record MembershipPaymentsResult(
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyList<MembershipPaymentItem> Payments,
    decimal TotalValue);

public sealed record MonthlyAllocationSummary(
    bool IsComplete,
    int UnresolvedBookingCount,
    int UnresolvedEventCount,
    int UnallocatedPaymentCount,
    decimal UnallocatedPaymentValue);

public sealed record MonthlyOverviewResult(
    DateOnly StartDate,
    DateOnly EndDate,
    decimal TotalPaymentsReceived,
    int PaymentCount,
    decimal MembershipIncome,
    int MembershipPaymentCount,
    decimal AllocatedEventIncome,
    int EventsWithPayments,
    MonthlyAllocationSummary AllocationSummary);

public sealed record UnresolvedEventItem(
    Guid BookingEventId,
    string EventId,
    string EventName,
    DateTime? StartsAt,
    string AttendeeName,
    string TicketName,
    string TicketType);

public sealed record UnresolvedBookingItem(
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

public sealed record UnresolvedBookingsResult(
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyList<UnresolvedBookingItem> Bookings,
    IReadOnlyList<UnallocatedPaymentItem> UnallocatedPayments,
    int UnresolvedEventCount,
    decimal TotalRemainingUnallocatedValue);

public sealed record ImportResult(int ImportedRowCount);
