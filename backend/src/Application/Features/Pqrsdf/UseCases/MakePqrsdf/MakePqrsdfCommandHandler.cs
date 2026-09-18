using MediatR;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.Services;
using Pqrsdf.Domain.Shared.Results;
using Pqrsdf.Domain.ValueObjects;

namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.MakePqrsdf;

public sealed class MakePqrsdfCommandHandler
    : IRequestHandler<MakePqrsdfCommand, Result<MakePqrsdfResponse>>
{
    private static readonly TimeSpan ColombiaTimeZoneOffset = TimeSpan.FromHours(-5);

    private readonly IPqrsdfTicketRepository _ticketRepository;
    private readonly IDestinationAreaRepository _destinationAreaRepository;
    private readonly IRadicadoSequenceGenerator _radicadoSequenceGenerator;
    private readonly DueDateCalculator _dueDateCalculator;

    public MakePqrsdfCommandHandler(
        IPqrsdfTicketRepository ticketRepository,
        IDestinationAreaRepository destinationAreaRepository,
        IRadicadoSequenceGenerator radicadoSequenceGenerator,
        DueDateCalculator dueDateCalculator)
    {
        _ticketRepository = ticketRepository;
        _destinationAreaRepository = destinationAreaRepository;
        _radicadoSequenceGenerator = radicadoSequenceGenerator;
        _dueDateCalculator = dueDateCalculator;
    }

    public async Task<Result<MakePqrsdfResponse>> Handle(
        MakePqrsdfCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Input validation
        var validationResult = MakePqrsdfCommandValidator.Validate(command);
        if (validationResult.IsFailure)
        {
            return Result<MakePqrsdfResponse>.Failure(validationResult.Error);
        }

        var req = command.Request;

        // 2. Validate destination area
        var area = await _destinationAreaRepository.GetByIdAsync(req.DestinationAreaId, cancellationToken);
        if (area is null || !area.IsActive)
        {
            return Result<MakePqrsdfResponse>.Failure(Error.NotFound(
                "MakePqrsdf.AreaNotFoundOrInactive",
                "El área o dependencia seleccionada no existe o no se encuentra habilitada para recibir solicitudes."));
        }

        // 3. Build Applicant if not anonymous
        Applicant? applicant = null;
        if (!req.IsAnonymous)
        {
            var applicantResult = Applicant.Create(
                req.Applicant!.FullName,
                req.Applicant.IdentificationType,
                req.Applicant.IdentificationNumber,
                req.Applicant.Email,
                req.Applicant.PhoneNumber);

            if (applicantResult.IsFailure)
            {
                return Result<MakePqrsdfResponse>.Failure(applicantResult.Error);
            }

            applicant = applicantResult.Value;
        }

        // 4. Calculate filing calendar date in Colombia (UTC-5)
        var utcNow = DateTimeOffset.UtcNow;
        var colombiaTime = utcNow.ToOffset(ColombiaTimeZoneOffset);
        var filingDate = DateOnly.FromDateTime(colombiaTime.DateTime);

        // 5. Compute statutory due date
        var dueDateResult = _dueDateCalculator.CalculateDueDate(filingDate, req.Type);
        if (dueDateResult.IsFailure)
        {
            return Result<MakePqrsdfResponse>.Failure(dueDateResult.Error);
        }

        // 6. Generate atomic sequential radicado
        var radicadoNumber = await _radicadoSequenceGenerator.NextAsync(filingDate.Year, cancellationToken);

        // 7. Create aggregate root
        var ticketResult = PqrsdfTicket.Create(
            Guid.NewGuid(),
            radicadoNumber,
            req.Type,
            req.DestinationAreaId,
            req.IsAnonymous,
            applicant,
            req.Subject,
            req.Description,
            dueDateResult.Value);

        if (ticketResult.IsFailure)
        {
            return Result<MakePqrsdfResponse>.Failure(ticketResult.Error);
        }

        var ticket = ticketResult.Value;

        // 8. Persist ticket
        await _ticketRepository.AddAsync(ticket, cancellationToken);

        // 9. Return response DTO
        var response = new MakePqrsdfResponse(
            ticket.Id,
            ticket.RadicadoNumber.Value,
            ticket.Type.ToString(),
            ticket.CreatedAtUtc,
            ticket.DueDate.Value,
            ticket.DueDate.BusinessDaysCount,
            ticket.Status.ToString());

        return Result<MakePqrsdfResponse>.Success(response);
    }
}
