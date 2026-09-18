using System.Net.Mail;
using System.Text.RegularExpressions;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Shared.Results;
using Pqrsdf.Domain.Shared.ValueObjects;

namespace Pqrsdf.Domain.ValueObjects;

/// <summary>
/// Value object encapsulating citizen applicant identity and contact information.
/// </summary>
public sealed partial record Applicant : ValueObject
{
    private static readonly Regex IdNumberRegex = GetIdNumberRegex();

    public string FullName { get; }
    public IdentificationType IdentificationType { get; }
    public string IdentificationNumber { get; }
    public string Email { get; }
    public string? PhoneNumber { get; }

    private Applicant(
        string fullName,
        IdentificationType identificationType,
        string identificationNumber,
        string email,
        string? phoneNumber)
    {
        FullName = fullName;
        IdentificationType = identificationType;
        IdentificationNumber = identificationNumber;
        Email = email;
        PhoneNumber = phoneNumber;
    }

    public static Result<Applicant> Create(
        string? fullName,
        IdentificationType identificationType,
        string? identificationNumber,
        string? email,
        string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Trim().Length < 3 || fullName.Trim().Length > 150)
        {
            return Result<Applicant>.Failure(Error.Validation(
                "Applicant.InvalidFullName",
                "El nombre completo del solicitante debe tener entre 3 y 150 caracteres."));
        }

        if (string.IsNullOrWhiteSpace(identificationNumber) || !IdNumberRegex.IsMatch(identificationNumber.Trim()))
        {
            return Result<Applicant>.Failure(Error.Validation(
                "Applicant.InvalidIdentificationNumber",
                "El número de documento debe tener entre 4 y 20 caracteres alfanuméricos."));
        }

        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email.Trim()))
        {
            return Result<Applicant>.Failure(Error.Validation(
                "Applicant.InvalidEmail",
                "El correo electrónico proporcionado no tiene un formato válido."));
        }

        string? cleanedPhone = null;
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            cleanedPhone = phoneNumber.Trim();
            if (cleanedPhone.Length < 7 || cleanedPhone.Length > 20)
            {
                return Result<Applicant>.Failure(Error.Validation(
                    "Applicant.InvalidPhone",
                    "El número de teléfono debe tener entre 7 y 20 dígitos."));
            }
        }

        return Result<Applicant>.Success(new Applicant(
            fullName.Trim(),
            identificationType,
            identificationNumber.Trim(),
            email.Trim().ToLowerInvariant(),
            cleanedPhone));
    }

    private static bool IsValidEmail(string email)
    {
        if (email.Length > 254) return false;
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

    [GeneratedRegex(@"^[a-zA-Z0-9\-]{4,20}$")]
    private static partial Regex GetIdNumberRegex();
}
