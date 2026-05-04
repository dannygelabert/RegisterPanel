using RegisterPanel.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace RegisterPanel.Api.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult ToActionResult(Result result)
    {
        if (result.IsSuccess)
            return NoContent();

        return result.ErrorCode switch
        {
            "NOT_FOUND"        => NotFound(Problem(result.ErrorMessage, statusCode: 404)),
            "ALREADY_INACTIVE"
            or "ALREADY_ACTIVE"
            or "CONFLICT"
            or "EMAIL_ALREADY_IN_USE" => Conflict(Problem(result.ErrorMessage, statusCode: 409)),
            "INVALID_CREDENTIALS"
            or "EMAIL_NOT_VERIFIED"
            or "ACCOUNT_INACTIVE"  => Unauthorized(Problem(result.ErrorMessage, statusCode: 401)),
            "FORBIDDEN"        => Forbid(),
            "VALIDATION_ERROR" => UnprocessableEntity(Problem(result.ErrorMessage, statusCode: 422)),
            _                  => BadRequest(Problem(result.ErrorMessage, statusCode: 400))
        };
    }

    protected IActionResult ToActionResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return result.ErrorCode switch
        {
            "NOT_FOUND"        => NotFound(Problem(result.ErrorMessage, statusCode: 404)),
            "ALREADY_INACTIVE"
            or "ALREADY_ACTIVE"
            or "CONFLICT"
            or "EMAIL_ALREADY_IN_USE" => Conflict(Problem(result.ErrorMessage, statusCode: 409)),
            "INVALID_CREDENTIALS"
            or "EMAIL_NOT_VERIFIED"
            or "ACCOUNT_INACTIVE"  => Unauthorized(Problem(result.ErrorMessage, statusCode: 401)),
            "FORBIDDEN"        => Forbid(),
            "VALIDATION_ERROR" => UnprocessableEntity(Problem(result.ErrorMessage, statusCode: 422)),
            _                  => BadRequest(Problem(result.ErrorMessage, statusCode: 400))
        };
    }

    protected IActionResult ToCreatedActionResult<T>(Result<T> result, string routeName, object routeValues)
    {
        if (result.IsSuccess)
            return CreatedAtRoute(routeName, routeValues, result.Value);

        return ToActionResult(Result<T>.Failure(result.ErrorCode!, result.ErrorMessage!));
    }
}
