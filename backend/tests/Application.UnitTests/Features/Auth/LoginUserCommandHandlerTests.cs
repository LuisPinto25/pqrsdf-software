using FluentAssertions;
using Moq;
using Pqrsdf.Application.Common.Interfaces;
using Pqrsdf.Application.Features.Auth.UseCases.LoginUser;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Repositories;
using Pqrsdf.Domain.ValueObjects;
using Xunit;

namespace Pqrsdf.Application.UnitTests.Features.Auth;

public sealed class LoginUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();

    private readonly LoginUserCommandHandler _handler;

    private readonly User _activeUser;
    private readonly User _inactiveUser;

    public LoginUserCommandHandlerTests()
    {
        _handler = new LoginUserCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object);

        var email = Email.Create("funcionario@pqrsdf.gov.co").Value;
        var hash = PasswordHash.Create("$2a$11$fzrWhddqPT09gX.1iZRKf.pOLf4epbqVmw1qUhwF2DBU07Tfi2Vgy").Value;
        _activeUser = User.Create(Guid.NewGuid(), email, hash, "Funcionario de PQRSDF", UserRole.Funcionario, isActive: true).Value;

        var inactiveEmail = Email.Create("inactivo@pqrsdf.gov.co").Value;
        _inactiveUser = User.Create(Guid.NewGuid(), inactiveEmail, hash, "Usuario Inactivo", UserRole.Funcionario, isActive: false).Value;
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsSuccessWithJwtAndProfile()
    {
        // Arrange
        var command = new LoginUserCommand("funcionario@pqrsdf.gov.co", "Funcionario123*");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(It.Is<Email>(e => e.Value == "funcionario@pqrsdf.gov.co"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_activeUser);
        _passwordHasherMock.Setup(p => p.VerifyPassword("Funcionario123*", _activeUser.PasswordHash.Value))
            .Returns(true);
        _jwtTokenGeneratorMock.Setup(j => j.GenerateToken(_activeUser, It.IsAny<TimeSpan>()))
            .Returns("valid.jwt.token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("valid.jwt.token");
        result.Value.TokenType.Should().Be("Bearer");
        result.Value.ExpiresIn.Should().Be(28800);
        result.Value.User.Email.Should().Be("funcionario@pqrsdf.gov.co");
        result.Value.User.FullName.Should().Be("Funcionario de PQRSDF");
        result.Value.User.Role.Should().Be("Funcionario");

        _userRepositoryMock.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Id == _activeUser.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUnknownEmail_ReturnsUnauthorizedFailure()
    {
        // Arrange
        var command = new LoginUserCommand("noexiste@pqrsdf.gov.co", "Password123*");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
        _passwordHasherMock.Verify(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithIncorrectPassword_ReturnsUnauthorizedFailure()
    {
        // Arrange
        var command = new LoginUserCommand("funcionario@pqrsdf.gov.co", "WrongPassword*");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(It.Is<Email>(e => e.Value == "funcionario@pqrsdf.gov.co"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_activeUser);
        _passwordHasherMock.Setup(p => p.VerifyPassword("WrongPassword*", _activeUser.PasswordHash.Value))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
        _jwtTokenGeneratorMock.Verify(j => j.GenerateToken(It.IsAny<User>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInactiveAccount_ReturnsUnauthorizedFailure()
    {
        // Arrange
        var command = new LoginUserCommand("inactivo@pqrsdf.gov.co", "Funcionario123*");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(It.Is<Email>(e => e.Value == "inactivo@pqrsdf.gov.co"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_inactiveUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.AccountInactive");
        _passwordHasherMock.Verify(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("", "password")]
    [InlineData("email@test.com", "")]
    [InlineData("   ", "   ")]
    public async Task Handle_WithEmptyCredentials_ReturnsUnauthorizedFailure(string email, string password)
    {
        var command = new LoginUserCommand(email, password);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
    }
}
