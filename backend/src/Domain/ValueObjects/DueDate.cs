using Pqrsdf.Domain.Shared.Results;
using Pqrsdf.Domain.Shared.ValueObjects;

namespace Pqrsdf.Domain.ValueObjects;

/// <summary>
/// Value object representing a legal statutory deadline calculated in official Colombian business days.
/// </summary>
public sealed record DueDate : ValueObject
{
    public DateOnly Value { get; }
    public int BusinessDaysCount { get; }

    private DueDate(DateOnly value, int businessDaysCount)
    {
        Value = value;
        BusinessDaysCount = businessDaysCount;
    }

    public static Result<DueDate> Create(DateOnly value, int businessDaysCount)
    {
        if (businessDaysCount is not (15 or 30))
        {
            return Result<DueDate>.Failure(Error.Validation(
                "DueDate.InvalidBusinessDaysCount",
                "El plazo legal en días hábiles debe ser de 15 días (regla general) o 30 días (denuncias)."));
        }

        return Result<DueDate>.Success(new DueDate(value, businessDaysCount));
    }

    public override string ToString() => Value.ToString("yyyy-MM-dd");
}
