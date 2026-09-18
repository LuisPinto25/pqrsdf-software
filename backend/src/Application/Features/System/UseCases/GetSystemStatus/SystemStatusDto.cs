namespace Pqrsdf.Application.Features.System.UseCases.GetSystemStatus;

/// <summary>
/// DTO representing the system operational status.
/// Modeled as an immutable C# record with value semantics.
/// </summary>
public sealed record SystemStatusDto(
    string Status,
    string Service,
    string Version,
    string Environment,
    DateTime Timestamp,
    bool DatabaseConnected,
    string Message
);
