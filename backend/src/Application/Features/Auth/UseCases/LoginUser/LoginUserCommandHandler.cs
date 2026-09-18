using MediatR;
using Pqrsdf.Application.Common.Interfaces;
using Pqrsdf.Application.Shared.Dtos;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.Shared.Results;
using Pqrsdf.Domain.ValueObjects;

namespace Pqrsdf.Application.Features.Auth.UseCases.LoginUser;

public sealed class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<LoginUserResponse>>
{
    private const int TokenLifespanSeconds = 28800; // 8 hours
    private static readonly TimeSpan TokenLifespan = TimeSpan.FromSeconds(TokenLifespanSeconds);

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<LoginUserResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var invalidCredentialsError = Error.Unauthorized(
            "Auth.InvalidCredentials",
            "Credenciales inválidas. Por favor verifique su correo y contraseña.");

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<LoginUserResponse>.Failure(invalidCredentialsError);
        }

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result<LoginUserResponse>.Failure(invalidCredentialsError);
        }

        var user = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user is null)
        {
            return Result<LoginUserResponse>.Failure(invalidCredentialsError);
        }

        if (!user.IsActive)
        {
            return Result<LoginUserResponse>.Failure(Error.Unauthorized(
                "Auth.AccountInactive",
                "La cuenta de usuario se encuentra inactiva. Contacte al administrador."));
        }

        bool isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash.Value);
        if (!isPasswordValid)
        {
            return Result<LoginUserResponse>.Failure(invalidCredentialsError);
        }

        user.RecordLogin(DateTime.UtcNow);
        await _userRepository.UpdateAsync(user, cancellationToken);

        string token = _jwtTokenGenerator.GenerateToken(user, TokenLifespan);

        var response = new LoginUserResponse(
            token,
            "Bearer",
            TokenLifespanSeconds,
            new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email.Value,
                FullName = user.FullName,
                Role = user.Role.ToString()
            });

        return Result<LoginUserResponse>.Success(response);
    }
}
