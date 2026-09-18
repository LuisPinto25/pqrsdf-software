using Microsoft.EntityFrameworkCore;
using Pqrsdf.Domain.Services;
using Pqrsdf.Domain.ValueObjects;
using Pqrsdf.Infrastructure.Persistence;
using Pqrsdf.Infrastructure.Persistence.Entities;

namespace Pqrsdf.Infrastructure.Services;

/// <summary>
/// Atomic sequential radicado generator providing concurrency-safe, non-colliding numbers per year.
/// </summary>
public sealed class RadicadoSequenceGenerator : IRadicadoSequenceGenerator
{
    private static readonly SemaphoreSlim Lock = new(1, 1);
    private readonly PqrsdfDbContext _dbContext;

    public RadicadoSequenceGenerator(PqrsdfDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RadicadoNumber> NextAsync(int year, CancellationToken cancellationToken = default)
    {
        await Lock.WaitAsync(cancellationToken);
        try
        {
            var sequence = await _dbContext.Set<RadicadoSequence>()
                .FirstOrDefaultAsync(s => s.Year == year, cancellationToken);

            long nextValue;
            if (sequence is null)
            {
                nextValue = 1;
                sequence = new RadicadoSequence(year, nextValue);
                _dbContext.Set<RadicadoSequence>().Add(sequence);
            }
            else
            {
                nextValue = ++sequence.CurrentValue;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            var radicadoResult = RadicadoNumber.Create(year, nextValue);
            if (radicadoResult.IsFailure)
            {
                throw new InvalidOperationException($"Error formatting generated radicado: {radicadoResult.Error.Message}");
            }

            return radicadoResult.Value;
        }
        finally
        {
            Lock.Release();
        }
    }
}
