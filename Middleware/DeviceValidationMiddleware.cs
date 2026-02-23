using System.Security.Claims;
using System.Text.Json;
using attendance_api.Data;
using Microsoft.EntityFrameworkCore;

namespace attendance_api.Middleware
{
    public class DeviceValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public DeviceValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ApplicationDbContext db)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // ✅ Skip auth + swagger + static uploads
            if (path.Contains("/api/auth/login") ||
                path.Contains("/api/auth/register") ||
                path.Contains("/api/auth/cleardevice") ||
                path.Contains("/swagger") ||
                path.StartsWith("/uploads"))
            {
                await _next(context);
                return;
            }

            if (context.User?.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                await Write401(context, "UNAUTHORIZED", "Invalid token");
                return;
            }

            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);

            if (user == null || !user.IsActive)
            {
                await Write401(context, "SESSION_REVOKED", "User not found or inactive");
                return;
            }

            // ✅ Token mismatch check — new login ya cleardevice ke baad purana token invalid
            var requestToken = context.Request.Headers["Authorization"]
                .ToString()
                .Replace("Bearer ", "")
                .Trim();

            if (!string.IsNullOrEmpty(requestToken) && user.CurrentToken != requestToken)
            {
                await Write401(context, "SESSION_REVOKED", "Session expired. Please login again.");
                return;
            }

            // ✅ Device cleared check — force logout
            if (string.IsNullOrWhiteSpace(user.DeviceId))
            {
                await Write401(context, "SESSION_REVOKED", "Device cleared. Please login again.");
                return;
            }

            // ✅ Device mismatch check
            var deviceFromHeader = context.Request.Headers["X-Device-Id"].ToString().Trim();
            var deviceFromToken = context.User.FindFirstValue("deviceId")?.Trim() ?? "";
            var reqDevice = !string.IsNullOrEmpty(deviceFromHeader) ? deviceFromHeader : deviceFromToken;

            if (!string.IsNullOrWhiteSpace(reqDevice) && user.DeviceId != reqDevice)
            {
                await Write401(context, "SESSION_REVOKED", "Device mismatch. Please login again.");
                return;
            }

            await _next(context);
        }

        private static async Task Write401(HttpContext context, string code, string message)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var payload = JsonSerializer.Serialize(new
            {
                success = false,
                code,
                message
            });

            await context.Response.WriteAsync(payload);
        }
    }
}