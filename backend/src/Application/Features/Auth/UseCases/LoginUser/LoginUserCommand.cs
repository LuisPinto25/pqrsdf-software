using MediatR;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Application.Features.Auth.UseCases.LoginUser;

public sealed record LoginUserCommand(string Email, string Password) : IRequest<Result<LoginUserResponse>>;
