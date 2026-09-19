using Pqrsdf.Domain.Entities;

namespace Pqrsdf.Domain.Repositories;

/// <summary>
/// Repository abstraction for ticket operational status transition and justification audit history.
/// </summary>
public interface ITicketStatusHistoryRepository
{
    Task AddAsync(TicketStatusHistory history, CancellationToken cancellationToken);
    Task<IReadOnlyList<TicketStatusHistory>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken);
}
