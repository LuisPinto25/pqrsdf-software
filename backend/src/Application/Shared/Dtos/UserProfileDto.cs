namespace Pqrsdf.Application.Shared.Dtos;

/// <summary>
/// Shared DTO representing user profile data across applications and features.
/// </summary>
public sealed record UserProfileDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}
