using FluentAssertions;
using Moq;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.ReassignTicket;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.ValueObjects;
using Xunit;

namespace Pqrsdf.Application.UnitTests.Features.Pqrsdf;

public class ReassignTicketCommandHandlerTests
{
    private readonly Mock<IPqrsdfTicketRepository> _ticketRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ITicketAssignmentHistoryRepository> _historyRepoMock = new();

    private readonly ReassignTicketCommandHandler _handler;

    public ReassignTicketCommandHandlerTests()
    {
        _handler = new ReassignTicketCommandHandler(
            _ticketRepoMock.Object,
            _userRepoMock.Object,
            _historyRepoMock.Object);
    }

    [Fact]
    public async Task ReassignTicket_WhenJustificationTooShort_ReturnsValidationFailure()
    {
        var radicadoStr = "2026-00000001";
        var radicado = RadicadoNumber.Create(radicadoStr).Value;
        var officialId1 = Guid.NewGuid();
        var officialId2 = Guid.NewGuid();

        var ticket = PqrsdfTicket.Create(
            Guid.NewGuid(),
            radicado,
            PqrsdfType.Denunciation,
            Guid.NewGuid(),
            true,
            null,
            "Denuncia urgente",
            "Descripción de prueba mayor a diez caracteres",
            DueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)), 30).Value).Value;

        ticket.AssignToOfficial(officialId1, Guid.NewGuid(), null, 0, DateTime.UtcNow);

        _ticketRepoMock.Setup(r => r.GetByRadicadoAsync(radicado, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var command = new ReassignTicketCommand(radicadoStr, new ReassignTicketRequest(officialId2, "Corta"), Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TicketAssignmentHistory.JustificationRequired");
    }

    [Fact]
    public async Task ReassignTicket_WhenTargetOfficialAtCapacity_ReturnsConflict()
    {
        var radicadoStr = "2026-00000001";
        var radicado = RadicadoNumber.Create(radicadoStr).Value;
        var officialId1 = Guid.NewGuid();
        var officialId2 = Guid.NewGuid();

        var ticket = PqrsdfTicket.Create(
            Guid.NewGuid(),
            radicado,
            PqrsdfType.Denunciation,
            Guid.NewGuid(),
            true,
            null,
            "Denuncia urgente",
            "Descripción de prueba mayor a diez caracteres",
            DueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)), 30).Value).Value;

        ticket.AssignToOfficial(officialId1, Guid.NewGuid(), null, 0, DateTime.UtcNow);

        var newOfficial = User.Create(
            officialId2,
            Email.Create("oficial2@pqrsdf.gov.co").Value,
            PasswordHash.Create("$2a$11$fzrWhddqPT09gX.1iZRKf.pOLf4epbqVmw1qUhwF2DBU07Tfi2Vgy").Value,
            "Oficial Dos",
            UserRole.Funcionario).Value;

        _ticketRepoMock.Setup(r => r.GetByRadicadoAsync(radicado, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _userRepoMock.Setup(r => r.GetByIdAsync(officialId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newOfficial);

        _ticketRepoMock.Setup(r => r.CountActiveByOfficialIdAsync(officialId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var command = new ReassignTicketCommand(
            radicadoStr,
            new ReassignTicketRequest(officialId2, "Motivo válido mayor a diez caracteres"),
            Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PqrsdfTicket.OfficialWorkloadLimitReached");
    }

    [Fact]
    public async Task ReassignTicket_WhenValid_UpdatesAssignmentAndPersistsAuditHistory()
    {
        var radicadoStr = "2026-00000001";
        var radicado = RadicadoNumber.Create(radicadoStr).Value;
        var officialId1 = Guid.NewGuid();
        var officialId2 = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var ticket = PqrsdfTicket.Create(
            Guid.NewGuid(),
            radicado,
            PqrsdfType.Denunciation,
            Guid.NewGuid(),
            true,
            null,
            "Denuncia urgente",
            "Descripción de prueba mayor a diez caracteres",
            DueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)), 30).Value).Value;

        ticket.AssignToOfficial(officialId1, adminId, null, 0, DateTime.UtcNow);

        var newOfficial = User.Create(
            officialId2,
            Email.Create("oficial2@pqrsdf.gov.co").Value,
            PasswordHash.Create("$2a$11$fzrWhddqPT09gX.1iZRKf.pOLf4epbqVmw1qUhwF2DBU07Tfi2Vgy").Value,
            "Oficial Dos",
            UserRole.Funcionario).Value;

        _ticketRepoMock.Setup(r => r.GetByRadicadoAsync(radicado, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _userRepoMock.Setup(r => r.GetByIdAsync(officialId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newOfficial);

        _ticketRepoMock.Setup(r => r.CountActiveByOfficialIdAsync(officialId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var command = new ReassignTicketCommand(
            radicadoStr,
            new ReassignTicketRequest(officialId2, "Traslado de trámite por redistribución de carga"),
            adminId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.NewAssignedUserId.Should().Be(officialId2);
        result.Value.PreviousAssignedUserId.Should().Be(officialId1);

        _ticketRepoMock.Verify(r => r.UpdateAsync(ticket, It.IsAny<CancellationToken>()), Times.Once);
        _historyRepoMock.Verify(r => r.AddAsync(It.Is<TicketAssignmentHistory>(h => h.Type == AssignmentType.Reassignment), It.IsAny<CancellationToken>()), Times.Once);
    }
}
