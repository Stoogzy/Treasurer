using Treasurer.Core.Enums;

namespace Treasurer.Core.Entities;

public class PaymentEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required DateTime PaymentDate { get; set; }
    public required string BookingReference { get; set; }
    public required string PaymentStatus { get; set; }
    public required string PaymentMethod { get; set; }
    public required decimal PaymentValue { get; set; }
    public PaymentCategory PaymentCategory { get; set; }
    public string? CustomerName { get; set; }
    public string? PayerEmail { get; set; }
    public Guid? BookingId {  get; set; }

    // Navigation Properties
    public BookingEntity? Booking { get; set; }
}
