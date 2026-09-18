using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Shared.Entities;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Domain.Entities;

/// <summary>
/// Immutable audit log entity recording ticket assignment and reassignment events.
/// </summary>
public sealed class TicketAssignmentHistory : Entity<Guid>
{
    public Guid TicketId { get; private set; }
    public Guid? PreviousAssignedUserId { get; private set; }
    public Guid NewAssignedUserId { get; private set; }
    public Guid AssignedByUserId { get; private set; }
    public DateTime AssignedAtUtc { get; private set; }
    public string? Note { get; private set; }
    public AssignmentType Type { get; private set; }

    // Navigation properties for EF Core
    public PqrsdfTicket Ticket { get; private set; } = default!;
    public User? PreviousAssignedUser { get; private set; }
    public User NewAssignedUser { get; private set; } = default!;
    public User AssignedByUser { get; private set; } = default!;

    // Required for EF Core
    private TicketAssignmentHistory() { }

    private TicketAssignmentHistory(
        Guid id,
        Guid ticketId,
        Guid? previousAssignedUserId,
        Guid newAssignedUserId,
        Guid assignedByUserId,
        DateTime assignedAtUtc,
        string? note,
        AssignmentType type) : base(id)
    {
        TicketId = ticketId;
        PreviousAssignedUserId = previousAssignedUserId;
        NewAssignedUserId = newAssignedUserId;
        AssignedByUserId = assignedByUserId;
        AssignedAtUtc = assignedAtUtc;
        Note = note;
        Type = type;
        CreatedAtUtc = assignedAtUtc;
    }

    public static Result<TicketAssignmentHistory> Create(
        Guid id,
        Guid ticketId,
        Guid? previousAssignedUserId,
        Guid newAssignedUserId,
        Guid assignedByUserId,
        DateTime assignedAtUtc,
        string? note,
        AssignmentType type)
    {
        if (id == Guid.Empty)
        {
            return Result<TicketAssignmentHistory>.Failure(Error.Validation(
                "TicketAssignmentHistory.InvalidId",
                "El identificador del registro de auditoría no puede estar vacío."));
        }

        if (ticketId == Guid.Empty)
        {
            return Result<TicketAssignmentHistory>.Failure(Error.Validation(
                "TicketAssignmentHistory.InvalidTicketId",
                "El identificador del radicado no puede estar vacío."));
        }

        if (newAssignedUserId == Guid.Empty)
        {
            return Result<TicketAssignmentHistory>.Failure(Error.Validation(
                "TicketAssignmentHistory.InvalidNewOfficialId",
                "El identificador del funcionario asignado no puede estar vacío."));
        }

        if (assignedByUserId == Guid.Empty)
        {
            return Result<TicketAssignmentHistory>.Failure(Error.Validation(
                "TicketAssignmentHistory.InvalidAdminId",
                "El identificador del administrador responsable no puede estar vacío."));
        }

        if (type == AssignmentType.Reassignment && (string.IsNullOrWhiteSpace(note) || note.Trim().Length < 10))
        {
            return Result<TicketAssignmentHistory>.Failure(Error.Validation(
                "TicketAssignmentHistory.JustificationRequired",
                "Debe ingresar un motivo de reasignación de al menos 10 caracteres."));
        }

        if (note != null && note.Trim().Length > 500)
        {
            return Result<TicketAssignmentHistory>.Failure(Error.Validation(
                "TicketAssignmentHistory.NoteTooLong",
                "La nota o justificación no puede exceder los 500 caracteres."));
        }

        return Result<TicketAssignmentHistory>.Success(new TicketAssignmentHistory(
            id,
            ticketId,
            previousAssignedUserId,
            newAssignedUserId,
            assignedByUserId,
            assignedAtUtc,
            note?.Trim(),
            type));
    }
}
