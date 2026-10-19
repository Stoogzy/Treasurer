using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Treasurer.Application.Common.AppRequests;
using Treasurer.Application.Common.Imports.Exceptions;
using Treasurer.Application.Common.Imports.Models;
using Treasurer.Application.Common.Interfaces;
using Treasurer.Core.Entities;

namespace Treasurer.Application.Payments.Import.Commands;

public sealed record ImportPaymentsCommand(
    Stream CsvStream,
    string FileName)
{
    public sealed record Result(int ImportedRowCount);

    public sealed class Handler(IBookwhenCsvReader csvReader, IApplicationDbContext dbContext, IValidator<BookwhenPaymentCsvRow> validator) : IRequestHandler<ImportPaymentsCommand, Result>
    {
        public async Task<AppResponse<Result>> Handle(ImportPaymentsCommand command, CancellationToken cancellationToken)
        {
            try
            {
                IReadOnlyList<BookwhenPaymentCsvRow> rows = await csvReader.ReadPaymentsAsync(command.CsvStream, cancellationToken);

                Dictionary<string, string[]>? errors = await ValidateRowsAsync(rows, cancellationToken);

                if (errors is not null)
                {
                    return new AppResponse<Result>(
                        statusCode: 400,
                        content: default,
                        message: "One or more CSV rows are invalid.",
                        errors: errors);
                }

                int importedRowCount = 0;

                foreach (BookwhenPaymentCsvRow row in rows)
                {
                    bool imported = await ImportRowAsync(row, cancellationToken);

                    if (imported)
                    {
                        importedRowCount++;
                    }
                }

                await dbContext.SaveChangesAsync(cancellationToken);

                return AppResponse<Result>.Ok(new Result(importedRowCount));
            }
            catch (CsvImportException exception)
            {
                return new AppResponse<Result>(statusCode: 400, message: exception.Message);
            }
        }

        private async Task<Dictionary<string, string[]>?> ValidateRowsAsync(IReadOnlyList<BookwhenPaymentCsvRow> rows, CancellationToken cancellationToken)
        {
            Dictionary<string, List<string>> errors = new();

            for (int index = 0; index < rows.Count; index++)
            {
                BookwhenPaymentCsvRow row = rows[index];

                ValidationResult result = await validator.ValidateAsync(row, cancellationToken);

                // The CSV header is row 1.
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

        private async Task<bool> ImportRowAsync(BookwhenPaymentCsvRow row, CancellationToken cancellationToken)
        {
            string bookingReference = row.BookingReference.Trim();
            string paymentStatus = row.Status.Trim();
            string paymentMethod = row.PaymentMethod.Trim();
            string? customerName = NormalizeOptionalValue(row.CustomerName);
            string? payerEmail = NormalizeOptionalValue(row.PayerEmail);

            bool alreadyTracked = dbContext.Payments.Local.Any(
                payment =>
                    payment.PaymentDate == row.PaymentDate &&
                    payment.BookingReference == bookingReference &&
                    payment.PaymentStatus == paymentStatus &&
                    payment.PaymentMethod == paymentMethod &&
                    payment.PaymentValue == row.PaymentValue &&
                    payment.PayerEmail == payerEmail);

            if (alreadyTracked)
            {
                return false;
            }

            bool alreadyPersisted = await dbContext.Payments.AnyAsync(
                payment =>
                    payment.PaymentDate == row.PaymentDate &&
                    payment.BookingReference == bookingReference &&
                    payment.PaymentStatus == paymentStatus &&
                    payment.PaymentMethod == paymentMethod &&
                    payment.PaymentValue == row.PaymentValue &&
                    payment.PayerEmail == payerEmail,
                cancellationToken);

            if (alreadyPersisted)
            {
                return false;
            }

            BookingEntity? booking =
                await dbContext.Bookings.SingleOrDefaultAsync(
                    existing =>
                        existing.BookingReference == bookingReference,
                    cancellationToken);

            PaymentEntity payment = new()
            {
                PaymentDate = row.PaymentDate,
                BookingReference = bookingReference,
                PaymentStatus = paymentStatus,
                PaymentMethod = paymentMethod,
                PaymentValue = row.PaymentValue,
                CustomerName = customerName,
                PayerEmail = payerEmail,
                BookingId = booking?.Id,
                Booking = booking
            };

            dbContext.Payments.Add(payment);

            return true;
        }

        private static string? NormalizeOptionalValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}