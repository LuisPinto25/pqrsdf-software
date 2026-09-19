using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pqrsdf.Api.Shared.Controllers;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.GetTicketManagementDetail;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Api.Controllers.V1;

/// <summary>
/// Controller for operational management, status transitions, and final response emission by authorized staff.
/// </summary>
[Authorize(Roles = "Funcionario,Administrador")]
[Route("api/v1/tickets")]
public class TicketsManagementController : ApiControllerBase
{
    /// <summary>
    /// Retrieves full operational and citizen details of an assigned ticket for internal management.
    /// </summary>
    [HttpGet("{radicado}/management-detail")]
    [ProducesResponseType(typeof(Result<TicketManagementDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetManagementDetail(
        [FromRoute] string radicado,
        CancellationToken cancellationToken)
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var currentUserId))
        {
            return Unauthorized();
        }

        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        var query = new GetTicketManagementDetailQuery(radicado, currentUserId, roleClaim);
        var result = await Sender.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Registers the final institutional response and closes an assigned PQRSDF ticket.
    /// </summary>
    [HttpPost("{radicado}/response")]
    [ProducesResponseType(typeof(Result<Pqrsdf.Application.Features.Pqrsdf.UseCases.RespondTicket.RespondTicketResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RespondTicket(
        [FromRoute] string radicado,
        [FromBody] Pqrsdf.Application.Features.Pqrsdf.UseCases.RespondTicket.RespondTicketRequest request,
        CancellationToken cancellationToken)
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var currentUserId))
        {
            return Unauthorized();
        }

        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        var command = new Pqrsdf.Application.Features.Pqrsdf.UseCases.RespondTicket.RespondTicketCommand(
            radicado,
            request.ResponseText,
            currentUserId,
            roleClaim);

        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Changes the operational status of an assigned PQRSDF ticket with mandatory justification logged to audit history.
    /// </summary>
    [HttpPost("{radicado}/status")]
    [ProducesResponseType(typeof(Result<Pqrsdf.Application.Features.Pqrsdf.UseCases.ChangeTicketStatus.ChangeTicketStatusResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeStatus(
        [FromRoute] string radicado,
        [FromBody] Pqrsdf.Application.Features.Pqrsdf.UseCases.ChangeTicketStatus.ChangeTicketStatusRequest request,
        CancellationToken cancellationToken)
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var currentUserId))
        {
            return Unauthorized();
        }

        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        var command = new Pqrsdf.Application.Features.Pqrsdf.UseCases.ChangeTicketStatus.ChangeTicketStatusCommand(
            radicado,
            request.NewStatus,
            request.Justification,
            currentUserId,
            roleClaim);

        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }
}

