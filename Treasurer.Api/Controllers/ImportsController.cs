using Microsoft.AspNetCore.Mvc;
using Treasurer.Api.Contracts.Requests;
using Treasurer.Api.Extensions;
using Treasurer.Application.Common.AppRequests;
using Treasurer.Application.Events.Imports.Commands;
using Treasurer.Application.Payments.Import.Commands;

namespace Treasurer.Api.Controllers;

[ApiController]
[Route("api/imports")]
public sealed class ImportsController(
    IRequestHandler<ImportEventsCommand, ImportEventsCommand.Result> importEventsHandler, 
    IRequestHandler<ImportPaymentsCommand, ImportPaymentsCommand.Result> importPaymentsHandler) 
    : ControllerBase
{
    [HttpPost("events")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<ImportEventsCommand.Result>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportEvents(
        [FromForm] CsvUploadRequest request,
        CancellationToken cancellationToken)
    {
        if (request.File is null)
        {
            return BadRequest("An events CSV file is required.");
        }

        if (request.File.Length == 0)
        {
            return BadRequest("The events CSV file cannot be empty.");
        }

        await using Stream stream = request.File.OpenReadStream();

        ImportEventsCommand command = new(stream, request.File.FileName);

        AppResponse<ImportEventsCommand.Result> response = await importEventsHandler.Handle(command, cancellationToken);

        return response.ToActionResult();
    }

    [HttpPost("payments")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<ImportPaymentsCommand.Result>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportPayments(
        [FromForm] CsvUploadRequest request,
        CancellationToken cancellationToken)
    {
        if (request.File is null)
        {
            return BadRequest("A payments CSV file is required.");
        }

        if (request.File.Length == 0)
        {
            return BadRequest("The payments CSV file cannot be empty.");
        }

        await using Stream stream = request.File.OpenReadStream();

        ImportPaymentsCommand command = new(stream, request.File.FileName);

        AppResponse<ImportPaymentsCommand.Result> response = await importPaymentsHandler.Handle(command, cancellationToken);

        return response.ToActionResult();
    }
}
