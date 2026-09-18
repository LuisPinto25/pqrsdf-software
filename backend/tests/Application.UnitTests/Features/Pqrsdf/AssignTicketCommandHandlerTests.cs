using FluentAssertions;
using Moq;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.AssignTicket;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.GetAssignableOfficials;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.ValueObjects;
using Xunit;

namespace Pqrsdf.Application.UnitTests.Features.Pqrsdf;

public class AssignTicketCommandHandlerTests
{
    private readonly Mock<IPqrsdfTicketRepository> _ticketRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ITicketAssignmentHistoryRepository> _historyRepoMock = new();

    private readonly AssignTicketCommandHandler _assignHandler;
    private readonly GetAssignableOfficialsQueryHandler _officialsHandler;

    public AssignTicketCommandHandlerTests()
    {
        _assignHandler = new AssignTicketCommandHandler(
            _ticketRepoMock.Object,
            _userRepoMock.Object,
            _historyRepoMock.Object);

        _officialsHandler = new GetAssignableOfficialsQueryHandler(
            _userRepoMock.Object,
            _ticketRepoMock.Object);
    }

    [Fact]
    public async Task GetAssignableOfficials_ReturnsOfficialsWithWorkloadAndEligibility()
    {
        // Arrange
        var official1 = User.Create(
            Guid.NewGuid(),
            Email.Create("of1@pqrsdf.gov.co").Value,
            PasswordHash.Create("$2a$11$fzrWhddqPT09gX.1iZRKf.pOLf4epbqVmw1qUhwF2DBU07Tfi2Vgy").Value,
            "Oficial Uno",
            UserRole.Funcionario).Value;

        var official2 = User.Create(
            Guid.NewGuid(),
            Email.Create("of2@pqrsdf.gov.co").Value,
            PasswordHash.Create("$2a$11$fzrWhddqPT09gX.1iZRKf.pOLf4epbqVmw1qUhwF2DBU07Tfi2Vgy").Value,
            "Oficial Dos",
            UserRole.Funcionario).Value;

        _userRepoMock.Setup(r => r.GetActiveOfficialsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { official1, official2 });

        _ticketRepoMock.Setup(r => r.CountActiveByOfficialIdAsync(official1.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        _ticketRepoMock.Setup(r => r.CountActiveByOfficialIdAsync(official2.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var result = await _officialsHandler.Handle(new GetAssignableOfficialsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        var dto1 = result.Value.First(o => o.Id == official1.Id);
        dto1.ActiveTicketsCount.Should().Be(3);
        dto1.CanAssign.Should().BeTrue();

        var dto2 = result.Value.First(o => o.Id == official2.Id);
        dto2.ActiveTicketsCount.Should().Be(5);
        dto2.CanAssign.Should().BeFalse();
    }

    [Fact]
    public async Task AssignTicket_WhenOfficialAtCapacity_ReturnsConflict()
    {
        // Arrange
        var radicadoStr = "2026-00000001";
        var radicado = RadicadoNumber.Create(radicadoStr).Value;
        var officialId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var official = User.Create(
            officialId,
            Email.Create("oficial@pqrsdf.gov.co").Value,
            PasswordHash.Create("$2a$11$fzrWhddqPT09gX.1iZRKf.pOLf4epbqVmw1qUhwF2DBU07Tfi2Vgy").Value,
            "Funcionario Test",
            UserRole.Funcionario).Value;

        var ticket = PqrsdfTicket.Create(
            Guid.NewGuid(),
            radicado,
            PqrsdfType.Denunciation,
            Guid.NewGuid(),
            true,
            null,
            "Denuncia urgente",
            "Descripción de prueba para asignación de funcionario",
            DueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)), 30).Value).Value;

        _ticketRepoMock.Setup(r => r.GetByRadicadoAsync(radicado, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _userRepoMock.Setup(r => r.GetByIdAsync(officialId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(official);

        _ticketRepoMock.Setup(r => r.CountActiveByOfficialIdAsync(officialId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var command = new AssignTicketCommand(radicadoStr, new AssignTicketRequest(officialId, "Nota"), adminId);

        // Act
        var result = await _assignHandler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PqrsdfTicket.OfficialWorkloadLimitReached");
    }

    [Fact]
    public async Task AssignTicket_WhenValid_TransitionsToInReviewAndSavesAudit()
    {
        // Arrange
        var radicadoStr = "2026-00000002";
        var radicado = RadicadoNumber.Create(radicadoStr).Value;
        var officialId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var official = User.Create(
            officialId,
            Email.Create("oficial@pqrsdf.gov.co").Value,
            PasswordHash.Create("$2a$11$fzrWhddqPT09gX.1iZRKf.pOLf4epbqVmw1qUhwF2DBU07Tfi2Vgy").Value,
            "Funcionario Test",
            UserRole.Funcionario).Value;

        var ticket = PqrsdfTicket.Create(
            Guid.NewGuid(),
            radicado,
            PqrsdfType.Denunciation,
            Guid.NewGuid(),
            true,
            null,
            "Denuncia urgente",
            "Descripción de prueba para asignación de funcionario",
            DueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)), 30).Value).Value;

        _ticketRepoMock.Setup(r => r.GetByRadicadoAsync(radicado, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _userRepoMock.Setup(r => r.GetByIdAsync(officialId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(official);

        _ticketRepoMock.Setup(r => r.CountActiveByOfficialIdAsync(officialId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var command = new AssignTicketCommand(radicadoStr, new AssignTicketRequest(officialId, "Instrucción"), adminId);

        // Act
        var result = await _assignHandler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("InReview");
        result.Value.AssignedToUserId.Should().Be(officialId);
        result.Value.AssignedOfficialName.Should().Be("Funcionario Test");

        _ticketRepoMock.Verify(r => r.UpdateAsync(ticket, It.IsAny<CancellationToken>()), Times.Once);
        _historyRepoMock.Verify(r => r.AddAsync(It.IsAny<TicketAssignmentHistory>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
