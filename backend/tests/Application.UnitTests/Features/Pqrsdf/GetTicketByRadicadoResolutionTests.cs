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

public class GetTicketByRadicadoResolutionTests
{
    private readonly Mock<IPqrsdfTicketRepository> _ticketRepositoryMock = new();
    private readonly Mock<IDestinationAreaRepository> _areaRepositoryMock = new();
    private readonly DueDateCalculator _dueDateCalculator = new(new ColombianHolidayService());
    private readonly GetTicketByRadicadoQueryHandler _handler;

    public GetTicketByRadicadoResolutionTests()
    {
        _handler = new GetTicketByRadicadoQueryHandler(
            _ticketRepositoryMock.Object,
            _areaRepositoryMock.Object,
            _dueDateCalculator);
    }

    [Fact]
    public async Task Handle_WhenTicketIsClosedWithResponse_ShouldMapResolutionAndStopRemainingDays()
    {
        // Arrange
        var radicadoStr = "2026-00000002";
        var radicado = RadicadoNumber.Create(radicadoStr).Value;
        var areaId = Guid.NewGuid();
        var area = DestinationArea.Create(areaId, "Secretaría General", "SG").Value;

        var dueDate = DueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), 15).Value;
        var ticket = PqrsdfTicket.Create(
            Guid.NewGuid(),
            radicado,
            PqrsdfType.Suggestion,
            areaId,
            isAnonymous: true,
            applicant: null,
            "Sugerencia sobre horario de atención",
            "Sugiero ampliar el horario de atención ciudadana los días viernes.",
            dueDate).Value;

        var responseDate = DateTime.UtcNow.AddDays(-1);
        var responseText = "Se ha acogido la sugerencia y el nuevo horario iniciará el próximo mes.";
        ticket.CloseWithResponse(responseText, responseDate);

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
        dto.RemainingBusinessDays.Should().BeNull();
        dto.IsOverdue.Should().BeFalse();
        dto.Resolution.Should().NotBeNull();
        dto.Resolution!.ResponseText.Should().Be(responseText);
        dto.Timeline.Should().Contain(m => m.Status == "Closed" && m.IsCompleted && m.IsCurrent);
    }
}
