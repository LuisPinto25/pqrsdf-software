namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.GetTicketByRadicado;

/// <summary>
/// Public read model representing ticket status and tracking information.
/// STRICT PRIVACY GUARANTEE: Excludes all citizen personal data (name, document, email, phone).
/// </summary>
public sealed record PublicTicketStatusDto(
    string RadicadoNumber,
    string RequestType,
    string DestinationAreaName,
    string Subject,
    string Description,
    string Status,
    DateOnly FilingDate,
    DateOnly DueDate,
    int? RemainingBusinessDays,
    bool IsOverdue,
    int? OverdueBusinessDays,
    IReadOnlyList<TicketTimelineMilestoneDto> Timeline,
    TicketResolutionDto? Resolution);

/// <summary>
/// Represents a single milestone in the ticket's operational lifecycle.
/// </summary>
public sealed record TicketTimelineMilestoneDto(
    string Status,
    string Title,
    DateTime? Date,
    bool IsCompleted,
    bool IsCurrent);

/// <summary>
/// Represents official response details when the ticket is answered or closed.
/// </summary>
public sealed record TicketResolutionDto(
    string ResponseText,
    DateTime ResponseDate);
