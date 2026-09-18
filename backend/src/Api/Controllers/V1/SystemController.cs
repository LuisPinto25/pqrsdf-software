using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pqrsdf.Api.Shared.Controllers;
using Pqrsdf.Application.Features.System.UseCases.GetSystemStatus;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Api.Controllers.V1;

/// <summary>
/// Controller providing operational and system status endpoints.
/// </summary>
[AllowAnonymous]
public class SystemController : ApiControllerBase
{
    /// <summary>
    /// Obtains the operational status of the platform and database connection.
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(Result<SystemStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetSystemStatusQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Simulates an unhandled exception to verify RFC 7807 ProblemDetails interception.
    /// </summary>
    [HttpGet("simulate-error")]
    public IActionResult SimulateError()
    {
        throw new InvalidOperationException("Simulated catastrophic crash to test GlobalExceptionHandler.");
    }
}
