using FluentAssertions;
using Moq;
using Pqrsdf.Application.Features.System.UseCases.GetSystemStatus;
using Pqrsdf.Application.Shared.Persistence;
using Pqrsdf.Domain.Constants;
using Xunit;

namespace Pqrsdf.Application.UnitTests.Features.System;

public class GetSystemStatusQueryHandlerTests
{
    private readonly Mock<IDatabaseStatusChecker> _databaseStatusCheckerMock;
    private readonly GetSystemStatusQueryHandler _handler;

    public GetSystemStatusQueryHandlerTests()
    {
        _databaseStatusCheckerMock = new Mock<IDatabaseStatusChecker>();
        _handler = new GetSystemStatusQueryHandler(_databaseStatusCheckerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenDatabaseIsConnected_ShouldReturnHealthyStatusAndSpanishMessage()
    {
        // Arrange
        _databaseStatusCheckerMock
            .Setup(c => c.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var query = new GetSystemStatusQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Status.Should().Be("Healthy");
        result.Value.DatabaseConnected.Should().BeTrue();
        result.Value.Message.Should().Be(DomainMessages.System.Operational);
    }

    [Fact]
    public async Task Handle_WhenDatabaseIsUnreachable_ShouldReturnDegradedStatusAndSpanishMessage()
    {
        // Arrange
        _databaseStatusCheckerMock
            .Setup(c => c.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var query = new GetSystemStatusQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Status.Should().Be("Degraded");
        result.Value.DatabaseConnected.Should().BeFalse();
        result.Value.Message.Should().Be(DomainMessages.System.DatabaseUnavailable);
    }
}
