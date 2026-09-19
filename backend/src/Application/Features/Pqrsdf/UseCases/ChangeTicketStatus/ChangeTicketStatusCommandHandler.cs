using MediatR;
using Pqrsdf.Domain.Constants;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.Shared.Results;
using Pqrsdf.Domain.ValueObjects;

namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.ChangeTicketStatus;

public sealed class ChangeTicketStatusCommandHandler
    : IRequestHandler<ChangeTicketStatusCommand, Result<ChangeTicketStatusResponse>>
{
    private readonly IPqrsdfTicketRepository _ticketRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITicketStatusHistoryRepository _statusHistoryRepository;

    public ChangeTicketStatusCommandHandler(
        IPqrsdfTicketRepository ticketRepository,
        IUserRepository userRepository,
        ITicketStatusHistoryRepository statusHistoryRepository)
    {
        _ticketRepository = ticketRepository;
        _userRepository = userRepository;
        _statusHistoryRepository = statusHistoryRepository;
    }

    public async Task<Result<ChangeTicketStatusResponse>> Handle(
        ChangeTicketStatusCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Radicado))
        {
            return Result<ChangeTicketStatusResponse>.Failure(Error.Validation(
                "PqrsdfTicket.InvalidRadicado",
                "El número de radicado es requerido."));
        }

        var radicadoResult = RadicadoNumber.Create(request.Radicado.Trim());
        if (radicadoResult.IsFailure)
        {
            return Result<ChangeTicketStatusResponse>.Failure(radicadoResult.Error);
        }

        var ticket = await _ticketRepository.GetByRadicadoAsync(radicadoResult.Value, cancellationToken);
        if (ticket is null)
        {
            return Result<ChangeTicketStatusResponse>.Failure(Error.NotFound(
                "PqrsdfTicket.NotFound",
                DomainMessages.TicketManagement.NotFound));
        }

        // Authorization check: Assigned official or Administrator
        bool isAssigned = ticket.AssignedToUserId.HasValue && ticket.AssignedToUserId.Value == request.CurrentUserId;
        bool isAdmin = string.Equals(request.CurrentUserRole, "Administrador", StringComparison.OrdinalIgnoreCase);

        if (!isAssigned && !isAdmin)
        {
            return Result<ChangeTicketStatusResponse>.Failure(Error.Unauthorized(
                "PqrsdfTicket.Forbidden",
                DomainMessages.TicketManagement.Forbidden));
        }

        // Conflict check: Already closed ticket
        if (ticket.Status == TicketStatus.Closed)
        {
            return Result<ChangeTicketStatusResponse>.Failure(Error.Conflict(
                "PqrsdfTicket.AlreadyClosed",
                DomainMessages.TicketManagement.AlreadyClosed));
        }

        // Status transition validation
        if (request.NewStatus == (int)TicketStatus.Closed)
        {
            return Result<ChangeTicketStatusResponse>.Failure(Error.Validation(
                "PqrsdfTicket.InvalidStatusTransition",
                "El estado Cerrado solo puede registrarse mediante la emisión de respuesta final institucional."));
        }

        if (!Enum.IsDefined(typeof(TicketStatus), request.NewStatus) || (TicketStatus)request.NewStatus == TicketStatus.Registered)
        {
            return Result<ChangeTicketStatusResponse>.Failure(Error.Validation(
                "PqrsdfTicket.InvalidStatus",
                "Estado operativo no válido."));
        }

        var targetStatus = (TicketStatus)request.NewStatus;
        var now = DateTime.UtcNow;
        var previousStatus = ticket.Status;

        // Create immutable audit history record (validates justification length 10-500)
        var auditRecordResult = TicketStatusHistory.Create(
            Guid.NewGuid(),
            ticket.Id,
            previousStatus,
            targetStatus,
            request.CurrentUserId,
            request.Justification,
            now);

        if (auditRecordResult.IsFailure)
        {
            return Result<ChangeTicketStatusResponse>.Failure(auditRecordResult.Error);
        }

        var changeResult = ticket.ChangeOperationalStatus(targetStatus, now);
        if (changeResult.IsFailure)
        {
            return Result<ChangeTicketStatusResponse>.Failure(changeResult.Error);
        }

        var currentUser = await _userRepository.GetByIdAsync(request.CurrentUserId, cancellationToken);
        string officialName = currentUser?.FullName ?? "Funcionario Institucional";

        await _statusHistoryRepository.AddAsync(auditRecordResult.Value, cancellationToken);
        await _ticketRepository.UpdateAsync(ticket, cancellationToken);

        var responseDto = new ChangeTicketStatusResponse(
            ticket.RadicadoNumber.Value,
            (int)previousStatus,
            (int)targetStatus,
            GetStatusName(targetStatus),
            now,
            officialName,
            request.Justification.Trim());

        return Result<ChangeTicketStatusResponse>.Success(responseDto);
    }

    private static string GetStatusName(TicketStatus status) => status switch
    {
        TicketStatus.Registered => "Registrado",
        TicketStatus.Assigned => "Asignado",
        TicketStatus.InReview => "En trámite",
        TicketStatus.Answered => "Respondido",
        TicketStatus.Closed => "Cerrado",
        _ => status.ToString()
    };
}
