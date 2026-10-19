using Microsoft.EntityFrameworkCore;
using Treasurer.Application.Common.AppRequests;
using Treasurer.Application.Common.Interfaces;

namespace Treasurer.Application.Events.ListAllEvents;

public sealed record ListAllEventsQuery
{
    public sealed record EventItem(
        string EventId,
        string EventName);

    public sealed record Result(IReadOnlyList<EventItem> Events);

    public sealed class Handler(IApplicationDbContext dbContext) : IRequestHandler<ListAllEventsQuery, Result>
    {
        public async Task<AppResponse<Result>> Handle(ListAllEventsQuery query, CancellationToken cancellationToken)
        {
            List<EventItem> events = await dbContext.Events
                .AsNoTracking()
                .OrderBy(x => x.EventName)
                .ThenBy(x => x.StartsAt)
                .Select(x => new EventItem(
                    x.BookwhenEventId,
                    x.EventName))
                .ToListAsync(cancellationToken);

            return AppResponse<Result>.Ok(new Result(events));
        }
    }
}
