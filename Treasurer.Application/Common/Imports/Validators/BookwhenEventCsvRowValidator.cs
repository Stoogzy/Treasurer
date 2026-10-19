using FluentValidation;
using Treasurer.Application.Common.Imports.Models;

namespace Treasurer.Application.Common.Imports.Validators;

internal sealed class BookwhenEventCsvRowValidator : AbstractValidator<BookwhenEventCsvRow>
{
    public BookwhenEventCsvRowValidator()
    {
        RuleFor(x => x.AttendanceId)
            .NotEmpty()
            .WithMessage("Attendance ID is required.");

        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event ID is required.");

        RuleFor(x => x.EventTitle)
            .NotEmpty()
            .WithMessage("Event title is required.");

        RuleFor(x => x.BookingId)
            .NotEmpty()
            .WithMessage("Booking ID is required.");

        RuleFor(x => x.BookingReference)
            .NotEmpty()
            .WithMessage("Booking reference is required.");

        RuleFor(x => x.TicketName)
            .NotEmpty()
            .WithMessage("Ticket name is required.");

        RuleFor(x => x.TicketType)
            .NotEmpty()
            .WithMessage("Ticket type is required.");

        RuleFor(x => x.AttendeeCustomerName)
            .NotEmpty()
            .WithMessage("Attendee customer name is required.");

        RuleFor(x => x.BookerEmail)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Booker email is required.")
            .EmailAddress()
            .WithMessage("Booker email must be a valid email address.");
    }
}
