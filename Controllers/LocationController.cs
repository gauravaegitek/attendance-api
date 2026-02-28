// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using attendance_api.Data;
// using attendance_api.DTOs;
// using attendance_api.Models;
// using System.Security.Claims;

// namespace attendance_api.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     [Authorize]
//     public class LocationController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;

//         public LocationController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         private int GetCurrentUserId()
//         {
//             var claim = User.FindFirst(ClaimTypes.NameIdentifier);
//             return claim != null ? int.Parse(claim.Value) : 0;
//         }

//         private string GetCurrentUserRole()
//         {
//             return User.FindFirst(ClaimTypes.Role)?.Value?.ToLower() ?? "";
//         }

//         // ✅ 1. Check-In — User field pe nikla
//         [HttpPost("checkin")]
//         public async Task<ActionResult<ApiResponse<LocationTrackingResponseDto>>> CheckIn([FromBody] LocationCheckInDto dto)
//         {
//             try
//             {
//                 var userId = GetCurrentUserId();
//                 if (userId == 0)
//                     return Unauthorized(new ApiResponse<LocationTrackingResponseDto> { Success = false, Message = "Invalid token" });

//                 // Agar aaj ka open checkin already hai to block karo
//                 var alreadyCheckedIn = await _context.LocationTrackings
//                     .AnyAsync(l => l.UserId == userId
//                         && l.CheckInTime.HasValue
//                         && !l.CheckOutTime.HasValue
//                         && l.CheckInTime.Value.Date == DateTime.Today);

//                 if (alreadyCheckedIn)
//                     return BadRequest(new ApiResponse<LocationTrackingResponseDto> { Success = false, Message = "Already checked in. Please check out first." });

//                 var tracking = new LocationTracking
//                 {
//                     UserId = userId,
//                     CheckInLatitude = dto.Latitude,
//                     CheckInLongitude = dto.Longitude,
//                     CheckInAddress = dto.Address,
//                     CheckInTime = DateTime.Now,
//                     WorkType = dto.WorkType.ToLower(),
//                     CreatedOn = DateTime.Now
//                 };

//                 _context.LocationTrackings.Add(tracking);
//                 await _context.SaveChangesAsync();

//                 var user = await _context.Users.FindAsync(userId);

//                 return Ok(new ApiResponse<LocationTrackingResponseDto>
//                 {
//                     Success = true,
//                     Message = "Check-in successful",
//                     Data = MapToDto(tracking, user!)
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<LocationTrackingResponseDto> { Success = false, Message = "Check-in failed", Errors = new List<string> { ex.Message } });
//             }
//         }

//         // ✅ 2. Check-Out — Client visit details bhi save hogi
//         [HttpPut("checkout")]
//         public async Task<ActionResult<ApiResponse<LocationTrackingResponseDto>>> CheckOut([FromBody] LocationCheckOutDto dto)
//         {
//             try
//             {
//                 var userId = GetCurrentUserId();

//                 var tracking = await _context.LocationTrackings
//                     .Include(l => l.User)
//                     .FirstOrDefaultAsync(l => l.TrackingId == dto.TrackingId && l.UserId == userId);

//                 if (tracking == null)
//                     return NotFound(new ApiResponse<LocationTrackingResponseDto> { Success = false, Message = "Check-in record not found" });

//                 if (tracking.CheckOutTime.HasValue)
//                     return BadRequest(new ApiResponse<LocationTrackingResponseDto> { Success = false, Message = "Already checked out" });

//                 tracking.CheckOutLatitude = dto.Latitude;
//                 tracking.CheckOutLongitude = dto.Longitude;
//                 tracking.CheckOutAddress = dto.Address;
//                 tracking.CheckOutTime = DateTime.Now;

//                 // Total time calculate karo
//                 if (tracking.CheckInTime.HasValue)
//                     tracking.TotalTimeMinutes = (int)(tracking.CheckOutTime.Value - tracking.CheckInTime.Value).TotalMinutes;

//                 // Client visit details
//                 tracking.IsClientVisit = dto.IsClientVisit;
//                 if (dto.IsClientVisit)
//                 {
//                     tracking.ClientName = dto.ClientName;
//                     tracking.ClientAddress = dto.ClientAddress;
//                     tracking.ClientLatitude = dto.ClientLatitude;
//                     tracking.ClientLongitude = dto.ClientLongitude;
//                     tracking.VisitPurpose = dto.VisitPurpose;
//                     tracking.MeetingNotes = dto.MeetingNotes;
//                     tracking.Outcome = dto.Outcome;
//                 }

//                 await _context.SaveChangesAsync();

//                 return Ok(new ApiResponse<LocationTrackingResponseDto>
//                 {
//                     Success = true,
//                     Message = "Check-out successful",
//                     Data = MapToDto(tracking, tracking.User!)
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<LocationTrackingResponseDto> { Success = false, Message = "Check-out failed", Errors = new List<string> { ex.Message } });
//             }
//         }

//         // ✅ 3. My Today's Tracking
//         [HttpGet("my/today")]
//         public async Task<ActionResult<ApiResponse<List<LocationTrackingResponseDto>>>> GetMyTodayTracking()
//         {
//             try
//             {
//                 var userId = GetCurrentUserId();
//                 var records = await _context.LocationTrackings
//                     .Include(l => l.User)
//                     .Where(l => l.UserId == userId && l.CheckInTime.HasValue && l.CheckInTime.Value.Date == DateTime.Today)
//                     .OrderByDescending(l => l.CheckInTime)
//                     .ToListAsync();

//                 return Ok(new ApiResponse<List<LocationTrackingResponseDto>>
//                 {
//                     Success = true,
//                     Message = "Today's tracking",
//                     Data = records.Select(r => MapToDto(r, r.User!)).ToList()
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<List<LocationTrackingResponseDto>> { Success = false, Message = "Failed", Errors = new List<string> { ex.Message } });
//             }
//         }

//         // ✅ 4. My All Tracking History
//         [HttpGet("my/history")]
//         public async Task<ActionResult<ApiResponse<List<LocationTrackingResponseDto>>>> GetMyHistory(
//             [FromQuery] DateTime? fromDate = null,
//             [FromQuery] DateTime? toDate = null)
//         {
//             try
//             {
//                 var userId = GetCurrentUserId();

//                 var query = _context.LocationTrackings
//                     .Include(l => l.User)
//                     .Where(l => l.UserId == userId);

//                 if (fromDate.HasValue)
//                     query = query.Where(l => l.CheckInTime >= fromDate.Value.Date);

//                 if (toDate.HasValue)
//                     query = query.Where(l => l.CheckInTime <= toDate.Value.Date.AddDays(1));

//                 var records = await query.OrderByDescending(l => l.CheckInTime).ToListAsync();

//                 return Ok(new ApiResponse<List<LocationTrackingResponseDto>>
//                 {
//                     Success = true,
//                     Message = $"Total {records.Count} records",
//                     Data = records.Select(r => MapToDto(r, r.User!)).ToList()
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<List<LocationTrackingResponseDto>> { Success = false, Message = "Failed", Errors = new List<string> { ex.Message } });
//             }
//         }

//         // ✅ 5. All Users Ki Tracking — Admin/Manager
//         [HttpGet("all")]
//         public async Task<ActionResult<ApiResponse<List<LocationTrackingResponseDto>>>> GetAllTracking(
//             [FromQuery] int? userId = null,
//             [FromQuery] DateTime? fromDate = null,
//             [FromQuery] DateTime? toDate = null,
//             [FromQuery] bool? isClientVisit = null)
//         {
//             try
//             {
//                 var role = GetCurrentUserRole();
//                 if (role != "admin" && role != "manager" && role != "hr")
//                     return StatusCode(403, new ApiResponse<List<LocationTrackingResponseDto>> { Success = false, Message = "Access denied." });

//                 var query = _context.LocationTrackings.Include(l => l.User).AsQueryable();

//                 if (userId.HasValue)
//                     query = query.Where(l => l.UserId == userId.Value);

//                 if (fromDate.HasValue)
//                     query = query.Where(l => l.CheckInTime >= fromDate.Value.Date);

//                 if (toDate.HasValue)
//                     query = query.Where(l => l.CheckInTime <= toDate.Value.Date.AddDays(1));

//                 if (isClientVisit.HasValue)
//                     query = query.Where(l => l.IsClientVisit == isClientVisit.Value);

//                 var records = await query.OrderByDescending(l => l.CheckInTime).ToListAsync();

//                 return Ok(new ApiResponse<List<LocationTrackingResponseDto>>
//                 {
//                     Success = true,
//                     Message = $"Total {records.Count} records",
//                     Data = records.Select(r => MapToDto(r, r.User!)).ToList()
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<List<LocationTrackingResponseDto>> { Success = false, Message = "Failed", Errors = new List<string> { ex.Message } });
//             }
//         }

//         // ✅ 6. Today All Users Ki Tracking — Admin
//         [HttpGet("all/today")]
//         public async Task<ActionResult<ApiResponse<List<LocationTrackingResponseDto>>>> GetTodayAllTracking()
//         {
//             try
//             {
//                 var role = GetCurrentUserRole();
//                 if (role != "admin" && role != "manager" && role != "hr")
//                     return StatusCode(403, new ApiResponse<List<LocationTrackingResponseDto>> { Success = false, Message = "Access denied." });

//                 var records = await _context.LocationTrackings
//                     .Include(l => l.User)
//                     .Where(l => l.CheckInTime.HasValue && l.CheckInTime.Value.Date == DateTime.Today)
//                     .OrderByDescending(l => l.CheckInTime)
//                     .ToListAsync();

//                 return Ok(new ApiResponse<List<LocationTrackingResponseDto>>
//                 {
//                     Success = true,
//                     Message = $"Today's tracking — {records.Count} records",
//                     Data = records.Select(r => MapToDto(r, r.User!)).ToList()
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<List<LocationTrackingResponseDto>> { Success = false, Message = "Failed", Errors = new List<string> { ex.Message } });
//             }
//         }

//         // ✅ Helper: Format minutes to "2hr 30min"
//         private string FormatMinutes(int? minutes)
//         {
//             if (!minutes.HasValue) return "N/A";
//             int hr = minutes.Value / 60;
//             int min = minutes.Value % 60;
//             if (hr == 0) return $"{min}min";
//             if (min == 0) return $"{hr}hr";
//             return $"{hr}hr {min}min";
//         }

//         private LocationTrackingResponseDto MapToDto(LocationTracking t, User user)
//         {
//             return new LocationTrackingResponseDto
//             {
//                 TrackingId = t.TrackingId,
//                 UserId = t.UserId,
//                 UserName = user.UserName,
//                 Role = user.Role,
//                 CheckInLatitude = t.CheckInLatitude,
//                 CheckInLongitude = t.CheckInLongitude,
//                 CheckInAddress = t.CheckInAddress,
//                 CheckInTime = t.CheckInTime,
//                 CheckOutLatitude = t.CheckOutLatitude,
//                 CheckOutLongitude = t.CheckOutLongitude,
//                 CheckOutAddress = t.CheckOutAddress,
//                 CheckOutTime = t.CheckOutTime,
//                 TotalTimeMinutes = t.TotalTimeMinutes,
//                 TotalTimeFormatted = FormatMinutes(t.TotalTimeMinutes),
//                 IsClientVisit = t.IsClientVisit,
//                 ClientName = t.ClientName,
//                 ClientAddress = t.ClientAddress,
//                 VisitPurpose = t.VisitPurpose,
//                 MeetingNotes = t.MeetingNotes,
//                 Outcome = t.Outcome,
//                 WorkType = t.WorkType,
//                 CreatedOn = t.CreatedOn
//             };
//         }
//     }
// }










using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using attendance_api.Data;
using attendance_api.DTOs;
using attendance_api.Models;

namespace attendance_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LocationController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public LocationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("checkin")]
        public async Task<ActionResult<ApiResponse<LocationTrackingResponseDto>>> CheckIn([FromBody] LocationCheckInDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0) return ApiUnauthorized("Invalid token");

                var alreadyCheckedIn = await _context.LocationTrackings
                    .AnyAsync(l => l.UserId == userId
                        && l.CheckInTime.HasValue
                        && !l.CheckOutTime.HasValue
                        && l.CheckInTime.Value.Date == DateTime.Today);

                if (alreadyCheckedIn)
                    return ApiBadRequest("Already checked in. Please check out first.");

                var tracking = new LocationTracking
                {
                    UserId           = userId,
                    CheckInLatitude  = dto.Latitude,
                    CheckInLongitude = dto.Longitude,
                    CheckInAddress   = dto.Address,
                    CheckInTime      = DateTime.Now,
                    WorkType         = dto.WorkType.ToLower(),
                    CreatedOn        = DateTime.Now
                };

                _context.LocationTrackings.Add(tracking);
                await _context.SaveChangesAsync();

                var user = await _context.Users.FindAsync(userId);
                return ApiOk("Check-in successful", MapToDto(tracking, user!));
            }
            catch (Exception ex) { return ApiServerError("Check-in failed", ex); }
        }

        [HttpPut("checkout")]
        public async Task<ActionResult<ApiResponse<LocationTrackingResponseDto>>> CheckOut([FromBody] LocationCheckOutDto dto)
        {
            try
            {
                var userId   = GetCurrentUserId();
                var tracking = await _context.LocationTrackings
                    .Include(l => l.User)
                    .FirstOrDefaultAsync(l => l.TrackingId == dto.TrackingId && l.UserId == userId);

                if (tracking == null)    return ApiNotFound("Check-in record not found");
                if (tracking.CheckOutTime.HasValue) return ApiBadRequest("Already checked out");

                tracking.CheckOutLatitude  = dto.Latitude;
                tracking.CheckOutLongitude = dto.Longitude;
                tracking.CheckOutAddress   = dto.Address;
                tracking.CheckOutTime      = DateTime.Now;

                if (tracking.CheckInTime.HasValue)
                    tracking.TotalTimeMinutes = (int)(tracking.CheckOutTime.Value - tracking.CheckInTime.Value).TotalMinutes;

                tracking.IsClientVisit = dto.IsClientVisit;
                if (dto.IsClientVisit)
                {
                    tracking.ClientName      = dto.ClientName;
                    tracking.ClientAddress   = dto.ClientAddress;
                    tracking.ClientLatitude  = dto.ClientLatitude;
                    tracking.ClientLongitude = dto.ClientLongitude;
                    tracking.VisitPurpose    = dto.VisitPurpose;
                    tracking.MeetingNotes    = dto.MeetingNotes;
                    tracking.Outcome         = dto.Outcome;
                }

                await _context.SaveChangesAsync();
                return ApiOk("Check-out successful", MapToDto(tracking, tracking.User!));
            }
            catch (Exception ex) { return ApiServerError("Check-out failed", ex); }
        }

        [HttpGet("my/today")]
        public async Task<ActionResult<ApiResponse<List<LocationTrackingResponseDto>>>> GetMyTodayTracking()
        {
            try
            {
                var userId  = GetCurrentUserId();
                var records = await _context.LocationTrackings
                    .Include(l => l.User)
                    .Where(l => l.UserId == userId && l.CheckInTime.HasValue && l.CheckInTime.Value.Date == DateTime.Today)
                    .OrderByDescending(l => l.CheckInTime)
                    .ToListAsync();

                return ApiOk("Today's tracking", records.Select(r => MapToDto(r, r.User!)).ToList());
            }
            catch (Exception ex) { return ApiServerError("Failed to retrieve tracking", ex); }
        }

        [HttpGet("my/history")]
        public async Task<ActionResult<ApiResponse<List<LocationTrackingResponseDto>>>> GetMyHistory(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var query  = _context.LocationTrackings.Include(l => l.User).Where(l => l.UserId == userId);

                if (fromDate.HasValue) query = query.Where(l => l.CheckInTime >= fromDate.Value.Date);
                if (toDate.HasValue)   query = query.Where(l => l.CheckInTime <= toDate.Value.Date.AddDays(1));

                var records = await query.OrderByDescending(l => l.CheckInTime).ToListAsync();
                return ApiOk($"Total {records.Count} records", records.Select(r => MapToDto(r, r.User!)).ToList());
            }
            catch (Exception ex) { return ApiServerError("Failed to retrieve history", ex); }
        }

        [HttpGet("all")]
        public async Task<ActionResult<ApiResponse<List<LocationTrackingResponseDto>>>> GetAllTracking(
            [FromQuery] int? userId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] bool? isClientVisit = null)
        {
            try
            {
                var role = GetCurrentUserRole();
                if (role != "admin" && role != "manager" && role != "hr")
                    return ApiForbidden("Access denied.");

                var query = _context.LocationTrackings.Include(l => l.User).AsQueryable();

                if (userId.HasValue)         query = query.Where(l => l.UserId == userId.Value);
                if (fromDate.HasValue)       query = query.Where(l => l.CheckInTime >= fromDate.Value.Date);
                if (toDate.HasValue)         query = query.Where(l => l.CheckInTime <= toDate.Value.Date.AddDays(1));
                if (isClientVisit.HasValue)  query = query.Where(l => l.IsClientVisit == isClientVisit.Value);

                var records = await query.OrderByDescending(l => l.CheckInTime).ToListAsync();
                return ApiOk($"Total {records.Count} records", records.Select(r => MapToDto(r, r.User!)).ToList());
            }
            catch (Exception ex) { return ApiServerError("Failed to retrieve tracking", ex); }
        }

        [HttpGet("all/today")]
        public async Task<ActionResult<ApiResponse<List<LocationTrackingResponseDto>>>> GetTodayAllTracking()
        {
            try
            {
                var role = GetCurrentUserRole();
                if (role != "admin" && role != "manager" && role != "hr")
                    return ApiForbidden("Access denied.");

                var records = await _context.LocationTrackings
                    .Include(l => l.User)
                    .Where(l => l.CheckInTime.HasValue && l.CheckInTime.Value.Date == DateTime.Today)
                    .OrderByDescending(l => l.CheckInTime)
                    .ToListAsync();

                return ApiOk($"Today's tracking — {records.Count} records", records.Select(r => MapToDto(r, r.User!)).ToList());
            }
            catch (Exception ex) { return ApiServerError("Failed to retrieve today's tracking", ex); }
        }

        private string FormatMinutes(int? minutes)
        {
            if (!minutes.HasValue) return "N/A";
            int hr = minutes.Value / 60, min = minutes.Value % 60;
            if (hr == 0)  return $"{min}min";
            if (min == 0) return $"{hr}hr";
            return $"{hr}hr {min}min";
        }

        private LocationTrackingResponseDto MapToDto(LocationTracking t, User user) => new()
        {
            TrackingId          = t.TrackingId,
            UserId              = t.UserId,
            UserName            = user.UserName,
            Role                = user.Role,
            CheckInLatitude     = t.CheckInLatitude,
            CheckInLongitude    = t.CheckInLongitude,
            CheckInAddress      = t.CheckInAddress,
            CheckInTime         = t.CheckInTime,
            CheckOutLatitude    = t.CheckOutLatitude,
            CheckOutLongitude   = t.CheckOutLongitude,
            CheckOutAddress     = t.CheckOutAddress,
            CheckOutTime        = t.CheckOutTime,
            TotalTimeMinutes    = t.TotalTimeMinutes,
            TotalTimeFormatted  = FormatMinutes(t.TotalTimeMinutes),
            IsClientVisit       = t.IsClientVisit,
            ClientName          = t.ClientName,
            ClientAddress       = t.ClientAddress,
            VisitPurpose        = t.VisitPurpose,
            MeetingNotes        = t.MeetingNotes,
            Outcome             = t.Outcome,
            WorkType            = t.WorkType,
            CreatedOn           = t.CreatedOn
        };
    }
}