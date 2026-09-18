using FluentAssertions;
using Pqrsdf.Infrastructure.Services;
using Xunit;

namespace Pqrsdf.Application.UnitTests.Domain;

public class ColombianHolidayServiceTests
{
    private readonly ColombianHolidayService _holidayService = new();

    [Theory]
    [InlineData(2026, 1, 1)]   // Año Nuevo
    [InlineData(2026, 1, 12)]  // Reyes Magos (shifted Jan 6 -> Jan 12)
    [InlineData(2026, 3, 23)]  // San José (shifted Mar 19 -> Mar 23)
    [InlineData(2026, 4, 2)]   // Jueves Santo
    [InlineData(2026, 4, 3)]   // Viernes Santo
    [InlineData(2026, 5, 1)]   // Día del Trabajo
    [InlineData(2026, 5, 18)]  // Ascensión del Señor
    [InlineData(2026, 6, 8)]   // Corpus Christi
    [InlineData(2026, 6, 15)]  // Sagrado Corazón
    [InlineData(2026, 6, 29)]  // San Pedro y San Pablo
    [InlineData(2026, 7, 20)]  // Independencia de Colombia
    [InlineData(2026, 8, 7)]   // Batalla de Boyacá
    [InlineData(2026, 8, 17)]  // Asunción de la Virgen (shifted Aug 15 -> Aug 17)
    [InlineData(2026, 10, 12)] // Día de la Raza
    [InlineData(2026, 11, 2)]  // Todos los Santos (shifted Nov 1 -> Nov 2)
    [InlineData(2026, 11, 16)] // Independencia de Cartagena (shifted Nov 11 -> Nov 16)
    [InlineData(2026, 12, 8)]  // Inmaculada Concepción
    [InlineData(2026, 25, 25)] // will not match, test below
    public void IsHoliday_ShouldReturnTrue_ForStatutoryColombianHolidays(int year, int month, int day)
    {
        if (month > 12) return;
        var date = new DateOnly(year, month, day);
        _holidayService.IsHoliday(date).Should().BeTrue();
    }

    [Fact]
    public void IsHoliday_ShouldReturnTrue_ForChristmas()
    {
        var christmas = new DateOnly(2026, 12, 25);
        _holidayService.IsHoliday(christmas).Should().BeTrue();
    }

    [Theory]
    [InlineData(2026, 1, 2)]   // Friday after New Year
    [InlineData(2026, 4, 6)]   // Easter Monday (not a holiday in Colombia)
    [InlineData(2026, 7, 21)]  // Day after Independence
    [InlineData(2026, 9, 15)]  // Ordinary weekday in September
    public void IsHoliday_ShouldReturnFalse_ForRegularDays(int year, int month, int day)
    {
        var date = new DateOnly(year, month, day);
        _holidayService.IsHoliday(date).Should().BeFalse();
    }

    [Fact]
    public void GetHolidaysForYear_ShouldReturnExactly18Holidays_ForColombia()
    {
        var holidays2026 = _holidayService.GetHolidaysForYear(2026);
        holidays2026.Should().HaveCount(18);

        var holidays2027 = _holidayService.GetHolidaysForYear(2027);
        holidays2027.Should().HaveCount(18);
    }

    [Theory]
    [InlineData(2026, 1, 3)]   // Saturday
    [InlineData(2026, 1, 4)]   // Sunday
    [InlineData(2026, 7, 20)]  // Monday holiday
    public void IsBusinessDay_ShouldReturnFalse_ForWeekendsAndHolidays(int year, int month, int day)
    {
        var date = new DateOnly(year, month, day);
        _holidayService.IsBusinessDay(date).Should().BeFalse();
    }

    [Theory]
    [InlineData(2026, 1, 2)]   // Friday
    [InlineData(2026, 1, 5)]   // Monday (regular)
    [InlineData(2026, 2, 10)]  // Tuesday
    public void IsBusinessDay_ShouldReturnTrue_ForRegularWeekdays(int year, int month, int day)
    {
        var date = new DateOnly(year, month, day);
        _holidayService.IsBusinessDay(date).Should().BeTrue();
    }
}
