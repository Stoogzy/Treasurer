using Treasurer.Application.Common.Imports.Models;

namespace Treasurer.Application.Common.Imports.Assumptions;

/// <summary>
/// Assumption 4: When a booking has one row for one event and Bookwhen supplies no
/// attendance value, the whole booking payment is for that event.
/// </summary>
internal static class SingleEventBookingAssumption
{
    public static bool TryGetEventValue(
        BookwhenEventCsvRow row,
        IReadOnlyList<BookwhenEventCsvRow> bookingRows,
        out decimal eventValue)
    {
        eventValue = 0m;

        int distinctEventCount = bookingRows
            .Select(bookingRow => bookingRow.EventId)
            .Distinct(StringComparer.Ordinal)
            .Count();

        if (bookingRows.Count != 1 ||
            distinctEventCount != 1 ||
            !row.BookingPayment.HasValue)
        {
            return false;
        }

        eventValue = row.BookingPayment.Value;

        return true;
    }
}
