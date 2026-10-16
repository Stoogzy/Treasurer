namespace Treasurer.Core.Entities;

public class AttendeeEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? AttendeeCustomerId { get; set; }
    public required string AttendeeCustomerName { get; set; }
    public ICollection<BookingEventEntity> BookingEvents { get; set; } = [];
}
