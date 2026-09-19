using Microsoft.EntityFrameworkCore;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Repositories;

namespace Pqrsdf.Infrastructure.Persistence.Repositories;

public sealed class TicketStatusHistoryRepository : ITicketStatusHistoryRepository
{
    private readonly PqrsdfDbContext _dbContext;

    public TicketStatusHistoryRepository(PqrsdfDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(TicketStatusHistory history, CancellationToken cancellationToken)
    {
        await _dbContext.TicketStatusHistories.AddAsync(history, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TicketStatusHistory>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        return await _dbContext.TicketStatusHistories
            .AsNoTracking()
            .Include(x => x.ChangedByUser)
            .Where(x => x.TicketId == ticketId)
            .OrderByDescending(x => x.ChangedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
