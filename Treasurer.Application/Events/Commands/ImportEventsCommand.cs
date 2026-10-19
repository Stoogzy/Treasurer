using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Treasurer.Application.Common.AppRequests;
using Treasurer.Application.Common.Interfaces;
using Treasurer.Application.Imports.Exceptions;
using Treasurer.Application.Imports.Models;
using Treasurer.Core.Entities;

namespace Treasurer.Application.Events.Commands;

public sealed record ImportEventsCommand(
    Stream CsvStream,
    string FileName)
{
    public sealed record Result(int ImportedRowCount);

    public sealed class Handler(IBookwhenCsvReader csvReader, IApplicationDbContext dbContext, IValidator<BookwhenEventCsvRow> validator) : IRequestHandler<ImportEventsCommand, Result>
    {
        public async Task<AppResponse<Result>> Handle(ImportEventsCommand command, CancellationToken cancellationToken)
        {
            try
            {
                IReadOnlyList<BookwhenEventCsvRow> rows = await csvReader.ReadEventsAsync(command.CsvStream, cancellationToken);

                Dictionary<string, string[]>? errors = await ValidateRowsAsync(rows, cancellationToken);

                if (errors is not null)
                {
                    return new AppResponse<Result>(
                        statusCode: 400,
                        content: default,
                        message: "One or more CSV rows are invalid.",
                        errors: errors);
                }

                foreach (BookwhenEventCsvRow row in rows)
                {
                    await ImportRowAsync(row, cancellationToken);
                }

                await dbContext.SaveChangesAsync(cancellationToken);

                return AppResponse<Result>.Ok(new Result(rows.Count));
            }
            catch (CsvImportException exception)
            {
                return new AppResponse<Result>(statusCode: 400, message: exception.Message);
            }
        }

        private async Task<Dictionary<string, string[]>?> ValidateRowsAsync(IReadOnlyList<BookwhenEventCsvRow> rows, CancellationToken cancellationToken)
        {
            Dictionary<string, List<string>> errors = new ();

            for (int index = 0; index < rows.Count; index++)
            {
                BookwhenEventCsvRow row = rows[index];

                ValidationResult result = await validator.ValidateAsync(row, cancellationToken);

                // Header is physical row 1, so the first data row is row 2.
                int csvRowNumber = index + 2;

                foreach (ValidationFailure failure in result.Errors)
                {
                    string key = $"Rows[{csvRowNumber}].{failure.PropertyName}";

                    if (!errors.TryGetValue(key, out List<string>? messages))
                    {
                        messages = new List<string>();
                        errors[key] = messages;
                    }

                    messages.Add(failure.ErrorMessage);
                }
            }

            if (errors.Count == 0)
            {
                return null;
            }

            return errors.ToDictionary(
                pair => pair.Key,
                pair => pair.Value
                    .Distinct()
                    .ToArray());
        }

        private async Task ImportRowAsync(BookwhenEventCsvRow row, CancellationToken cancellationToken)
        {
            string bookerEmail = row.BookerEmail!.Trim();
            string contactName = row.ContactName!.Trim();
            string bookingReference = row.BookingReference.Trim();
            string attendeeName = row.AttendeeCustomerName.Trim();
            string? attendeeCustomerId = NormalizeOptionalValue(row.AttendeeCustomerId);

            BookerEntity? booker = dbContext.Bookers.Local
                .SingleOrDefault(existing => existing.BookerEmail == bookerEmail)
                ?? await dbContext.Bookers.SingleOrDefaultAsync(
                    existing => existing.BookerEmail == bookerEmail,
                    cancellationToken);

            if (booker is null)
            {
                booker = new BookerEntity
                {
                    BookerEmail = bookerEmail,
                    ContactName = contactName
                };

                dbContext.Bookers.Add(booker);
            }
            else if (!string.Equals(
                         booker.ContactName,
                         contactName,
                         StringComparison.Ordinal))
            {
                booker.ContactName = contactName;
            }

            EventEntity? eventEntity = dbContext.Events.Local
                .SingleOrDefault(existing => existing.BookwhenEventId == row.EventId)
                ?? await dbContext.Events.SingleOrDefaultAsync(
                    existing => existing.BookwhenEventId == row.EventId,
                    cancellationToken);

            if (eventEntity is null)
            {
                eventEntity = new EventEntity
                {
                    BookwhenEventId = row.EventId.Trim(),
                    EventName = row.EventTitle.Trim(),
                    EventCode = NormalizeOptionalValue(row.EventCode),
                    Location = NormalizeOptionalValue(row.Location),
                    StartsAt = row.EventStarts,
                    EndsAt = row.EventEnds
                };

                dbContext.Events.Add(eventEntity);
            }
            else
            {
                eventEntity.EventName = row.EventTitle.Trim();
                eventEntity.EventCode = NormalizeOptionalValue(row.EventCode);
                eventEntity.Location = NormalizeOptionalValue(row.Location);
                eventEntity.StartsAt = row.EventStarts;
                eventEntity.EndsAt = row.EventEnds;
            }

            AttendeeEntity? attendee = attendeeCustomerId is not null
                ? dbContext.Attendees.Local.SingleOrDefault(
                    existing => existing.AttendeeCustomerId == attendeeCustomerId)
                    ?? await dbContext.Attendees.SingleOrDefaultAsync(
                        existing => existing.AttendeeCustomerId == attendeeCustomerId,
                        cancellationToken)
                : dbContext.Attendees.Local.SingleOrDefault(
                    existing =>
                        existing.AttendeeCustomerId == null &&
                        existing.AttendeeCustomerName == attendeeName)
                    ?? await dbContext.Attendees.FirstOrDefaultAsync(
                        existing =>
                            existing.AttendeeCustomerId == null &&
                            existing.AttendeeCustomerName == attendeeName,
                        cancellationToken);

            if (attendee is null)
            {
                attendee = new AttendeeEntity
                {
                    AttendeeCustomerId = attendeeCustomerId,
                    AttendeeCustomerName = attendeeName
                };

                dbContext.Attendees.Add(attendee);
            }
            else
            {
                attendee.AttendeeCustomerName = attendeeName;
            }

            BookingEntity? booking = dbContext.Bookings.Local
                .SingleOrDefault(existing =>
                    existing.BookwhenBookingId == row.BookingId ||
                    existing.BookingReference == bookingReference)
                ?? await dbContext.Bookings.SingleOrDefaultAsync(
                    existing =>
                        existing.BookwhenBookingId == row.BookingId ||
                        existing.BookingReference == bookingReference,
                    cancellationToken);

            if (booking is null)
            {
                booking = new BookingEntity
                {
                    BookwhenBookingId = row.BookingId.Trim(),
                    BookingReference = bookingReference,
                    BookingPayment = row.BookingPayment,
                    BookerId = booker.Id,
                    Booker = booker
                };

                dbContext.Bookings.Add(booking);
            }
            else
            {
                booking.BookwhenBookingId = row.BookingId.Trim();
                booking.BookingReference = bookingReference;
                booking.BookingPayment = row.BookingPayment;
                booking.BookerId = booker.Id;
                booking.Booker = booker;
            }

            BookingEventEntity? bookingEvent = dbContext.BookingEvents.Local
                .SingleOrDefault(existing => existing.BookwhenEntryId == row.EntryId)
                ?? await dbContext.BookingEvents.SingleOrDefaultAsync(
                    existing => existing.BookwhenEntryId == row.EntryId,
                    cancellationToken);

            if (bookingEvent is null)
            {
                bookingEvent = new BookingEventEntity
                {
                    BookwhenEntryId = row.EntryId.Trim(),
                    BookingId = booking.Id,
                    Booking = booking,
                    EventId = eventEntity.Id,
                    Event = eventEntity,
                    AttendeeId = attendee.Id,
                    Attendee = attendee,
                    TicketName = row.TicketName.Trim(),
                    TicketType = row.TicketType.Trim(),
                    Attended = row.Attended
                };

                dbContext.BookingEvents.Add(bookingEvent);
            }
            else
            {
                bookingEvent.BookingId = booking.Id;
                bookingEvent.Booking = booking;
                bookingEvent.EventId = eventEntity.Id;
                bookingEvent.Event = eventEntity;
                bookingEvent.AttendeeId = attendee.Id;
                bookingEvent.Attendee = attendee;
                bookingEvent.TicketName = row.TicketName.Trim();
                bookingEvent.TicketType = row.TicketType.Trim();
                bookingEvent.Attended = row.Attended;
            }

            List<PaymentEntity> unmatchedPayments = await dbContext.Payments
                .Where(payment =>
                    payment.BookingId == null &&
                    payment.BookingReference == bookingReference)
                .ToListAsync(cancellationToken);

            foreach (PaymentEntity payment in unmatchedPayments)
            {
                payment.BookingId = booking.Id;
                payment.Booking = booking;
            }
        }

        private static string? NormalizeOptionalValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}