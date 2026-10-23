using Microsoft.AspNetCore.Mvc;
using Treasurer.Api.Extensions;
using Treasurer.Application.Common.AppRequests;
using Treasurer.Application.Payments.ListMembershipPayments;
using Treasurer.Application.Payments.ListMonthlyOverview;
using Treasurer.Application.Payments.ListUnresolvedBookingPayments;

namespace Treasurer.Api.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController(
    IRequestHandler<ListAllMembershipPaymentsQuery, ListAllMembershipPaymentsQuery.Result> listMembershipPaymentsHandler,
    IRequestHandler<ListMonthlyOverviewQuery, ListMonthlyOverviewQuery.Result> listMonthlyOverviewHandler,
    IRequestHandler<ListUnresolvedBookingPaymentsQuery, ListUnresolvedBookingPaymentsQuery.Result> listUnresolvedBookingPaymentsHandler)
    : ControllerBase
{
    [HttpGet("memberships")]
    [ProducesResponseType<ListAllMembershipPaymentsQuery.Result>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListMembershipPayments(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        CancellationToken cancellationToken)
    {
        ListAllMembershipPaymentsQuery query = new(startDate, endDate);

        AppResponse<ListAllMembershipPaymentsQuery.Result> response = await listMembershipPaymentsHandler.Handle(query, cancellationToken);

        return response.ToActionResult();
    }

    [HttpGet("overview")]
    [ProducesResponseType<ListMonthlyOverviewQuery.Result>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListMonthlyOverview(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        CancellationToken cancellationToken)
    {
        ListMonthlyOverviewQuery query = new(startDate, endDate);

        AppResponse<ListMonthlyOverviewQuery.Result> response = await listMonthlyOverviewHandler.Handle(query, cancellationToken);

        return response.ToActionResult();
    }

    [HttpGet("unresolved-bookings")]
    [ProducesResponseType<ListUnresolvedBookingPaymentsQuery.Result>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListUnresolvedBookingPayments(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        CancellationToken cancellationToken)
    {
        ListUnresolvedBookingPaymentsQuery query =
            new(startDate, endDate);

        AppResponse<ListUnresolvedBookingPaymentsQuery.Result> response = await listUnresolvedBookingPaymentsHandler.Handle(query, cancellationToken);

        return response.ToActionResult();
    }
}
