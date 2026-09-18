using MediatR;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.Services;
using Pqrsdf.Domain.Shared.Results;
using Pqrsdf.Domain.ValueObjects;

namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.GetTicketByRadicado;

/// <summary>
/// Query handler executing public consultation for a PQRSDF ticket by radicado.
/// Guarantees 0% exposure of applicant PII.
/// </summary>
public sealed class GetTicketByRadicadoQueryHandler : IRequestHandler<GetTicketByRadicadoQuery, Result<PublicTicketStatusDto>>
{
    private readonly IPqrsdfTicketRepository _ticketRepository;
    private readonly IDestinationAreaRepository _destinationAreaRepository;
    private readonly DueDateCalculator _dueDateCalculator;

    public GetTicketByRadicadoQueryHandler(
        IPqrsdfTicketRepository ticketRepository,
        IDestinationAreaRepository destinationAreaRepository,
        DueDateCalculator dueDateCalculator)
    {
        _ticketRepository = ticketRepository;
        _destinationAreaRepository = destinationAreaRepository;
        _dueDateCalculator = dueDateCalculator;
    }

    public async Task<Result<PublicTicketStatusDto>> Handle(
        GetTicketByRadicadoQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Radicado))
        {
            return Result<PublicTicketStatusDto>.Failure(Error.Validation(
                "Radicado.Required",
                "El número de radicado es obligatorio."));
        }

        var radicadoResult = RadicadoNumber.Create(request.Radicado.Trim());
        if (radicadoResult.IsFailure)
        {
            return Result<PublicTicketStatusDto>.Failure(radicadoResult.Error);
        }

        var ticket = await _ticketRepository.GetByRadicadoAsync(radicadoResult.Value, cancellationToken);
        if (ticket is null)
        {
            return Result<PublicTicketStatusDto>.Failure(Error.NotFound(
                "PqrsdfTicket.NotFound",
                "No se encontró ninguna solicitud con el radicado ingresado. Por favor verifique el número e intente de nuevo."));
        }

        var destinationArea = await _destinationAreaRepository.GetByIdAsync(ticket.DestinationAreaId, cancellationToken);
        string areaName = destinationArea?.Name ?? "Área Institucional";

        // Determine Colombian standard date (UTC-5)
        var nowColombia = ToColombiaTime(DateTime.UtcNow);
        var today = DateOnly.FromDateTime(nowColombia);

        var filingDate = DateOnly.FromDateTime(ToColombiaTime(ticket.CreatedAtUtc));
        var dueDate = ticket.DueDate.Value;

        int? remainingBusinessDays = null;
        bool isOverdue = false;
        int? overdueBusinessDays = null;

        if (ticket.Status is not (TicketStatus.Closed or TicketStatus.Answered))
        {
            if (today > dueDate)
            {
                isOverdue = true;
                overdueBusinessDays = _dueDateCalculator.CalculateOverdueBusinessDays(today, dueDate);
            }
            else
            {
                remainingBusinessDays = _dueDateCalculator.CalculateRemainingBusinessDays(today, dueDate);
            }
        }

        // Build lifecycle timeline
        var timeline = BuildTimeline(ticket);

        // Build resolution if present
        TicketResolutionDto? resolution = null;
        if (!string.IsNullOrWhiteSpace(ticket.ResponseText))
        {
            resolution = new TicketResolutionDto(
                ticket.ResponseText,
                ticket.ResponseDateUtc ?? ticket.UpdatedAtUtc ?? ticket.CreatedAtUtc);
        }

        var dto = new PublicTicketStatusDto(
            ticket.RadicadoNumber.Value,
            ticket.Type.ToString(),
            areaName,
            ticket.Subject,
            ticket.Description,
            ticket.Status.ToString(),
            filingDate,
            dueDate,
            remainingBusinessDays,
            isOverdue,
            overdueBusinessDays,
            timeline,
            resolution);

        return Result<PublicTicketStatusDto>.Success(dto);
    }

    private static IReadOnlyList<TicketTimelineMilestoneDto> BuildTimeline(PqrsdfTicket ticket)
    {
        var milestones = new List<TicketTimelineMilestoneDto>
        {
            new(
                TicketStatus.Registered.ToString(),
                "Radicado",
                ticket.CreatedAtUtc,
                IsCompleted: true,
                IsCurrent: ticket.Status == TicketStatus.Registered),

            new(
                TicketStatus.Assigned.ToString(),
                "Asignado",
                ticket.Status >= TicketStatus.Assigned ? ticket.UpdatedAtUtc : null,
                IsCompleted: ticket.Status >= TicketStatus.Assigned,
                IsCurrent: ticket.Status == TicketStatus.Assigned),

            new(
                TicketStatus.InReview.ToString(),
                "En trámite",
                ticket.Status >= TicketStatus.InReview ? ticket.UpdatedAtUtc : null,
                IsCompleted: ticket.Status >= TicketStatus.InReview,
                IsCurrent: ticket.Status == TicketStatus.InReview),

            new(
                TicketStatus.Answered.ToString(),
                "Respuesta emitida",
                ticket.ResponseDateUtc,
                IsCompleted: ticket.Status >= TicketStatus.Answered,
                IsCurrent: ticket.Status == TicketStatus.Answered),

            new(
                TicketStatus.Closed.ToString(),
                "Cerrado",
                ticket.Status == TicketStatus.Closed ? (ticket.ResponseDateUtc ?? ticket.UpdatedAtUtc) : null,
                IsCompleted: ticket.Status == TicketStatus.Closed,
                IsCurrent: ticket.Status == TicketStatus.Closed)
        };

        return milestones;
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
