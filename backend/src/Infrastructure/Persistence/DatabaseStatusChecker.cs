using Pqrsdf.Application.Shared.Persistence;

namespace Pqrsdf.Infrastructure.Persistence;

/// <summary>
/// Concrete database connectivity checker using EF Core DbContext.
/// </summary>
public class DatabaseStatusChecker : IDatabaseStatusChecker
{
    private readonly PqrsdfDbContext _dbContext;

    public DatabaseStatusChecker(PqrsdfDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Database.CanConnectAsync(cancellationToken);
        }
        catch
        {
            return false;
        }
    }
}
