using Microsoft.AspNetCore.Mvc;
using Pqrsdf.Api.Shared.Controllers;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.GetActiveDestinationAreas;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.MakePqrsdf;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Api.Controllers.V1;

/// <summary>
/// Controller for public citizen PQRSDF ticket filing operations and catalogs.
/// </summary>
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
}
