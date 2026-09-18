using Pqrsdf.Domain.Entities;

namespace Pqrsdf.Domain.Repositories;

/// <summary>
/// Repository abstraction for destination areas.
/// </summary>
public interface IDestinationAreaRepository
{
    Task<DestinationArea?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DestinationArea>> GetActiveAreasAsync(CancellationToken cancellationToken = default);
    Task AddAsync(DestinationArea area, CancellationToken cancellationToken = default);
}
