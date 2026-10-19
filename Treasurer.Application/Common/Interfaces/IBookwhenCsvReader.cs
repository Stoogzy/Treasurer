using Treasurer.Application.Common.Imports.Models;

namespace Treasurer.Application.Common.Interfaces;

public interface IBookwhenCsvReader
{
    Task<IReadOnlyList<BookwhenEventCsvRow>> ReadEventsAsync(
        Stream csvStream,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BookwhenPaymentCsvRow>> ReadPaymentsAsync(
        Stream csvStream,
        CancellationToken cancellationToken = default);
}
