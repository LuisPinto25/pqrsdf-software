using MediatR;
using Pqrsdf.Domain.Constants;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.Services;
using Pqrsdf.Domain.Shared.Results;
using Pqrsdf.Domain.ValueObjects;

namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.GetTicketManagementDetail;

public sealed class GetTicketManagementDetailQueryHandler 
    : IRequestHandler<GetTicketManagementDetailQuery, Result<TicketManagementDetailDto>>
{
    private readonly IPqrsdfTicketRepository _ticketRepository;
    private readonly IDestinationAreaRepository _destinationAreaRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITicketStatusHistoryRepository _statusHistoryRepository;
    private readonly DueDateCalculator _dueDateCalculator;

    public GetTicketManagementDetailQueryHandler(
        IPqrsdfTicketRepository ticketRepository,
        IDestinationAreaRepository destinationAreaRepository,
        IUserRepository userRepository,
        ITicketStatusHistoryRepository statusHistoryRepository,
        DueDateCalculator dueDateCalculator)
    {
        _ticketRepository = ticketRepository;
        _destinationAreaRepository = destinationAreaRepository;
        _userRepository = userRepository;
        _statusHistoryRepository = statusHistoryRepository;
        _dueDateCalculator = dueDateCalculator;
    }

    public async Task<Result<TicketManagementDetailDto>> Handle(
        GetTicketManagementDetailQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Radicado))
        {
            return Result<TicketManagementDetailDto>.Failure(Error.Validation(
                "PqrsdfTicket.InvalidRadicado",
                "El número de radicado es requerido."));
        }

        var radicadoResult = RadicadoNumber.Create(request.Radicado.Trim());
        if (radicadoResult.IsFailure)
        {
            return Result<TicketManagementDetailDto>.Failure(radicadoResult.Error);
        }

        var ticket = await _ticketRepository.GetByRadicadoAsync(radicadoResult.Value, cancellationToken);
        if (ticket is null)
        {
            return Result<TicketManagementDetailDto>.Failure(Error.NotFound(
                "PqrsdfTicket.NotFound",
                DomainMessages.TicketManagement.NotFound));
        }

        // Authorization check: User must be either the assigned official or an Administrator
        bool isAssigned = ticket.AssignedToUserId.HasValue && ticket.AssignedToUserId.Value == request.CurrentUserId;
        bool isAdmin = string.Equals(request.CurrentUserRole, "Administrador", StringComparison.OrdinalIgnoreCase);

        if (!isAssigned && !isAdmin)
        {
            return Result<TicketManagementDetailDto>.Failure(Error.Unauthorized(
                "PqrsdfTicket.Forbidden",
                DomainMessages.TicketManagement.Forbidden));
        }

        var destinationArea = await _destinationAreaRepository.GetByIdAsync(ticket.DestinationAreaId, cancellationToken);
        string areaName = destinationArea?.Name ?? "Área Institucional";

        string? assignedOfficialName = null;
        if (ticket.AssignedToUserId.HasValue)
        {
            var official = await _userRepository.GetByIdAsync(ticket.AssignedToUserId.Value, cancellationToken);
            assignedOfficialName = official?.FullName;
        }

        // SLA calculation
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-5)); // Colombia time UTC-5
        var dueDate = ticket.DueDate.Value;
        int? remainingBusinessDays = null;
        string urgencyLevel = "Normal";

        if (ticket.Status is not (TicketStatus.Closed or TicketStatus.Answered))
        {
            if (today > dueDate)
            {
                urgencyLevel = "Expired";
                remainingBusinessDays = -_dueDateCalculator.CalculateOverdueBusinessDays(today, dueDate);
            }
            else
            {
                remainingBusinessDays = _dueDateCalculator.CalculateRemainingBusinessDays(today, dueDate);
                if (remainingBusinessDays <= 3)
                {
                    urgencyLevel = "Critical";
                }
                else if (remainingBusinessDays <= 7)
                {
                    urgencyLevel = "Attention";
                }
                else
                {
                    urgencyLevel = "Normal";
                }
            }
        }

        // Status history
        var historyRecords = await _statusHistoryRepository.GetByTicketIdAsync(ticket.Id, cancellationToken);
        var statusHistoryDtos = historyRecords.Select(h => new TicketStatusHistoryItemDto(
            h.Id,
            (int)h.PreviousStatus,
            GetStatusName(h.PreviousStatus),
            (int)h.NewStatus,
            GetStatusName(h.NewStatus),
            h.ChangedByUser?.FullName ?? "Funcionario",
            h.Justification,
            h.ChangedAtUtc
        )).ToList();

        ApplicantDetailDto? applicantDto = null;
        if (!ticket.IsAnonymous && ticket.Applicant is not null)
        {
            applicantDto = new ApplicantDetailDto(
                ticket.Applicant.IdentificationType.ToString(),
                ticket.Applicant.IdentificationNumber,
                ticket.Applicant.FullName,
                ticket.Applicant.Email,
                ticket.Applicant.PhoneNumber
            );
        }

        bool canManage = (isAssigned || isAdmin) && ticket.Status != TicketStatus.Closed;

        var dto = new TicketManagementDetailDto(
            ticket.Id,
            ticket.RadicadoNumber.Value,
            (int)ticket.Type,
            GetTypeName(ticket.Type),
            ticket.DestinationAreaId,
            areaName,
            ticket.IsAnonymous,
            applicantDto,
            ticket.Subject,
            ticket.Description,
            ticket.CreatedAtUtc,
            dueDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            remainingBusinessDays,
            urgencyLevel,
            (int)ticket.Status,
            GetStatusName(ticket.Status),
            ticket.AssignedToUserId,
            assignedOfficialName,
            ticket.AssignedAtUtc,
            ticket.AssignmentNote,
            ticket.ResponseText,
            ticket.ResponseDateUtc,
            isAssigned,
            canManage,
            statusHistoryDtos
        );

        return Result<TicketManagementDetailDto>.Success(dto);
    }

    private static string GetTypeName(PqrsdfType type) => type switch
    {
        PqrsdfType.Petition => "Petición",
        PqrsdfType.Complaint => "Queja",
        PqrsdfType.Claim => "Reclamo",
        PqrsdfType.Suggestion => "Sugerencia",
        PqrsdfType.Denunciation => "Denuncia",
        PqrsdfType.Compliment => "Felicitación",
        _ => "Solicitud"
    };

    private static string GetStatusName(TicketStatus status) => status switch
    {
        TicketStatus.Registered => "Registrado",
        TicketStatus.Assigned => "Asignado",
        TicketStatus.InReview => "En trámite",
        TicketStatus.Answered => "Respondido",
        TicketStatus.Closed => "Cerrado",
        _ => "Desconocido"
    };
}
