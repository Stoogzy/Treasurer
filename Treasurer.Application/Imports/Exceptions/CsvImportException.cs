namespace Treasurer.Application.Imports.Exceptions;

public sealed class CsvImportException : Exception
{
    public int? RowNumber { get; }

    public CsvImportException(
        string message,
        int? rowNumber = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        RowNumber = rowNumber;
    }
}
