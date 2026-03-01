// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using attendance_api.Data;
// using attendance_api.DTOs;

// namespace attendance_api.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     [Authorize]
//     public class LoginHistoryController : BaseController
//     {
//         private readonly ApplicationDbContext _context;

//         public LoginHistoryController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         // ─────────────────────────────────────────────────────────────
//         // 1. APNI HISTORY  — Any logged-in user
//         //    GET /api/loginhistory/me
//         //    GET /api/loginhistory/me?fromDate=2026-03-01&toDate=2026-03-31
//         //    Default: current month
//         // ─────────────────────────────────────────────────────────────
//         [HttpGet("me")]
//         public async Task<ActionResult<ApiResponse<List<LoginHistoryDto>>>> GetMyHistory(
//             [FromQuery] DateTime? fromDate,
//             [FromQuery] DateTime? toDate)
//         {
//             try
//             {
//                 var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
//                 if (userIdClaim == null) return ApiUnauthorized("Invalid token");

//                 var userId = int.Parse(userIdClaim.Value);

//                 // Default: current month
//                 var from = (fromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
//                 var to   = (toDate   ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month,
//                                 DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month))).Date;

//                 if (from > to)
//                     return ApiBadRequest("fromDate cannot be greater than toDate");

//                 var list = await _context.UserLoginHistories
//                     .Where(h => h.UserId   == userId        &&
//                                 h.LoginDate >= from         &&
//                                 h.LoginDate <= to)
//                     .OrderByDescending(h => h.LoginDate)
//                     .ThenByDescending(h => h.LoginTime)
//                     .Select(h => new LoginHistoryDto
//                     {
//                         Id         = h.Id,
//                         UserId     = h.UserId,
//                         UserName   = h.UserName,
//                         LoginDate  = h.LoginDate,
//                         LoginTime  = h.LoginTime.HasValue ? h.LoginTime.Value.ToString(@"hh\:mm\:ss") : null,
//                         DeviceType = h.DeviceType,
//                         DeviceName = h.DeviceName,
//                         LogoutDate = h.LogoutDate,
//                         LogoutTime = h.LogoutTime.HasValue ? h.LogoutTime.Value.ToString(@"hh\:mm\:ss") : null,
//                         IsActive   = h.LogoutDate == null,
//                         CreatedAt  = h.CreatedAt,
//                         UpdatedAt  = h.UpdatedAt
//                     })
//                     .ToListAsync();

//                 return ApiOk($"{list.Count} records from {from:dd-MMM-yyyy} to {to:dd-MMM-yyyy}", list);
//             }
//             catch (Exception ex) { return ApiServerError("Failed to fetch your login history", ex); }
//         }

//         // ─────────────────────────────────────────────────────────────
//         // 2. AAJ KE / DATE RANGE LOGINS  — Admin only
//         //    GET /api/loginhistory/today
//         //    GET /api/loginhistory/today?fromDate=2026-03-01&toDate=2026-03-31
//         //    Default: current month
//         // ─────────────────────────────────────────────────────────────
//         [Authorize(Roles = "admin")]
//         [HttpGet("today")]
//         public async Task<ActionResult<ApiResponse<List<LoginHistoryDto>>>> GetToday(
//             [FromQuery] DateTime? fromDate,
//             [FromQuery] DateTime? toDate)
//         {
//             try
//             {
//                 // Default: current month
//                 var from = (fromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
//                 var to   = (toDate   ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month,
//                                 DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month))).Date;

//                 if (from > to)
//                     return ApiBadRequest("fromDate cannot be greater than toDate");

//                 var list = await _context.UserLoginHistories
//                     .Where(h => h.LoginDate >= from &&
//                                 h.LoginDate <= to)
//                     .OrderByDescending(h => h.LoginDate)
//                     .ThenByDescending(h => h.LoginTime)
//                     .Select(h => new LoginHistoryDto
//                     {
//                         Id         = h.Id,
//                         UserId     = h.UserId,
//                         UserName   = h.UserName,
//                         LoginDate  = h.LoginDate,
//                         LoginTime  = h.LoginTime.HasValue ? h.LoginTime.Value.ToString(@"hh\:mm\:ss") : null,
//                         DeviceType = h.DeviceType,
//                         DeviceName = h.DeviceName,
//                         LogoutDate = h.LogoutDate,
//                         LogoutTime = h.LogoutTime.HasValue ? h.LogoutTime.Value.ToString(@"hh\:mm\:ss") : null,
//                         IsActive   = h.LogoutDate == null,
//                         CreatedAt  = h.CreatedAt,
//                         UpdatedAt  = h.UpdatedAt
//                     })
//                     .ToListAsync();

//                 return ApiOk($"{list.Count} records from {from:dd-MMM-yyyy} to {to:dd-MMM-yyyy}", list);
//             }
//             catch (Exception ex) { return ApiServerError("Failed to fetch login history", ex); }
//         }

//         // ─────────────────────────────────────────────────────────────
//         // 3. EK USER KI HISTORY  — Admin / Manager
//         //    GET /api/loginhistory/user/{userId}
//         //    GET /api/loginhistory/user/{userId}?fromDate=2026-03-01&toDate=2026-03-31
//         //    Default: current month
//         // ─────────────────────────────────────────────────────────────
//         [Authorize(Roles = "admin,manager")]
//         [HttpGet("user/{userId}")]
//         public async Task<ActionResult<ApiResponse<List<LoginHistoryDto>>>> GetByUser(
//             int userId,
//             [FromQuery] DateTime? fromDate,
//             [FromQuery] DateTime? toDate)
//         {
//             try
//             {
//                 // Default: current month
//                 var from = (fromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
//                 var to   = (toDate   ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month,
//                                 DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month))).Date;

//                 if (from > to)
//                     return ApiBadRequest("fromDate cannot be greater than toDate");

//                 var list = await _context.UserLoginHistories
//                     .Where(h => h.UserId    == userId        &&
//                                 h.LoginDate >= from          &&
//                                 h.LoginDate <= to)
//                     .OrderByDescending(h => h.LoginDate)
//                     .ThenByDescending(h => h.LoginTime)
//                     .Select(h => new LoginHistoryDto
//                     {
//                         Id         = h.Id,
//                         UserId     = h.UserId,
//                         UserName   = h.UserName,
//                         LoginDate  = h.LoginDate,
//                         LoginTime  = h.LoginTime.HasValue ? h.LoginTime.Value.ToString(@"hh\:mm\:ss") : null,
//                         DeviceType = h.DeviceType,
//                         DeviceName = h.DeviceName,
//                         LogoutDate = h.LogoutDate,
//                         LogoutTime = h.LogoutTime.HasValue ? h.LogoutTime.Value.ToString(@"hh\:mm\:ss") : null,
//                         IsActive   = h.LogoutDate == null,
//                         CreatedAt  = h.CreatedAt,
//                         UpdatedAt  = h.UpdatedAt
//                     })
//                     .ToListAsync();

//                 if (!list.Any())
//                     return ApiNotFound($"No records found for user ID {userId} from {from:dd-MMM-yyyy} to {to:dd-MMM-yyyy}");

//                 return ApiOk($"{list.Count} records from {from:dd-MMM-yyyy} to {to:dd-MMM-yyyy}", list);
//             }
//             catch (Exception ex) { return ApiServerError("Failed to fetch user login history", ex); }
//         }
//     }
// }














using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using attendance_api.Data;
using attendance_api.DTOs;

namespace attendance_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LoginHistoryController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public LoginHistoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────
        // 1. APNI HISTORY  — Any logged-in user
        //    GET /api/loginhistory/me
        //    GET /api/loginhistory/me?fromDate=2026-03-01&toDate=2026-03-31
        // ─────────────────────────────────────────────────────────────
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<List<LoginHistoryDto>>>> GetMyHistory(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return ApiUnauthorized("Invalid token");

                var userId = int.Parse(userIdClaim.Value);

                var from = (fromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
                var to   = (toDate   ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month,
                                DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month))).Date;

                if (from > to)
                    return ApiBadRequest("fromDate cannot be greater than toDate");

                var list = await _context.UserLoginHistories
                    .Where(h => h.UserId    == userId &&
                                h.LoginDate >= from   &&
                                h.LoginDate <= to)
                    .OrderByDescending(h => h.LoginDate)
                    .ThenByDescending(h => h.LoginTime)
                    .Select(h => new LoginHistoryDto
                    {
                        Id            = h.Id,
                        UserId        = h.UserId,
                        UserName      = h.UserName,
                        LoginDate     = h.LoginDate,
                        LoginTime     = h.LoginTime.HasValue ? h.LoginTime.Value.ToString(@"hh\:mm\:ss") : null,
                        DeviceType    = h.DeviceType,
                        DeviceName    = h.DeviceName,
                        LogoutDate    = h.LogoutDate,
                        LogoutTime    = h.LogoutTime.HasValue ? h.LogoutTime.Value.ToString(@"hh\:mm\:ss") : null,
                        IsActive      = h.LogoutDate == null,
                        CreatedAt     = h.CreatedAt,
                        UpdatedAt     = h.UpdatedAt,
                        // ✅ NEW
                        TotalMinutes  = h.TotalMinutes,
                        TotalDuration = h.TotalMinutes.HasValue
                                        ? $"{h.TotalMinutes / 60}h {h.TotalMinutes % 60}m"
                                        : null,
                        LogoutReason  = h.LogoutReason,
                        SessionStatus = h.LogoutDate != null      ? "LoggedOut"
                                      : h.TokenExpiresAt < DateTime.Now ? "Expired"
                                      : "Active"
                    })
                    .ToListAsync();

                return ApiOk($"{list.Count} records from {from:dd-MMM-yyyy} to {to:dd-MMM-yyyy}", list);
            }
            catch (Exception ex) { return ApiServerError("Failed to fetch your login history", ex); }
        }

        // ─────────────────────────────────────────────────────────────
        // 2. AAJ KE / DATE RANGE LOGINS  — Admin only
        //    GET /api/loginhistory/today
        //    GET /api/loginhistory/today?fromDate=2026-03-01&toDate=2026-03-31
        // ─────────────────────────────────────────────────────────────
        [Authorize(Roles = "admin")]
        [HttpGet("today")]
        public async Task<ActionResult<ApiResponse<List<LoginHistoryDto>>>> GetToday(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                var from = (fromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
                var to   = (toDate   ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month,
                                DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month))).Date;

                if (from > to)
                    return ApiBadRequest("fromDate cannot be greater than toDate");

                var list = await _context.UserLoginHistories
                    .Where(h => h.LoginDate >= from &&
                                h.LoginDate <= to)
                    .OrderByDescending(h => h.LoginDate)
                    .ThenByDescending(h => h.LoginTime)
                    .Select(h => new LoginHistoryDto
                    {
                        Id            = h.Id,
                        UserId        = h.UserId,
                        UserName      = h.UserName,
                        LoginDate     = h.LoginDate,
                        LoginTime     = h.LoginTime.HasValue ? h.LoginTime.Value.ToString(@"hh\:mm\:ss") : null,
                        DeviceType    = h.DeviceType,
                        DeviceName    = h.DeviceName,
                        LogoutDate    = h.LogoutDate,
                        LogoutTime    = h.LogoutTime.HasValue ? h.LogoutTime.Value.ToString(@"hh\:mm\:ss") : null,
                        IsActive      = h.LogoutDate == null,
                        CreatedAt     = h.CreatedAt,
                        UpdatedAt     = h.UpdatedAt,
                        // ✅ NEW
                        TotalMinutes  = h.TotalMinutes,
                        TotalDuration = h.TotalMinutes.HasValue
                                        ? $"{h.TotalMinutes / 60}h {h.TotalMinutes % 60}m"
                                        : null,
                        LogoutReason  = h.LogoutReason,
                        SessionStatus = h.LogoutDate != null      ? "LoggedOut"
                                      : h.TokenExpiresAt < DateTime.Now ? "Expired"
                                      : "Active"
                    })
                    .ToListAsync();

                return ApiOk($"{list.Count} records from {from:dd-MMM-yyyy} to {to:dd-MMM-yyyy}", list);
            }
            catch (Exception ex) { return ApiServerError("Failed to fetch login history", ex); }
        }

        // ─────────────────────────────────────────────────────────────
        // 3. EK USER KI HISTORY  — Admin / Manager
        //    GET /api/loginhistory/user/{userId}
        //    GET /api/loginhistory/user/{userId}?fromDate=2026-03-01&toDate=2026-03-31
        // ─────────────────────────────────────────────────────────────
        [Authorize(Roles = "admin,manager")]
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<List<LoginHistoryDto>>>> GetByUser(
            int userId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                var from = (fromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
                var to   = (toDate   ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month,
                                DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month))).Date;

                if (from > to)
                    return ApiBadRequest("fromDate cannot be greater than toDate");

                var list = await _context.UserLoginHistories
                    .Where(h => h.UserId    == userId &&
                                h.LoginDate >= from   &&
                                h.LoginDate <= to)
                    .OrderByDescending(h => h.LoginDate)
                    .ThenByDescending(h => h.LoginTime)
                    .Select(h => new LoginHistoryDto
                    {
                        Id            = h.Id,
                        UserId        = h.UserId,
                        UserName      = h.UserName,
                        LoginDate     = h.LoginDate,
                        LoginTime     = h.LoginTime.HasValue ? h.LoginTime.Value.ToString(@"hh\:mm\:ss") : null,
                        DeviceType    = h.DeviceType,
                        DeviceName    = h.DeviceName,
                        LogoutDate    = h.LogoutDate,
                        LogoutTime    = h.LogoutTime.HasValue ? h.LogoutTime.Value.ToString(@"hh\:mm\:ss") : null,
                        IsActive      = h.LogoutDate == null,
                        CreatedAt     = h.CreatedAt,
                        UpdatedAt     = h.UpdatedAt,
                        // ✅ NEW
                        TotalMinutes  = h.TotalMinutes,
                        TotalDuration = h.TotalMinutes.HasValue
                                        ? $"{h.TotalMinutes / 60}h {h.TotalMinutes % 60}m"
                                        : null,
                        LogoutReason  = h.LogoutReason,
                        SessionStatus = h.LogoutDate != null      ? "LoggedOut"
                                      : h.TokenExpiresAt < DateTime.Now ? "Expired"
                                      : "Active"
                    })
                    .ToListAsync();

                if (!list.Any())
                    return ApiNotFound($"No records found for user ID {userId} from {from:dd-MMM-yyyy} to {to:dd-MMM-yyyy}");

                return ApiOk($"{list.Count} records from {from:dd-MMM-yyyy} to {to:dd-MMM-yyyy}", list);
            }
            catch (Exception ex) { return ApiServerError("Failed to fetch user login history", ex); }
        }
    }
}