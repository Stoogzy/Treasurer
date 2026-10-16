namespace Treasurer.Core.Entities;

public class BookerEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string BookerEmail { get; set; }
    public required string ContactName { get; set; }
    public ICollection<BookingEntity> Bookings { get; set; } = [];
}
