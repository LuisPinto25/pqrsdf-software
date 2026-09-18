using Pqrsdf.Domain.Entities;

namespace Pqrsdf.Domain.Repositories;

/// <summary>
/// Repository abstraction for ticket assignment and transfer audit history.
/// </summary>
public interface ITicketAssignmentHistoryRepository
{
    Task AddAsync(TicketAssignmentHistory history, CancellationToken cancellationToken);
    Task<IReadOnlyList<TicketAssignmentHistory>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken);
}
