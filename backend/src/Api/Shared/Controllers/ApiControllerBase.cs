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

        var errorResponse = new
        {
            isSuccess = false,
            isFailure = true,
            error = new
            {
                code = result.Error.Code,
                message = result.Error.Message,
                type = result.Error.Type.ToString()
            }
        };

        return result.Error.Type switch
        {
            ErrorType.NotFound => NotFound(errorResponse),
            ErrorType.Validation => BadRequest(errorResponse),
            ErrorType.Conflict => Conflict(errorResponse),
            ErrorType.Unauthorized => Unauthorized(errorResponse),
            _ => BadRequest(errorResponse)
        };
    }
}
