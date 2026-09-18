namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.GetActiveDestinationAreas;

/// <summary>
/// Data Transfer Object representing an active destination area for PQRSDF filings.
/// </summary>
public sealed record DestinationAreaDto(
    Guid Id,
    string Name,
    string Code);
