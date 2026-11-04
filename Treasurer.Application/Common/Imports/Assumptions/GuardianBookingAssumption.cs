using Treasurer.Application.Common.Imports.Models;

namespace Treasurer.Application.Common.Imports.Assumptions;

/// <summary>
/// Assumption 6: When the booking contact name differs from the attendee name, someone
/// else (usually a parent or guardian) booked on the attendee's behalf. Returns null when
/// a contact name is missing and no difference was found.
/// </summary>
internal static class GuardianBookingAssumption
{
    public static bool? Determine(IReadOnlyList<BookwhenEventCsvRow> bookingRows)
    {
        bool hasMissingContactName = false;

        foreach (BookwhenEventCsvRow row in bookingRows)
        {
            if (string.IsNullOrWhiteSpace(row.ContactName))
            {
                hasMissingContactName = true;
                continue;
            }

            if (!string.Equals(
                    row.ContactName.Trim(),
                    row.AttendeeCustomerName.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return hasMissingContactName ? null : false;
    }
}
