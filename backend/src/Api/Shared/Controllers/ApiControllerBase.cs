using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Api.Shared.Controllers;

/// <summary>
/// Base API controller providing standardized result translation and MediatR dispatching.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _sender;

    protected ISender Sender => _sender ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected ActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return result.Error.Type switch
        {
            ErrorType.NotFound => NotFound(result),
            ErrorType.Validation => BadRequest(result),
            ErrorType.Conflict => Conflict(result),
            ErrorType.Unauthorized => Unauthorized(result),
            _ => BadRequest(result)
        };
    }
}
