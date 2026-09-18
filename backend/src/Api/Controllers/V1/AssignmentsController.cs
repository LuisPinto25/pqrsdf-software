using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pqrsdf.Api.Shared.Controllers;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.AssignTicket;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.GetAssignableOfficials;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.GetAssignedTickets;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.GetOfficialInbox;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.GetUnassignedTickets;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.ReassignTicket;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Api.Controllers.V1;

/// <summary>
/// Controller for administrative ticket assignment, reassignment, and official workload inbox operations.
/// </summary>
[Authorize]
[Route("api/v1/assignments")]
public class AssignmentsController : ApiControllerBase
{
    /// <summary>
    /// Retrieves unassigned PQRSDF tickets ordered by statutory due date, with optional type, area, and search filters.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpGet("unassigned")]
    [ProducesResponseType(typeof(Result<IReadOnlyList<UnassignedTicketDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUnassignedTickets(
        [FromQuery] int? type,
        [FromQuery] Guid? destinationAreaId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var query = new GetUnassignedTicketsQuery(type, destinationAreaId, search);
        var result = await Sender.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves active staff users with role Funcionario and their current active workload count.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpGet("officials")]
    [ProducesResponseType(typeof(Result<IReadOnlyList<AssignableOfficialDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAssignableOfficials(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetAssignableOfficialsQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Assigns a registered PQRSDF ticket to a selected active official.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpPost("{radicado}/assign")]
    [ProducesResponseType(typeof(Result<AssignTicketResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignTicket(
        [FromRoute] string radicado,
        [FromBody] AssignTicketRequest request,
        CancellationToken cancellationToken)
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(idClaim, out var adminId))
        {
            return Unauthorized();
        }

        var command = new AssignTicketCommand(radicado, request, adminId);
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves in-review tickets for administrative monitoring and reassignment.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpGet("assigned")]
    [ProducesResponseType(typeof(Result<IReadOnlyList<AssignedTicketDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAssignedTickets(
        [FromQuery] string? search,
        [FromQuery] Guid? officialId,
        CancellationToken cancellationToken)
    {
        var query = new GetAssignedTicketsQuery(search, officialId);
        var result = await Sender.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Reassigns an in-review PQRSDF ticket to another active official with mandatory justification.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpPost("{radicado}/reassign")]
    [ProducesResponseType(typeof(Result<ReassignTicketResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReassignTicket(
        [FromRoute] string radicado,
        [FromBody] ReassignTicketRequest request,
        CancellationToken cancellationToken)
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(idClaim, out var adminId))
        {
            return Unauthorized();
        }

        var command = new ReassignTicketCommand(radicado, request, adminId);
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves active assigned tickets (Status = InReview, max 5) for the authenticated staff official.
    /// </summary>
    [Authorize(Roles = "Funcionario,Administrador")]
    [HttpGet("my-inbox")]
    [HttpGet("inbox")]
    [ProducesResponseType(typeof(Result<OfficialInboxResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOfficialInbox(CancellationToken cancellationToken)
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(idClaim, out var officialId))
        {
            return Unauthorized();
        }

        var query = new GetOfficialInboxQuery(officialId);
        var result = await Sender.Send(query, cancellationToken);
        return HandleResult(result);
    }
}
