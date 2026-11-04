using Treasurer.Application.Common.Imports.Models;

namespace Treasurer.Application.Common.Imports.Assumptions;

/// <summary>
/// Assumption 3: League players attend recreational sessions as part of their membership.
/// An unpaid booking for a "Recreational" event with a "League Player" ticket is valued
/// at £0 instead of being reported as missing income.
/// </summary>
internal static class MembershipIncludedRecreationalSessionAssumption
{
    private const string RecreationalEventTitle = "Recreational";
    private const string MembershipTicketName = "League Player";

    public static bool Applies(
        BookwhenEventCsvRow row,
        IReadOnlyList<BookwhenEventCsvRow> bookingRows)
    {
        bool hasNoBookingPayment = bookingRows.All(
            bookingRow =>
                !bookingRow.BookingPayment.HasValue ||
                bookingRow.BookingPayment.Value == 0m);

        return hasNoBookingPayment &&
               row.EventTitle.Contains(
                   RecreationalEventTitle,
                   StringComparison.OrdinalIgnoreCase) &&
               row.TicketName.Contains(
                   MembershipTicketName,
                   StringComparison.OrdinalIgnoreCase);
    }
}
