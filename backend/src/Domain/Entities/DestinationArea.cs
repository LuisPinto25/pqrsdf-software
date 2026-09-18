using Pqrsdf.Domain.Shared.Entities;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Domain.Entities;

/// <summary>
/// Organizational unit or department eligible to process PQRSDF filings.
/// </summary>
public sealed class DestinationArea : Entity<Guid>
{
    public string Name { get; private set; }
    public string Code { get; private set; }
    public bool IsActive { get; private set; }

    // Required for EF Core
    private DestinationArea()
    {
        Name = string.Empty;
        Code = string.Empty;
    }

    private DestinationArea(Guid id, string name, string code, bool isActive) : base(id)
    {
        Name = name;
        Code = code;
        IsActive = isActive;
    }

    public static Result<DestinationArea> Create(Guid id, string? name, string? code, bool isActive = true)
    {
        if (id == Guid.Empty)
        {
            return Result<DestinationArea>.Failure(Error.Validation(
                "DestinationArea.InvalidId",
                "El identificador del área de destino no puede estar vacío."));
        }

        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 2 || name.Trim().Length > 100)
        {
            return Result<DestinationArea>.Failure(Error.Validation(
                "DestinationArea.InvalidName",
                "El nombre del área de destino debe tener entre 2 y 100 caracteres."));
        }

        if (string.IsNullOrWhiteSpace(code) || code.Trim().Length < 2 || code.Trim().Length > 10)
        {
            return Result<DestinationArea>.Failure(Error.Validation(
                "DestinationArea.InvalidCode",
                "El código del área de destino debe tener entre 2 y 10 caracteres."));
        }

        return Result<DestinationArea>.Success(new DestinationArea(
            id,
            name.Trim(),
            code.Trim().ToUpperInvariant(),
            isActive));
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
