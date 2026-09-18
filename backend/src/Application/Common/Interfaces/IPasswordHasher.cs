namespace Pqrsdf.Application.Common.Interfaces;

/// <summary>
/// Abstraction for cryptographic password hashing and verification.
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}
