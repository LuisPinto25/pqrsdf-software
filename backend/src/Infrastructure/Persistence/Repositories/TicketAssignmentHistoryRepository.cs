using Microsoft.EntityFrameworkCore;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Repositories;

namespace Pqrsdf.Infrastructure.Persistence.Repositories;

public sealed class TicketAssignmentHistoryRepository : ITicketAssignmentHistoryRepository
{
    private readonly PqrsdfDbContext _dbContext;

    public TicketAssignmentHistoryRepository(PqrsdfDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(TicketAssignmentHistory history, CancellationToken cancellationToken)
    {
        await _dbContext.TicketAssignmentHistories.AddAsync(history, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TicketAssignmentHistory>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        return await _dbContext.TicketAssignmentHistories
            .AsNoTracking()
            .Include(x => x.PreviousAssignedUser)
            .Include(x => x.NewAssignedUser)
            .Include(x => x.AssignedByUser)
            .Where(x => x.TicketId == ticketId)
            .OrderBy(x => x.AssignedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
