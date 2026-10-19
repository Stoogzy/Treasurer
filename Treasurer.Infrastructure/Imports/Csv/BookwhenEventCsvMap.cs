using CsvHelper.Configuration;
using Treasurer.Application.Common.Imports.Models;

namespace Treasurer.Infrastructure.Imports.Csv;

internal sealed class BookwhenEventCsvMap
    : ClassMap<BookwhenEventCsvRow>
{
    public BookwhenEventCsvMap()
    {
        Map(x => x.EntryId)
            .Name("EntryID");

        Map(x => x.EventId)
            .Name("EventID");

        Map(x => x.EventTitle)
            .Name("Event title");

        Map(x => x.EventCode)
            .Name("Event code");

        Map(x => x.EventStarts)
            .Name("Event starts")
            .TypeConverterOption.Format(
                "yyyy-MM-dd HH:mm:ss zzz",
                "dd/MM/yyyy HH:mm",
                "dd/MM/yyyy HH:mm:ss",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss");

        Map(x => x.EventEnds)
            .Name("Event ends")
            .TypeConverterOption.Format(
                "yyyy-MM-dd HH:mm:ss zzz",
                "dd/MM/yyyy HH:mm",
                "dd/MM/yyyy HH:mm:ss",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss");

        Map(x => x.Location)
            .Name("Location");

        Map(x => x.Attended)
            .Name("Attended?")
            .TypeConverterOption.BooleanValues(
                true,
                true,
                "yes",
                "Yes",
                "YES",
                "true",
                "True",
                "TRUE",
                "1")
            .TypeConverterOption.BooleanValues(
                false,
                true,
                "no",
                "No",
                "NO",
                "false",
                "False",
                "FALSE",
                "0");

        Map(x => x.BookingId)
            .Name("BookingID");

        Map(x => x.BookingReference)
            .Name("Booking ref");

        Map(x => x.BookingPayment)
            .Name("Booking payments");

        Map(x => x.TicketName)
            .Name("Ticket name");

        Map(x => x.TicketType)
            .Name("Ticket type");

        Map(x => x.BookerEmail)
            .Name("Booker email");

        Map(x => x.ContactName)
            .Name("Contact name");

        Map(x => x.AttendeeCustomerId)
            .Name("Attendee CustomerID");

        Map(x => x.AttendeeCustomerName)
            .Name("Attendee customer name");
    }
}
