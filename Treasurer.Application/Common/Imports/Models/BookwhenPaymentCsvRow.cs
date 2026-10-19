namespace Treasurer.Application.Common.Imports.Models;

public sealed class BookwhenPaymentCsvRow
{
    public DateTime PaymentDate { get; init; }
    public required string BookingReference { get; init; }
    public required string Status { get; init; }
    public required string PaymentMethod { get; init; }
    public decimal PaymentValue { get; init; }
    public string? CustomerName { get; init; }
    public string? PayerEmail { get; init; }
}
