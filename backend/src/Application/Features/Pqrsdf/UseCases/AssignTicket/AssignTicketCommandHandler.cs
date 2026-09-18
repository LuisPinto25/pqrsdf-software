using MediatR;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.Shared.Results;
using Pqrsdf.Domain.ValueObjects;

namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.AssignTicket;

public sealed class AssignTicketCommandHandler
    : IRequestHandler<AssignTicketCommand, Result<AssignTicketResponse>>
{
    private readonly IPqrsdfTicketRepository _ticketRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITicketAssignmentHistoryRepository _historyRepository;

    public AssignTicketCommandHandler(
        IPqrsdfTicketRepository ticketRepository,
        IUserRepository userRepository,
        ITicketAssignmentHistoryRepository historyRepository)
    {
        _ticketRepository = ticketRepository;
        _userRepository = userRepository;
        _historyRepository = historyRepository;
    }

    public async Task<Result<AssignTicketResponse>> Handle(
        AssignTicketCommand request,
        CancellationToken cancellationToken)
    {
        var radicadoResult = RadicadoNumber.Create(request.Radicado);
        if (radicadoResult.IsFailure)
        {
            return Result<AssignTicketResponse>.Failure(radicadoResult.Error);
        }

        var ticket = await _ticketRepository.GetByRadicadoAsync(radicadoResult.Value, cancellationToken);
        if (ticket is null)
        {
            return Result<AssignTicketResponse>.Failure(Error.NotFound(
                "PqrsdfTicket.NotFound",
                "No se encontró ninguna solicitud con el radicado ingresado."));
        }

        var official = await _userRepository.GetByIdAsync(request.Request.OfficialId, cancellationToken);
        if (official is null || !official.IsActive || official.Role != UserRole.Funcionario)
        {
            return Result<AssignTicketResponse>.Failure(Error.Validation(
                "AssignTicket.InvalidOfficial",
                "El funcionario seleccionado no existe o no se encuentra activo."));
        }

        var activeWorkload = await _ticketRepository.CountActiveByOfficialIdAsync(official.Id, cancellationToken);
        if (activeWorkload >= 5)
        {
            return Result<AssignTicketResponse>.Failure(Error.Conflict(
                "PqrsdfTicket.OfficialWorkloadLimitReached",
                "El funcionario seleccionado ha alcanzado el límite máximo de 5 solicitudes asignadas activas."));
        }

        var now = DateTime.UtcNow;
        var assignResult = ticket.AssignToOfficial(
            official.Id,
            request.AdminId,
            request.Request.Note,
            activeWorkload,
            now);

        if (assignResult.IsFailure)
        {
            return Result<AssignTicketResponse>.Failure(assignResult.Error);
        }

        var historyResult = TicketAssignmentHistory.Create(
            Guid.NewGuid(),
            ticket.Id,
            null,
            official.Id,
            request.AdminId,
            now,
            request.Request.Note,
            AssignmentType.InitialAssignment);

        if (historyResult.IsFailure)
        {
            return Result<AssignTicketResponse>.Failure(historyResult.Error);
        }

        await _historyRepository.AddAsync(historyResult.Value, cancellationToken);
        await _ticketRepository.UpdateAsync(ticket, cancellationToken);

        return Result<AssignTicketResponse>.Success(new AssignTicketResponse(
            ticket.RadicadoNumber.Value,
            ticket.Status.ToString(),
            official.Id,
            official.FullName,
            now,
            "Solicitud asignada exitosamente al funcionario."));
    }
}
