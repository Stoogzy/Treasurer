using Microsoft.AspNetCore.Mvc;
using Treasurer.Application.Common.AppRequests;

namespace Treasurer.Api.Extensions;

public static class AppResponseExtensions
{
    public static IActionResult ToActionResult(this AppResponse response) =>
        response.StatusCode switch
        {
            200 => new OkResult(),
            201 => new CreatedResult(string.Empty, response),
            204 => new NoContentResult(),
            400 => response.Errors is { Count: > 0 }
                ? new BadRequestObjectResult(new ValidationProblemDetails(response.Errors))
                : new BadRequestObjectResult(response.Message),
            401 => new UnauthorizedResult(),
            403 => new ObjectResult(null) { StatusCode = StatusCodes.Status403Forbidden },
            404 => new NotFoundResult(),
            409 => new ConflictObjectResult(response.Message),
            _ => new ObjectResult(response.Message) { StatusCode = response.StatusCode },
        };

    public static IActionResult ToActionResult<T>(this AppResponse<T> response) =>
        response.StatusCode switch
        {
            200 => new OkObjectResult(response.Content),
            201 => new CreatedResult(string.Empty, response.Content),
            204 => new NoContentResult(),
            400 => response.Errors is { Count: > 0 }
                ? new BadRequestObjectResult(new ValidationProblemDetails(response.Errors))
                : new BadRequestObjectResult(response.Message),
            401 => new UnauthorizedResult(),
            403 => new ObjectResult(null) { StatusCode = StatusCodes.Status403Forbidden },
            404 => new NotFoundResult(),
            409 => new ConflictObjectResult(response.Message),
            _ => new ObjectResult(response.Message) { StatusCode = response.StatusCode },
        };
}
