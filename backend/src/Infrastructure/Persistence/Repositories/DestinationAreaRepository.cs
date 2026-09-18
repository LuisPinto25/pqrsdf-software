using Microsoft.EntityFrameworkCore;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Repositories;

namespace Pqrsdf.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of IDestinationAreaRepository.
/// </summary>
public sealed class DestinationAreaRepository : IDestinationAreaRepository
{
    private readonly PqrsdfDbContext _dbContext;

    public DestinationAreaRepository(PqrsdfDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DestinationArea?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.DestinationAreas
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<DestinationArea>> GetActiveAreasAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.DestinationAreas
            .AsNoTracking()
            .Where(a => a.IsActive)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(DestinationArea area, CancellationToken cancellationToken = default)
    {
        await _dbContext.DestinationAreas.AddAsync(area, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
