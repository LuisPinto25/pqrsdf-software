using FluentAssertions;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.ValueObjects;
using Xunit;

namespace Pqrsdf.Domain.UnitTests.Entities;

public sealed class PqrsdfTicketAssignmentTests
{
    private readonly Guid _ticketId = Guid.NewGuid();
    private readonly Guid _areaId = Guid.NewGuid();
    private readonly Guid _adminId = Guid.NewGuid();
    private readonly Guid _officialId = Guid.NewGuid();
    private readonly Guid _newOfficialId = Guid.NewGuid();

    private PqrsdfTicket CreateValidRegisteredTicket()
    {
        var radicado = RadicadoNumber.Create("2026-00000001").Value;
        var dueDate = DueDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)), 15).Value;
        var applicant = Applicant.Create(
            "Ciudadano de Prueba",
            IdentificationType.CC,
            "123456789",
            "ciudadano@ejemplo.com",
            "3001234567").Value;

        return PqrsdfTicket.Create(
            _ticketId,
            radicado,
            PqrsdfType.Petition,
            _areaId,
            false,
            applicant,
            "Solicitud inicial de prueba",
            "Descripción de prueba para verificación de asignación",
            dueDate).Value;
    }

    [Fact]
    public void AssignToOfficial_WhenValid_TransitionsToInReviewAndSetsOfficial()
    {
        var ticket = CreateValidRegisteredTicket();
        var now = DateTime.UtcNow;

        var result = ticket.AssignToOfficial(_officialId, _adminId, "Nota de prueba", 2, now);

        result.IsSuccess.Should().BeTrue();
        ticket.Status.Should().Be(TicketStatus.InReview);
        ticket.AssignedToUserId.Should().Be(_officialId);
        ticket.AssignedAtUtc.Should().Be(now);
        ticket.AssignmentNote.Should().Be("Nota de prueba");
    }

    [Fact]
    public void AssignToOfficial_WhenStatusNotRegistered_ReturnsConflict()
    {
        var ticket = CreateValidRegisteredTicket();
        ticket.AssignToOfficial(_officialId, _adminId, null, 0, DateTime.UtcNow);

        var result = ticket.AssignToOfficial(Guid.NewGuid(), _adminId, null, 0, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PqrsdfTicket.NotEligibleForAssignment");
    }

    [Fact]
    public void AssignToOfficial_WhenOfficialIdEmpty_ReturnsValidationFailure()
    {
        var ticket = CreateValidRegisteredTicket();

        var result = ticket.AssignToOfficial(Guid.Empty, _adminId, null, 0, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PqrsdfTicket.InvalidOfficialId");
    }

    [Fact]
    public void AssignToOfficial_WhenWorkloadAtOrAboveCapacity_ReturnsConflict()
    {
        var ticket = CreateValidRegisteredTicket();

        var result = ticket.AssignToOfficial(_officialId, _adminId, null, 5, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PqrsdfTicket.OfficialWorkloadLimitReached");
    }

    [Fact]
    public void AssignToOfficial_WhenNoteExceeds500Chars_ReturnsValidationFailure()
    {
        var ticket = CreateValidRegisteredTicket();
        var longNote = new string('A', 501);

        var result = ticket.AssignToOfficial(_officialId, _adminId, longNote, 0, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PqrsdfTicket.AssignmentNoteTooLong");
    }

    [Fact]
    public void ReassignToOfficial_WhenValid_UpdatesAssignedOfficialAndNote()
    {
        var ticket = CreateValidRegisteredTicket();
        var now = DateTime.UtcNow;
        ticket.AssignToOfficial(_officialId, _adminId, null, 0, now);

        var reassignNow = now.AddHours(1);
        var result = ticket.ReassignToOfficial(_newOfficialId, _adminId, "Traslado de solicitud por motivo operativo", 1, reassignNow);

        result.IsSuccess.Should().BeTrue();
        ticket.Status.Should().Be(TicketStatus.InReview);
        ticket.AssignedToUserId.Should().Be(_newOfficialId);
        ticket.AssignedAtUtc.Should().Be(reassignNow);
        ticket.AssignmentNote.Should().Be("Traslado de solicitud por motivo operativo");
    }

    [Fact]
    public void ReassignToOfficial_WhenTicketNotEnTramite_ReturnsConflict()
    {
        var ticket = CreateValidRegisteredTicket();

        var result = ticket.ReassignToOfficial(_newOfficialId, _adminId, "Justificación de prueba", 0, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PqrsdfTicket.NotEligibleForReassignment");
    }

    [Fact]
    public void ReassignToOfficial_WhenReassigningToSameOfficial_ReturnsConflict()
    {
        var ticket = CreateValidRegisteredTicket();
        ticket.AssignToOfficial(_officialId, _adminId, null, 0, DateTime.UtcNow);

        var result = ticket.ReassignToOfficial(_officialId, _adminId, "Justificación de prueba", 0, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PqrsdfTicket.SameOfficialReassignment");
    }

    [Fact]
    public void ReassignToOfficial_WhenJustificationLessThan10Chars_ReturnsValidationFailure()
    {
        var ticket = CreateValidRegisteredTicket();
        ticket.AssignToOfficial(_officialId, _adminId, null, 0, DateTime.UtcNow);

        var result = ticket.ReassignToOfficial(_newOfficialId, _adminId, "Corta", 0, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PqrsdfTicket.JustificationTooShort");
    }

    [Fact]
    public void ReassignToOfficial_WhenTargetWorkloadAtOrAboveCapacity_ReturnsConflict()
    {
        var ticket = CreateValidRegisteredTicket();
        ticket.AssignToOfficial(_officialId, _adminId, null, 0, DateTime.UtcNow);

        var result = ticket.ReassignToOfficial(_newOfficialId, _adminId, "Justificación con longitud válida", 5, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PqrsdfTicket.OfficialWorkloadLimitReached");
    }
}
