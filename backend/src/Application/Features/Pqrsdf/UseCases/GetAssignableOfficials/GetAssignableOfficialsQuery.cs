using MediatR;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.GetAssignableOfficials;

public record GetAssignableOfficialsQuery() : IRequest<Result<IReadOnlyList<AssignableOfficialDto>>>;

public record AssignableOfficialDto(
    Guid Id,
    string FullName,
    string Email,
    int ActiveTicketsCount,
    int MaxCapacity,
    bool CanAssign);

public sealed class GetAssignableOfficialsQueryHandler
    : IRequestHandler<GetAssignableOfficialsQuery, Result<IReadOnlyList<AssignableOfficialDto>>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPqrsdfTicketRepository _ticketRepository;

    public GetAssignableOfficialsQueryHandler(
        IUserRepository userRepository,
        IPqrsdfTicketRepository ticketRepository)
    {
        _userRepository = userRepository;
        _ticketRepository = ticketRepository;
    }

    public async Task<Result<IReadOnlyList<AssignableOfficialDto>>> Handle(
        GetAssignableOfficialsQuery request,
        CancellationToken cancellationToken)
    {
        var officials = await _userRepository.GetActiveOfficialsAsync(cancellationToken);
        var dtos = new List<AssignableOfficialDto>(officials.Count);

        foreach (var official in officials)
        {
            var count = await _ticketRepository.CountActiveByOfficialIdAsync(official.Id, cancellationToken);
            var canAssign = count < 5;

            dtos.Add(new AssignableOfficialDto(
                official.Id,
                official.FullName,
                official.Email.Value,
                count,
                5,
                canAssign));
        }

        return Result<IReadOnlyList<AssignableOfficialDto>>.Success(dtos);
    }
}
