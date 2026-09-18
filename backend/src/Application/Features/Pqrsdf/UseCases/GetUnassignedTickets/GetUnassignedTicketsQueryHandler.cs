using MediatR;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.Services;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.GetUnassignedTickets;

public sealed class GetUnassignedTicketsQueryHandler
    : IRequestHandler<GetUnassignedTicketsQuery, Result<IReadOnlyList<UnassignedTicketDto>>>
{
    private readonly IPqrsdfTicketRepository _ticketRepository;
    private readonly IDestinationAreaRepository _destinationAreaRepository;
    private readonly DueDateCalculator _dueDateCalculator;

    public GetUnassignedTicketsQueryHandler(
        IPqrsdfTicketRepository ticketRepository,
        IDestinationAreaRepository destinationAreaRepository,
        DueDateCalculator dueDateCalculator)
    {
        _ticketRepository = ticketRepository;
        _destinationAreaRepository = destinationAreaRepository;
        _dueDateCalculator = dueDateCalculator;
    }

    public async Task<Result<IReadOnlyList<UnassignedTicketDto>>> Handle(
        GetUnassignedTicketsQuery request,
        CancellationToken cancellationToken)
    {
        var tickets = await _ticketRepository.GetUnassignedAsync(
            request.Type,
            request.DestinationAreaId,
            request.Search,
            cancellationToken);

        var areas = await _destinationAreaRepository.GetActiveAreasAsync(cancellationToken) ?? (IReadOnlyList<Domain.Entities.DestinationArea>)Array.Empty<Domain.Entities.DestinationArea>();
        var areaDict = areas.ToDictionary(a => a.Id, a => a.Name);

        var nowColombia = ToColombiaTime(DateTime.UtcNow);
        var today = DateOnly.FromDateTime(nowColombia);

        var dtos = new List<UnassignedTicketDto>(tickets.Count);

        foreach (var ticket in tickets)
        {
            var areaName = areaDict.TryGetValue(ticket.DestinationAreaId, out var name)
                ? name
                : (await _destinationAreaRepository.GetByIdAsync(ticket.DestinationAreaId, cancellationToken))?.Name ?? "Área Institucional";

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

            dtos.Add(new UnassignedTicketDto(
                ticket.Id,
                ticket.RadicadoNumber.Value,
                (int)ticket.Type,
                ticket.Type.ToString(),
                ticket.DestinationAreaId,
                areaName,
                ticket.Subject,
                ticket.Description,
                ticket.CreatedAtUtc,
                ticket.DueDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                remainingDays,
                urgencyLevel));
        }

        return Result<IReadOnlyList<UnassignedTicketDto>>.Success(dtos);
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
