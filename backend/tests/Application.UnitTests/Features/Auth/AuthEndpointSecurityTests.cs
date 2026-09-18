using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Pqrsdf.Api.Controllers.V1;
using Pqrsdf.Application.Shared.Dtos;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.Shared.Results;
using Pqrsdf.Domain.ValueObjects;
using Pqrsdf.Infrastructure.Security;
using Xunit;

namespace Pqrsdf.Application.UnitTests.Features.Auth;

public class AuthEndpointSecurityTests
{
    private const string TestSecret = "PqrsdfSuperSecretKeyForJwtAuthentication2026!#";
    private const string TestIssuer = "Pqrsdf.Api";
    private const string TestAudience = "Pqrsdf.Client";

    private readonly IConfiguration _configuration;
    private readonly JwtTokenGenerator _tokenGenerator;

    public AuthEndpointSecurityTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "JwtSettings:Secret", TestSecret },
            { "JwtSettings:Issuer", TestIssuer },
            { "JwtSettings:Audience", TestAudience }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _tokenGenerator = new JwtTokenGenerator(_configuration);
    }

    [Fact]
    public void LoginEndpoint_MustHaveAllowAnonymousAttribute()
    {
        // Act
        var method = typeof(AuthController).GetMethod(nameof(AuthController.Login));
        var allowAnonymous = method?.GetCustomAttributes(typeof(AllowAnonymousAttribute), true);

        // Assert
        allowAnonymous.Should().NotBeNull();
        allowAnonymous.Should().NotBeEmpty("Login endpoint must be accessible without prior authentication.");
    }

    [Fact]
    public void GetCurrentUserEndpoint_MustHaveAuthorizeAttribute()
    {
        // Act
        var method = typeof(AuthController).GetMethod(nameof(AuthController.GetCurrentUser));
        var authorize = method?.GetCustomAttributes(typeof(AuthorizeAttribute), true);

        // Assert
        authorize.Should().NotBeNull();
        authorize.Should().NotBeEmpty("Me/Profile endpoint must be strictly protected with [Authorize].");
    }

    [Fact]
    public void AdminCheckEndpoint_MustEnforceAdministradorRole()
    {
        // Act
        var method = typeof(AuthController).GetMethod(nameof(AuthController.AdminCheck));
        var authorize = method?.GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .OfType<AuthorizeAttribute>()
            .FirstOrDefault();

        // Assert
        authorize.Should().NotBeNull();
        authorize!.Roles.Should().Be("Administrador", "Admin-only endpoints must explicitly restrict access to Administrador role.");
    }

    [Fact]
    public void PublicControllers_PqrsdfAndSystem_MustHaveAllowAnonymousAttribute()
    {
        // Act
        var pqrsdfAllowAnon = typeof(PqrsdfController).GetCustomAttributes(typeof(AllowAnonymousAttribute), true);
        var systemAllowAnon = typeof(SystemController).GetCustomAttributes(typeof(AllowAnonymousAttribute), true);

        // Assert
        pqrsdfAllowAnon.Should().NotBeEmpty("Citizen filing and tracking controller must be publicly accessible.");
        systemAllowAnon.Should().NotBeEmpty("System status controller must be publicly accessible.");
    }

    [Fact]
    public void GetCurrentUser_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var controller = new AuthController();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = controller.GetCurrentUser();

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public void GetCurrentUser_WhenAuthenticatedWithValidClaims_ReturnsUserProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, "funcionario@pqrsdf.gov.co"),
            new Claim(ClaimTypes.Name, "Funcionario PQRSDF"),
            new Claim(ClaimTypes.Role, "Funcionario")
        };

        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        var controller = new AuthController();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act
        var result = controller.GetCurrentUser() as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status200OK);

        var okResult = result.Value as Result<UserProfileDto>;
        okResult.Should().NotBeNull();
        okResult!.IsSuccess.Should().BeTrue();
        okResult.Value.Id.Should().Be(userId);
        okResult.Value.Email.Should().Be("funcionario@pqrsdf.gov.co");
        okResult.Value.FullName.Should().Be("Funcionario PQRSDF");
        okResult.Value.Role.Should().Be("Funcionario");
    }

    [Fact]
    public void TokenValidation_ValidToken_SuccessfullyValidatesClaimsAndLifetime()
    {
        // Arrange
        var user = User.Create(
            Guid.NewGuid(),
            Email.Create("funcionario@pqrsdf.gov.co").Value,
            PasswordHash.Create("$2a$11$fzrWhddqPT09gX.1iZRKf.pOLf4epbqVmw1qUhwF2DBU07Tfi2Vgy").Value,
            "Funcionario de Prueba",
            UserRole.Funcionario).Value;

        var token = _tokenGenerator.GenerateToken(user, TimeSpan.FromHours(8));

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = TestIssuer,
            ValidAudience = TestAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecret)),
            ClockSkew = TimeSpan.Zero
        };

        // Act
        var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

        // Assert
        validatedToken.Should().NotBeNull();
        principal.FindFirst(ClaimTypes.Role)?.Value.Should().Be("Funcionario");
        principal.FindFirst(ClaimTypes.Email)?.Value.Should().Be("funcionario@pqrsdf.gov.co");
        principal.FindFirst(ClaimTypes.Name)?.Value.Should().Be("Funcionario de Prueba");
    }

    [Fact]
    public void TokenValidation_InvalidSignature_ThrowsSecurityTokenException()
    {
        // Arrange
        var user = User.Create(
            Guid.NewGuid(),
            Email.Create("admin@pqrsdf.gov.co").Value,
            PasswordHash.Create("$2a$11$MJdcdRnVBhcwT5M/6j6VWedZXXxbf2S4xRTp5/JZKUheied8ihHO2").Value,
            "Administrador de Prueba",
            UserRole.Administrador).Value;

        var token = _tokenGenerator.GenerateToken(user, TimeSpan.FromHours(8));

        var tokenHandler = new JwtSecurityTokenHandler();
        var forgedValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = TestIssuer,
            ValidAudience = TestAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("AnotherKeyThatDoesNotMatchTheSignature1234567890!")),
            ClockSkew = TimeSpan.Zero
        };

        // Act
        var act = () => tokenHandler.ValidateToken(token, forgedValidationParameters, out _);

        // Assert
        act.Should().Throw<SecurityTokenInvalidSignatureException>();
    }
}
