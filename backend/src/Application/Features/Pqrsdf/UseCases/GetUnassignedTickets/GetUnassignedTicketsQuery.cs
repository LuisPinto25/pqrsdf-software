using MediatR;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.GetUnassignedTickets;

public record GetUnassignedTicketsQuery(
    int? Type = null,
    Guid? DestinationAreaId = null,
    string? Search = null) : IRequest<Result<IReadOnlyList<UnassignedTicketDto>>>;

public record UnassignedTicketDto(
    Guid Id,
    string RadicadoNumber,
    int Type,
    string TypeName,
    Guid DestinationAreaId,
    string DestinationAreaName,
    string Subject,
    string Description,
    DateTime FilingDateUtc,
    DateTime DueDateUtc,
    int RemainingBusinessDays,
    string UrgencyLevel);
