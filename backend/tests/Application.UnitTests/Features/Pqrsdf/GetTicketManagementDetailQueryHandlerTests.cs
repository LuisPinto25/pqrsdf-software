using FluentAssertions;
using Moq;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.GetTicketManagementDetail;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.Services;
using Pqrsdf.Domain.ValueObjects;
using Pqrsdf.Infrastructure.Services;
using Xunit;

namespace Pqrsdf.Application.UnitTests.Features.Pqrsdf;

public class GetTicketManagementDetailQueryHandlerTests
{
    private readonly Mock<IPqrsdfTicketRepository> _ticketRepoMock = new();
    private readonly Mock<IDestinationAreaRepository> _areaRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ITicketStatusHistoryRepository> _historyRepoMock = new();
    private readonly DueDateCalculator _dueDateCalculator = new(new ColombianHolidayService());

    private readonly GetTicketManagementDetailQueryHandler _handler;

    public GetTicketManagementDetailQueryHandlerTests()
    {
        _handler = new GetTicketManagementDetailQueryHandler(
            _ticketRepoMock.Object,
            _areaRepoMock.Object,
            _userRepoMock.Object,
            _historyRepoMock.Object,
            _dueDateCalculator);
    }

    [Fact]
    public async Task Handle_WhenAssignedOfficialRequests_ReturnsFullDetail()
    {
        var officialId = Guid.NewGuid();
        var radicado = RadicadoNumber.Create("2026-00000001").Value;
        var areaId = Guid.NewGuid();

        var ticket = PqrsdfTicket.Create(
            Guid.NewGuid(),
            radicado,
            PqrsdfType.Petition,
            areaId,
            false,
            Applicant.Create("Carlos Rodríguez", IdentificationType.CC, "1098765432", "carlos@example.com", "3001234567").Value,
            "Asunto prueba",
            "Descripción detallada de la solicitud de prueba",
            DueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), 15).Value).Value;

        ticket.AssignToOfficial(officialId, Guid.NewGuid(), "Instrucciones de asignación", 0, DateTime.UtcNow);

        _ticketRepoMock.Setup(r => r.GetByRadicadoAsync(radicado, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _areaRepoMock.Setup(r => r.GetByIdAsync(areaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DestinationArea.Create(areaId, "Secretaría General", "SEC", true).Value);

        _historyRepoMock.Setup(r => r.GetByTicketIdAsync(ticket.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TicketStatusHistory>());

        var query = new GetTicketManagementDetailQuery("2026-00000001", officialId, "Funcionario");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RadicadoNumber.Should().Be("2026-00000001");
        result.Value.IsAssignedToCurrentUser.Should().BeTrue();
        result.Value.CanManage.Should().BeTrue();
        result.Value.Applicant.Should().NotBeNull();
        result.Value.Applicant!.FullName.Should().Be("Carlos Rodríguez");
    }

    [Fact]
    public async Task Handle_WhenUserNotAssignedAndNotAdmin_ReturnsForbidden()
    {
        var assignedOfficialId = Guid.NewGuid();
        var anotherOfficialId = Guid.NewGuid();
        var radicado = RadicadoNumber.Create("2026-00000002").Value;
        var areaId = Guid.NewGuid();

        var ticket = PqrsdfTicket.Create(
            Guid.NewGuid(),
            radicado,
            PqrsdfType.Denunciation,
            areaId,
            true,
            null,
            "Asunto prueba",
            "Descripción detallada de la solicitud de prueba",
            DueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), 30).Value).Value;

        ticket.AssignToOfficial(assignedOfficialId, Guid.NewGuid(), null, 0, DateTime.UtcNow);

        _ticketRepoMock.Setup(r => r.GetByRadicadoAsync(radicado, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var query = new GetTicketManagementDetailQuery("2026-00000002", anotherOfficialId, "Funcionario");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PqrsdfTicket.Forbidden");
    }

    [Fact]
    public async Task Handle_WhenAdminRequests_ReturnsSuccessEvenIfNotAssigned()
    {
        var assignedOfficialId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var radicado = RadicadoNumber.Create("2026-00000003").Value;
        var areaId = Guid.NewGuid();

        var ticket = PqrsdfTicket.Create(
            Guid.NewGuid(),
            radicado,
            PqrsdfType.Denunciation,
            areaId,
            true,
            null,
            "Asunto prueba",
            "Descripción detallada de la solicitud de prueba",
            DueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), 30).Value).Value;

        ticket.AssignToOfficial(assignedOfficialId, adminId, null, 0, DateTime.UtcNow);

        _ticketRepoMock.Setup(r => r.GetByRadicadoAsync(radicado, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _historyRepoMock.Setup(r => r.GetByTicketIdAsync(ticket.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TicketStatusHistory>());

        var query = new GetTicketManagementDetailQuery("2026-00000003", adminId, "Administrador");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CanManage.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenTicketNotFound_ReturnsNotFound()
    {
        var radicado = RadicadoNumber.Create("2026-00000099").Value;
        _ticketRepoMock.Setup(r => r.GetByRadicadoAsync(radicado, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PqrsdfTicket?)null);

        var query = new GetTicketManagementDetailQuery("2026-00000099", Guid.NewGuid(), "Funcionario");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PqrsdfTicket.NotFound");
    }
}
