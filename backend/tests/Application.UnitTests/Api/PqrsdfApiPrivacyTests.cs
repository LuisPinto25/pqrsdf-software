using System.Text.Encodings.Web;
using System.Text.Json;
using FluentAssertions;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.GetTicketByRadicado;
using Pqrsdf.Domain.Shared.Results;
using Xunit;

namespace Pqrsdf.Application.UnitTests.Api;

public class PqrsdfApiPrivacyTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    [Fact]
    public void SerializedResult_WhenTicketContainsPublicData_ShouldNotContainAnyApplicantPIIFields()
    {
        // Arrange
        var timeline = new List<TicketTimelineMilestoneDto>
        {
            new("Registered", "Radicado", DateTime.UtcNow, true, true),
            new("Assigned", "Asignado", null, false, false),
            new("InReview", "En trámite", null, false, false),
            new("Answered", "Respuesta emitida", null, false, false),
            new("Closed", "Cerrado", null, false, false)
        };

        var resolution = new TicketResolutionDto(
            "Respuesta institucional oficial",
            DateTime.UtcNow);

        var dto = new PublicTicketStatusDto(
            "2026-00000001",
            "Petition",
            "Atención al Ciudadano",
            "Asunto de consulta ciudadana",
            "Descripción detallada de la solicitud ciudadana",
            "Registered",
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 22),
            15,
            false,
            null,
            timeline,
            resolution);

        var apiResult = Result<PublicTicketStatusDto>.Success(dto);

        // Act
        var json = JsonSerializer.Serialize(apiResult, JsonOptions);

        // Assert
        var forbiddenTokens = new[]
        {
            "\"applicant\"",
            "\"fullName\"",
            "\"fullname\"",
            "\"identificationType\"",
            "\"identificationNumber\"",
            "\"email\"",
            "\"phoneNumber\"",
            "\"phone\"",
            "\"telephone\"",
            "\"cedula\"",
            "\"documentNumber\"",
            "\"documentType\""
        };

        foreach (var token in forbiddenTokens)
        {
            json.Should().NotContain(token, $"Serialized API response must not expose citizen PII token {token}");
        }

        // Expected properties must be present
        json.Should().Contain("\"radicadoNumber\":\"2026-00000001\"");
        json.Should().Contain("\"requestType\":\"Petition\"");
        json.Should().Contain("\"destinationAreaName\":\"Atención al Ciudadano\"");
        json.Should().Contain("\"subject\":\"Asunto de consulta ciudadana\"");
        json.Should().Contain("\"description\":\"Descripción detallada de la solicitud ciudadana\"");
        json.Should().Contain("\"status\":\"Registered\"");
        json.Should().Contain("\"timeline\":[");
        json.Should().Contain("\"resolution\":{");
    }

    [Fact]
    public void SerializedAnonymousTicket_ShouldAlsoHaveZeroPII()
    {
        // Arrange
        var timeline = new List<TicketTimelineMilestoneDto>
        {
            new("Registered", "Radicado", DateTime.UtcNow, true, true)
        };

        var dto = new PublicTicketStatusDto(
            "2026-00000002",
            "Complaint",
            "Control Interno",
            "Queja anónima",
            "Descripción de la queja anónima",
            "Registered",
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 22),
            15,
            false,
            null,
            timeline,
            null);

        var apiResult = Result<PublicTicketStatusDto>.Success(dto);

        // Act
        var json = JsonSerializer.Serialize(apiResult, JsonOptions);

        // Assert
        json.Should().NotContain("\"applicant\"");
        json.Should().NotContain("\"email\"");
        json.Should().NotContain("\"phone\"");
        json.Should().Contain("\"resolution\":null");
    }
}
