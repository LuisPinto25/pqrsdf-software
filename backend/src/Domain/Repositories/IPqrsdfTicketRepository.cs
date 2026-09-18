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
}
