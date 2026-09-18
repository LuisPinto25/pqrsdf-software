namespace Pqrsdf.Infrastructure.Persistence.Entities;

/// <summary>
/// Persistence entity tracking the latest sequential counter for radicado generation per calendar year.
/// </summary>
public sealed class RadicadoSequence
{
    public int Year { get; set; }
    public long CurrentValue { get; set; }

    public RadicadoSequence() { }

    public RadicadoSequence(int year, long currentValue)
    {
        Year = year;
        CurrentValue = currentValue;
    }
}
