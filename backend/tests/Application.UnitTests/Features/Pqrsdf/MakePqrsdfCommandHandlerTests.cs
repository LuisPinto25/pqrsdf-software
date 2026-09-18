using FluentAssertions;
using Moq;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.MakePqrsdf;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.Services;
using Pqrsdf.Domain.ValueObjects;
using Pqrsdf.Infrastructure.Services;
using Xunit;

namespace Pqrsdf.Application.UnitTests.Features.Pqrsdf;

public class MakePqrsdfCommandHandlerTests
{
    private readonly Mock<IPqrsdfTicketRepository> _ticketRepoMock;
    private readonly Mock<IDestinationAreaRepository> _areaRepoMock;
    private readonly Mock<IRadicadoSequenceGenerator> _sequenceMock;
    private readonly DueDateCalculator _dueDateCalculator;
    private readonly MakePqrsdfCommandHandler _handler;

    private readonly Guid _validAreaId = Guid.NewGuid();

    public MakePqrsdfCommandHandlerTests()
    {
        _ticketRepoMock = new Mock<IPqrsdfTicketRepository>();
        _areaRepoMock = new Mock<IDestinationAreaRepository>();
        _sequenceMock = new Mock<IRadicadoSequenceGenerator>();

        var holidayService = new ColombianHolidayService();
        _dueDateCalculator = new DueDateCalculator(holidayService);

        var activeArea = DestinationArea.Create(_validAreaId, "Atención al Ciudadano", "ATC").Value;
        _areaRepoMock
            .Setup(r => r.GetByIdAsync(_validAreaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeArea);

        _sequenceMock
            .Setup(s => s.NextAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RadicadoNumber.Create(2026, 1).Value);

        _handler = new MakePqrsdfCommandHandler(
            _ticketRepoMock.Object,
            _areaRepoMock.Object,
            _sequenceMock.Object,
            _dueDateCalculator);
    }

    [Fact]
    public async Task Handle_ValidStandardPetition_ShouldReturnSuccessWith15BusinessDays()
    {
        // Arrange
        var applicantReq = new ApplicantRequest(
            "María Fernanda López",
            IdentificationType.CC,
            "1098765432",
            "maria.lopez@example.com",
            "3001234567");

        var request = new MakePqrsdfRequest(
            PqrsdfType.Petition,
            _validAreaId,
            false,
            applicantReq,
            "Solicitud de información sobre licencias",
            "Por favor requiero información sobre el estado de la licencia de construcción radicada.");

        var command = new MakePqrsdfCommand(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.RadicadoNumber.Should().Be("2026-00000001");
        result.Value.Type.Should().Be("Petition");
        result.Value.BusinessDaysCount.Should().Be(15);
        result.Value.Status.Should().Be("Registered");

        _ticketRepoMock.Verify(r => r.AddAsync(It.IsAny<PqrsdfTicket>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidAnonymousDenunciation_ShouldReturnSuccessWith30BusinessDays()
    {
        // Arrange
        var request = new MakePqrsdfRequest(
            PqrsdfType.Denunciation,
            _validAreaId,
            true,
            null,
            "Denuncia sobre presunta irregularidad",
            "Se comunica presunto cobro no autorizado en la ventanilla única de atención.");

        var command = new MakePqrsdfCommand(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.BusinessDaysCount.Should().Be(30);
        result.Value.Type.Should().Be("Denunciation");

        _ticketRepoMock.Verify(r => r.AddAsync(It.Is<PqrsdfTicket>(t => t.IsAnonymous && t.Applicant == null), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AnonymousPetition_ShouldReturnFailure_BecauseAnonymousNotAllowed()
    {
        // Arrange
        var request = new MakePqrsdfRequest(
            PqrsdfType.Petition,
            _validAreaId,
            true,
            null,
            "Petición anónima indebida",
            "Intento de radicar petición anónima.");

        var command = new MakePqrsdfCommand(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("MakePqrsdf.AnonymousNotAllowed");
    }

    [Fact]
    public async Task Handle_StandardFilingWithoutApplicant_ShouldReturnFailure()
    {
        // Arrange
        var request = new MakePqrsdfRequest(
            PqrsdfType.Complaint,
            _validAreaId,
            false,
            null,
            "Queja por mala atención",
            "Queja presentada sin diligenciar datos del ciudadano solicitante.");

        var command = new MakePqrsdfCommand(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("MakePqrsdf.ApplicantRequired");
    }

    [Fact]
    public async Task Handle_InactiveOrNonExistentArea_ShouldReturnFailure()
    {
        // Arrange
        var inactiveAreaId = Guid.NewGuid();
        var inactiveArea = DestinationArea.Create(inactiveAreaId, "Área Inactiva", "INA", false).Value;

        _areaRepoMock
            .Setup(r => r.GetByIdAsync(inactiveAreaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveArea);

        var applicantReq = new ApplicantRequest(
            "Carlos Gómez",
            IdentificationType.CC,
            "12345678",
            "carlos@example.com",
            null);

        var request = new MakePqrsdfRequest(
            PqrsdfType.Suggestion,
            inactiveAreaId,
            false,
            applicantReq,
            "Sugerencia para mejorar horarios",
            "Sugerencia sobre ampliación de horarios de atención los días sábados.");

        var command = new MakePqrsdfCommand(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("MakePqrsdf.AreaNotFoundOrInactive");
    }
}
