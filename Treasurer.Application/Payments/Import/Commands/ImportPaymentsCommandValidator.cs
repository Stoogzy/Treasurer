using FluentValidation;

namespace Treasurer.Application.Payments.Import.Commands;

internal sealed class ImportPaymentsCommandValidator : AbstractValidator<ImportPaymentsCommand>
{
    public ImportPaymentsCommandValidator()
    {
        RuleFor(x => x.CsvStream)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(stream => stream.CanRead)
            .WithMessage("The CSV stream must be readable.");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .Must(fileName =>
                string.Equals(
                    Path.GetExtension(fileName),
                    ".csv",
                    StringComparison.OrdinalIgnoreCase))
            .WithMessage("The imported file must be a CSV file.");
    }
}
