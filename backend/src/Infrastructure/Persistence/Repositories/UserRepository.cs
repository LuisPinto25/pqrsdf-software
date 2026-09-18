using Microsoft.EntityFrameworkCore;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.ValueObjects;

namespace Pqrsdf.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core repository implementation for internal users.
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly PqrsdfDbContext _dbContext;

    public UserRepository(PqrsdfDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetActiveOfficialsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<User>()
            .AsNoTracking()
            .Where(u => u.IsActive && u.Role == Domain.Enums.UserRole.Funcionario)
            .OrderBy(u => u.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<User>().AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<User>().Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
