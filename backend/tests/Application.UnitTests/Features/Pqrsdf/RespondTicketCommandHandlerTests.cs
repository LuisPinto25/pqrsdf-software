using FluentAssertions;
using Moq;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.RespondTicket;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.ValueObjects;
using Xunit;

namespace Pqrsdf.Application.UnitTests.Features.Pqrsdf;

public class RespondTicketCommandHandlerTests
{
    private readonly Mock<IPqrsdfTicketRepository> _ticketRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ITicketStatusHistoryRepository> _historyRepoMock = new();

    private readonly RespondTicketCommandHandler _handler;

    public RespondTicketCommandHandlerTests()
    {
        _handler = new RespondTicketCommandHandler(
            _ticketRepoMock.Object,
            _userRepoMock.Object,
            _historyRepoMock.Object);
    }

    private PqrsdfTicket CreateTicket(Guid officialId, string radicadoStr = "2026-00000010")
    {
        var radicado = RadicadoNumber.Create(radicadoStr).Value;
        var ticket = PqrsdfTicket.Create(
            Guid.NewGuid(),
            radicado,
            PqrsdfType.Petition,
            Guid.NewGuid(),
            false,
            Applicant.Create("Ana Gómez", IdentificationType.CC, "52000111", "ana@example.com", "3110000000").Value,
            "Asunto prueba",
            "Descripción de la solicitud para prueba de respuesta",
            DueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)), 15).Value).Value;

        ticket.AssignToOfficial(officialId, Guid.NewGuid(), null, 0, DateTime.UtcNow);
        return ticket;
    }

    [Fact]
    public async Task Handle_WhenAssignedOfficial_ClosesTicketAndPersistsAuditHistory()
    {
        var officialId = Guid.NewGuid();
        var ticket = CreateTicket(officialId, "2026-00000011");

        _ticketRepoMock.Setup(r => r.GetByRadicadoAsync(ticket.RadicadoNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _userRepoMock.Setup(r => r.GetByIdAsync(officialId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(User.Create(officialId, Email.Create("funcionario@pqrsdf.gov.co").Value, PasswordHash.Create("$2a$11$fzrWhddqPT09gX.1iZRKf.pOLf4epbqVmw1qUhwF2DBU07Tfi2Vgy").Value, "Funcionario Juan", UserRole.Funcionario, true).Value);

        var command = new RespondTicketCommand(
            "2026-00000011",
            "Esta es la respuesta institucional completa y formal al requerimiento del ciudadano.",
            officialId,
            "Funcionario");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(5);
        result.Value.StatusName.Should().Be("Cerrado");
        ticket.Status.Should().Be(TicketStatus.Closed);

        _historyRepoMock.Verify(h => h.AddAsync(It.IsAny<TicketStatusHistory>(), It.IsAny<CancellationToken>()), Times.Once);
        _ticketRepoMock.Verify(r => r.UpdateAsync(ticket, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAssignedAndNotAdmin_ReturnsForbidden()
    {
        var assignedId = Guid.NewGuid();
        var unauthorizedId = Guid.NewGuid();
        var ticket = CreateTicket(assignedId, "2026-00000012");

        _ticketRepoMock.Setup(r => r.GetByRadicadoAsync(ticket.RadicadoNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var command = new RespondTicketCommand(
            "2026-00000012",
            "Intento de respuesta no autorizado.",
            unauthorizedId,
            "Funcionario");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PqrsdfTicket.Forbidden");
    }

    [Fact]
    public async Task Handle_WhenAdminResponds_SucceedsEvenIfNotAssigned()
    {
        var assignedId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var ticket = CreateTicket(assignedId, "2026-00000013");

        _ticketRepoMock.Setup(r => r.GetByRadicadoAsync(ticket.RadicadoNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _userRepoMock.Setup(r => r.GetByIdAsync(adminId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(User.Create(adminId, Email.Create("admin@pqrsdf.gov.co").Value, PasswordHash.Create("$2a$11$fzrWhddqPT09gX.1iZRKf.pOLf4epbqVmw1qUhwF2DBU07Tfi2Vgy").Value, "Admin Principal", UserRole.Administrador, true).Value);

        var command = new RespondTicketCommand(
            "2026-00000013",
            "Respuesta emitida directamente por el Administrador del Sistema.",
            adminId,
            "Administrador");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        ticket.Status.Should().Be(TicketStatus.Closed);
    }

    [Fact]
    public async Task Handle_WhenResponseTextTooShort_ReturnsValidationError()
    {
        var officialId = Guid.NewGuid();
        var ticket = CreateTicket(officialId, "2026-00000014");

        _ticketRepoMock.Setup(r => r.GetByRadicadoAsync(ticket.RadicadoNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var command = new RespondTicketCommand(
            "2026-00000014",
            "Corto",
            officialId,
            "Funcionario");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PqrsdfTicket.InvalidResponseText");
    }
}
