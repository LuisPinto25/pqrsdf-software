using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.MakePqrsdf;

public static class MakePqrsdfCommandValidator
{
    public static Result<bool> Validate(MakePqrsdfCommand command)
    {
        var req = command.Request;

        if (req.DestinationAreaId == Guid.Empty)
        {
            return Result<bool>.Failure(Error.Validation(
                "MakePqrsdf.DestinationAreaRequired",
                "Debe seleccionar una dependencia o área de destino válida."));
        }

        if (string.IsNullOrWhiteSpace(req.Subject) || req.Subject.Trim().Length < 5 || req.Subject.Trim().Length > 150)
        {
            return Result<bool>.Failure(Error.Validation(
                "MakePqrsdf.InvalidSubject",
                "El asunto debe tener entre 5 y 150 caracteres."));
        }

        if (string.IsNullOrWhiteSpace(req.Description) || req.Description.Trim().Length < 10 || req.Description.Trim().Length > 4000)
        {
            return Result<bool>.Failure(Error.Validation(
                "MakePqrsdf.InvalidDescription",
                "La descripción debe tener entre 10 y 4000 caracteres."));
        }

        if (req.IsAnonymous && req.Type is not (PqrsdfType.Denunciation or PqrsdfType.Suggestion))
        {
            return Result<bool>.Failure(Error.Validation(
                "MakePqrsdf.AnonymousNotAllowed",
                "La radicación anónima está permitida exclusivamente para Denuncias y Sugerencias."));
        }

        if (!req.IsAnonymous)
        {
            if (req.Applicant is null)
            {
                return Result<bool>.Failure(Error.Validation(
                    "MakePqrsdf.ApplicantRequired",
                    "Los datos del solicitante son obligatorios para este tipo de solicitud."));
            }

            if (string.IsNullOrWhiteSpace(req.Applicant.FullName) ||
                req.Applicant.FullName.Trim().Length < 3 ||
                req.Applicant.FullName.Trim().Length > 150)
            {
                return Result<bool>.Failure(Error.Validation(
                    "MakePqrsdf.InvalidApplicantName",
                    "El nombre completo del solicitante debe tener entre 3 y 150 caracteres."));
            }

            if (string.IsNullOrWhiteSpace(req.Applicant.IdentificationNumber) ||
                req.Applicant.IdentificationNumber.Trim().Length < 4 ||
                req.Applicant.IdentificationNumber.Trim().Length > 20)
            {
                return Result<bool>.Failure(Error.Validation(
                    "MakePqrsdf.InvalidIdentificationNumber",
                    "El número de identificación debe tener entre 4 y 20 caracteres."));
            }

            if (string.IsNullOrWhiteSpace(req.Applicant.Email) || !req.Applicant.Email.Contains('@'))
            {
                return Result<bool>.Failure(Error.Validation(
                    "MakePqrsdf.InvalidEmail",
                    "El correo electrónico proporcionado no tiene un formato válido."));
            }
        }

        return Result<bool>.Success(true);
    }
}
