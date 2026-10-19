using Microsoft.AspNetCore.Mvc;
using Treasurer.Api.Extensions;
using Treasurer.Application.Common.AppRequests;
using Treasurer.Application.Events.ListAllEvents;
using Treasurer.Application.Payments.ListAllPaymentsByEvent;

namespace Treasurer.Api.Controllers;

[ApiController]
[Route("api/events")]
public sealed class EventsController(
    IRequestHandler<ListAllEventsQuery, ListAllEventsQuery.Result> listAllEventsHandler,
    IRequestHandler<ListAllPaymentsByEventQuery, ListAllPaymentsByEventQuery.Result> listPaymentsHandler)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ListAllEventsQuery.Result>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAllEvents(CancellationToken cancellationToken)
    {
        ListAllEventsQuery query = new();

        AppResponse<ListAllEventsQuery.Result> response = await listAllEventsHandler.Handle(query, cancellationToken);

        return response.ToActionResult();
    }

    [HttpGet("{eventId}/payments")]
    [ProducesResponseType<ListAllPaymentsByEventQuery.Result>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListPaymentsByEvent(
        [FromRoute] string eventId,
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        CancellationToken cancellationToken)
    {
        ListAllPaymentsByEventQuery query =
            new(eventId, startDate, endDate);

        AppResponse<ListAllPaymentsByEventQuery.Result> response = await listPaymentsHandler.Handle(query, cancellationToken);

        return response.ToActionResult();
    }
}
