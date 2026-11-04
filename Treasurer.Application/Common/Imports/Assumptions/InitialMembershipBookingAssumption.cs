using Treasurer.Application.Common.Imports.Models;

namespace Treasurer.Application.Common.Imports.Assumptions;

/// <summary>
/// Assumption 1: When a direct debit membership is set up, Bookwhen records the first
/// payment as a Stripe event booking. These bookings have a payment but no ticket or
/// attendance values on any row, so the whole payment is treated as membership and the
/// event rows are valued at £0.
/// </summary>
internal static class InitialMembershipBookingAssumption
{
    public static bool Applies(IReadOnlyList<BookwhenEventCsvRow> bookingRows)
    {
        bool hasPositiveBookingPayment = bookingRows.Any(
            row => row.BookingPayment is > 0m);

        bool hasNoTicketOrAttendanceValues = bookingRows.All(
            row =>
                !row.TicketFaceValue.HasValue &&
                !row.TicketCurrentValue.HasValue &&
                !row.CurrentAttendanceValue.HasValue);

        return hasPositiveBookingPayment &&
               hasNoTicketOrAttendanceValues;
    }
}
