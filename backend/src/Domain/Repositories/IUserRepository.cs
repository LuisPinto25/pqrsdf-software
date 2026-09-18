using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.ValueObjects;

namespace Pqrsdf.Domain.Repositories;

/// <summary>
/// Repository interface for persisting and retrieving internal staff users.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}
