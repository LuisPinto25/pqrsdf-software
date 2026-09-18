using FluentAssertions;
using Moq;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.GetTicketByRadicado;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.Services;
using Pqrsdf.Domain.ValueObjects;
using Pqrsdf.Infrastructure.Services;
using Xunit;

namespace Pqrsdf.Application.UnitTests.Features.Pqrsdf;

public class GetTicketByRadicadoOverdueTests
{
    private readonly Mock<IPqrsdfTicketRepository> _ticketRepositoryMock = new();
    private readonly Mock<IDestinationAreaRepository> _areaRepositoryMock = new();
    private readonly DueDateCalculator _dueDateCalculator = new(new ColombianHolidayService());
    private readonly GetTicketByRadicadoQueryHandler _handler;

    public GetTicketByRadicadoOverdueTests()
    {
        _handler = new GetTicketByRadicadoQueryHandler(
            _ticketRepositoryMock.Object,
            _areaRepositoryMock.Object,
            _dueDateCalculator);
    }

    [Fact]
    public async Task Handle_WhenTicketIsInTerm_ShouldReturnRemainingBusinessDaysAndNotOverdue()
    {
        // Arrange
        var radicadoStr = "2026-00000010";
        var radicado = RadicadoNumber.Create(radicadoStr).Value;
        var areaId = Guid.NewGuid();
        var area = DestinationArea.Create(areaId, "Secretaría de Educación", "SED").Value;

        // Due date 15 calendar days in future
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15));
        var dueDate = DueDate.Create(futureDate, 15).Value;
        var ticket = PqrsdfTicket.Create(
            Guid.NewGuid(),
            radicado,
            PqrsdfType.Suggestion,
            areaId,
            isAnonymous: true,
            applicant: null,
            "Sugerencia en término",
            "Descripción de prueba en término",
            dueDate).Value;

        _ticketRepositoryMock
            .Setup(r => r.GetByRadicadoAsync(radicado, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _areaRepositoryMock
            .Setup(r => r.GetByIdAsync(areaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(area);

        var query = new GetTicketByRadicadoQuery(radicadoStr);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value;
        dto.IsOverdue.Should().BeFalse();
        dto.RemainingBusinessDays.Should().NotBeNull();
        dto.RemainingBusinessDays.Should().BeGreaterThan(0);
        dto.OverdueBusinessDays.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenTicketIsOverdue_ShouldSetIsOverdueTrueAndCalculateOverdueBusinessDays()
    {
        // Arrange
        var radicadoStr = "2026-00000011";
        var radicado = RadicadoNumber.Create(radicadoStr).Value;
        var areaId = Guid.NewGuid();
        var area = DestinationArea.Create(areaId, "Secretaría de Movilidad", "MOV").Value;

        // Due date in the past (e.g. 10 days ago)
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));
        var dueDate = DueDate.Create(pastDate, 15).Value;
        var ticket = PqrsdfTicket.Create(
            Guid.NewGuid(),
            radicado,
            PqrsdfType.Suggestion,
            areaId,
            isAnonymous: true,
            applicant: null,
            "Sugerencia vencida",
            "Descripción de sugerencia que superó fecha de vencimiento",
            dueDate).Value;

        _ticketRepositoryMock
            .Setup(r => r.GetByRadicadoAsync(radicado, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _areaRepositoryMock
            .Setup(r => r.GetByIdAsync(areaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(area);

        var query = new GetTicketByRadicadoQuery(radicadoStr);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value;
        dto.IsOverdue.Should().BeTrue();
        dto.RemainingBusinessDays.Should().BeNull();
        dto.OverdueBusinessDays.Should().NotBeNull();
        dto.OverdueBusinessDays.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_WhenOverdueTicketIsClosed_ShouldNotBeMarkedAsOverdue()
    {
        // Arrange
        var radicadoStr = "2026-00000012";
        var radicado = RadicadoNumber.Create(radicadoStr).Value;
        var areaId = Guid.NewGuid();
        var area = DestinationArea.Create(areaId, "Secretaría de Salud", "SAL").Value;

        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15));
        var dueDate = DueDate.Create(pastDate, 15).Value;
        var ticket = PqrsdfTicket.Create(
            Guid.NewGuid(),
            radicado,
            PqrsdfType.Suggestion,
            areaId,
            isAnonymous: true,
            applicant: null,
            "Sugerencia cerrada con fecha pasada",
            "Descripción de sugerencia",
            dueDate).Value;

        ticket.CloseWithResponse("Agradecemos sus comentarios.", DateTime.UtcNow.AddDays(-2));

        _ticketRepositoryMock
            .Setup(r => r.GetByRadicadoAsync(radicado, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _areaRepositoryMock
            .Setup(r => r.GetByIdAsync(areaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(area);

        var query = new GetTicketByRadicadoQuery(radicadoStr);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value;
        dto.Status.Should().Be("Closed");
        dto.IsOverdue.Should().BeFalse();
        dto.RemainingBusinessDays.Should().BeNull();
        dto.OverdueBusinessDays.Should().BeNull();
        dto.Resolution.Should().NotBeNull();
    }
}
