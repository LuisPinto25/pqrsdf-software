using MediatR;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.ChangeTicketStatus;

public sealed record ChangeTicketStatusRequest(
    int NewStatus,
    string Justification);

public sealed record ChangeTicketStatusResponse(
    string RadicadoNumber,
    int PreviousStatus,
    int NewStatus,
    string StatusName,
    DateTime UpdatedAtUtc,
    string ChangedByOfficialName,
    string Justification);

public sealed record ChangeTicketStatusCommand(
    string Radicado,
    int NewStatus,
    string Justification,
    Guid CurrentUserId,
    string CurrentUserRole) : IRequest<Result<ChangeTicketStatusResponse>>;
