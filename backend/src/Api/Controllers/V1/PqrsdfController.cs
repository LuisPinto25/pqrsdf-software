using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pqrsdf.Api.Shared.Controllers;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.GetActiveDestinationAreas;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.GetTicketByRadicado;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.MakePqrsdf;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Api.Controllers.V1;

/// <summary>
/// Controller for public citizen PQRSDF ticket filing operations and catalogs.
/// </summary>
[AllowAnonymous]
public class PqrsdfController : ApiControllerBase
{
    /// <summary>
    /// Retrieves active organizational destination areas eligible for receiving PQRSDF filings.
    /// </summary>
    [HttpGet("areas")]
    [ProducesResponseType(typeof(Result<IReadOnlyList<DestinationAreaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActiveAreas(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetActiveDestinationAreasQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Submits a new citizen PQRSDF ticket publicly without requiring authentication.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Result<MakePqrsdfResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> MakePqrsdf(
        [FromBody] MakePqrsdfRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new MakePqrsdfCommand(request), cancellationToken);
        if (result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status201Created, result);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves public operational status, timeline, and response of a PQRSDF ticket by radicado number.
    /// Excludes citizen applicant personal data. Protected by rate limiting (30 req/min/IP).
    /// </summary>
    [HttpGet("{radicado}")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("PublicTrackingPolicy")]
    [ProducesResponseType(typeof(Result<PublicTicketStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetTicketByRadicado(
        [FromRoute] string radicado,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetTicketByRadicadoQuery(radicado), cancellationToken);
        return HandleResult(result);
    }
}
