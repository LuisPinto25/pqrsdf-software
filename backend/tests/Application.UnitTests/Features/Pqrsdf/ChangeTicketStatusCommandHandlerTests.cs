using FluentAssertions;
using Moq;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.ChangeTicketStatus;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.ValueObjects;
using Xunit;

namespace Pqrsdf.Application.UnitTests.Features.Pqrsdf;

public class ChangeTicketStatusCommandHandlerTests
{
    private readonly Mock<IPqrsdfTicketRepository> _ticketRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ITicketStatusHistoryRepository> _historyRepoMock = new();

    private readonly ChangeTicketStatusCommandHandler _handler;

    public ChangeTicketStatusCommandHandlerTests()
    {
        _handler = new ChangeTicketStatusCommandHandler(
            _ticketRepoMock.Object,
            _userRepoMock.Object,
            _historyRepoMock.Object);
    }

    private PqrsdfTicket CreateTicket(Guid officialId, string radicadoStr = "2026-00000020")
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
            "Descripción de la solicitud para prueba de cambio de estado",
            DueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)), 15).Value).Value;

        ticket.AssignToOfficial(officialId, Guid.NewGuid(), null, 0, DateTime.UtcNow);
        return ticket;
    }

    [Fact]
    public async Task Handle_WhenAssignedOfficial_ChangesStatusAndPersistsAuditHistory()
    {
        var officialId = Guid.NewGuid();
        var ticket = CreateTicket(officialId, "2026-00000021");

        _ticketRepoMock.Setup(r => r.GetByRadicadoAsync(ticket.RadicadoNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _userRepoMock.Setup(r => r.GetByIdAsync(officialId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(User.Create(officialId, Email.Create("funcionario@pqrsdf.gov.co").Value, PasswordHash.Create("$2a$11$fzrWhddqPT09gX.1iZRKf.pOLf4epbqVmw1qUhwF2DBU07Tfi2Vgy").Value, "Funcionario Juan", UserRole.Funcionario, true).Value);

        var command = new ChangeTicketStatusCommand(
            "2026-00000021",
            (int)TicketStatus.InReview,
            "Se requiere concepto técnico previo del área jurídica para continuar el trámite.",
            officialId,
            "Funcionario");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.NewStatus.Should().Be((int)TicketStatus.InReview);
        result.Value.StatusName.Should().Be("En trámite");
        ticket.Status.Should().Be(TicketStatus.InReview);

        _historyRepoMock.Verify(h => h.AddAsync(It.IsAny<TicketStatusHistory>(), It.IsAny<CancellationToken>()), Times.Once);
        _ticketRepoMock.Verify(r => r.UpdateAsync(ticket, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAssignedAndNotAdmin_ReturnsForbidden()
    {
        var assignedId = Guid.NewGuid();
        var unauthorizedId = Guid.NewGuid();
        var ticket = CreateTicket(assignedId, "2026-00000022");

        _ticketRepoMock.Setup(r => r.GetByRadicadoAsync(ticket.RadicadoNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var command = new ChangeTicketStatusCommand(
            "2026-00000022",
            (int)TicketStatus.InReview,
            "Intento de cambio no autorizado.",
            unauthorizedId,
            "Funcionario");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PqrsdfTicket.Forbidden");
    }

    [Fact]
    public async Task Handle_WhenAdminChangesStatus_SucceedsEvenIfNotAssigned()
    {
        var assignedId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var ticket = CreateTicket(assignedId, "2026-00000023");

        _ticketRepoMock.Setup(r => r.GetByRadicadoAsync(ticket.RadicadoNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _userRepoMock.Setup(r => r.GetByIdAsync(adminId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(User.Create(adminId, Email.Create("admin@pqrsdf.gov.co").Value, PasswordHash.Create("$2a$11$fzrWhddqPT09gX.1iZRKf.pOLf4epbqVmw1qUhwF2DBU07Tfi2Vgy").Value, "Admin Principal", UserRole.Administrador, true).Value);

        var command = new ChangeTicketStatusCommand(
            "2026-00000023",
            (int)TicketStatus.InReview,
            "Ajuste de estado realizado directamente por la administración.",
            adminId,
            "Administrador");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        ticket.Status.Should().Be(TicketStatus.InReview);
    }

    [Fact]
    public async Task Handle_WhenJustificationTooShort_ReturnsValidationError()
    {
        var officialId = Guid.NewGuid();
        var ticket = CreateTicket(officialId, "2026-00000024");

        _ticketRepoMock.Setup(r => r.GetByRadicadoAsync(ticket.RadicadoNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var command = new ChangeTicketStatusCommand(
            "2026-00000024",
            (int)TicketStatus.InReview,
            "Corta", // Less than 10 chars
            officialId,
            "Funcionario");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TicketStatusHistory.JustificationRequired");
    }

    [Fact]
    public async Task Handle_WhenTicketAlreadyClosed_ReturnsConflict()
    {
        var officialId = Guid.NewGuid();
        var ticket = CreateTicket(officialId, "2026-00000025");
        ticket.CloseWithResponse("Respuesta previa institucional válida", DateTime.UtcNow);

        _ticketRepoMock.Setup(r => r.GetByRadicadoAsync(ticket.RadicadoNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var command = new ChangeTicketStatusCommand(
            "2026-00000025",
            (int)TicketStatus.InReview,
            "Intento de modificar un ticket ya cerrado.",
            officialId,
            "Funcionario");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PqrsdfTicket.AlreadyClosed");
    }

    [Fact]
    public async Task Handle_WhenNewStatusIsClosed_ReturnsValidationError()
    {
        var officialId = Guid.NewGuid();
        var ticket = CreateTicket(officialId, "2026-00000026");

        _ticketRepoMock.Setup(r => r.GetByRadicadoAsync(ticket.RadicadoNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var command = new ChangeTicketStatusCommand(
            "2026-00000026",
            (int)TicketStatus.Closed,
            "Intento de cerrar el ticket por endpoint de estado.",
            officialId,
            "Funcionario");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PqrsdfTicket.InvalidStatusTransition");
    }
}
