namespace Pqrsdf.Domain.Services;

/// <summary>
/// Domain service interface for computing official Colombian statutory holidays and business days
/// in accordance with Colombian national laws (Law 51 of 1983 - Ley Emiliani).
/// </summary>
public interface IColombianHolidayService
{
    /// <summary>
    /// Checks whether the specified date is an official statutory holiday in Colombia.
    /// </summary>
    bool IsHoliday(DateOnly date);

    /// <summary>
    /// Checks whether the specified date is an official business day in Colombia (neither Saturday, Sunday, nor holiday).
    /// </summary>
    bool IsBusinessDay(DateOnly date);

    /// <summary>
    /// Retrieves all official holidays for the specified calendar year.
    /// </summary>
    IReadOnlySet<DateOnly> GetHolidaysForYear(int year);
}
