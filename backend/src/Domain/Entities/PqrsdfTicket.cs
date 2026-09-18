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
        TicketStatus status) : base(id)
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
}
