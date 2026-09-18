using System.Text.RegularExpressions;
using Pqrsdf.Domain.Shared.Results;
using Pqrsdf.Domain.Shared.ValueObjects;

namespace Pqrsdf.Domain.ValueObjects;

/// <summary>
/// Value object representing an official Colombian PQRSDF tracking number with format YYYY-NNNNNNNN.
/// </summary>
public sealed partial record RadicadoNumber : ValueObject
{
    private static readonly Regex FormatRegex = GetFormatRegex();

    public string Value { get; }

    private RadicadoNumber(string value)
    {
        Value = value;
    }

    public static Result<RadicadoNumber> Create(int year, long sequence)
    {
        if (year < 2000 || year > 2100)
        {
            return Result<RadicadoNumber>.Failure(Error.Validation(
                "RadicadoNumber.InvalidYear",
                "El año del radicado debe ser válido."));
        }

        if (sequence <= 0 || sequence > 99_999_999)
        {
            return Result<RadicadoNumber>.Failure(Error.Validation(
                "RadicadoNumber.InvalidSequence",
                "El consecutivo del radicado debe estar entre 1 y 99999999."));
        }

        string formatted = $"{year:D4}-{sequence:D8}";
        return Result<RadicadoNumber>.Success(new RadicadoNumber(formatted));
    }

    public static Result<RadicadoNumber> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<RadicadoNumber>.Failure(Error.Validation(
                "RadicadoNumber.Required",
                "El número de radicado es obligatorio."));
        }

        string trimmed = value.Trim();
        if (!FormatRegex.IsMatch(trimmed))
        {
            return Result<RadicadoNumber>.Failure(Error.Validation(
                "RadicadoNumber.InvalidFormat",
                "El número de radicado debe tener el formato AAAA-NNNNNNNN (ej. 2026-00000001)."));
        }

        return Result<RadicadoNumber>.Success(new RadicadoNumber(trimmed));
    }

    public int GetYear() => int.Parse(Value[..4]);

    public long GetSequence() => long.Parse(Value[5..]);

    public override string ToString() => Value;

    [GeneratedRegex(@"^\d{4}-\d{8}$")]
    private static partial Regex GetFormatRegex();
}
