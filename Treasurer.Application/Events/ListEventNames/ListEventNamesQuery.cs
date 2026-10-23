using Microsoft.EntityFrameworkCore;
using Treasurer.Application.Common.AppRequests;
using Treasurer.Application.Common.Interfaces;

namespace Treasurer.Application.Events.ListEventNames;

public sealed record ListEventNamesQuery(
    DateOnly StartDate,
    DateOnly EndDate)
{
    public sealed record EventNameItem(
        string EventName,
        int OccurrenceCount,
        DateTime FirstOccurrence,
        DateTime LastOccurrence);

    public sealed record Result(
        DateOnly StartDate,
        DateOnly EndDate,
        IReadOnlyList<EventNameItem> EventNames);

    public sealed class Handler(IApplicationDbContext dbContext)
        : IRequestHandler<ListEventNamesQuery, Result>
    {
        public async Task<AppResponse<Result>> Handle(
            ListEventNamesQuery query,
            CancellationToken cancellationToken)
        {
            DateTime startDate =
                query.StartDate.ToDateTime(TimeOnly.MinValue);
            DateTime endDateExclusive = query.EndDate
                .AddDays(1)
                .ToDateTime(TimeOnly.MinValue);

            var eventRows = await dbContext.Events
                .AsNoTracking()
                .Where(eventEntity =>
                    eventEntity.StartsAt.HasValue &&
                    eventEntity.StartsAt >= startDate &&
                    eventEntity.StartsAt < endDateExclusive)
                .Select(eventEntity => new
                {
                    eventEntity.EventName,
                    StartsAt = eventEntity.StartsAt!.Value
                })
                .ToListAsync(cancellationToken);

            List<EventNameItem> eventNames = eventRows
                .GroupBy(eventEntity => eventEntity.EventName)
                .Select(group => new EventNameItem(
                    group.Key,
                    group.Count(),
                    group.Min(eventEntity =>
                        eventEntity.StartsAt),
                    group.Max(eventEntity =>
                        eventEntity.StartsAt)))
                .OrderBy(item => item.EventName)
                .ToList();

            return AppResponse<Result>.Ok(
                new Result(
                    query.StartDate,
                    query.EndDate,
                    eventNames));
        }
    }
}
