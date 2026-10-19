namespace Treasurer.Core.Entities;

public class BookingEventEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string BookwhenEntryId { get; set; }
    public Guid BookingId { get; set; }
    public Guid EventId { get; set; }
    public Guid AttendeeId { get; set; }
    public required string TicketName { get; set; }
    public required string TicketType { get; set; }
    public bool? Attended { get; set; }

    // Navigation Properties
    public BookingEntity Booking { get; set; } = null!;
    public EventEntity Event { get; set; } = null!;
    public AttendeeEntity Attendee { get; set; } = null!;
}
