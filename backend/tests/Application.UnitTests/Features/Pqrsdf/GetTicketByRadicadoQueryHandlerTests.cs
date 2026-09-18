using FluentAssertions;
using Moq;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.GetTicketByRadicado;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.Services;
using Pqrsdf.Domain.Shared.Results;
using Pqrsdf.Domain.ValueObjects;
using Pqrsdf.Infrastructure.Services;
using Xunit;

namespace Pqrsdf.Application.UnitTests.Features.Pqrsdf;

public class GetTicketByRadicadoQueryHandlerTests
{
    private readonly Mock<IPqrsdfTicketRepository> _ticketRepositoryMock = new();
    private readonly Mock<IDestinationAreaRepository> _areaRepositoryMock = new();
    private readonly DueDateCalculator _dueDateCalculator = new(new ColombianHolidayService());
    private readonly GetTicketByRadicadoQueryHandler _handler;

    public GetTicketByRadicadoQueryHandlerTests()
    {
        _handler = new GetTicketByRadicadoQueryHandler(
            _ticketRepositoryMock.Object,
            _areaRepositoryMock.Object,
            _dueDateCalculator);
    }

    [Fact]
    public async Task Handle_WhenRadicadoIsBlank_ShouldReturnValidationError()
    {
        // Arrange
        var query = new GetTicketByRadicadoQuery("   ");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Radicado.Required");
    }

    [Fact]
    public async Task Handle_WhenRadicadoFormatIsInvalid_ShouldReturnValidationError()
    {
        // Arrange
        var query = new GetTicketByRadicadoQuery("invalid-radicado");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Handle_WhenTicketDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        var query = new GetTicketByRadicadoQuery("2026-00009999");
        _ticketRepositoryMock
            .Setup(r => r.GetByRadicadoAsync(It.IsAny<RadicadoNumber>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PqrsdfTicket?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("PqrsdfTicket.NotFound");
    }

    [Fact]
    public async Task Handle_WhenTicketExists_ShouldReturnPublicTicketStatusDtoWithoutPII()
    {
        // Arrange
        var radicadoStr = "2026-00000001";
        var radicado = RadicadoNumber.Create(radicadoStr).Value;
        var areaId = Guid.NewGuid();
        var area = DestinationArea.Create(areaId, "Atención al Ciudadano", "ATC").Value;

        var applicant = Applicant.Create(
            "Juan Pérez",
            IdentificationType.CC,
            "1234567890",
            "juan.perez@example.com",
            "3001234567").Value;

        var dueDate = DueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)), 15).Value;
        var ticket = PqrsdfTicket.Create(
            Guid.NewGuid(),
            radicado,
            PqrsdfType.Petition,
            areaId,
            isAnonymous: false,
            applicant,
            "Solicitud de certificado catastral",
            "Por medio de la presente solicito el certificado catastral del predio.",
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
        dto.RadicadoNumber.Should().Be(radicadoStr);
        dto.RequestType.Should().Be("Petition");
        dto.DestinationAreaName.Should().Be("Atención al Ciudadano");
        dto.Subject.Should().Be("Solicitud de certificado catastral");
        dto.Description.Should().Be("Por medio de la presente solicito el certificado catastral del predio.");
        dto.Status.Should().Be("Registered");
        dto.Timeline.Should().HaveCount(5);
        dto.Timeline[0].Status.Should().Be("Registered");
        dto.Timeline[0].IsCurrent.Should().BeTrue();
        dto.Timeline[0].IsCompleted.Should().BeTrue();
        dto.Resolution.Should().BeNull();
    }
}
