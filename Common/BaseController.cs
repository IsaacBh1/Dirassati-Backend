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
            return result.StatusCode switch
            {
                // 201
                (int)HttpStatusCode.Created => Created(string.Empty, result.Value),
                // 204
                (int)HttpStatusCode.NoContent => NoContent(),
                _ => Ok(result.Value),
            };
        }

        return result.StatusCode switch
        {
            // 404
            (int)HttpStatusCode.NotFound => NotFound(result.Errors),
            // 400
            (int)HttpStatusCode.BadRequest => BadRequest(result.Errors),
            // 401
            (int)HttpStatusCode.Unauthorized => Unauthorized(result.Errors),
            // 403
            (int)HttpStatusCode.Forbidden => Forbid(),
            _ => StatusCode(result.StatusCode != 0 ? result.StatusCode : 500, result.Errors),
        };
    }
}