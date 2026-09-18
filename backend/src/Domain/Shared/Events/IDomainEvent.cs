namespace Pqrsdf.Domain.Shared.Events;

/// <summary>
/// Represents an event that occurred in the domain.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
