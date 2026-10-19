using FluentValidation;

namespace Treasurer.Application.Payments.ListMembershipPayments;

internal sealed class ListAllMembershipPaymentsQueryValidator
    : AbstractValidator<ListAllMembershipPaymentsQuery>
{
    public ListAllMembershipPaymentsQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("End date is required.")
            .LessThan(DateOnly.MaxValue)
            .WithMessage("End date must be earlier than 9999-12-31.")
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be on or after the start date.");
    }
}
