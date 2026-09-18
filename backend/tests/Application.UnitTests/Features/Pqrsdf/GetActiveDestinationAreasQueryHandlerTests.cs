using FluentAssertions;
using Moq;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.GetActiveDestinationAreas;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Repositories;
using Xunit;

namespace Pqrsdf.Application.UnitTests.Features.Pqrsdf;

public class GetActiveDestinationAreasQueryHandlerTests
{
    private readonly Mock<IDestinationAreaRepository> _repositoryMock;
    private readonly GetActiveDestinationAreasQueryHandler _handler;

    public GetActiveDestinationAreasQueryHandlerTests()
    {
        _repositoryMock = new Mock<IDestinationAreaRepository>();
        _handler = new GetActiveDestinationAreasQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnActiveAreas_WhenAreasExist()
    {
        // Arrange
        var area1 = DestinationArea.Create(Guid.NewGuid(), "Atención al Ciudadano", "ATC").Value;
        var area2 = DestinationArea.Create(Guid.NewGuid(), "Oficina Jurídica", "JUR").Value;
        var activeAreas = new List<DestinationArea> { area1, area2 };

        _repositoryMock
            .Setup(r => r.GetActiveAreasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeAreas);

        var query = new GetActiveDestinationAreasQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Name.Should().Be("Atención al Ciudadano");
        result.Value[0].Code.Should().Be("ATC");
        result.Value[1].Name.Should().Be("Oficina Jurídica");
        result.Value[1].Code.Should().Be("JUR");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoActiveAreasExist()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetActiveAreasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DestinationArea>());

        var query = new GetActiveDestinationAreasQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
