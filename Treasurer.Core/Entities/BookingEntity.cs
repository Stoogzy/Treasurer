namespace Treasurer.Core.Entities;

public class BookingEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string BookwhenBookingId { get; set; }
    public required string BookingReference { get; set; }
    public decimal? BookingPayment { get; set; }
    public bool? IsGuardianBooking { get; set; }
    public required Guid BookerId { get; set; }

    public ICollection<BookingEventEntity> BookingEvents { get; set; } = [];
    public ICollection<PaymentEntity> Payments { get; set; } = [];

    // Navigation Properties
    public required BookerEntity Booker { get; set; }
}
