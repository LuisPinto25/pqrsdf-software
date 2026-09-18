using MediatR;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.GetActiveDestinationAreas;

/// <summary>
/// Handler for retrieving active destination areas.
/// </summary>
public sealed class GetActiveDestinationAreasQueryHandler
    : IRequestHandler<GetActiveDestinationAreasQuery, Result<IReadOnlyList<DestinationAreaDto>>>
{
    private readonly IDestinationAreaRepository _destinationAreaRepository;

    public GetActiveDestinationAreasQueryHandler(IDestinationAreaRepository destinationAreaRepository)
    {
        _destinationAreaRepository = destinationAreaRepository;
    }

    public async Task<Result<IReadOnlyList<DestinationAreaDto>>> Handle(
        GetActiveDestinationAreasQuery request,
        CancellationToken cancellationToken)
    {
        var areas = await _destinationAreaRepository.GetActiveAreasAsync(cancellationToken);

        var dtos = areas
            .Select(a => new DestinationAreaDto(a.Id, a.Name, a.Code))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<DestinationAreaDto>>.Success(dtos);
    }
}
