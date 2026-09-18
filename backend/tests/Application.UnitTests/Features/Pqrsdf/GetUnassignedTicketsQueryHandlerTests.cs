using FluentAssertions;
using Moq;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.GetUnassignedTickets;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.Services;
using Pqrsdf.Domain.ValueObjects;
using Pqrsdf.Infrastructure.Services;
using Xunit;

namespace Pqrsdf.Application.UnitTests.Features.Pqrsdf;

public class GetUnassignedTicketsQueryHandlerTests
{
    private readonly Mock<IPqrsdfTicketRepository> _ticketRepositoryMock = new();
    private readonly Mock<IDestinationAreaRepository> _areaRepositoryMock = new();
    private readonly DueDateCalculator _dueDateCalculator = new(new ColombianHolidayService());
    private readonly GetUnassignedTicketsQueryHandler _handler;

    public GetUnassignedTicketsQueryHandlerTests()
    {
        _handler = new GetUnassignedTicketsQueryHandler(
            _ticketRepositoryMock.Object,
            _areaRepositoryMock.Object,
            _dueDateCalculator);
    }

    [Fact]
    public async Task Handle_WhenNoUnassignedTicketsExist_ShouldReturnEmptyListSuccess()
    {
        // Arrange
        _ticketRepositoryMock.Setup(r => r.GetUnassignedAsync(null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PqrsdfTicket>());

        var query = new GetUnassignedTicketsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenUnassignedTicketsExist_ShouldReturnMappedDtosWithUrgencyTier()
    {
        // Arrange
        var areaId = Guid.NewGuid();
        var destinationArea = DestinationArea.Create(areaId, "Oficina Jurídica", "JUR", true).Value;
        _areaRepositoryMock.Setup(r => r.GetActiveAreasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DestinationArea> { destinationArea });
        _areaRepositoryMock.Setup(r => r.GetByIdAsync(areaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinationArea);

        var radicado = RadicadoNumber.Create("2026-00000001").Value;
        var dueDate = DueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), 30).Value;
        var ticket = PqrsdfTicket.Create(
            Guid.NewGuid(),
            radicado,
            PqrsdfType.Denunciation,
            areaId,
            true,
            null,
            "Asunto de prueba",
            "Descripción de prueba mayor a diez caracteres",
            dueDate).Value;

        _ticketRepositoryMock.Setup(r => r.GetUnassignedAsync(null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PqrsdfTicket> { ticket });

        var query = new GetUnassignedTicketsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        var item = result.Value[0];
        item.RadicadoNumber.Should().Be("2026-00000001");
        item.DestinationAreaName.Should().Be("Oficina Jurídica");
        item.UrgencyLevel.Should().NotBeNullOrWhiteSpace();
    }
}
