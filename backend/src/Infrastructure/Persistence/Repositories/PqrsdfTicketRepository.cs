using Microsoft.EntityFrameworkCore;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.ValueObjects;

namespace Pqrsdf.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core repository implementation for PQRSDF tickets.
/// </summary>
public sealed class PqrsdfTicketRepository : IPqrsdfTicketRepository
{
    private readonly PqrsdfDbContext _dbContext;

    public PqrsdfTicketRepository(PqrsdfDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PqrsdfTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<PqrsdfTicket>()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<PqrsdfTicket?> GetByRadicadoAsync(RadicadoNumber radicadoNumber, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<PqrsdfTicket>()
            .FirstOrDefaultAsync(t => t.RadicadoNumber == radicadoNumber, cancellationToken);
    }

    public async Task AddAsync(PqrsdfTicket ticket, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<PqrsdfTicket>().AddAsync(ticket, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
