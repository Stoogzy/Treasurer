# Treasurer

A local .NET 10 application for importing Bookwhen CSV exports and reporting
event and membership income for a social sports team.

## Projects

- `Treasurer.Api` - CSV imports and reporting endpoints.
- `Treasurer.Web` - Blazor Web App user interface.
- `Treasurer.AppHost` - Aspire orchestration for the API and web app.
- `Treasurer.Application`, `Treasurer.Core`, and `Treasurer.Infrastructure` -
  application logic, entities, CSV processing, and EF Core persistence.

The API continues to use SQL Server LocalDB through the `Sql` connection string
in `Treasurer.Api/appsettings.Development.json`.

## Run locally

Start the complete application through Aspire:

```powershell
dotnet run --project .\Treasurer.AppHost\Treasurer.AppHost.csproj
```

Open the web resource from the Aspire dashboard. The dashboard also provides
the API and web logs, traces, health state, and endpoint links.

The web application includes:

- Monthly dashboard.
- Events and payments CSV imports.
- Date-filtered payments by event.
- Date-filtered membership payments.
- Unresolved booking-allocation review.
