using MediatR;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.RespondTicket;

public sealed record RespondTicketRequest(string ResponseText);

public sealed record RespondTicketResponse(
    string RadicadoNumber,
    int Status,
    string StatusName,
    DateTime ResponseDateUtc,
    string ClosedByOfficialName,
    string Message);

public sealed record RespondTicketCommand(
    string Radicado,
    string ResponseText,
    Guid CurrentUserId,
    string CurrentUserRole) : IRequest<Result<RespondTicketResponse>>;
