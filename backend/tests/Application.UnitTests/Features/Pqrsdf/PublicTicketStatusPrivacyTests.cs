using System.Reflection;
using FluentAssertions;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.GetTicketByRadicado;
using Xunit;

namespace Pqrsdf.Application.UnitTests.Features.Pqrsdf;

public class PublicTicketStatusPrivacyTests
{
    [Fact]
    public void PublicTicketStatusDto_ShouldNotContainAnyApplicantPersonalDataProperties()
    {
        // Arrange
        var forbiddenWords = new[]
        {
            "applicant",
            "fullname",
            "identification",
            "idnumber",
            "idtype",
            "email",
            "phone",
            "telephone",
            "cedula",
            "document"
        };

        var properties = typeof(PublicTicketStatusDto).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Act & Assert
        foreach (var prop in properties)
        {
            var lowerName = prop.Name.ToLowerInvariant();
            foreach (var forbidden in forbiddenWords)
            {
                lowerName.Should().NotContain(
                    forbidden,
                    $"Property '{prop.Name}' in PublicTicketStatusDto leaks personal identifying information!");
            }
        }
    }

    [Fact]
    public void AllNestedDtos_ShouldNotContainAnyApplicantPersonalDataProperties()
    {
        // Arrange
        var forbiddenWords = new[]
        {
            "applicant",
            "fullname",
            "identification",
            "idnumber",
            "idtype",
            "email",
            "phone",
            "telephone"
        };

        var milestoneProps = typeof(TicketTimelineMilestoneDto).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var resolutionProps = typeof(TicketResolutionDto).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Act & Assert
        foreach (var prop in milestoneProps.Concat(resolutionProps))
        {
            var lowerName = prop.Name.ToLowerInvariant();
            foreach (var forbidden in forbiddenWords)
            {
                lowerName.Should().NotContain(
                    forbidden,
                    $"Property '{prop.Name}' in nested DTO leaks personal identifying information!");
            }
        }
    }
}
