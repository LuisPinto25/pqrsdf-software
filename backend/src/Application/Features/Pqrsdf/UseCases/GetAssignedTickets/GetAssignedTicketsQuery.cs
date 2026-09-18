using MediatR;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.Services;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.GetAssignedTickets;

public record GetAssignedTicketsQuery(
    string? Search = null,
    Guid? OfficialId = null) : IRequest<Result<IReadOnlyList<AssignedTicketDto>>>;

public record AssignedTicketDto(
    Guid Id,
    string RadicadoNumber,
    int Type,
    string TypeName,
    string DestinationAreaName,
    string Subject,
    string? Description,
    Guid AssignedToUserId,
    string AssignedOfficialName,
    DateTime AssignedAtUtc,
    string? AssignmentNote,
    DateTime DueDateUtc,
    int RemainingBusinessDays,
    string UrgencyLevel);

public sealed class GetAssignedTicketsQueryHandler
    : IRequestHandler<GetAssignedTicketsQuery, Result<IReadOnlyList<AssignedTicketDto>>>
{
    private readonly IPqrsdfTicketRepository _ticketRepository;
    private readonly IDestinationAreaRepository _destinationAreaRepository;
    private readonly IUserRepository _userRepository;
    private readonly DueDateCalculator _dueDateCalculator;

    public GetAssignedTicketsQueryHandler(
        IPqrsdfTicketRepository ticketRepository,
        IDestinationAreaRepository destinationAreaRepository,
        IUserRepository userRepository,
        DueDateCalculator dueDateCalculator)
    {
        _ticketRepository = ticketRepository;
        _destinationAreaRepository = destinationAreaRepository;
        _userRepository = userRepository;
        _dueDateCalculator = dueDateCalculator;
    }

    public async Task<Result<IReadOnlyList<AssignedTicketDto>>> Handle(
        GetAssignedTicketsQuery request,
        CancellationToken cancellationToken)
    {
        var tickets = await _ticketRepository.GetAssignedAsync(
            request.Search,
            request.OfficialId,
            cancellationToken);

        var areas = await _destinationAreaRepository.GetActiveAreasAsync(cancellationToken) ?? (IReadOnlyList<Domain.Entities.DestinationArea>)Array.Empty<Domain.Entities.DestinationArea>();
        var areaDict = areas.ToDictionary(a => a.Id, a => a.Name);

        var officials = await _userRepository.GetActiveOfficialsAsync(cancellationToken) ?? (IReadOnlyList<Domain.Entities.User>)Array.Empty<Domain.Entities.User>();
        var officialDict = officials.ToDictionary(u => u.Id, u => u.FullName);

        var nowColombia = ToColombiaTime(DateTime.UtcNow);
        var today = DateOnly.FromDateTime(nowColombia);

        var dtos = new List<AssignedTicketDto>(tickets.Count);

        foreach (var ticket in tickets)
        {
            var areaName = areaDict.TryGetValue(ticket.DestinationAreaId, out var name)
                ? name
                : (await _destinationAreaRepository.GetByIdAsync(ticket.DestinationAreaId, cancellationToken))?.Name ?? "Área Institucional";

            string officialName = "Funcionario Desconocido";
            if (ticket.AssignedToUserId.HasValue)
            {
                if (!officialDict.TryGetValue(ticket.AssignedToUserId.Value, out var offName))
                {
                    var user = await _userRepository.GetByIdAsync(ticket.AssignedToUserId.Value, cancellationToken);
                    offName = user?.FullName ?? "Funcionario";
                }
                officialName = offName;
            }

            int remainingDays;
            string urgencyLevel;

            if (today > ticket.DueDate.Value)
            {
                var overdue = _dueDateCalculator.CalculateOverdueBusinessDays(today, ticket.DueDate.Value);
                remainingDays = -overdue;
                urgencyLevel = "Critical";
            }
            else
            {
                remainingDays = _dueDateCalculator.CalculateRemainingBusinessDays(today, ticket.DueDate.Value);
                urgencyLevel = remainingDays <= 3 ? "Critical" : remainingDays <= 7 ? "Attention" : "OnTime";
            }

            dtos.Add(new AssignedTicketDto(
                ticket.Id,
                ticket.RadicadoNumber.Value,
                (int)ticket.Type,
                ticket.Type.ToString(),
                areaName,
                ticket.Subject,
                ticket.Description,
                ticket.AssignedToUserId ?? Guid.Empty,
                officialName,
                ticket.AssignedAtUtc ?? ticket.CreatedAtUtc,
                ticket.AssignmentNote,
                ticket.DueDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                remainingDays,
                urgencyLevel));
        }

        return Result<IReadOnlyList<AssignedTicketDto>>.Success(dtos);
    }

    private static DateTime ToColombiaTime(DateTime utc)
    {
        try
        {
            var tzId = OperatingSystem.IsWindows() ? "SA Pacific Standard Time" : "America/Bogota";
            var tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
        }
        catch
        {
            return utc.AddHours(-5);
        }
    }
}
