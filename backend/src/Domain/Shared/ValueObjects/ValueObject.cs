namespace Pqrsdf.Domain.Shared.ValueObjects;

/// <summary>
/// Base record for domain value objects.
/// In C#, records natively provide value-based equality, immutability, and compiler-generated equality operators (==, !=).
/// </summary>
public abstract record ValueObject;
