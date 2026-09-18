using MediatR;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.GetActiveDestinationAreas;

/// <summary>
/// Query to retrieve all active destination areas eligible to receive citizen PQRSDF filings.
/// </summary>
public sealed record GetActiveDestinationAreasQuery : IRequest<Result<IReadOnlyList<DestinationAreaDto>>>;
