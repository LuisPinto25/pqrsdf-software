using FluentAssertions;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Services;
using Pqrsdf.Infrastructure.Services;
using Xunit;

namespace Pqrsdf.Application.UnitTests.Domain;

public class DueDateCalculationTests
{
    private readonly IColombianHolidayService _holidayService = new ColombianHolidayService();
    private readonly DueDateCalculator _calculator;

    public DueDateCalculationTests()
    {
        _calculator = new DueDateCalculator(_holidayService);
    }

    [Fact]
    public void CalculateDueDate_StandardPetition_ShouldGrant15BusinessDaysStartingNextBusinessDay()
    {
        // Arrange: Friday Jan 2, 2026.
        // Day 1 of counting is Monday Jan 5.
        // Jan 5, 6, 7, 8, 9 (5 days)
        // Jan 12 is Reyes Magos Holiday! (skip)
        // Jan 13, 14, 15, 16 (4 days -> total 9)
        // Jan 17, 18 weekend (skip)
        // Jan 19, 20, 21, 22, 23 (5 days -> total 14)
        // Jan 24, 25 weekend (skip)
        // Jan 26 is day 15!
        var filingDate = new DateOnly(2026, 1, 2);

        // Act
        var result = _calculator.CalculateDueDate(filingDate, PqrsdfType.Petition);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.BusinessDaysCount.Should().Be(15);
        result.Value.Value.Should().Be(new DateOnly(2026, 1, 26));
    }

    [Fact]
    public void CalculateDueDate_Denunciation_ShouldGrant30BusinessDaysExcludingHolidays()
    {
        // Arrange
        var filingDate = new DateOnly(2026, 1, 2);

        // Act
        var result = _calculator.CalculateDueDate(filingDate, PqrsdfType.Denunciation);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.BusinessDaysCount.Should().Be(30);
        result.Value.Value.Should().BeAfter(new DateOnly(2026, 2, 10));
    }

    [Theory]
    [InlineData(PqrsdfType.Petition, 15)]
    [InlineData(PqrsdfType.Complaint, 15)]
    [InlineData(PqrsdfType.Claim, 15)]
    [InlineData(PqrsdfType.Suggestion, 15)]
    [InlineData(PqrsdfType.Compliment, 15)]
    [InlineData(PqrsdfType.Denunciation, 30)]
    public void CalculateDueDate_ShouldAssignCorrectBusinessDays_AccordingToType(PqrsdfType type, int expectedDays)
    {
        var filingDate = new DateOnly(2026, 6, 1);
        var result = _calculator.CalculateDueDate(filingDate, type);

        result.IsSuccess.Should().BeTrue();
        result.Value.BusinessDaysCount.Should().Be(expectedDays);
    }
}
