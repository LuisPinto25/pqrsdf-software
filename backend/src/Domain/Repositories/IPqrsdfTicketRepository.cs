using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.ValueObjects;

namespace Pqrsdf.Domain.Repositories;

/// <summary>
/// Repository interface for persisting and retrieving PQRSDF tickets.
/// </summary>
public interface IPqrsdfTicketRepository
{
    Task<PqrsdfTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PqrsdfTicket?> GetByRadicadoAsync(RadicadoNumber radicadoNumber, CancellationToken cancellationToken = default);
    Task AddAsync(PqrsdfTicket ticket, CancellationToken cancellationToken = default);
    Task UpdateAsync(PqrsdfTicket ticket, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PqrsdfTicket>> GetUnassignedAsync(int? type = null, Guid? destinationAreaId = null, string? search = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PqrsdfTicket>> GetAssignedAsync(string? search = null, Guid? officialId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PqrsdfTicket>> GetOfficialInboxAsync(Guid officialId, CancellationToken cancellationToken = default);
    Task<int> CountActiveByOfficialIdAsync(Guid officialId, CancellationToken cancellationToken = default);
}
