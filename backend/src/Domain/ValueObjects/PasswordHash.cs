using Pqrsdf.Domain.Shared.Results;
using Pqrsdf.Domain.Shared.ValueObjects;

namespace Pqrsdf.Domain.ValueObjects;

/// <summary>
/// Value object encapsulating a cryptographic password hash.
/// </summary>
public sealed record PasswordHash : ValueObject
{
    public string Value { get; }

    private PasswordHash(string value)
    {
        Value = value;
    }

    public static Result<PasswordHash> Create(string? hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            return Result<PasswordHash>.Failure(Error.Validation(
                "PasswordHash.Required",
                "El hash de la contraseña no puede estar vacío."));
        }

        string trimmed = hash.Trim();
        if (trimmed.Length < 20)
        {
            return Result<PasswordHash>.Failure(Error.Validation(
                "PasswordHash.InvalidLength",
                "El hash de la contraseña no cumple con la longitud mínima requerida."));
        }

        return Result<PasswordHash>.Success(new PasswordHash(trimmed));
    }

    public override string ToString() => Value;
}
