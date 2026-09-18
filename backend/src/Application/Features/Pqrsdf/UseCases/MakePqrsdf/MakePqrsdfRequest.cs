using Pqrsdf.Domain.Enums;

namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.MakePqrsdf;

public sealed record ApplicantRequest(
    string FullName,
    IdentificationType IdentificationType,
    string IdentificationNumber,
    string Email,
    string? PhoneNumber);

public sealed record MakePqrsdfRequest(
    PqrsdfType Type,
    Guid DestinationAreaId,
    bool IsAnonymous,
    ApplicantRequest? Applicant,
    string Subject,
    string Description);
