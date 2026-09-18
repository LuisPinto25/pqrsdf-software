using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pqrsdf.Api.Shared.Controllers;
using Pqrsdf.Application.Features.Auth.UseCases.LoginUser;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Api.Controllers.V1;

/// <summary>
/// Controller for internal staff authentication and JWT credential verification.
/// </summary>
[Route("api/v1/auth")]
[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    /// <summary>
    /// Authenticates staff credentials (email and password) via BCrypt and issues a signed JWT access token.
    /// Rate limited to 5 attempts per minute per client IP address.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("LoginRateLimit")]
    [ProducesResponseType(typeof(Result<LoginUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new LoginUserCommand(request.Email, request.Password);
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Returns the authenticated staff member's profile claims from the validated JWT token.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(Result<Pqrsdf.Application.Shared.Dtos.UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        var emailClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;
        var nameClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
            ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Name)?.Value;
        var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var userId))
        {
            return Unauthorized();
        }

        var role = Enum.TryParse<Pqrsdf.Domain.Enums.UserRole>(roleClaim, out var parsedRole)
            ? parsedRole
            : Pqrsdf.Domain.Enums.UserRole.Funcionario;

        var profile = new Pqrsdf.Application.Shared.Dtos.UserProfileDto
        {
            Id = userId,
            Email = emailClaim ?? string.Empty,
            FullName = nameClaim ?? string.Empty,
            Role = roleClaim ?? "Funcionario"
        };

        return Ok(Result<Pqrsdf.Application.Shared.Dtos.UserProfileDto>.Success(profile));
    }

    /// <summary>
    /// Administrator-only management diagnostic endpoint verifying role authorization.
    /// </summary>
    [HttpGet("admin-check")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult AdminCheck()
    {
        return Ok(Result<string>.Success("Acceso autorizado para Administrador."));
    }
}
