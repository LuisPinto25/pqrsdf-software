using FluentAssertions;
using Pqrsdf.Domain.Services;
using Pqrsdf.Infrastructure.Services;
using Xunit;

namespace Pqrsdf.Application.UnitTests.Domain;

public class DueDateCalculatorRemainingDaysTests
{
    private readonly IColombianHolidayService _holidayService = new ColombianHolidayService();
    private readonly DueDateCalculator _calculator;

    public DueDateCalculatorRemainingDaysTests()
    {
        _calculator = new DueDateCalculator(_holidayService);
    }

    [Fact]
    public void CalculateRemainingBusinessDays_WhenFutureDate_ShouldCountBusinessDaysExcludingHolidaysAndWeekends()
    {
        // Arrange: Today is Friday Jan 2, 2026. Due date is Jan 26, 2026.
        // Between Jan 2 and Jan 26:
        // Jan 5, 6, 7, 8, 9 (5 days)
        // Jan 12 Holiday (0)
        // Jan 13, 14, 15, 16 (4 days)
        // Jan 19, 20, 21, 22, 23 (5 days)
        // Jan 26 (1 day)
        // Total = 15 business days
        var today = new DateOnly(2026, 1, 2);
        var dueDate = new DateOnly(2026, 1, 26);

        // Act
        int remaining = _calculator.CalculateRemainingBusinessDays(today, dueDate);

        // Assert
        remaining.Should().Be(15);
    }

    [Fact]
    public void CalculateRemainingBusinessDays_WhenTodayEqualsDueDate_ShouldReturnZero()
    {
        // Arrange
        var today = new DateOnly(2026, 1, 26);
        var dueDate = new DateOnly(2026, 1, 26);

        // Act
        int remaining = _calculator.CalculateRemainingBusinessDays(today, dueDate);

        // Assert
        remaining.Should().Be(0);
    }

    [Fact]
    public void CalculateRemainingBusinessDays_WhenTodayIsPastDueDate_ShouldReturnZero()
    {
        // Arrange
        var today = new DateOnly(2026, 1, 28);
        var dueDate = new DateOnly(2026, 1, 26);

        // Act
        int remaining = _calculator.CalculateRemainingBusinessDays(today, dueDate);

        // Assert
        remaining.Should().Be(0);
    }

    [Fact]
    public void CalculateOverdueBusinessDays_WhenTodayIsPastDueDate_ShouldCountOnlyBusinessDaysElapsed()
    {
        // Arrange: Due date was Friday Jan 9, 2026.
        // Today is Tuesday Jan 13, 2026.
        // Jan 10 (Saturday), Jan 11 (Sunday)
        // Jan 12 (Monday - Reyes Magos Holiday!)
        // Jan 13 (Tuesday - Business day! -> 1 business day overdue)
        var dueDate = new DateOnly(2026, 1, 9);
        var today = new DateOnly(2026, 1, 13);

        // Act
        int overdue = _calculator.CalculateOverdueBusinessDays(today, dueDate);

        // Assert
        overdue.Should().Be(1);
    }

    [Fact]
    public void CalculateOverdueBusinessDays_WhenTodayIsBeforeOrEqualToDueDate_ShouldReturnZero()
    {
        // Arrange
        var dueDate = new DateOnly(2026, 1, 26);
        var today = new DateOnly(2026, 1, 20);

        // Act
        int overdueBefore = _calculator.CalculateOverdueBusinessDays(today, dueDate);
        int overdueSameDay = _calculator.CalculateOverdueBusinessDays(dueDate, dueDate);

        // Assert
        overdueBefore.Should().Be(0);
        overdueSameDay.Should().Be(0);
    }
}
