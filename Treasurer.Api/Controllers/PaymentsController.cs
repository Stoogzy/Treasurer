using Microsoft.AspNetCore.Mvc;
using Treasurer.Api.Extensions;
using Treasurer.Application.Common.AppRequests;
using Treasurer.Application.Payments.ListMembershipPayments;

namespace Treasurer.Api.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController(
    IRequestHandler<ListAllMembershipPaymentsQuery, ListAllMembershipPaymentsQuery.Result> handler)
    : ControllerBase
{
    [HttpGet("memberships")]
    [ProducesResponseType<ListAllMembershipPaymentsQuery.Result>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListMembershipPayments(CancellationToken cancellationToken)
    {
        ListAllMembershipPaymentsQuery query = new();

        AppResponse<ListAllMembershipPaymentsQuery.Result> response = await handler.Handle(query, cancellationToken);

        return response.ToActionResult();
    }
}
