using Pqrsdf.Domain.ValueObjects;

namespace Pqrsdf.Domain.Services;

/// <summary>
/// Domain service interface for generating atomic, concurrency-safe sequential radicado numbers per calendar year.
/// </summary>
public interface IRadicadoSequenceGenerator
{
    Task<RadicadoNumber> NextAsync(int year, CancellationToken cancellationToken = default);
}
