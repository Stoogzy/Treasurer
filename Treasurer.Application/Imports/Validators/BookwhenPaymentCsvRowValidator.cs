using FluentValidation;
using Treasurer.Application.Imports.Models;

namespace Treasurer.Application.Imports.Validators;

internal sealed class BookwhenPaymentCsvRowValidator : AbstractValidator<BookwhenPaymentCsvRow>
{
    public BookwhenPaymentCsvRowValidator()
    {
        RuleFor(x => x.PaymentDate)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Payment date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Payment date cannot be in the future.");

        RuleFor(x => x.BookingReference)
            .NotEmpty()
            .WithMessage("Booking reference is required.");

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Payment status is required.");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .WithMessage("Payment method is required.");

        RuleFor(x => x.PaymentValue)
            .NotEqual(0)
            .WithMessage("Payment value cannot be zero.");
    }
}
