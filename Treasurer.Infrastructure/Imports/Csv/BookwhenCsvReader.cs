using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;
using Treasurer.Application.Common.Imports.Exceptions;
using Treasurer.Application.Common.Imports.Models;
using Treasurer.Application.Common.Interfaces;

namespace Treasurer.Infrastructure.Imports.Csv;

internal sealed class BookwhenCsvReader : IBookwhenCsvReader
{
    private static readonly CultureInfo CsvCulture = CultureInfo.GetCultureInfo("en-GB");

    public Task<IReadOnlyList<BookwhenEventCsvRow>> ReadEventsAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        return ReadAsync<BookwhenEventCsvRow, BookwhenEventCsvMap>(stream, cancellationToken);
    }

    public Task<IReadOnlyList<BookwhenPaymentCsvRow>> ReadPaymentsAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        return ReadAsync<BookwhenPaymentCsvRow, BookwhenPaymentCsvMap>(stream, cancellationToken);
    }

    private static async Task<IReadOnlyList<TRow>> ReadAsync<TRow, TMap>(Stream stream,CancellationToken cancellationToken)
        where TMap : ClassMap<TRow>, new()
    {
        ArgumentNullException.ThrowIfNull(stream);

        CsvConfiguration configuration = new (CsvCulture)
        {
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim,
            DetectDelimiter = true,

            // CsvHelper will throw for missing or invalid headers.
            // HeaderValidated = null,
            // MissingFieldFound = null
        };

        using StreamReader textReader = new (
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            bufferSize: 4096,
            leaveOpen: true);

        using CsvReader csv = new (textReader, configuration);

        csv.Context.RegisterClassMap<TMap>();

        List<TRow> rows = new();

        try
        {
            await foreach (TRow? row in csv
                    .GetRecordsAsync<TRow>(cancellationToken)
                    .ConfigureAwait(false))
            {
                rows.Add(row);
            }
        }
        catch (CsvHelperException exception)
        {
            int? rowNumber = exception.Context?.Parser?.Row;

            throw new CsvImportException($"The CSV could not be read at row {rowNumber}.", rowNumber, exception);
        }

        return rows;
    }
}
