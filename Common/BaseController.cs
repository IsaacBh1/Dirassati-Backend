using Dirassati_Backend.Common;
using Microsoft.AspNetCore.Mvc;
using System.Net;

[ApiController]
public class BaseController : ControllerBase
{
    /// <summary>
    /// Handles the result of an operation and returns an appropriate <see cref="ActionResult"/> based on the status code and success of the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result value.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="result">The result of the operation, containing the status code, value, and any errors.</param>
    /// <returns>
    /// An <see cref="ActionResult"/> that represents the outcome of the operation:
    /// <list type="bullet">
    /// <item><description>If the operation is successful, returns a 201 Created, 204 No Content, or 200 OK response.</description></item>
    /// <item><description>If the operation fails, returns a 404 Not Found, 400 Bad Request, 401 Unauthorized, 403 Forbidden, or a custom status code response.</description></item>
    /// </list>
    /// </returns>
    protected ActionResult HandleResult<TResult, TError>(Result<TResult, TError> result)
    {
        if (result.IsSuccess)
        {
            switch (result.StatusCode)
            {
                case (int)HttpStatusCode.Created: // 201
                    return Created(string.Empty, result.Value);
                case (int)HttpStatusCode.NoContent: // 204
                    return NoContent();
                default:
                    return Ok(result.Value);
            }
        }

        switch (result.StatusCode)
        {
            case (int)HttpStatusCode.NotFound: // 404
                return NotFound(result.Errors);
            case (int)HttpStatusCode.BadRequest: // 400
                return BadRequest(result.Errors);
            case (int)HttpStatusCode.Unauthorized: // 401
                return Unauthorized(result.Errors);
            case (int)HttpStatusCode.Forbidden: // 403
                return Forbid();
            default:
                return StatusCode(result.StatusCode != 0 ? result.StatusCode : 500, result.Errors);
        }
    }
}