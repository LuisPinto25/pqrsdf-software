using MediatR;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.Shared.Results;
using Pqrsdf.Domain.ValueObjects;

namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.ReassignTicket;

public record ReassignTicketRequest(Guid NewOfficialId, string Justification);

public record ReassignTicketCommand(
    string Radicado,
    ReassignTicketRequest Request,
    Guid AdminId) : IRequest<Result<ReassignTicketResponse>>;

public record ReassignTicketResponse(
    string RadicadoNumber,
    Guid? PreviousAssignedUserId,
    Guid NewAssignedUserId,
    string NewOfficialName,
    DateTime ReassignedAtUtc,
    string Message);

public sealed class ReassignTicketCommandHandler
    : IRequestHandler<ReassignTicketCommand, Result<ReassignTicketResponse>>
{
    private readonly IPqrsdfTicketRepository _ticketRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITicketAssignmentHistoryRepository _historyRepository;

    public ReassignTicketCommandHandler(
        IPqrsdfTicketRepository ticketRepository,
        IUserRepository userRepository,
        ITicketAssignmentHistoryRepository historyRepository)
    {
        _ticketRepository = ticketRepository;
        _userRepository = userRepository;
        _historyRepository = historyRepository;
    }

    public async Task<Result<ReassignTicketResponse>> Handle(
        ReassignTicketCommand request,
        CancellationToken cancellationToken)
    {
        var radicadoResult = RadicadoNumber.Create(request.Radicado);
        if (radicadoResult.IsFailure)
        {
            return Result<ReassignTicketResponse>.Failure(radicadoResult.Error);
        }

        var ticket = await _ticketRepository.GetByRadicadoAsync(radicadoResult.Value, cancellationToken);
        if (ticket is null)
        {
            return Result<ReassignTicketResponse>.Failure(Error.NotFound(
                "PqrsdfTicket.NotFound",
                "No se encontró ninguna solicitud con el radicado ingresado."));
        }

        if (ticket.Status != TicketStatus.InReview)
        {
            return Result<ReassignTicketResponse>.Failure(Error.Conflict(
                "PqrsdfTicket.NotEligibleForReassignment",
                "Solo se pueden reasignar solicitudes que se encuentren en estado En Trámite."));
        }

        if (string.IsNullOrWhiteSpace(request.Request.Justification) || request.Request.Justification.Trim().Length < 10)
        {
            return Result<ReassignTicketResponse>.Failure(Error.Validation(
                "TicketAssignmentHistory.JustificationRequired",
                "Debe ingresar un motivo de reasignación de al menos 10 caracteres."));
        }

        if (request.Request.Justification.Trim().Length > 500)
        {
            return Result<ReassignTicketResponse>.Failure(Error.Validation(
                "TicketAssignmentHistory.JustificationTooLong",
                "El motivo de reasignación no puede exceder los 500 caracteres."));
        }

        var newOfficial = await _userRepository.GetByIdAsync(request.Request.NewOfficialId, cancellationToken);
        if (newOfficial is null || !newOfficial.IsActive || newOfficial.Role != UserRole.Funcionario)
        {
            return Result<ReassignTicketResponse>.Failure(Error.Validation(
                "AssignTicket.InvalidOfficial",
                "El funcionario seleccionado no existe o no se encuentra activo."));
        }

        if (ticket.AssignedToUserId.HasValue && ticket.AssignedToUserId.Value == newOfficial.Id)
        {
            return Result<ReassignTicketResponse>.Failure(Error.Conflict(
                "PqrsdfTicket.SameOfficialReassignment",
                "La solicitud ya se encuentra asignada a este funcionario."));
        }

        var targetWorkload = await _ticketRepository.CountActiveByOfficialIdAsync(newOfficial.Id, cancellationToken);
        if (targetWorkload >= 5)
        {
            return Result<ReassignTicketResponse>.Failure(Error.Conflict(
                "PqrsdfTicket.OfficialWorkloadLimitReached",
                "El funcionario seleccionado para reasignación ha alcanzado el límite de 5 solicitudes activas."));
        }

        var previousOfficialId = ticket.AssignedToUserId;
        var now = DateTime.UtcNow;

        var reassignResult = ticket.ReassignToOfficial(
            newOfficial.Id,
            request.AdminId,
            request.Request.Justification,
            targetWorkload,
            now);

        if (reassignResult.IsFailure)
        {
            return Result<ReassignTicketResponse>.Failure(reassignResult.Error);
        }

        var historyResult = TicketAssignmentHistory.Create(
            Guid.NewGuid(),
            ticket.Id,
            previousOfficialId,
            newOfficial.Id,
            request.AdminId,
            now,
            request.Request.Justification,
            AssignmentType.Reassignment);

        if (historyResult.IsFailure)
        {
            return Result<ReassignTicketResponse>.Failure(historyResult.Error);
        }

        await _historyRepository.AddAsync(historyResult.Value, cancellationToken);
        await _ticketRepository.UpdateAsync(ticket, cancellationToken);

        return Result<ReassignTicketResponse>.Success(new ReassignTicketResponse(
            ticket.RadicadoNumber.Value,
            previousOfficialId,
            newOfficial.Id,
            newOfficial.FullName,
            now,
            "Solicitud reasignada exitosamente."));
    }
}
