using FluentAssertions;
using Moq;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.GetOfficialInbox;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.Services;
using Pqrsdf.Domain.ValueObjects;
using Pqrsdf.Infrastructure.Services;
using Xunit;

namespace Pqrsdf.Application.UnitTests.Features.Pqrsdf;

public class GetOfficialInboxQueryHandlerTests
{
    private readonly Mock<IPqrsdfTicketRepository> _ticketRepoMock = new();
    private readonly Mock<IDestinationAreaRepository> _areaRepoMock = new();
    private readonly DueDateCalculator _dueDateCalculator = new(new ColombianHolidayService());

    private readonly GetOfficialInboxQueryHandler _handler;

    public GetOfficialInboxQueryHandlerTests()
    {
        _handler = new GetOfficialInboxQueryHandler(
            _ticketRepoMock.Object,
            _areaRepoMock.Object,
            _dueDateCalculator);
    }

    [Fact]
    public async Task Handle_WhenOfficialHasTickets_ReturnsInboxWithWorkloadAndUrgency()
    {
        var officialId = Guid.NewGuid();
        var areaId = Guid.NewGuid();
        var destinationArea = DestinationArea.Create(areaId, "Secretaría General", "SEC", true).Value;

        _areaRepoMock.Setup(r => r.GetActiveAreasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DestinationArea> { destinationArea });

        var radicado = RadicadoNumber.Create("2026-00000001").Value;
        var ticket = PqrsdfTicket.Create(
            Guid.NewGuid(),
            radicado,
            PqrsdfType.Denunciation,
            areaId,
            true,
            null,
            "Denuncia urgente",
            "Descripción de prueba mayor a diez caracteres",
            DueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 30).Value).Value;

        ticket.AssignToOfficial(officialId, Guid.NewGuid(), "Instrucción inicial", 0, DateTime.UtcNow);

        _ticketRepoMock.Setup(r => r.GetOfficialInboxAsync(officialId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PqrsdfTicket> { ticket });

        _ticketRepoMock.Setup(r => r.CountActiveByOfficialIdAsync(officialId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var query = new GetOfficialInboxQuery(officialId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ActiveCount.Should().Be(1);
        result.Value.MaxCapacity.Should().Be(5);
        result.Value.Tickets.Should().HaveCount(1);
        result.Value.Tickets[0].RadicadoNumber.Should().Be("2026-00000001");
        result.Value.Tickets[0].DestinationAreaName.Should().Be("Secretaría General");
        result.Value.Tickets[0].UrgencyLevel.Should().NotBeNullOrWhiteSpace();
    }
}
