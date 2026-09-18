namespace Pqrsdf.Application.Shared.Persistence;

/// <summary>
/// Abstraction for verifying database connectivity without coupling Application to EF Core.
/// </summary>
public interface IDatabaseStatusChecker
{
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
}
