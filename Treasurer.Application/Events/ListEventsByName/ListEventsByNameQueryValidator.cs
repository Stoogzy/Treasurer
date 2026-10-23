using FluentValidation;

namespace Treasurer.Application.Events.ListEventsByName;

internal sealed class ListEventsByNameQueryValidator
    : AbstractValidator<ListEventsByNameQuery>
{
    public ListEventsByNameQueryValidator()
    {
        RuleFor(x => x.EventName)
            .NotEmpty()
            .WithMessage("Event name is required.");

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
