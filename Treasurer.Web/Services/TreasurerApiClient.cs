using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Treasurer.Web.Models;

namespace Treasurer.Web.Services;

public sealed class TreasurerApiClient(HttpClient httpClient)
{
    private const long MaxCsvSize = 20 * 1024 * 1024;

    public Task<EventsResult> GetEventsAsync(
        CancellationToken cancellationToken = default)
    {
        return GetAsync<EventsResult>(
            "api/events",
            cancellationToken);
    }

    public Task<EventPaymentsResult> GetEventPaymentsAsync(
        string eventId,
        CancellationToken cancellationToken = default)
    {
        string path =
            $"api/events/{Uri.EscapeDataString(eventId)}/payments";

        return GetAsync<EventPaymentsResult>(path, cancellationToken);
    }

    public Task<EventNamesResult> GetEventNamesAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        string path =
            "api/events/by-name" +
            $"?startDate={startDate:yyyy-MM-dd}" +
            $"&endDate={endDate:yyyy-MM-dd}";

        return GetAsync<EventNamesResult>(path, cancellationToken);
    }

    public Task<EventsByNameResult> GetEventsByNameAsync(
        string eventName,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        string path =
            "api/events/by-name/details" +
            $"?eventName={Uri.EscapeDataString(eventName)}" +
            $"&startDate={startDate:yyyy-MM-dd}" +
            $"&endDate={endDate:yyyy-MM-dd}";

        return GetAsync<EventsByNameResult>(path, cancellationToken);
    }

    public Task<MembershipPaymentsResult> GetMembershipPaymentsAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        string path =
            "api/payments/memberships" +
            $"?startDate={startDate:yyyy-MM-dd}" +
            $"&endDate={endDate:yyyy-MM-dd}";

        return GetAsync<MembershipPaymentsResult>(
            path,
            cancellationToken);
    }

    public Task<MonthlyOverviewResult> GetMonthlyOverviewAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        string path =
            "api/payments/overview" +
            $"?startDate={startDate:yyyy-MM-dd}" +
            $"&endDate={endDate:yyyy-MM-dd}";

        return GetAsync<MonthlyOverviewResult>(
            path,
            cancellationToken);
    }

    public Task<UnresolvedBookingsResult> GetUnresolvedBookingsAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        string path =
            "api/payments/unresolved-bookings" +
            $"?startDate={startDate:yyyy-MM-dd}" +
            $"&endDate={endDate:yyyy-MM-dd}";

        return GetAsync<UnresolvedBookingsResult>(
            path,
            cancellationToken);
    }

    public Task<ImportResult> ImportEventsAsync(
        IBrowserFile file,
        CancellationToken cancellationToken = default)
    {
        return ImportCsvAsync(
            "api/imports/events",
            file,
            cancellationToken);
    }

    public Task<ImportResult> ImportPaymentsAsync(
        IBrowserFile file,
        CancellationToken cancellationToken = default)
    {
        return ImportCsvAsync(
            "api/imports/payments",
            file,
            cancellationToken);
    }

    private async Task<T> GetAsync<T>(
        string path,
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage response =
            await httpClient.GetAsync(path, cancellationToken);

        return await ReadResponseAsync<T>(response, cancellationToken);
    }

    private async Task<ImportResult> ImportCsvAsync(
        string path,
        IBrowserFile file,
        CancellationToken cancellationToken)
    {
        await using Stream fileStream =
            file.OpenReadStream(MaxCsvSize, cancellationToken);
        using StreamContent fileContent = new(fileStream);
        fileContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(
                file.ContentType.Length > 0
                    ? file.ContentType
                    : "text/csv");

        using MultipartFormDataContent content = new();
        content.Add(fileContent, "File", file.Name);

        using HttpResponseMessage response =
            await httpClient.PostAsync(path, content, cancellationToken);

        return await ReadResponseAsync<ImportResult>(
            response,
            cancellationToken);
    }

    private static async Task<T> ReadResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            string details =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new HttpRequestException(
                string.IsNullOrWhiteSpace(details)
                    ? $"The API returned {(int)response.StatusCode}."
                    : details);
        }

        T? result = await response.Content.ReadFromJsonAsync<T>(
            cancellationToken);

        return result
            ?? throw new InvalidOperationException(
                "The API returned an empty response.");
    }
}
