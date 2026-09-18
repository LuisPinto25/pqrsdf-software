using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Shared.Entities;
using Pqrsdf.Domain.Shared.Results;
using Pqrsdf.Domain.ValueObjects;

namespace Pqrsdf.Domain.Entities;

/// <summary>
/// Aggregate root representing a citizen PQRSDF filing ticket.
/// </summary>
public sealed class PqrsdfTicket : Entity<Guid>, IAggregateRoot
{
    public RadicadoNumber RadicadoNumber { get; private set; }
    public PqrsdfType Type { get; private set; }
    public Guid DestinationAreaId { get; private set; }
    public bool IsAnonymous { get; private set; }
    public Applicant? Applicant { get; private set; }
    public string Subject { get; private set; }
    public string Description { get; private set; }
    public DueDate DueDate { get; private set; }
    public TicketStatus Status { get; private set; }
    public string? ResponseText { get; private set; }
    public DateTime? ResponseDateUtc { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public DateTime? AssignedAtUtc { get; private set; }
    public string? AssignmentNote { get; private set; }

    // Required for EF Core
    private PqrsdfTicket()
    {
        RadicadoNumber = default!;
        Subject = string.Empty;
        Description = string.Empty;
        DueDate = default!;
    }

    private PqrsdfTicket(
        Guid id,
        RadicadoNumber radicadoNumber,
        PqrsdfType type,
        Guid destinationAreaId,
        bool isAnonymous,
        Applicant? applicant,
        string subject,
        string description,
        DueDate dueDate,
        TicketStatus status,
        string? responseText = null,
        DateTime? responseDateUtc = null,
        Guid? assignedToUserId = null,
        DateTime? assignedAtUtc = null,
        string? assignmentNote = null) : base(id)
    {
        RadicadoNumber = radicadoNumber;
        Type = type;
        DestinationAreaId = destinationAreaId;
        IsAnonymous = isAnonymous;
        Applicant = applicant;
        Subject = subject;
        Description = description;
        DueDate = dueDate;
        Status = status;
        ResponseText = responseText;
        ResponseDateUtc = responseDateUtc;
        AssignedToUserId = assignedToUserId;
        AssignedAtUtc = assignedAtUtc;
        AssignmentNote = assignmentNote;
    }

    public static Result<PqrsdfTicket> Create(
        Guid id,
        RadicadoNumber radicadoNumber,
        PqrsdfType type,
        Guid destinationAreaId,
        bool isAnonymous,
        Applicant? applicant,
        string? subject,
        string? description,
        DueDate dueDate)
    {
        if (id == Guid.Empty)
        {
            return Result<PqrsdfTicket>.Failure(Error.Validation(
                "PqrsdfTicket.InvalidId",
                "El identificador del radicado no puede estar vacío."));
        }

        if (destinationAreaId == Guid.Empty)
        {
            return Result<PqrsdfTicket>.Failure(Error.Validation(
                "PqrsdfTicket.InvalidDestinationAreaId",
                "Debe seleccionar un área de destino válida."));
        }

        if (isAnonymous && type is not (PqrsdfType.Denunciation or PqrsdfType.Suggestion))
        {
            return Result<PqrsdfTicket>.Failure(Error.Validation(
                "PqrsdfTicket.AnonymousNotAllowed",
                "La radicación anónima está permitida exclusivamente para Denuncias y Sugerencias."));
        }

        if (!isAnonymous && applicant is null)
        {
            return Result<PqrsdfTicket>.Failure(Error.Validation(
                "PqrsdfTicket.ApplicantRequired",
                "Los datos del solicitante son obligatorios para este tipo de solicitud."));
        }

        if (string.IsNullOrWhiteSpace(subject) || subject.Trim().Length < 5 || subject.Trim().Length > 150)
        {
            return Result<PqrsdfTicket>.Failure(Error.Validation(
                "PqrsdfTicket.InvalidSubject",
                "El asunto debe tener entre 5 y 150 caracteres."));
        }

        if (string.IsNullOrWhiteSpace(description) || description.Trim().Length < 10 || description.Trim().Length > 4000)
        {
            return Result<PqrsdfTicket>.Failure(Error.Validation(
                "PqrsdfTicket.InvalidDescription",
                "La descripción debe tener entre 10 y 4000 caracteres."));
        }

        var effectiveApplicant = isAnonymous ? null : applicant;

        return Result<PqrsdfTicket>.Success(new PqrsdfTicket(
            id,
            radicadoNumber,
            type,
            destinationAreaId,
            isAnonymous,
            effectiveApplicant,
            subject.Trim(),
            description.Trim(),
            dueDate,
            TicketStatus.Registered));
    }

    public Result CloseWithResponse(string? responseText, DateTime responseDateUtc)
    {
        if (string.IsNullOrWhiteSpace(responseText) || responseText.Trim().Length < 10 || responseText.Trim().Length > 4000)
        {
            return Result.Failure(Error.Validation(
                "PqrsdfTicket.InvalidResponseText",
                "El texto de la respuesta institucional debe tener entre 10 y 4000 caracteres."));
        }

        if (Status == TicketStatus.Closed)
        {
            return Result.Failure(Error.Conflict(
                "PqrsdfTicket.AlreadyClosed",
                "La solicitud ya se encuentra en estado cerrado."));
        }

        ResponseText = responseText.Trim();
        ResponseDateUtc = responseDateUtc;
        Status = TicketStatus.Closed;
        UpdatedAtUtc = responseDateUtc;

        return Result.Success();
    }

    public Result ChangeStatus(TicketStatus newStatus)
    {
        if (Status == TicketStatus.Closed)
        {
            return Result.Failure(Error.Conflict(
                "PqrsdfTicket.AlreadyClosed",
                "No se puede cambiar el estado de una solicitud cerrada."));
        }

        Status = newStatus;
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    public Result AssignToOfficial(
        Guid officialId,
        Guid adminId,
        string? assignmentNote,
        int currentOfficialActiveWorkload,
        DateTime utcNow)
    {
        if (Status != TicketStatus.Registered)
        {
            return Result.Failure(Error.Conflict(
                "PqrsdfTicket.NotEligibleForAssignment",
                "Solo se pueden asignar solicitudes que se encuentren en estado Registrado."));
        }

        if (officialId == Guid.Empty)
        {
            return Result.Failure(Error.Validation(
                "PqrsdfTicket.InvalidOfficialId",
                "Debe seleccionar un funcionario válido."));
        }

        if (currentOfficialActiveWorkload >= 5)
        {
            return Result.Failure(Error.Conflict(
                "PqrsdfTicket.OfficialWorkloadLimitReached",
                "El funcionario seleccionado ha alcanzado el límite máximo de 5 solicitudes asignadas activas."));
        }

        if (assignmentNote != null && assignmentNote.Trim().Length > 500)
        {
            return Result.Failure(Error.Validation(
                "PqrsdfTicket.AssignmentNoteTooLong",
                "La nota de asignación no puede exceder los 500 caracteres."));
        }

        AssignedToUserId = officialId;
        AssignedAtUtc = utcNow;
        AssignmentNote = string.IsNullOrWhiteSpace(assignmentNote) ? null : assignmentNote.Trim();
        Status = TicketStatus.InReview;
        UpdatedAtUtc = utcNow;

        return Result.Success();
    }

    public Result ReassignToOfficial(
        Guid newOfficialId,
        Guid adminId,
        string justification,
        int targetOfficialActiveWorkload,
        DateTime utcNow)
    {
        if (Status != TicketStatus.InReview)
        {
            return Result.Failure(Error.Conflict(
                "PqrsdfTicket.NotEligibleForReassignment",
                "Solo se pueden reasignar solicitudes que se encuentren en estado En Trámite."));
        }

        if (newOfficialId == Guid.Empty)
        {
            return Result.Failure(Error.Validation(
                "PqrsdfTicket.InvalidOfficialId",
                "Debe seleccionar un funcionario válido para el traslado."));
        }

        if (AssignedToUserId.HasValue && AssignedToUserId.Value == newOfficialId)
        {
            return Result.Failure(Error.Conflict(
                "PqrsdfTicket.SameOfficialReassignment",
                "La solicitud ya se encuentra asignada a este funcionario."));
        }

        if (string.IsNullOrWhiteSpace(justification) || justification.Trim().Length < 10)
        {
            return Result.Failure(Error.Validation(
                "PqrsdfTicket.JustificationTooShort",
                "Debe ingresar un motivo de reasignación de al menos 10 caracteres."));
        }

        if (justification.Trim().Length > 500)
        {
            return Result.Failure(Error.Validation(
                "PqrsdfTicket.JustificationTooLong",
                "El motivo de reasignación no puede exceder los 500 caracteres."));
        }

        if (targetOfficialActiveWorkload >= 5)
        {
            return Result.Failure(Error.Conflict(
                "PqrsdfTicket.OfficialWorkloadLimitReached",
                "El funcionario seleccionado para reasignación ha alcanzado el límite de 5 solicitudes activas."));
        }

        AssignedToUserId = newOfficialId;
        AssignedAtUtc = utcNow;
        AssignmentNote = justification.Trim();
        UpdatedAtUtc = utcNow;

        return Result.Success();
    }
}

