namespace Treasurer.Core.Entities;

public class EventEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string EventId { get; set; }
    public required string EventName { get; set; }
    public string? EventCode { get; set; }
    public string? Location { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    // public ICollection<BookingEventEntity> BookingEvents { get; set; } = [];
}
