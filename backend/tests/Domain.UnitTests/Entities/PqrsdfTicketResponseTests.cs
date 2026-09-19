using FluentAssertions;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.ValueObjects;
using Xunit;

namespace Pqrsdf.Domain.UnitTests.Entities;

public sealed class PqrsdfTicketResponseTests
{
    private readonly Guid _ticketId = Guid.NewGuid();
    private readonly Guid _areaId = Guid.NewGuid();
    private readonly Guid _officialId = Guid.NewGuid();

    private PqrsdfTicket CreateTicketInReview()
    {
        var radicado = RadicadoNumber.Create("2026-00000010").Value;
        var dueDate = DueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)), 15).Value;
        var applicant = Applicant.Create(
            "Ciudadano Respuesta",
            IdentificationType.CC,
            "123456789",
            "ciudadano@ejemplo.com",
            "3001234567").Value;

        var ticket = PqrsdfTicket.Create(
            _ticketId,
            radicado,
            PqrsdfType.Petition,
            _areaId,
            false,
            applicant,
            "Asunto para responder",
            "Descripción de la solicitud para prueba de respuesta",
            dueDate).Value;

        ticket.AssignToOfficial(_officialId, Guid.NewGuid(), "Asignado para respuesta", 1, DateTime.UtcNow);
        return ticket;
    }

    [Fact]
    public void CloseWithResponse_WhenValid_TransitionsToClosedAndSetsResponse()
    {
        var ticket = CreateTicketInReview();
        var now = DateTime.UtcNow;
        var responseText = "Se brinda respuesta formal y detallada a los requerimientos del ciudadano solicitante.";

        var result = ticket.CloseWithResponse(responseText, now);

        result.IsSuccess.Should().BeTrue();
        ticket.Status.Should().Be(TicketStatus.Closed);
        ticket.ResponseText.Should().Be(responseText);
        ticket.ResponseDateUtc.Should().Be(now);
        ticket.UpdatedAtUtc.Should().Be(now);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Corto")]
    [InlineData("123456789")]
    public void CloseWithResponse_WhenInvalidLength_ReturnsValidationError(string? invalidText)
    {
        var ticket = CreateTicketInReview();

        var result = ticket.CloseWithResponse(invalidText, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PqrsdfTicket.InvalidResponseText");
    }

    [Fact]
    public void CloseWithResponse_WhenTooLong_ReturnsValidationError()
    {
        var ticket = CreateTicketInReview();
        var longText = new string('A', 4001);

        var result = ticket.CloseWithResponse(longText, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PqrsdfTicket.InvalidResponseText");
    }

    [Fact]
    public void CloseWithResponse_WhenAlreadyClosed_ReturnsConflict()
    {
        var ticket = CreateTicketInReview();
        ticket.CloseWithResponse("Primera respuesta formal que cierra el caso.", DateTime.UtcNow);

        var result = ticket.CloseWithResponse("Segunda respuesta que no debería permitirse.", DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PqrsdfTicket.AlreadyClosed");
    }

    [Fact]
    public void ChangeOperationalStatus_WhenInReview_UpdatesStatusAndTimestamp()
    {
        var ticket = CreateTicketInReview();
        var now = DateTime.UtcNow;

        var result = ticket.ChangeOperationalStatus(TicketStatus.InReview, now);

        result.IsSuccess.Should().BeTrue();
        ticket.Status.Should().Be(TicketStatus.InReview);
        ticket.UpdatedAtUtc.Should().Be(now);
    }

    [Fact]
    public void ChangeOperationalStatus_WhenAlreadyClosed_ReturnsConflict()
    {
        var ticket = CreateTicketInReview();
        ticket.CloseWithResponse("Respuesta formal para cerrar.", DateTime.UtcNow);

        var result = ticket.ChangeOperationalStatus(TicketStatus.InReview, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PqrsdfTicket.AlreadyClosed");
    }
}
