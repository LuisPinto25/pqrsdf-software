using System.Net.Mail;
using Pqrsdf.Domain.Shared.Results;
using Pqrsdf.Domain.Shared.ValueObjects;

namespace Pqrsdf.Domain.ValueObjects;

/// <summary>
/// Value object representing a validated, normalized email address.
/// </summary>
public sealed record Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result<Email>.Failure(Error.Validation(
                "Email.Required",
                "El correo electrónico es obligatorio."));
        }

        string trimmed = email.Trim();
        if (trimmed.Length > 254)
        {
            return Result<Email>.Failure(Error.Validation(
                "Email.TooLong",
                "El correo electrónico no puede exceder 254 caracteres."));
        }

        if (!IsValid(trimmed))
        {
            return Result<Email>.Failure(Error.Validation(
                "Email.InvalidFormat",
                "El correo electrónico proporcionado no tiene un formato válido."));
        }

        return Result<Email>.Success(new Email(trimmed.ToLowerInvariant()));
    }

    private static bool IsValid(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public override string ToString() => Value;
}
