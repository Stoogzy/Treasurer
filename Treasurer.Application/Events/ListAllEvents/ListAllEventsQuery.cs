using Microsoft.EntityFrameworkCore;
using Treasurer.Application.Common.AppRequests;
using Treasurer.Application.Common.Interfaces;

namespace Treasurer.Application.Events.ListAllEvents;

public sealed record ListAllEventsQuery
{
    public sealed record EventItem(
        string EventId,
        string EventName,
        string? EventCode,
        string? Location,
        DateTime? StartsAt,
        DateTime? EndsAt);

    public sealed record Result(IReadOnlyList<EventItem> Events);

    public sealed class Handler(IApplicationDbContext dbContext) : IRequestHandler<ListAllEventsQuery, Result>
    {
        public async Task<AppResponse<Result>> Handle(ListAllEventsQuery query, CancellationToken cancellationToken)
        {
            List<EventItem> events = await dbContext.Events
                .AsNoTracking()
                .OrderBy(x => x.StartsAt == null)
                .ThenBy(x => x.StartsAt)
                .ThenBy(x => x.EventName)
                .Select(x => new EventItem(
                    x.BookwhenEventId,
                    x.EventName,
                    x.EventCode,
                    x.Location,
                    x.StartsAt,
                    x.EndsAt))
                .ToListAsync(cancellationToken);

            return AppResponse<Result>.Ok(new Result(events));
        }
    }
}
