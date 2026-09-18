using Pqrsdf.Domain.Entities;

namespace Pqrsdf.Application.Common.Interfaces;

/// <summary>
/// Abstraction for generating signed JSON Web Tokens (JWT) for authenticated users.
/// </summary>
public interface IJwtTokenGenerator
{
    string GenerateToken(User user, TimeSpan expiration);
}
