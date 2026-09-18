using MediatR;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Application.Features.System.UseCases.GetSystemStatus;

/// <summary>
/// Query to obtain the system operational readiness and status.
/// Returns a Result containing SystemStatusDto.
/// </summary>
public sealed record GetSystemStatusQuery : IRequest<Result<SystemStatusDto>>;
