namespace Treasurer.Application.Common.Imports.Models;

public sealed class BookwhenEventCsvRow
{
    public required string EntryId { get; init; }
    public required string EventId { get; init; }
    public required string EventTitle { get; init; }
    public string? EventCode { get; init; }
    public DateTime? EventStarts { get; init; }
    public DateTime? EventEnds { get; init; }
    public string? Location { get; init; }
    public bool? Attended { get; init; }
    public required string BookingId { get; init; }
    public required string BookingReference { get; init; }
    public decimal BookingPayment { get; init; }
    public required string TicketName { get; init; }
    public required string TicketType { get; init; }
    public string? BookerEmail { get; init; }
    public string? ContactName { get; init; }
    public string? AttendeeCustomerId { get; init; }
    public required string AttendeeCustomerName { get; init; }
}
