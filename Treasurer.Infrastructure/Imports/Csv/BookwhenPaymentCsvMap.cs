using CsvHelper.Configuration;
using Treasurer.Application.Common.Imports.Models;

namespace Treasurer.Infrastructure.Imports.Csv;

internal sealed class BookwhenPaymentCsvMap
    : ClassMap<BookwhenPaymentCsvRow>
{
    public BookwhenPaymentCsvMap()
    {
        Map(x => x.PaymentDate)
            .Name("Date")
            .TypeConverterOption.Format(
                "yyyy-MM-ddTHH:mm:sszzz",
                "yyyy-MM-dd HH:mm:ss zzz",
                "dd/MM/yyyy",
                "dd/MM/yyyy HH:mm",
                "dd/MM/yyyy HH:mm:ss",
                "yyyy-MM-dd",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss");

        Map(x => x.BookingReference)
            .Name("Reference");

        Map(x => x.Status)
            .Name("Status");

        Map(x => x.PaymentMethod)
            .Name("Payment method");

        Map(x => x.PaymentValue)
            .Name("Value");

        Map(x => x.CustomerName)
            .Name("Customer");

        Map(x => x.PayerEmail)
            .Name("Payer email");
    }
}
