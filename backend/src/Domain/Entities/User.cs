using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Shared.Entities;
using Pqrsdf.Domain.Shared.Results;
using Pqrsdf.Domain.ValueObjects;

namespace Pqrsdf.Domain.Entities;

/// <summary>
/// Aggregate root representing an internal staff user or system administrator.
/// </summary>
public sealed class User : Entity<Guid>, IAggregateRoot
{
    public Email Email { get; private set; }
    public PasswordHash PasswordHash { get; private set; }
    public string FullName { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastLoginAtUtc { get; private set; }


    // Required for EF Core
    private User()
    {
        Email = default!;
        PasswordHash = default!;
        FullName = string.Empty;
    }

    private User(
        Guid id,
        Email email,
        PasswordHash passwordHash,
        string fullName,
        UserRole role,
        bool isActive,
        DateTime createdAtUtc,
        DateTime? lastLoginAtUtc = null) : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        Role = role;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
        LastLoginAtUtc = lastLoginAtUtc;
    }

    public static Result<User> Create(
        Guid id,
        Email? email,
        PasswordHash? passwordHash,
        string? fullName,
        UserRole role,
        bool isActive = true,
        DateTime? createdAtUtc = null)
    {
        if (id == Guid.Empty)
        {
            return Result<User>.Failure(Error.Validation(
                "User.InvalidId",
                "El identificador de usuario no puede estar vacío."));
        }

        if (email is null)
        {
            return Result<User>.Failure(Error.Validation(
                "User.EmailRequired",
                "El correo electrónico de usuario es obligatorio."));
        }

        if (passwordHash is null)
        {
            return Result<User>.Failure(Error.Validation(
                "User.PasswordHashRequired",
                "El hash de contraseña es obligatorio."));
        }

        if (string.IsNullOrWhiteSpace(fullName) || fullName.Trim().Length < 2 || fullName.Trim().Length > 150)
        {
            return Result<User>.Failure(Error.Validation(
                "User.InvalidFullName",
                "El nombre completo del usuario debe tener entre 2 y 150 caracteres."));
        }

        if (!Enum.IsDefined(typeof(UserRole), role))
        {
            return Result<User>.Failure(Error.Validation(
                "User.InvalidRole",
                "El rol del usuario no es válido."));
        }

        var user = new User(
            id,
            email,
            passwordHash,
            fullName.Trim(),
            role,
            isActive,
            createdAtUtc ?? DateTime.UtcNow);

        return Result<User>.Success(user);
    }

    public void RecordLogin(DateTime utcNow)
    {
        LastLoginAtUtc = utcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public Result<bool> ChangePassword(PasswordHash newPasswordHash)
    {
        if (newPasswordHash is null)
        {
            return Result<bool>.Failure(Error.Validation(
                "User.PasswordHashRequired",
                "La nueva contraseña no puede estar vacía."));
        }

        if (PasswordHash == newPasswordHash)
        {
            return Result<bool>.Failure(Error.Validation(
                "User.SamePassword",
                "La nueva contraseña debe ser diferente a la anterior."));
        }

        PasswordHash = newPasswordHash;
        return Result<bool>.Success(true);
    }
}
