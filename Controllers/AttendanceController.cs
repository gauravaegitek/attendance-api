// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using attendance_api.Data;
// using attendance_api.DTOs;
// using attendance_api.Models;
// using attendance_api.Services;

// namespace attendance_api.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     [Authorize]
//     public class AttendanceController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;
//         private readonly IFileService _fileService;
//         private readonly IPdfService _pdfService;
//         private readonly IConfiguration _configuration;

//         public AttendanceController(
//             ApplicationDbContext context,
//             IFileService fileService,
//             IPdfService pdfService,
//             IConfiguration configuration)
//         {
//             _context = context;
//             _fileService = fileService;
//             _pdfService = pdfService;
//             _configuration = configuration;
//         }

//         [HttpPost("markin")]
//         public async Task<ActionResult<ApiResponse<object>>> MarkIn([FromForm] MarkInDto dto)
//         {
//             try
//             {
//                 // Auto-populate UserId from JWT token
//                 var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
//                 if (userIdClaim == null)
//                 {
//                     return Unauthorized(new ApiResponse<object>
//                     {
//                         Success = false,
//                         Message = "Invalid token"
//                     });
//                 }
//                 dto.UserId = int.Parse(userIdClaim.Value);

//                 // Validate Biometric Data (Required for both In and Out)
//                 if (string.IsNullOrWhiteSpace(dto.BiometricData))
//                 {
//                     return BadRequest(new ApiResponse<object>
//                     {
//                         Success = false,
//                         Message = "Biometric data is required for check-in"
//                     });
//                 }

//                 // Get user with role information for conditional selfie check
//                 var user = await _context.Users
//                     .Include(u => u.RoleEntity)
//                     .FirstOrDefaultAsync(u => u.UserId == dto.UserId);

//                 if (user == null)
//                 {
//                     return NotFound(new ApiResponse<object>
//                     {
//                         Success = false,
//                         Message = "User not found"
//                     });
//                 }

//                 // Conditional Selfie Based on Role
//                 bool requiresSelfie = user.RoleEntity?.RequiresSelfie ?? false;
//                 if (requiresSelfie && dto.SelfieImage == null)
//                 {
//                     return BadRequest(new ApiResponse<object>
//                     {
//                         Success = false,
//                         Message = "Selfie is required for your role"
//                     });
//                 }

//                 // Auto-populate InTime with current time
//                 var currentTime = DateTime.Now.TimeOfDay;
//                 dto.InTime = currentTime;

//                 // Check if attendance already exists for today
//                 var existingAttendance = await _context.Attendances
//                     .FirstOrDefaultAsync(a => a.UserId == dto.UserId &&
//                                             a.AttendanceDate.Date == dto.AttendanceDate.Date);

//                 if (existingAttendance != null && existingAttendance.InTime.HasValue)
//                 {
//                     return BadRequest(new ApiResponse<object>
//                     {
//                         Success = false,
//                         Message = "Attendance already marked for today"
//                     });
//                 }

//                 // Save selfie (only if provided)
//                 string? selfiePath = null;
//                 if (dto.SelfieImage != null)
//                 {
//                     var selfieFolderPath = _configuration["AppSettings:SelfieFolderPath"] ?? "wwwroot/uploads/selfies";
//                     selfiePath = await _fileService.SaveSelfieAsync(dto.SelfieImage, selfieFolderPath);
//                 }

//                 // Create or update attendance record
//                 if (existingAttendance == null)
//                 {
//                     var attendance = new Attendance
//                     {
//                         UserId = dto.UserId,
//                         AttendanceDate = dto.AttendanceDate.Date,
//                         InTime = dto.InTime.Value,
//                         InTimeDateTime = dto.AttendanceDate.Date.Add(dto.InTime.Value),
//                         InLatitude = dto.Latitude,
//                         InLongitude = dto.Longitude,
//                         InLocationAddress = dto.LocationAddress,
//                         InSelfie = selfiePath,
//                         InBiometric = dto.BiometricData,
//                         CreatedOn = DateTime.Now
//                     };

//                     _context.Attendances.Add(attendance);
//                 }
//                 else
//                 {
//                     existingAttendance.InTime = dto.InTime.Value;
//                     existingAttendance.InTimeDateTime = dto.AttendanceDate.Date.Add(dto.InTime.Value);
//                     existingAttendance.InLatitude = dto.Latitude;
//                     existingAttendance.InLongitude = dto.Longitude;
//                     existingAttendance.InLocationAddress = dto.LocationAddress;
//                     existingAttendance.InSelfie = selfiePath;
//                     existingAttendance.InBiometric = dto.BiometricData;
//                 }

//                 await _context.SaveChangesAsync();

//                 return Ok(new ApiResponse<object>
//                 {
//                     Success = true,
//                     Message = "Check-in successful",
//                     Data = new
//                     {
//                         attendanceDate = dto.AttendanceDate.ToString("dd-MMM-yyyy"),
//                         inTime = dto.InTime.Value.ToString(@"hh\:mm\:ss"),
//                         location = dto.LocationAddress,
//                         biometricCaptured = true,
//                         selfieCaptured = selfiePath != null
//                     }
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<object>
//                 {
//                     Success = false,
//                     Message = "Check-in failed",
//                     Errors = new List<string> { ex.Message }
//                 });
//             }
//         }

//         [HttpPost("markout")]
//         public async Task<ActionResult<ApiResponse<object>>> MarkOut([FromForm] MarkOutDto dto)
//         {
//             try
//             {
//                 // Auto-populate UserId from JWT token
//                 var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
//                 if (userIdClaim == null)
//                 {
//                     return Unauthorized(new ApiResponse<object>
//                     {
//                         Success = false,
//                         Message = "Invalid token"
//                     });
//                 }
//                 dto.UserId = int.Parse(userIdClaim.Value);

//                 // Validate Biometric Data (Required for both In and Out)
//                 if (string.IsNullOrWhiteSpace(dto.BiometricData))
//                 {
//                     return BadRequest(new ApiResponse<object>
//                     {
//                         Success = false,
//                         Message = "Biometric data is required for check-out"
//                     });
//                 }

//                 // Get user with role information for conditional selfie check
//                 var user = await _context.Users
//                     .Include(u => u.RoleEntity)
//                     .FirstOrDefaultAsync(u => u.UserId == dto.UserId);

//                 if (user == null)
//                 {
//                     return NotFound(new ApiResponse<object>
//                     {
//                         Success = false,
//                         Message = "User not found"
//                     });
//                 }

//                 // Conditional Selfie Based on Role
//                 bool requiresSelfie = user.RoleEntity?.RequiresSelfie ?? false;
//                 if (requiresSelfie && dto.SelfieImage == null)
//                 {
//                     return BadRequest(new ApiResponse<object>
//                     {
//                         Success = false,
//                         Message = "Selfie is required for your role"
//                     });
//                 }

//                 // Auto-populate OutTime with current time
//                 var currentTime = DateTime.Now.TimeOfDay;
//                 dto.OutTime = currentTime;

//                 // Find today's attendance record
//                 var attendance = await _context.Attendances
//                     .FirstOrDefaultAsync(a => a.UserId == dto.UserId &&
//                                             a.AttendanceDate.Date == dto.AttendanceDate.Date);

//                 if (attendance == null || !attendance.InTime.HasValue)
//                 {
//                     return BadRequest(new ApiResponse<object>
//                     {
//                         Success = false,
//                         Message = "Please mark check-in first"
//                     });
//                 }

//                 if (attendance.OutTime.HasValue)
//                 {
//                     return BadRequest(new ApiResponse<object>
//                     {
//                         Success = false,
//                         Message = "Check-out already marked for today"
//                     });
//                 }

//                 // Save selfie (only if provided)
//                 string? selfiePath = null;
//                 if (dto.SelfieImage != null)
//                 {
//                     var selfieFolderPath = _configuration["AppSettings:SelfieFolderPath"] ?? "wwwroot/uploads/selfies";
//                     selfiePath = await _fileService.SaveSelfieAsync(dto.SelfieImage, selfieFolderPath);
//                 }

//                 // Update attendance record
//                 attendance.OutTime = dto.OutTime.Value;
//                 attendance.OutTimeDateTime = dto.AttendanceDate.Date.Add(dto.OutTime.Value);
//                 attendance.OutLatitude = dto.Latitude;
//                 attendance.OutLongitude = dto.Longitude;
//                 attendance.OutLocationAddress = dto.LocationAddress;
//                 attendance.OutSelfie = selfiePath;
//                 attendance.OutBiometric = dto.BiometricData;
//                 attendance.UpdatedOn = DateTime.Now;

//                 // Calculate total hours
//                 if (attendance.InTime.HasValue && attendance.OutTime.HasValue)
//                 {
//                     var totalMinutes = (attendance.OutTime.Value - attendance.InTime.Value).TotalMinutes;
//                     attendance.TotalHours = (decimal)(totalMinutes / 60.0);
//                 }

//                 await _context.SaveChangesAsync();

//                 return Ok(new ApiResponse<object>
//                 {
//                     Success = true,
//                     Message = "Check-out successful",
//                     Data = new
//                     {
//                         attendanceDate = dto.AttendanceDate.ToString("dd-MMM-yyyy"),
//                         outTime = dto.OutTime.Value.ToString(@"hh\:mm\:ss"),
//                         totalHours = attendance.TotalHours?.ToString("0.00"),
//                         location = dto.LocationAddress,
//                         biometricCaptured = true,
//                         selfieCaptured = selfiePath != null
//                     }
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<object>
//                 {
//                     Success = false,
//                     Message = "Check-out failed",
//                     Errors = new List<string> { ex.Message }
//                 });
//             }
//         }

//         [HttpGet("usersummary")]
//         public async Task<ActionResult<ApiResponse<List<AttendanceSummaryDto>>>> GetUserSummary(
//             [FromQuery] DateTime fromDate,
//             [FromQuery] DateTime toDate)
//         {
//             try
//             {
//                 // Auto-populate UserId from JWT token
//                 var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
//                 if (userIdClaim == null)
//                 {
//                     return Unauthorized(new ApiResponse<List<AttendanceSummaryDto>>
//                     {
//                         Success = false,
//                         Message = "Invalid token"
//                     });
//                 }
//                 var userId = int.Parse(userIdClaim.Value);

//                 // Validate date range - only 1 month allowed
//                 var daysDifference = (toDate.Date - fromDate.Date).Days;
//                 if (daysDifference > 31)
//                 {
//                     return BadRequest(new ApiResponse<List<AttendanceSummaryDto>>
//                     {
//                         Success = false,
//                         Message = "Date range cannot exceed one month (31 days)"
//                     });
//                 }

//                 var query = _context.Attendances
//                     .Include(a => a.User)
//                     .Where(a => a.UserId == userId);

//                 query = query.Where(a => a.AttendanceDate >= fromDate.Date && a.AttendanceDate <= toDate.Date);

//                 var attendances = await query
//                     .OrderByDescending(a => a.AttendanceDate)
//                     .Select(a => new AttendanceSummaryDto
//                     {
//                         AttendanceId = a.AttendanceId,
//                         UserName = a.User.UserName,
//                         Role = a.User.Role,
//                         AttendanceDate = a.AttendanceDate,
//                         InTime = a.InTime.HasValue ? a.InTime.Value.ToString(@"hh\:mm\:ss") : null,
//                         OutTime = a.OutTime.HasValue ? a.OutTime.Value.ToString(@"hh\:mm\:ss") : null,
//                         InLocation = a.InLocationAddress,
//                         OutLocation = a.OutLocationAddress,
//                         TotalHours = a.TotalHours,
//                         Status = !a.InTime.HasValue ? "Not Marked" :
//                                 !a.OutTime.HasValue ? "In Time Only" : "Complete"
//                     })
//                     .ToListAsync();

//                 return Ok(new ApiResponse<List<AttendanceSummaryDto>>
//                 {
//                     Success = true,
//                     Message = "Summary retrieved successfully",
//                     Data = attendances
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<List<AttendanceSummaryDto>>
//                 {
//                     Success = false,
//                     Message = "Failed to retrieve summary",
//                     Errors = new List<string> { ex.Message }
//                 });
//             }
//         }

//         [Authorize(Roles = "admin")]
//         [HttpGet("adminsummary")]
//         public async Task<ActionResult<ApiResponse<List<AttendanceSummaryDto>>>> GetAdminSummary(
//             [FromQuery] string role,
//             [FromQuery] DateTime fromDate,
//             [FromQuery] DateTime toDate)
//         {
//             try
//             {
//                 // Validate date range - only 1 month allowed
//                 var daysDifference = (toDate.Date - fromDate.Date).Days;
//                 if (daysDifference > 31)
//                 {
//                     return BadRequest(new ApiResponse<List<AttendanceSummaryDto>>
//                     {
//                         Success = false,
//                         Message = "Date range cannot exceed one month (31 days)"
//                     });
//                 }

//                 var query = _context.Attendances
//                     .Include(a => a.User)
//                     .AsQueryable();

//                 // ✅ Filter by role - skip filter if role is "all" or empty
//                 if (!string.IsNullOrEmpty(role) && role.ToLower() != "all")
//                 {
//                     query = query.Where(a => a.User.Role.ToLower() == role.ToLower());
//                 }

//                 // Filter by date range (required)
//                 query = query.Where(a => a.AttendanceDate >= fromDate.Date && a.AttendanceDate <= toDate.Date);

//                 var attendances = await query
//                     .OrderByDescending(a => a.AttendanceDate)
//                     .ThenBy(a => a.User.UserName)
//                     .Select(a => new AttendanceSummaryDto
//                     {
//                         AttendanceId = a.AttendanceId,
//                         UserName = a.User.UserName,
//                         Role = a.User.Role,
//                         AttendanceDate = a.AttendanceDate,
//                         InTime = a.InTime.HasValue ? a.InTime.Value.ToString(@"hh\:mm\:ss") : null,
//                         OutTime = a.OutTime.HasValue ? a.OutTime.Value.ToString(@"hh\:mm\:ss") : null,
//                         InLocation = a.InLocationAddress,
//                         OutLocation = a.OutLocationAddress,
//                         TotalHours = a.TotalHours,
//                         Status = !a.InTime.HasValue ? "Not Marked" :
//                                 !a.OutTime.HasValue ? "In Time Only" : "Complete"
//                     })
//                     .ToListAsync();

//                 return Ok(new ApiResponse<List<AttendanceSummaryDto>>
//                 {
//                     Success = true,
//                     Message = "Admin summary retrieved successfully",
//                     Data = attendances
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<List<AttendanceSummaryDto>>
//                 {
//                     Success = false,
//                     Message = "Failed to retrieve admin summary",
//                     Errors = new List<string> { ex.Message }
//                 });
//             }
//         }

//         [HttpPost("exportusersummary")]
//         public async Task<IActionResult> ExportUserSummary([FromBody] ExportUserSummaryDto dto)
//         {
//             try
//             {
//                 // Auto-populate UserId from JWT token
//                 var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
//                 if (userIdClaim == null)
//                 {
//                     return Unauthorized(new { message = "Invalid token" });
//                 }
//                 dto.UserId = int.Parse(userIdClaim.Value);

//                 var user = await _context.Users.FindAsync(dto.UserId);
//                 if (user == null)
//                 {
//                     return NotFound(new { message = "User not found" });
//                 }

//                 var attendances = await _context.Attendances
//                     .Include(a => a.User)
//                     .Where(a => a.UserId == dto.UserId &&
//                                a.AttendanceDate >= dto.FromDate.Date &&
//                                a.AttendanceDate <= dto.ToDate.Date)
//                     .OrderBy(a => a.AttendanceDate)
//                     .Select(a => new AttendanceSummaryDto
//                     {
//                         AttendanceId = a.AttendanceId,
//                         UserName = a.User.UserName,
//                         Role = a.User.Role,
//                         AttendanceDate = a.AttendanceDate,
//                         InTime = a.InTime.HasValue ? a.InTime.Value.ToString(@"hh\:mm\:ss") : null,
//                         OutTime = a.OutTime.HasValue ? a.OutTime.Value.ToString(@"hh\:mm\:ss") : null,
//                         InLocation = a.InLocationAddress,
//                         OutLocation = a.OutLocationAddress,
//                         TotalHours = a.TotalHours
//                     })
//                     .ToListAsync();

//                 if (attendances == null || !attendances.Any())
//                 {
//                     return BadRequest(new { message = "No attendance records found for the given date range" });
//                 }

//                 var pdfBytes = _pdfService.GenerateUserSummaryPdf(
//                     attendances,
//                     user.UserName,
//                     user.Role,
//                     dto.FromDate,
//                     dto.ToDate);

//                 var fileName = $"attendance_summary_{user.UserName}_{dto.FromDate:yyyyMMdd}_{dto.ToDate:yyyyMMdd}.pdf";

//                 return File(pdfBytes, "application/pdf", fileName);
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new { message = "PDF generation failed", error = ex.Message });
//             }
//         }

//         [Authorize(Roles = "admin")]
//         [HttpPost("exportadminsummary")]
//         public async Task<IActionResult> ExportAdminSummary([FromBody] ExportAdminSummaryDto dto)
//         {
//             try
//             {
//                 var query = _context.Attendances
//                     .Include(a => a.User)
//                     .AsQueryable();

//                 // Filter by specific user (optional)
//                 if (dto.FilterType == "user" && dto.UserId.HasValue)
//                 {
//                     query = query.Where(a => a.UserId == dto.UserId.Value);
//                 }

//                 // ✅ Filter by role - skip filter if role is "all" or empty
//                 if (!string.IsNullOrEmpty(dto.Role) && dto.Role.ToLower() != "all")
//                 {
//                     query = query.Where(a => a.User.Role.ToLower() == dto.Role.ToLower());
//                 }

//                 query = query.Where(a => a.AttendanceDate >= dto.FromDate.Date);
//                 query = query.Where(a => a.AttendanceDate <= dto.ToDate.Date);

//                 var attendances = await query
//                     .OrderBy(a => a.AttendanceDate)
//                     .ThenBy(a => a.User.UserName)
//                     .Select(a => new AttendanceSummaryDto
//                     {
//                         AttendanceId = a.AttendanceId,
//                         UserName = a.User.UserName,
//                         Role = a.User.Role,
//                         AttendanceDate = a.AttendanceDate,
//                         InTime = a.InTime.HasValue ? a.InTime.Value.ToString(@"hh\:mm\:ss") : null,
//                         OutTime = a.OutTime.HasValue ? a.OutTime.Value.ToString(@"hh\:mm\:ss") : null,
//                         InLocation = a.InLocationAddress,
//                         OutLocation = a.OutLocationAddress,
//                         TotalHours = a.TotalHours
//                     })
//                     .ToListAsync();

//                 if (attendances == null || !attendances.Any())
//                 {
//                     return BadRequest(new { message = "No attendance records found for the given criteria" });
//                 }

//                 // ✅ Role label for PDF title
//                 var roleLabel = string.IsNullOrEmpty(dto.Role) || dto.Role.ToLower() == "all"
//                     ? "All Roles"
//                     : dto.Role.ToUpper();

//                 var pdfBytes = _pdfService.GenerateAdminSummaryPdf(
//                     attendances,
//                     dto.FromDate,
//                     dto.ToDate,
//                     roleLabel);

//                 var fileName = $"admin_attendance_summary_{dto.FromDate:yyyyMMdd}_{dto.ToDate:yyyyMMdd}.pdf";

//                 return File(pdfBytes, "application/pdf", fileName);
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new { message = "PDF generation failed", error = ex.Message });
//             }
//         }
//     }
// }







using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using attendance_api.Data;
using attendance_api.DTOs;
using attendance_api.Models;
using attendance_api.Services;

namespace attendance_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AttendanceController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly IPdfService _pdfService;
        private readonly IConfiguration _configuration;

        public AttendanceController(
            ApplicationDbContext context,
            IFileService fileService,
            IPdfService pdfService,
            IConfiguration configuration)
        {
            _context = context;
            _fileService = fileService;
            _pdfService = pdfService;
            _configuration = configuration;
        }

        [HttpPost("markin")]
        public async Task<ActionResult<ApiResponse<object>>> MarkIn([FromForm] MarkInDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return ApiUnauthorized("Invalid token");
                dto.UserId = int.Parse(userIdClaim.Value);

                if (string.IsNullOrWhiteSpace(dto.BiometricData))
                    return ApiBadRequest("Biometric data is required for check-in");

                var user = await _context.Users
                    .Include(u => u.RoleEntity)
                    .FirstOrDefaultAsync(u => u.UserId == dto.UserId);

                if (user == null) return ApiNotFound("User not found");

                bool requiresSelfie = user.RoleEntity?.RequiresSelfie ?? false;
                if (requiresSelfie && dto.SelfieImage == null)
                    return ApiBadRequest("Selfie is required for your role");

                var currentTime = DateTime.Now.TimeOfDay;
                dto.InTime = currentTime;

                var existingAttendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.UserId == dto.UserId &&
                                            a.AttendanceDate.Date == dto.AttendanceDate.Date);

                if (existingAttendance != null && existingAttendance.InTime.HasValue)
                    return ApiBadRequest("Attendance already marked for today");

                string? selfiePath = null;
                if (dto.SelfieImage != null)
                {
                    var selfieFolderPath = _configuration["AppSettings:SelfieFolderPath"] ?? "wwwroot/uploads/selfies";
                    selfiePath = await _fileService.SaveSelfieAsync(dto.SelfieImage, selfieFolderPath);
                }

                if (existingAttendance == null)
                {
                    var attendance = new Attendance
                    {
                        UserId = dto.UserId,
                        AttendanceDate = dto.AttendanceDate.Date,
                        InTime = dto.InTime.Value,
                        InTimeDateTime = dto.AttendanceDate.Date.Add(dto.InTime.Value),
                        InLatitude = dto.Latitude,
                        InLongitude = dto.Longitude,
                        InLocationAddress = dto.LocationAddress,
                        InSelfie = selfiePath,
                        InBiometric = dto.BiometricData,
                        CreatedOn = DateTime.Now
                    };
                    _context.Attendances.Add(attendance);
                }
                else
                {
                    existingAttendance.InTime = dto.InTime.Value;
                    existingAttendance.InTimeDateTime = dto.AttendanceDate.Date.Add(dto.InTime.Value);
                    existingAttendance.InLatitude = dto.Latitude;
                    existingAttendance.InLongitude = dto.Longitude;
                    existingAttendance.InLocationAddress = dto.LocationAddress;
                    existingAttendance.InSelfie = selfiePath;
                    existingAttendance.InBiometric = dto.BiometricData;
                }

                await _context.SaveChangesAsync();

                return ApiOk("Check-in successful", new
                {
                    attendanceDate    = dto.AttendanceDate.ToString("dd-MMM-yyyy"),
                    inTime            = dto.InTime.Value.ToString(@"hh\:mm\:ss"),
                    location          = dto.LocationAddress,
                    biometricCaptured = true,
                    selfieCaptured    = selfiePath != null
                });
            }
            catch (Exception ex) { return ApiServerError("Check-in failed", ex); }
        }

        [HttpPost("markout")]
        public async Task<ActionResult<ApiResponse<object>>> MarkOut([FromForm] MarkOutDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return ApiUnauthorized("Invalid token");
                dto.UserId = int.Parse(userIdClaim.Value);

                if (string.IsNullOrWhiteSpace(dto.BiometricData))
                    return ApiBadRequest("Biometric data is required for check-out");

                var user = await _context.Users
                    .Include(u => u.RoleEntity)
                    .FirstOrDefaultAsync(u => u.UserId == dto.UserId);

                if (user == null) return ApiNotFound("User not found");

                bool requiresSelfie = user.RoleEntity?.RequiresSelfie ?? false;
                if (requiresSelfie && dto.SelfieImage == null)
                    return ApiBadRequest("Selfie is required for your role");

                var currentTime = DateTime.Now.TimeOfDay;
                dto.OutTime = currentTime;

                var attendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.UserId == dto.UserId &&
                                            a.AttendanceDate.Date == dto.AttendanceDate.Date);

                if (attendance == null || !attendance.InTime.HasValue)
                    return ApiBadRequest("Please mark check-in first");

                if (attendance.OutTime.HasValue)
                    return ApiBadRequest("Check-out already marked for today");

                string? selfiePath = null;
                if (dto.SelfieImage != null)
                {
                    var selfieFolderPath = _configuration["AppSettings:SelfieFolderPath"] ?? "wwwroot/uploads/selfies";
                    selfiePath = await _fileService.SaveSelfieAsync(dto.SelfieImage, selfieFolderPath);
                }

                attendance.OutTime = dto.OutTime.Value;
                attendance.OutTimeDateTime = dto.AttendanceDate.Date.Add(dto.OutTime.Value);
                attendance.OutLatitude = dto.Latitude;
                attendance.OutLongitude = dto.Longitude;
                attendance.OutLocationAddress = dto.LocationAddress;
                attendance.OutSelfie = selfiePath;
                attendance.OutBiometric = dto.BiometricData;
                attendance.UpdatedOn = DateTime.Now;

                if (attendance.InTime.HasValue && attendance.OutTime.HasValue)
                {
                    var totalMinutes = (attendance.OutTime.Value - attendance.InTime.Value).TotalMinutes;
                    attendance.TotalHours = (decimal)(totalMinutes / 60.0);
                }

                await _context.SaveChangesAsync();

                return ApiOk("Check-out successful", new
                {
                    attendanceDate    = dto.AttendanceDate.ToString("dd-MMM-yyyy"),
                    outTime           = dto.OutTime.Value.ToString(@"hh\:mm\:ss"),
                    totalHours        = attendance.TotalHours?.ToString("0.00"),
                    location          = dto.LocationAddress,
                    biometricCaptured = true,
                    selfieCaptured    = selfiePath != null
                });
            }
            catch (Exception ex) { return ApiServerError("Check-out failed", ex); }
        }

        [HttpGet("usersummary")]
        public async Task<ActionResult<ApiResponse<List<AttendanceSummaryDto>>>> GetUserSummary(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return ApiUnauthorized("Invalid token");
                var userId = int.Parse(userIdClaim.Value);

                if ((toDate.Date - fromDate.Date).Days > 31)
                    return ApiBadRequest("Date range cannot exceed one month (31 days)");

                var attendances = await _context.Attendances
                    .Include(a => a.User)
                    .Where(a => a.UserId == userId &&
                                a.AttendanceDate >= fromDate.Date &&
                                a.AttendanceDate <= toDate.Date)
                    .OrderByDescending(a => a.AttendanceDate)
                    .Select(a => new AttendanceSummaryDto
                    {
                        AttendanceId  = a.AttendanceId,
                        UserName      = a.User.UserName,
                        Role          = a.User.Role,
                        AttendanceDate = a.AttendanceDate,
                        InTime        = a.InTime.HasValue ? a.InTime.Value.ToString(@"hh\:mm\:ss") : null,
                        OutTime       = a.OutTime.HasValue ? a.OutTime.Value.ToString(@"hh\:mm\:ss") : null,
                        InLocation    = a.InLocationAddress,
                        OutLocation   = a.OutLocationAddress,
                        TotalHours    = a.TotalHours,
                        Status        = !a.InTime.HasValue ? "Not Marked" :
                                        !a.OutTime.HasValue ? "In Time Only" : "Complete"
                    })
                    .ToListAsync();

                return ApiOk("Summary retrieved successfully", attendances);
            }
            catch (Exception ex) { return ApiServerError("Failed to retrieve summary", ex); }
        }

        [Authorize(Roles = "admin")]
        [HttpGet("adminsummary")]
        public async Task<ActionResult<ApiResponse<List<AttendanceSummaryDto>>>> GetAdminSummary(
            [FromQuery] string role,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            try
            {
                if ((toDate.Date - fromDate.Date).Days > 31)
                    return ApiBadRequest("Date range cannot exceed one month (31 days)");

                var query = _context.Attendances.Include(a => a.User).AsQueryable();

                if (!string.IsNullOrEmpty(role) && role.ToLower() != "all")
                    query = query.Where(a => a.User.Role.ToLower() == role.ToLower());

                query = query.Where(a => a.AttendanceDate >= fromDate.Date && a.AttendanceDate <= toDate.Date);

                var attendances = await query
                    .OrderByDescending(a => a.AttendanceDate)
                    .ThenBy(a => a.User.UserName)
                    .Select(a => new AttendanceSummaryDto
                    {
                        AttendanceId  = a.AttendanceId,
                        UserName      = a.User.UserName,
                        Role          = a.User.Role,
                        AttendanceDate = a.AttendanceDate,
                        InTime        = a.InTime.HasValue ? a.InTime.Value.ToString(@"hh\:mm\:ss") : null,
                        OutTime       = a.OutTime.HasValue ? a.OutTime.Value.ToString(@"hh\:mm\:ss") : null,
                        InLocation    = a.InLocationAddress,
                        OutLocation   = a.OutLocationAddress,
                        TotalHours    = a.TotalHours,
                        Status        = !a.InTime.HasValue ? "Not Marked" :
                                        !a.OutTime.HasValue ? "In Time Only" : "Complete"
                    })
                    .ToListAsync();

                return ApiOk("Admin summary retrieved successfully", attendances);
            }
            catch (Exception ex) { return ApiServerError("Failed to retrieve admin summary", ex); }
        }

        [HttpPost("exportusersummary")]
        public async Task<IActionResult> ExportUserSummary([FromBody] ExportUserSummaryDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return ApiUnauthorized("Invalid token");
                dto.UserId = int.Parse(userIdClaim.Value);

                var user = await _context.Users.FindAsync(dto.UserId);
                if (user == null) return ApiNotFound("User not found");

                var attendances = await _context.Attendances
                    .Include(a => a.User)
                    .Where(a => a.UserId == dto.UserId &&
                               a.AttendanceDate >= dto.FromDate.Date &&
                               a.AttendanceDate <= dto.ToDate.Date)
                    .OrderBy(a => a.AttendanceDate)
                    .Select(a => new AttendanceSummaryDto
                    {
                        AttendanceId  = a.AttendanceId,
                        UserName      = a.User.UserName,
                        Role          = a.User.Role,
                        AttendanceDate = a.AttendanceDate,
                        InTime        = a.InTime.HasValue ? a.InTime.Value.ToString(@"hh\:mm\:ss") : null,
                        OutTime       = a.OutTime.HasValue ? a.OutTime.Value.ToString(@"hh\:mm\:ss") : null,
                        InLocation    = a.InLocationAddress,
                        OutLocation   = a.OutLocationAddress,
                        TotalHours    = a.TotalHours
                    })
                    .ToListAsync();

                if (!attendances.Any())
                    return ApiBadRequest("No attendance records found for the given date range");

                var pdfBytes = _pdfService.GenerateUserSummaryPdf(
                    attendances, user.UserName, user.Role, dto.FromDate, dto.ToDate);

                var fileName = $"attendance_summary_{user.UserName}_{dto.FromDate:yyyyMMdd}_{dto.ToDate:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex) { return ApiServerError("PDF generation failed", ex); }
        }

        [Authorize(Roles = "admin")]
        [HttpPost("exportadminsummary")]
        public async Task<IActionResult> ExportAdminSummary([FromBody] ExportAdminSummaryDto dto)
        {
            try
            {
                var query = _context.Attendances.Include(a => a.User).AsQueryable();

                if (dto.FilterType == "user" && dto.UserId.HasValue)
                    query = query.Where(a => a.UserId == dto.UserId.Value);

                if (!string.IsNullOrEmpty(dto.Role) && dto.Role.ToLower() != "all")
                    query = query.Where(a => a.User.Role.ToLower() == dto.Role.ToLower());

                query = query.Where(a => a.AttendanceDate >= dto.FromDate.Date && a.AttendanceDate <= dto.ToDate.Date);

                var attendances = await query
                    .OrderBy(a => a.AttendanceDate)
                    .ThenBy(a => a.User.UserName)
                    .Select(a => new AttendanceSummaryDto
                    {
                        AttendanceId  = a.AttendanceId,
                        UserName      = a.User.UserName,
                        Role          = a.User.Role,
                        AttendanceDate = a.AttendanceDate,
                        InTime        = a.InTime.HasValue ? a.InTime.Value.ToString(@"hh\:mm\:ss") : null,
                        OutTime       = a.OutTime.HasValue ? a.OutTime.Value.ToString(@"hh\:mm\:ss") : null,
                        InLocation    = a.InLocationAddress,
                        OutLocation   = a.OutLocationAddress,
                        TotalHours    = a.TotalHours
                    })
                    .ToListAsync();

                if (!attendances.Any())
                    return ApiBadRequest("No attendance records found for the given criteria");

                var roleLabel = string.IsNullOrEmpty(dto.Role) || dto.Role.ToLower() == "all"
                    ? "All Roles" : dto.Role.ToUpper();

                var pdfBytes = _pdfService.GenerateAdminSummaryPdf(attendances, dto.FromDate, dto.ToDate, roleLabel);
                var fileName = $"admin_attendance_summary_{dto.FromDate:yyyyMMdd}_{dto.ToDate:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex) { return ApiServerError("PDF generation failed", ex); }
        }
    }
}