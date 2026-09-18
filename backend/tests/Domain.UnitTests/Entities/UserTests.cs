using FluentAssertions;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.ValueObjects;
using Xunit;

namespace Pqrsdf.Domain.UnitTests.Entities;

public sealed class UserTests
{
    private readonly Guid _validId = Guid.NewGuid();
    private readonly Email _validEmail = Email.Create("funcionario@pqrsdf.gov.co").Value;
    private readonly PasswordHash _validHash = PasswordHash.Create("$2a$11$fzrWhddqPT09gX.1iZRKf.pOLf4epbqVmw1qUhwF2DBU07Tfi2Vgy").Value;
    private const string ValidName = "Funcionario de PQRSDF";
    private const UserRole ValidRole = UserRole.Funcionario;

    [Fact]
    public void Create_WithValidParameters_ReturnsSuccess()
    {
        var result = User.Create(_validId, _validEmail, _validHash, ValidName, ValidRole);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(_validId);
        result.Value.Email.Should().Be(_validEmail);
        result.Value.FullName.Should().Be(ValidName);
        result.Value.Role.Should().Be(ValidRole);
        result.Value.IsActive.Should().BeTrue();
        result.Value.LastLoginAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyId_ReturnsFailure()
    {
        var result = User.Create(Guid.Empty, _validEmail, _validHash, ValidName, ValidRole);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.InvalidId");
    }

    [Fact]
    public void Create_WithNullEmail_ReturnsFailure()
    {
        var result = User.Create(_validId, null, _validHash, ValidName, ValidRole);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.EmailRequired");
    }

    [Fact]
    public void Create_WithNullPasswordHash_ReturnsFailure()
    {
        var result = User.Create(_validId, _validEmail, null, ValidName, ValidRole);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.PasswordHashRequired");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void Create_WithInvalidFullName_ReturnsFailure(string invalidName)
    {
        var result = User.Create(_validId, _validEmail, _validHash, invalidName, ValidRole);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.InvalidFullName");
    }

    [Fact]
    public void RecordLogin_UpdatesLastLoginAtUtc()
    {
        var user = User.Create(_validId, _validEmail, _validHash, ValidName, ValidRole).Value;
        var loginTime = DateTime.UtcNow;

        user.RecordLogin(loginTime);

        user.LastLoginAtUtc.Should().Be(loginTime);
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        var user = User.Create(_validId, _validEmail, _validHash, ValidName, ValidRole).Value;

        user.Deactivate();

        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_SetsIsActiveToTrue()
    {
        var user = User.Create(_validId, _validEmail, _validHash, ValidName, ValidRole, isActive: false).Value;

        user.Activate();

        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ChangePassword_WithNewHash_UpdatesPasswordHash()
    {
        var user = User.Create(_validId, _validEmail, _validHash, ValidName, ValidRole).Value;
        var newHash = PasswordHash.Create("$2a$11$MJdcdRnVBhcwT5M/6j6VWedZXXxbf2S4xRTp5/JZKUheied8ihHO2").Value;

        var result = user.ChangePassword(newHash);

        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Should().Be(newHash);
    }

    [Fact]
    public void ChangePassword_WithSameHash_ReturnsFailure()
    {
        var user = User.Create(_validId, _validEmail, _validHash, ValidName, ValidRole).Value;

        var result = user.ChangePassword(_validHash);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.SamePassword");
    }
}
