using MediatR;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.Services;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.GetOfficialInbox;

public record GetOfficialInboxQuery(Guid OfficialId) : IRequest<Result<OfficialInboxResponse>>;

public record OfficialInboxItemDto(
    Guid Id,
    string RadicadoNumber,
    int Type,
    string TypeName,
    string DestinationAreaName,
    string Subject,
    string Description,
    DateTime FilingDateUtc,
    DateTime AssignedAtUtc,
    string? AssignmentNote,
    DateTime DueDateUtc,
    int RemainingBusinessDays,
    string UrgencyLevel);

public record OfficialInboxResponse(
    int ActiveCount,
    int MaxCapacity,
    IReadOnlyList<OfficialInboxItemDto> Tickets);

public sealed class GetOfficialInboxQueryHandler
    : IRequestHandler<GetOfficialInboxQuery, Result<OfficialInboxResponse>>
{
    private readonly IPqrsdfTicketRepository _ticketRepository;
    private readonly IDestinationAreaRepository _destinationAreaRepository;
    private readonly DueDateCalculator _dueDateCalculator;

    public GetOfficialInboxQueryHandler(
        IPqrsdfTicketRepository ticketRepository,
        IDestinationAreaRepository destinationAreaRepository,
        DueDateCalculator dueDateCalculator)
    {
        _ticketRepository = ticketRepository;
        _destinationAreaRepository = destinationAreaRepository;
        _dueDateCalculator = dueDateCalculator;
    }

    public async Task<Result<OfficialInboxResponse>> Handle(
        GetOfficialInboxQuery request,
        CancellationToken cancellationToken)
    {
        var tickets = await _ticketRepository.GetOfficialInboxAsync(request.OfficialId, cancellationToken);
        var activeCount = await _ticketRepository.CountActiveByOfficialIdAsync(request.OfficialId, cancellationToken);

        var areas = await _destinationAreaRepository.GetActiveAreasAsync(cancellationToken) ?? (IReadOnlyList<Domain.Entities.DestinationArea>)Array.Empty<Domain.Entities.DestinationArea>();
        var areaDict = areas.ToDictionary(a => a.Id, a => a.Name);

        var nowColombia = ToColombiaTime(DateTime.UtcNow);
        var today = DateOnly.FromDateTime(nowColombia);

        var dtos = new List<OfficialInboxItemDto>(tickets.Count);

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

            dtos.Add(new OfficialInboxItemDto(
                ticket.Id,
                ticket.RadicadoNumber.Value,
                (int)ticket.Type,
                ticket.Type.ToString(),
                areaName,
                ticket.Subject,
                ticket.Description,
                ticket.CreatedAtUtc,
                ticket.AssignedAtUtc ?? ticket.CreatedAtUtc,
                ticket.AssignmentNote,
                ticket.DueDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                remainingDays,
                urgencyLevel));
        }

        return Result<OfficialInboxResponse>.Success(new OfficialInboxResponse(activeCount, 5, dtos));
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
