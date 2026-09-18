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

    public async Task UpdateAsync(PqrsdfTicket ticket, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<PqrsdfTicket>().Update(ticket);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PqrsdfTicket>> GetUnassignedAsync(
        int? type = null,
        Guid? destinationAreaId = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<PqrsdfTicket>()
            .AsNoTracking()
            .Where(t => t.Status == Domain.Enums.TicketStatus.Registered);

        if (type.HasValue)
        {
            var typeEnum = (Domain.Enums.PqrsdfType)type.Value;
            query = query.Where(t => t.Type == typeEnum);
        }

        if (destinationAreaId.HasValue && destinationAreaId.Value != Guid.Empty)
        {
            query = query.Where(t => t.DestinationAreaId == destinationAreaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(t => t.Subject.ToLower().Contains(term) || ((string)(object)t.RadicadoNumber).ToLower().Contains(term));
        }

        return await query
            .OrderBy(t => t.DueDate.Value)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PqrsdfTicket>> GetAssignedAsync(
        string? search = null,
        Guid? officialId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<PqrsdfTicket>()
            .AsNoTracking()
            .Where(t => t.Status == Domain.Enums.TicketStatus.InReview);

        if (officialId.HasValue && officialId.Value != Guid.Empty)
        {
            query = query.Where(t => t.AssignedToUserId == officialId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(t => t.Subject.ToLower().Contains(term) || ((string)(object)t.RadicadoNumber).ToLower().Contains(term));
        }

        return await query
            .OrderBy(t => t.DueDate.Value)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PqrsdfTicket>> GetOfficialInboxAsync(
        Guid officialId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<PqrsdfTicket>()
            .AsNoTracking()
            .Where(t => t.AssignedToUserId == officialId && t.Status == Domain.Enums.TicketStatus.InReview)
            .OrderBy(t => t.DueDate.Value)
            .Take(5)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountActiveByOfficialIdAsync(
        Guid officialId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<PqrsdfTicket>()
            .CountAsync(t => t.AssignedToUserId == officialId && t.Status == Domain.Enums.TicketStatus.InReview, cancellationToken);
    }
}
