using MediatR;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.GetTicketManagementDetail;

/// <summary>
/// Query to retrieve full operational and citizen details of an assigned ticket for internal management.
/// </summary>
public sealed record GetTicketManagementDetailQuery(
    string Radicado,
    Guid CurrentUserId,
    string CurrentUserRole) : IRequest<Result<TicketManagementDetailDto>>;

public sealed record ApplicantDetailDto(
    string DocumentType,
    string DocumentNumber,
    string FullName,
    string Email,
    string? Phone);

public sealed record TicketStatusHistoryItemDto(
    Guid Id,
    int PreviousStatus,
    string PreviousStatusName,
    int NewStatus,
    string NewStatusName,
    string ChangedByOfficialName,
    string Justification,
    DateTime ChangedAtUtc);

public sealed record TicketManagementDetailDto(
    Guid Id,
    string RadicadoNumber,
    int Type,
    string TypeName,
    Guid DestinationAreaId,
    string DestinationAreaName,
    bool IsAnonymous,
    ApplicantDetailDto? Applicant,
    string Subject,
    string Description,
    DateTime FilingDateUtc,
    DateTime DueDateUtc,
    int? RemainingBusinessDays,
    string UrgencyLevel,
    int Status,
    string StatusName,
    Guid? AssignedToUserId,
    string? AssignedOfficialName,
    DateTime? AssignedAtUtc,
    string? AssignmentNote,
    string? ResponseText,
    DateTime? ResponseDateUtc,
    bool IsAssignedToCurrentUser,
    bool CanManage,
    IReadOnlyList<TicketStatusHistoryItemDto> StatusHistory);
