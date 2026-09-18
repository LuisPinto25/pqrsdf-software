using MediatR;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.MakePqrsdf;

public sealed record MakePqrsdfCommand(MakePqrsdfRequest Request)
    : IRequest<Result<MakePqrsdfResponse>>;
