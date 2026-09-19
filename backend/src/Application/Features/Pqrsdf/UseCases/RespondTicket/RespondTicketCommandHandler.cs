using MediatR;
using Pqrsdf.Domain.Constants;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.Shared.Results;
using Pqrsdf.Domain.ValueObjects;

namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.RespondTicket;

public sealed class RespondTicketCommandHandler 
    : IRequestHandler<RespondTicketCommand, Result<RespondTicketResponse>>
{
    private readonly IPqrsdfTicketRepository _ticketRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITicketStatusHistoryRepository _statusHistoryRepository;

    public RespondTicketCommandHandler(
        IPqrsdfTicketRepository ticketRepository,
        IUserRepository userRepository,
        ITicketStatusHistoryRepository statusHistoryRepository)
    {
        _ticketRepository = ticketRepository;
        _userRepository = userRepository;
        _statusHistoryRepository = statusHistoryRepository;
    }

    public async Task<Result<RespondTicketResponse>> Handle(
        RespondTicketCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Radicado))
        {
            return Result<RespondTicketResponse>.Failure(Error.Validation(
                "PqrsdfTicket.InvalidRadicado",
                "El número de radicado es requerido."));
        }

        var radicadoResult = RadicadoNumber.Create(request.Radicado.Trim());
        if (radicadoResult.IsFailure)
        {
            return Result<RespondTicketResponse>.Failure(radicadoResult.Error);
        }

        var ticket = await _ticketRepository.GetByRadicadoAsync(radicadoResult.Value, cancellationToken);
        if (ticket is null)
        {
            return Result<RespondTicketResponse>.Failure(Error.NotFound(
                "PqrsdfTicket.NotFound",
                DomainMessages.TicketManagement.NotFound));
        }

        // Authorization check: Must be assigned official or Administrator
        bool isAssigned = ticket.AssignedToUserId.HasValue && ticket.AssignedToUserId.Value == request.CurrentUserId;
        bool isAdmin = string.Equals(request.CurrentUserRole, "Administrador", StringComparison.OrdinalIgnoreCase);

        if (!isAssigned && !isAdmin)
        {
            return Result<RespondTicketResponse>.Failure(Error.Unauthorized(
                "PqrsdfTicket.Forbidden",
                DomainMessages.TicketManagement.Forbidden));
        }

        var currentUser = await _userRepository.GetByIdAsync(request.CurrentUserId, cancellationToken);
        string officialName = currentUser?.FullName ?? "Funcionario Institucional";

        var now = DateTime.UtcNow;
        var previousStatus = ticket.Status;

        // Domain method execution
        var closeResult = ticket.CloseWithResponse(request.ResponseText, now);
        if (closeResult.IsFailure)
        {
            return Result<RespondTicketResponse>.Failure(closeResult.Error);
        }

        // Create and persist immutable audit record
        var auditRecord = TicketStatusHistory.Create(
            Guid.NewGuid(),
            ticket.Id,
            previousStatus,
            TicketStatus.Closed,
            request.CurrentUserId,
            "Respuesta final institucional entregada al ciudadano. Radicado cerrado.",
            now);

        if (auditRecord.IsFailure)
        {
            return Result<RespondTicketResponse>.Failure(auditRecord.Error);
        }

        await _statusHistoryRepository.AddAsync(auditRecord.Value, cancellationToken);
        await _ticketRepository.UpdateAsync(ticket, cancellationToken);

        var responseDto = new RespondTicketResponse(
            ticket.RadicadoNumber.Value,
            (int)ticket.Status,
            "Cerrado",
            now,
            officialName,
            DomainMessages.TicketManagement.ResponseSuccess
        );

        return Result<RespondTicketResponse>.Success(responseDto);
    }
}
