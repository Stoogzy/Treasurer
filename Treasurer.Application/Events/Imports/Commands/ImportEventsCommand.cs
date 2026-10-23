using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Treasurer.Application.Common.AppRequests;
using Treasurer.Application.Common.Imports.Exceptions;
using Treasurer.Application.Common.Imports.Models;
using Treasurer.Application.Common.Interfaces;
using Treasurer.Application.Common.Payments;
using Treasurer.Core.Entities;
using Treasurer.Core.Enums;

namespace Treasurer.Application.Events.Imports.Commands;

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

                Dictionary<string, IReadOnlyList<BookwhenEventCsvRow>> rowsByBooking =
                    rows.GroupBy(row => row.BookingId)
                        .ToDictionary(
                            group => group.Key,
                            group => (IReadOnlyList<BookwhenEventCsvRow>)group.ToList());

                foreach (BookwhenEventCsvRow row in rows)
                {
                    await ImportRowAsync(row, rowsByBooking[row.BookingId], cancellationToken);
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

        private async Task ImportRowAsync(
            BookwhenEventCsvRow row,
            IReadOnlyList<BookwhenEventCsvRow> bookingRows,
            CancellationToken cancellationToken)
        {
            string bookerEmail = row.BookerEmail!.Trim();
            string bookingReference = row.BookingReference.Trim();
            string attendeeName = row.AttendeeCustomerName.Trim();
            string? suppliedContactName = NormalizeOptionalValue(row.ContactName);
            string contactName = suppliedContactName ?? attendeeName;
            string? attendeeCustomerId = NormalizeOptionalValue(row.AttendeeCustomerId);
            decimal? bookingPayment = bookingRows
                .Select(bookingRow => bookingRow.BookingPayment)
                .FirstOrDefault(value => value.HasValue);

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
            else if (suppliedContactName is not null &&
                     !string.Equals(
                         booker.ContactName,
                         suppliedContactName,
                         StringComparison.Ordinal))
            {
                booker.ContactName = suppliedContactName;
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
                    BookingPayment = bookingPayment,
                    IsGuardianBooking = DetermineGuardianBooking(bookingRows),
                    BookerId = booker.Id,
                    Booker = booker
                };

                dbContext.Bookings.Add(booking);
            }
            else
            {
                booking.BookwhenBookingId = row.BookingId.Trim();
                booking.BookingReference = bookingReference;
                booking.BookingPayment = bookingPayment;
                booking.IsGuardianBooking = DetermineGuardianBooking(bookingRows);
                booking.BookerId = booker.Id;
                booking.Booker = booker;
            }

            (decimal? eventValue, EventValueSource eventValueSource) = DetermineEventValue(row, bookingRows);

            BookingEventEntity? bookingEvent = dbContext.BookingEvents.Local
                .SingleOrDefault(existing =>
                    existing.BookwhenEntryId == row.AttendanceId)
                ?? await dbContext.BookingEvents.SingleOrDefaultAsync(
                    existing =>
                        existing.BookwhenEntryId == row.AttendanceId,
                    cancellationToken);

            if (bookingEvent is null)
            {
                bookingEvent = new BookingEventEntity
                {
                    BookwhenEntryId = row.AttendanceId.Trim(),
                    BookingId = booking.Id,
                    Booking = booking,
                    EventId = eventEntity.Id,
                    Event = eventEntity,
                    AttendeeId = attendee.Id,
                    Attendee = attendee,
                    TicketName = row.TicketName.Trim(),
                    TicketType = row.TicketType.Trim(),
                    Attended = row.Attended,
                    EventValue = eventValue,
                    EventValueSource = eventValueSource
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

                if (eventValueSource != EventValueSource.Unresolved ||
                    bookingEvent.EventValueSource != EventValueSource.Manual)
                {
                    bookingEvent.EventValue = eventValue;
                    bookingEvent.EventValueSource = eventValueSource;
                }
            }

            bool isEntirePaymentMembership =
                IsInitialMembershipBooking(bookingRows);
            bool hasMembershipComponent =
                TryGetMixedMembershipValue(
                    bookingRows,
                    out _);

            List<PaymentEntity> relatedPayments = await dbContext.Payments
                .Where(payment =>
                    payment.BookingReference == bookingReference)
                .ToListAsync(cancellationToken);

            foreach (PaymentEntity payment in relatedPayments)
            {
                payment.BookingId = booking.Id;
                payment.Booking = booking;
                payment.PaymentCategory =
                    PaymentClassification.DetermineCategory(
                        payment.PaymentMethod,
                        hasLinkedBooking: true,
                        isEntirePaymentMembership:
                            isEntirePaymentMembership,
                        hasMembershipComponent:
                            hasMembershipComponent);
            }
        }

        private static string? NormalizeOptionalValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static bool? DetermineGuardianBooking(IReadOnlyList<BookwhenEventCsvRow> bookingRows)
        {
            bool hasMissingContactName = false;

            foreach (BookwhenEventCsvRow row in bookingRows)
            {
                string? contactName = NormalizeOptionalValue(row.ContactName);

                if (contactName is null)
                {
                    hasMissingContactName = true;
                    continue;
                }

                if (!string.Equals(
                        contactName,
                        row.AttendeeCustomerName.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return hasMissingContactName ? null : false;
        }

        private static (decimal? Value, EventValueSource Source) DetermineEventValue(BookwhenEventCsvRow row, IReadOnlyList<BookwhenEventCsvRow> bookingRows)
        {
            if (row.CurrentAttendanceValue.HasValue)
            {
                return (
                    row.CurrentAttendanceValue.Value,
                    EventValueSource.BookwhenAttendanceValue);
            }

            if (IsInitialMembershipBooking(bookingRows))
            {
                return (
                    0m,
                    EventValueSource.InitialMembershipPayment);
            }

            if (TryGetMixedMembershipValue(
                    bookingRows,
                    out decimal mixedMembershipValue) &&
                IsMixedMembershipRow(row))
            {
                return (
                    mixedMembershipValue,
                    EventValueSource.InitialMembershipPayment);
            }

            if (IsMembershipIncludedRecreationalSession(
                    row,
                    bookingRows))
            {
                return (0m, EventValueSource.MembershipIncluded);
            }

            int distinctEventCount = bookingRows
                .Select(bookingRow => bookingRow.EventId)
                .Distinct(StringComparer.Ordinal)
                .Count();

            if (bookingRows.Count == 1 &&
                distinctEventCount == 1 &&
                row.BookingPayment.HasValue)
            {
                return (
                    row.BookingPayment.Value,
                    EventValueSource.SingleEventBookingPayment);
            }

            return (null, EventValueSource.Unresolved);
        }

        private static bool IsInitialMembershipBooking(
            IReadOnlyList<BookwhenEventCsvRow> bookingRows)
        {
            bool hasPositiveBookingPayment = bookingRows.Any(
                row => row.BookingPayment is > 0m);

            bool hasNoTicketOrAttendanceValues = bookingRows.All(
                row =>
                    !row.TicketFaceValue.HasValue &&
                    !row.TicketCurrentValue.HasValue &&
                    !row.CurrentAttendanceValue.HasValue);

            return hasPositiveBookingPayment &&
                   hasNoTicketOrAttendanceValues;
        }

        private static bool TryGetMixedMembershipValue(
            IReadOnlyList<BookwhenEventCsvRow> bookingRows,
            out decimal membershipValue)
        {
            membershipValue = 0m;

            List<BookwhenEventCsvRow> membershipRows = bookingRows
                .Where(IsMixedMembershipRow)
                .ToList();

            if (membershipRows.Count != 1 ||
                !bookingRows.Any(row =>
                    row.CurrentAttendanceValue.HasValue))
            {
                return false;
            }

            decimal? bookingPayment = bookingRows
                .Select(row => row.BookingPayment)
                .FirstOrDefault(value => value.HasValue);

            if (!bookingPayment.HasValue)
            {
                return false;
            }

            decimal knownValue = bookingRows
                .Where(row =>
                    row.CurrentAttendanceValue.HasValue)
                .Sum(row =>
                    row.CurrentAttendanceValue!.Value);

            membershipValue = bookingPayment.Value - knownValue;

            return membershipValue > 0m;
        }

        private static bool IsMixedMembershipRow(
            BookwhenEventCsvRow row)
        {
            return !row.TicketFaceValue.HasValue &&
                   !row.TicketCurrentValue.HasValue &&
                   !row.CurrentAttendanceValue.HasValue &&
                   row.TicketName.Contains(
                       "League player",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsMembershipIncludedRecreationalSession(
            BookwhenEventCsvRow row,
            IReadOnlyList<BookwhenEventCsvRow> bookingRows)
        {
            bool hasNoBookingPayment = bookingRows.All(
                bookingRow =>
                    !bookingRow.BookingPayment.HasValue ||
                    bookingRow.BookingPayment.Value == 0m);

            return hasNoBookingPayment &&
                   row.EventTitle.Contains(
                       "Recreational",
                       StringComparison.OrdinalIgnoreCase) &&
                   row.TicketName.Contains(
                       "League Player",
                       StringComparison.OrdinalIgnoreCase);
        }
    }
}