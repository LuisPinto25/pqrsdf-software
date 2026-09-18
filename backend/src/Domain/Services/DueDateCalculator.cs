using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Shared.Results;
using Pqrsdf.Domain.ValueObjects;

namespace Pqrsdf.Domain.Services;

/// <summary>
/// Domain service that calculates the official legal response deadline in Colombian statutory business days.
/// </summary>
public sealed class DueDateCalculator
{
    private readonly IColombianHolidayService _holidayService;

    public DueDateCalculator(IColombianHolidayService holidayService)
    {
        _holidayService = holidayService;
    }

    public Result<DueDate> CalculateDueDate(DateOnly filingDate, PqrsdfType type)
    {
        int targetBusinessDays = type == PqrsdfType.Denunciation ? 30 : 15;

        // Per Colombian administrative law and Clarification Q3:
        // Business days count starts on the first business day immediately following the calendar filing date.
        DateOnly currentDate = filingDate;
        int businessDaysAdded = 0;

        while (businessDaysAdded < targetBusinessDays)
        {
            currentDate = currentDate.AddDays(1);
            if (_holidayService.IsBusinessDay(currentDate))
            {
                businessDaysAdded++;
            }
        }

        return DueDate.Create(currentDate, targetBusinessDays);
    }

    /// <summary>
    /// Computes the number of business days remaining between today and the due date.
    /// </summary>
    public int CalculateRemainingBusinessDays(DateOnly today, DateOnly dueDate)
    {
        if (today >= dueDate)
        {
            return 0;
        }

        int remaining = 0;
        DateOnly current = today;

        while (current < dueDate)
        {
            current = current.AddDays(1);
            if (_holidayService.IsBusinessDay(current))
            {
                remaining++;
            }
        }

        return remaining;
    }

    /// <summary>
    /// Computes the number of Colombian business days elapsed past the statutory deadline when overdue.
    /// </summary>
    public int CalculateOverdueBusinessDays(DateOnly today, DateOnly dueDate)
    {
        if (today <= dueDate)
        {
            return 0;
        }

        int overdue = 0;
        DateOnly current = dueDate;

        while (current < today)
        {
            current = current.AddDays(1);
            if (_holidayService.IsBusinessDay(current))
            {
                overdue++;
            }
        }

        return overdue;
    }
}
