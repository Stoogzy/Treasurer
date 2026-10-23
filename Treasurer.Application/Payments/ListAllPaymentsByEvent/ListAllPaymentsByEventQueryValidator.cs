using FluentValidation;

namespace Treasurer.Application.Payments.ListAllPaymentsByEvent;

internal sealed class ListAllPaymentsByEventQueryValidator : AbstractValidator<ListAllPaymentsByEventQuery>
{
    public ListAllPaymentsByEventQueryValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event ID is required.");
    }
}
