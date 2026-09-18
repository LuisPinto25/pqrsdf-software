using MediatR;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.AssignTicket;

public record AssignTicketRequest(Guid OfficialId, string? Note);

public record AssignTicketCommand(
    string Radicado,
    AssignTicketRequest Request,
    Guid AdminId) : IRequest<Result<AssignTicketResponse>>;

public record AssignTicketResponse(
    string RadicadoNumber,
    string Status,
    Guid AssignedToUserId,
    string AssignedOfficialName,
    DateTime AssignedAtUtc,
    string Message);
