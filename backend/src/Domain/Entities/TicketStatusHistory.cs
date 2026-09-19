using Pqrsdf.Domain.Constants;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Shared.Entities;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Domain.Entities;

/// <summary>
/// Immutable audit log entity recording ticket status transition events and mandatory justifications.
/// </summary>
public sealed class TicketStatusHistory : Entity<Guid>
{
    public Guid TicketId { get; private set; }
    public TicketStatus PreviousStatus { get; private set; }
    public TicketStatus NewStatus { get; private set; }
    public Guid ChangedByUserId { get; private set; }
    public string Justification { get; private set; }
    public DateTime ChangedAtUtc { get; private set; }

    // Navigation properties for EF Core
    public PqrsdfTicket Ticket { get; private set; } = default!;
    public User ChangedByUser { get; private set; } = default!;

    // Required for EF Core
    private TicketStatusHistory()
    {
        Justification = string.Empty;
    }

    private TicketStatusHistory(
        Guid id,
        Guid ticketId,
        TicketStatus previousStatus,
        TicketStatus newStatus,
        Guid changedByUserId,
        string justification,
        DateTime changedAtUtc) : base(id)
    {
        TicketId = ticketId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        ChangedByUserId = changedByUserId;
        Justification = justification;
        ChangedAtUtc = changedAtUtc;
        CreatedAtUtc = changedAtUtc;
    }

    public static Result<TicketStatusHistory> Create(
        Guid id,
        Guid ticketId,
        TicketStatus previousStatus,
        TicketStatus newStatus,
        Guid changedByUserId,
        string? justification,
        DateTime changedAtUtc)
    {
        if (id == Guid.Empty)
        {
            return Result<TicketStatusHistory>.Failure(Error.Validation(
                "TicketStatusHistory.InvalidId",
                "El identificador del registro de auditoría no puede estar vacío."));
        }

        if (ticketId == Guid.Empty)
        {
            return Result<TicketStatusHistory>.Failure(Error.Validation(
                "TicketStatusHistory.InvalidTicketId",
                "El identificador del radicado no puede estar vacío."));
        }

        if (changedByUserId == Guid.Empty)
        {
            return Result<TicketStatusHistory>.Failure(Error.Validation(
                "TicketStatusHistory.InvalidUserId",
                "El identificador del usuario responsable no puede estar vacío."));
        }

        if (string.IsNullOrWhiteSpace(justification) || justification.Trim().Length < 10)
        {
            return Result<TicketStatusHistory>.Failure(Error.Validation(
                "TicketStatusHistory.JustificationRequired",
                DomainMessages.TicketManagement.JustificationRequired));
        }

        if (justification.Trim().Length > 500)
        {
            return Result<TicketStatusHistory>.Failure(Error.Validation(
                "TicketStatusHistory.JustificationTooLong",
                DomainMessages.TicketManagement.JustificationTooLong));
        }

        return Result<TicketStatusHistory>.Success(new TicketStatusHistory(
            id,
            ticketId,
            previousStatus,
            newStatus,
            changedByUserId,
            justification.Trim(),
            changedAtUtc));
    }
}
