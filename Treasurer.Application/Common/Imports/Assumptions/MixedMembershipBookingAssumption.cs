using Treasurer.Application.Common.Imports.Models;

namespace Treasurer.Application.Common.Imports.Assumptions;

/// <summary>
/// Assumption 2: A membership can be bought in the same booking as priced event tickets.
/// The membership appears as exactly one blank "League player" row, while the other rows
/// carry Bookwhen attendance values. The membership value is the booking payment minus
/// the known event values, and the payment is treated as Mixed.
/// </summary>
internal static class MixedMembershipBookingAssumption
{
    private const string MembershipTicketName = "League player";

    public static bool TryGetMembershipValue(
        IReadOnlyList<BookwhenEventCsvRow> bookingRows,
        out decimal membershipValue)
    {
        membershipValue = 0m;

        List<BookwhenEventCsvRow> membershipRows = bookingRows
            .Where(IsMembershipRow)
            .ToList();

        if (membershipRows.Count != 1 ||
            !bookingRows.Any(row => row.CurrentAttendanceValue.HasValue))
        {
            return false;
        }

        decimal? bookingPayment = bookingRows
            .Select(row => row.BookingPayment)
            .FirstOrDefault(value => value.HasValue);

        if (!bookingPayment.HasValue)
        {
            return false;
        }

        decimal knownValue = bookingRows
            .Where(row => row.CurrentAttendanceValue.HasValue)
            .Sum(row => row.CurrentAttendanceValue!.Value);

        membershipValue = bookingPayment.Value - knownValue;

        return membershipValue > 0m;
    }

    public static bool IsMembershipRow(BookwhenEventCsvRow row)
    {
        return !row.TicketFaceValue.HasValue &&
               !row.TicketCurrentValue.HasValue &&
               !row.CurrentAttendanceValue.HasValue &&
               row.TicketName.Contains(
                   MembershipTicketName,
                   StringComparison.OrdinalIgnoreCase);
    }
}
