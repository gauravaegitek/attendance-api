using Microsoft.AspNetCore.Mvc;
using attendance_api.DTOs;
using System.Security.Claims;

namespace attendance_api.Controllers
{
    public class BaseController : ControllerBase
    {
        protected int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        protected string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value?.ToLower() ?? "";
        }

        protected OkObjectResult ApiOk<T>(string message, T data)
            => Ok(new ApiResponse<T> { Message = message, Success = true, Data = data });

        protected OkObjectResult ApiOk(string message)
            => Ok(new ApiResponse<object> { Message = message, Success = true });

        protected BadRequestObjectResult ApiBadRequest(string message, List<string>? errors = null)
            => BadRequest(new ApiResponse<object> { Message = message, Success = false, Errors = errors });

        protected NotFoundObjectResult ApiNotFound(string message)
            => NotFound(new ApiResponse<object> { Message = message, Success = false });

        protected UnauthorizedObjectResult ApiUnauthorized(string message)
            => Unauthorized(new ApiResponse<object> { Message = message, Success = false });

        protected ObjectResult ApiForbidden(string message)
            => StatusCode(403, new ApiResponse<object> { Message = message, Success = false, Errors = new List<string> { "ACCESS_DENIED" } });

        protected ObjectResult ApiServerError(string message, Exception ex)
            => StatusCode(500, new ApiResponse<object> { Message = message, Success = false, Errors = new List<string> { ex.Message } });
    }
}