using Pqrsdf.Application.Shared.Dtos;

namespace Pqrsdf.Application.Features.Auth.UseCases.LoginUser;

public sealed record LoginUserResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    UserProfileDto User);
