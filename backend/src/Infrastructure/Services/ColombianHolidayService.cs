using System.Collections.Concurrent;
using Pqrsdf.Domain.Services;

namespace Pqrsdf.Infrastructure.Services;

/// <summary>
/// Domain service implementation for computing official statutory holidays in Colombia
/// pursuant to Law 51 of 1983 (Ley Emiliani) and Gregorian Easter calculations.
/// </summary>
public sealed class ColombianHolidayService : IColombianHolidayService
{
    private readonly ConcurrentDictionary<int, HashSet<DateOnly>> _holidaysCache = new();

    public bool IsHoliday(DateOnly date)
    {
        var holidays = GetHolidaysSet(date.Year);
        return holidays.Contains(date);
    }

    public bool IsBusinessDay(DateOnly date)
    {
        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            return false;
        }

        return !IsHoliday(date);
    }

    public IReadOnlySet<DateOnly> GetHolidaysForYear(int year)
    {
        return GetHolidaysSet(year);
    }

    private HashSet<DateOnly> GetHolidaysSet(int year)
    {
        return _holidaysCache.GetOrAdd(year, CalculateHolidaysForYear);
    }

    private static HashSet<DateOnly> CalculateHolidaysForYear(int year)
    {
        var holidays = new HashSet<DateOnly>(18);

        // 1. Fixed date holidays (never shifted)
        holidays.Add(new DateOnly(year, 1, 1));   // Año Nuevo
        holidays.Add(new DateOnly(year, 5, 1));   // Día del Trabajo
        holidays.Add(new DateOnly(year, 7, 20));  // Grito de Independencia
        holidays.Add(new DateOnly(year, 8, 7));   // Batalla de Boyacá
        holidays.Add(new DateOnly(year, 12, 8));  // Inmaculada Concepción
        holidays.Add(new DateOnly(year, 12, 25)); // Navidad

        // 2. Emiliani fixed holidays (shifted to next Monday if not already Monday)
        holidays.Add(ShiftToNextMondayIfApplicable(new DateOnly(year, 1, 6)));   // Epifanía / Reyes Magos
        holidays.Add(ShiftToNextMondayIfApplicable(new DateOnly(year, 3, 19)));  // San José
        holidays.Add(ShiftToNextMondayIfApplicable(new DateOnly(year, 6, 29)));  // San Pedro y San Pablo
        holidays.Add(ShiftToNextMondayIfApplicable(new DateOnly(year, 8, 15)));  // Asunción de la Virgen
        holidays.Add(ShiftToNextMondayIfApplicable(new DateOnly(year, 10, 12))); // Día de la Raza
        holidays.Add(ShiftToNextMondayIfApplicable(new DateOnly(year, 11, 1)));  // Todos los Santos
        holidays.Add(ShiftToNextMondayIfApplicable(new DateOnly(year, 11, 11))); // Independencia de Cartagena

        // 3. Easter-dependent movable holidays
        var easter = CalculateEasterSunday(year);

        // Holy Thursday and Good Friday (never shifted)
        holidays.Add(easter.AddDays(-3)); // Jueves Santo
        holidays.Add(easter.AddDays(-2)); // Viernes Santo

        // Movable Easter feasts shifted to Monday
        holidays.Add(easter.AddDays(43)); // Ascensión del Señor (Easter + 43 days = 6th Monday after Easter)
        holidays.Add(easter.AddDays(64)); // Corpus Christi (Easter + 64 days = 9th Monday after Easter)
        holidays.Add(easter.AddDays(71)); // Sagrado Corazón de Jesús (Easter + 71 days = 10th Monday after Easter)

        return holidays;
    }

    private static DateOnly ShiftToNextMondayIfApplicable(DateOnly date)
    {
        if (date.DayOfWeek == DayOfWeek.Monday)
        {
            return date;
        }

        int daysUntilMonday = ((int)DayOfWeek.Monday - (int)date.DayOfWeek + 7) % 7;
        return date.AddDays(daysUntilMonday);
    }

    /// <summary>
    /// Calculates Easter Sunday using the Anonymous Gregorian algorithm (Meeus/Jones/Butcher algorithm).
    /// </summary>
    private static DateOnly CalculateEasterSunday(int year)
    {
        int a = year % 19;
        int b = year / 100;
        int c = year % 100;
        int d = b / 4;
        int e = b % 4;
        int f = (b + 8) / 25;
        int g = (b - f + 1) / 3;
        int h = (19 * a + b - d - g + 15) % 30;
        int i = c / 4;
        int k = c % 4;
        int l = (32 + 2 * e + 2 * i - h - k) % 7;
        int m = (a + 11 * h + 22 * l) / 451;
        int month = (h + l - 7 * m + 114) / 31;
        int day = ((h + l - 7 * m + 114) % 31) + 1;

        return new DateOnly(year, month, day);
    }
}
