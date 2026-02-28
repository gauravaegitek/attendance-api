// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using System.Security.Claims;
// using attendance_api.Data;
// using attendance_api.DTOs;
// using attendance_api.Models;

// namespace attendance_api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     [Authorize]
//     public class WFHController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;

//         public WFHController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         // ─── Helper: UserId from JWT ──────────────────────────────────────
//         private int GetUserId()
//         {
//             var claim = User.FindFirst(ClaimTypes.NameIdentifier);
//             return claim != null ? int.Parse(claim.Value) : 0;
//         }

//         // ─────────────────────────────────────────────────────────────────
//         // POST /api/WFH/request  — Employee: Submit WFH request
//         // ─────────────────────────────────────────────────────────────────
//         [HttpPost("request")]
//         public async Task<IActionResult> SubmitWFHRequest([FromBody] WFHRequestDto dto)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

//             var userId = GetUserId();
//             if (userId == 0)
//                 return Unauthorized(new { success = false, message = "Invalid token" });

//             // Validate: Cannot request WFH for past dates
//             if (dto.WFHDate.Date < DateTime.Today)
//                 return BadRequest(new { success = false, message = "WFH request cannot be submitted for past dates" });

//             // Validate: Cannot request WFH for weekends
//             // if (dto.WFHDate.DayOfWeek == DayOfWeek.Saturday || dto.WFHDate.DayOfWeek == DayOfWeek.Sunday)
//             //     return BadRequest(new { success = false, message = "WFH cannot be requested for Saturday or Sunday" });

//             // Check duplicate request for same date
//             var existingRequest = await _context.WFHRequests
//                 .FirstOrDefaultAsync(w => w.UserId == userId && w.WFHDate.Date == dto.WFHDate.Date);

//             if (existingRequest != null)
//                 return BadRequest(new
//                 {
//                     success = false,
//                     message = $"WFH request already submitted for {dto.WFHDate:dd-MMM-yyyy}",
//                     data = new { status = existingRequest.Status }
//                 });

//             var wfh = new WFHRequest
//             {
//                 UserId = userId,
//                 WFHDate = dto.WFHDate.Date,
//                 Reason = dto.Reason,
//                 Status = "Pending",
//                 CreatedOn = DateTime.Now
//             };

//             _context.WFHRequests.Add(wfh);
//             await _context.SaveChangesAsync();

//             return Ok(new
//             {
//                 success = true,
//                 message = "WFH request submitted successfully",
//                 data = new
//                 {
//                     wfhId = wfh.WFHId,
//                     wfhDate = wfh.WFHDate.ToString("dd-MMM-yyyy"),
//                     dayOfWeek = wfh.WFHDate.DayOfWeek.ToString(),
//                     reason = wfh.Reason,
//                     status = wfh.Status,
//                     submittedOn = wfh.CreatedOn.ToString("dd-MMM-yyyy HH:mm:ss")
//                 }
//             });
//         }

//         // ─────────────────────────────────────────────────────────────────
//         // GET /api/WFH/summary?month=2&year=2026
//         // Employee: own summary | Admin: any user (optional userId filter)
//         // ─────────────────────────────────────────────────────────────────
//         [HttpGet("summary")]
//         public async Task<IActionResult> GetWFHSummary(
//             [FromQuery] int month,
//             [FromQuery] int year,
//             [FromQuery] int? filterUserId = null)
//         {
//             if (month < 1 || month > 12)
//                 return BadRequest(new { success = false, message = "Invalid month. Use 1-12" });

//             if (year < 2020)
//                 return BadRequest(new { success = false, message = "Invalid year" });

//             var loggedInUserId = GetUserId();
//             bool isAdmin = User.IsInRole("admin");

//             // Non-admin can only see their own summary
//             int targetUserId = isAdmin && filterUserId.HasValue
//                 ? filterUserId.Value
//                 : loggedInUserId;

//             var user = await _context.Users.FindAsync(targetUserId);
//             if (user == null)
//                 return NotFound(new { success = false, message = "User not found" });

//             var requests = await _context.WFHRequests
//                 .Where(w => w.UserId == targetUserId
//                          && w.WFHDate.Month == month
//                          && w.WFHDate.Year == year)
//                 .OrderBy(w => w.WFHDate)
//                 .ToListAsync();

//             var details = requests.Select(w => new WFHResponseDto
//             {
//                 WFHId      = w.WFHId,
//                 UserId     = w.UserId,
//                 UserName   = user.UserName,
//                 Role       = user.Role,
//                 WFHDate    = w.WFHDate.ToString("dd-MMM-yyyy"),
//                 Reason     = w.Reason,
//                 Status     = w.Status,
//                 ApprovedOn = w.ApprovedOn?.ToString("dd-MMM-yyyy HH:mm"),
//                 RejectionReason = w.RejectionReason,
//                 CreatedOn  = w.CreatedOn.ToString("dd-MMM-yyyy HH:mm:ss")
//             }).ToList();

//             var summary = new WFHSummaryDto
//             {
//                 UserId       = targetUserId,
//                 UserName     = user.UserName,
//                 Month        = month,
//                 Year         = year,
//                 TotalWFHDays = requests.Count,
//                 ApprovedDays = requests.Count(r => r.Status == "Approved"),
//                 PendingDays  = requests.Count(r => r.Status == "Pending"),
//                 RejectedDays = requests.Count(r => r.Status == "Rejected"),
//                 Details      = details
//             };

//             return Ok(new { success = true, message = "WFH summary retrieved", data = summary });
//         }

//         // ─────────────────────────────────────────────────────────────────
//         // GET /api/WFH/myrequests  — Employee: own all requests
//         // ─────────────────────────────────────────────────────────────────
//         [HttpGet("myrequests")]
//         public async Task<IActionResult> GetMyRequests([FromQuery] string status = "all")
//         {
//             var userId = GetUserId();

//             var query = _context.WFHRequests.Where(w => w.UserId == userId);

//             if (status != "all")
//                 query = query.Where(w => w.Status == status);

//             var requests = await query.OrderByDescending(w => w.WFHDate).ToListAsync();

//             var user = await _context.Users.FindAsync(userId);

//             var result = requests.Select(w => new WFHResponseDto
//             {
//                 WFHId      = w.WFHId,
//                 UserId     = w.UserId,
//                 UserName   = user?.UserName ?? "",
//                 Role       = user?.Role ?? "",
//                 WFHDate    = w.WFHDate.ToString("dd-MMM-yyyy"),
//                 Reason     = w.Reason,
//                 Status     = w.Status,
//                 ApprovedOn = w.ApprovedOn?.ToString("dd-MMM-yyyy HH:mm"),
//                 RejectionReason = w.RejectionReason,
//                 CreatedOn  = w.CreatedOn.ToString("dd-MMM-yyyy HH:mm:ss")
//             }).ToList();

//             return Ok(new { success = true, message = "Requests retrieved", totalCount = result.Count, data = result });
//         }

//         // ─────────────────────────────────────────────────────────────────
//         // POST /api/WFH/approve  — Admin: Approve or Reject
//         // ─────────────────────────────────────────────────────────────────
//         [HttpPost("approve")]
//         [Authorize(Roles = "admin")]
//         public async Task<IActionResult> ApproveRejectWFH([FromBody] WFHApprovalDto dto)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { success = false, message = "Invalid data" });

//             if (dto.Action != "Approved" && dto.Action != "Rejected")
//                 return BadRequest(new { success = false, message = "Action must be 'Approved' or 'Rejected'" });

//             if (dto.Action == "Rejected" && string.IsNullOrWhiteSpace(dto.RejectionReason))
//                 return BadRequest(new { success = false, message = "Rejection reason is required when rejecting" });

//             var wfh = await _context.WFHRequests
//                 .Include(w => w.User)
//                 .FirstOrDefaultAsync(w => w.WFHId == dto.WFHId);

//             if (wfh == null)
//                 return NotFound(new { success = false, message = "WFH request not found" });

//             if (wfh.Status != "Pending")
//                 return BadRequest(new { success = false, message = $"Request is already {wfh.Status}" });

//             wfh.Status           = dto.Action;
//             wfh.ApprovedByUserId = GetUserId();
//             wfh.ApprovedOn       = DateTime.Now;
//             wfh.RejectionReason  = dto.Action == "Rejected" ? dto.RejectionReason : null;

//             await _context.SaveChangesAsync();

//             return Ok(new
//             {
//                 success = true,
//                 message = $"WFH request {dto.Action} successfully",
//                 data = new
//                 {
//                     wfhId        = wfh.WFHId,
//                     employeeName = wfh.User?.UserName,
//                     wfhDate      = wfh.WFHDate.ToString("dd-MMM-yyyy"),
//                     status       = wfh.Status,
//                     actionOn     = wfh.ApprovedOn?.ToString("dd-MMM-yyyy HH:mm:ss")
//                 }
//             });
//         }

//         // ─────────────────────────────────────────────────────────────────
//         // GET /api/WFH/all  — Admin: All WFH requests with filters
//         // Query: ?status=Pending&month=2&year=2026
//         // ─────────────────────────────────────────────────────────────────
//         [HttpGet("all")]
//         [Authorize(Roles = "admin")]
//         public async Task<IActionResult> GetAllWFHRequests(
//             [FromQuery] string status = "all",
//             [FromQuery] int? month = null,
//             [FromQuery] int? year = null)
//         {
//             var query = _context.WFHRequests.Include(w => w.User).AsQueryable();

//             if (status != "all")
//                 query = query.Where(w => w.Status == status);

//             if (month.HasValue)
//                 query = query.Where(w => w.WFHDate.Month == month.Value);

//             if (year.HasValue)
//                 query = query.Where(w => w.WFHDate.Year == year.Value);

//             var requests = await query.OrderByDescending(w => w.CreatedOn).ToListAsync();

//             var result = requests.Select(w => new WFHResponseDto
//             {
//                 WFHId      = w.WFHId,
//                 UserId     = w.UserId,
//                 UserName   = w.User?.UserName ?? "",
//                 Role       = w.User?.Role ?? "",
//                 WFHDate    = w.WFHDate.ToString("dd-MMM-yyyy"),
//                 Reason     = w.Reason,
//                 Status     = w.Status,
//                 ApprovedOn = w.ApprovedOn?.ToString("dd-MMM-yyyy HH:mm"),
//                 RejectionReason = w.RejectionReason,
//                 CreatedOn  = w.CreatedOn.ToString("dd-MMM-yyyy HH:mm:ss")
//             }).ToList();

//             return Ok(new { success = true, message = "WFH requests retrieved", totalCount = result.Count, data = result });
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
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WFHController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public WFHController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("request")]
        public async Task<IActionResult> SubmitWFHRequest([FromBody] WFHRequestDto dto)
        {
            if (!ModelState.IsValid)
                return ApiBadRequest("Invalid data");

            var userId = GetCurrentUserId();
            if (userId == 0) return ApiUnauthorized("Invalid token");

            if (dto.WFHDate.Date < DateTime.Today)
                return ApiBadRequest("WFH request cannot be submitted for past dates");

            var existingRequest = await _context.WFHRequests
                .FirstOrDefaultAsync(w => w.UserId == userId && w.WFHDate.Date == dto.WFHDate.Date);

            if (existingRequest != null)
                return ApiBadRequest($"WFH request already submitted for {dto.WFHDate:dd-MMM-yyyy}",
                    new List<string> { existingRequest.Status });

            var wfh = new WFHRequest
            {
                UserId    = userId,
                WFHDate   = dto.WFHDate.Date,
                Reason    = dto.Reason,
                Status    = "Pending",
                CreatedOn = DateTime.Now
            };

            _context.WFHRequests.Add(wfh);
            await _context.SaveChangesAsync();

            return ApiOk("WFH request submitted successfully", new
            {
                wfhId       = wfh.WFHId,
                wfhDate     = wfh.WFHDate.ToString("dd-MMM-yyyy"),
                dayOfWeek   = wfh.WFHDate.DayOfWeek.ToString(),
                reason      = wfh.Reason,
                status      = wfh.Status,
                submittedOn = wfh.CreatedOn.ToString("dd-MMM-yyyy HH:mm:ss")
            });
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetWFHSummary(
            [FromQuery] int month,
            [FromQuery] int year,
            [FromQuery] int? filterUserId = null)
        {
            if (month < 1 || month > 12) return ApiBadRequest("Invalid month. Use 1-12");
            if (year < 2020)             return ApiBadRequest("Invalid year");

            var loggedInUserId = GetCurrentUserId();
            bool isAdmin       = User.IsInRole("admin");

            int targetUserId = isAdmin && filterUserId.HasValue ? filterUserId.Value : loggedInUserId;

            var user = await _context.Users.FindAsync(targetUserId);
            if (user == null) return ApiNotFound("User not found");

            var requests = await _context.WFHRequests
                .Where(w => w.UserId == targetUserId && w.WFHDate.Month == month && w.WFHDate.Year == year)
                .OrderBy(w => w.WFHDate)
                .ToListAsync();

            var details = requests.Select(w => new WFHResponseDto
            {
                WFHId           = w.WFHId,
                UserId          = w.UserId,
                UserName        = user.UserName,
                Role            = user.Role,
                WFHDate         = w.WFHDate.ToString("dd-MMM-yyyy"),
                Reason          = w.Reason,
                Status          = w.Status,
                ApprovedOn      = w.ApprovedOn?.ToString("dd-MMM-yyyy HH:mm"),
                RejectionReason = w.RejectionReason,
                CreatedOn       = w.CreatedOn.ToString("dd-MMM-yyyy HH:mm:ss")
            }).ToList();

            var summary = new WFHSummaryDto
            {
                UserId       = targetUserId,
                UserName     = user.UserName,
                Month        = month,
                Year         = year,
                TotalWFHDays = requests.Count,
                ApprovedDays = requests.Count(r => r.Status == "Approved"),
                PendingDays  = requests.Count(r => r.Status == "Pending"),
                RejectedDays = requests.Count(r => r.Status == "Rejected"),
                Details      = details
            };

            return ApiOk("WFH summary retrieved", summary);
        }

        [HttpGet("myrequests")]
        public async Task<IActionResult> GetMyRequests([FromQuery] string status = "all")
        {
            var userId = GetCurrentUserId();
            var query  = _context.WFHRequests.Where(w => w.UserId == userId);

            if (status != "all") query = query.Where(w => w.Status == status);

            var requests = await query.OrderByDescending(w => w.WFHDate).ToListAsync();
            var user     = await _context.Users.FindAsync(userId);

            var result = requests.Select(w => new WFHResponseDto
            {
                WFHId           = w.WFHId,
                UserId          = w.UserId,
                UserName        = user?.UserName ?? "",
                Role            = user?.Role ?? "",
                WFHDate         = w.WFHDate.ToString("dd-MMM-yyyy"),
                Reason          = w.Reason,
                Status          = w.Status,
                ApprovedOn      = w.ApprovedOn?.ToString("dd-MMM-yyyy HH:mm"),
                RejectionReason = w.RejectionReason,
                CreatedOn       = w.CreatedOn.ToString("dd-MMM-yyyy HH:mm:ss")
            }).ToList();

            return ApiOk("Requests retrieved", result);
        }

        [HttpPost("approve")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ApproveRejectWFH([FromBody] WFHApprovalDto dto)
        {
            if (!ModelState.IsValid) return ApiBadRequest("Invalid data");

            if (dto.Action != "Approved" && dto.Action != "Rejected")
                return ApiBadRequest("Action must be 'Approved' or 'Rejected'");

            if (dto.Action == "Rejected" && string.IsNullOrWhiteSpace(dto.RejectionReason))
                return ApiBadRequest("Rejection reason is required when rejecting");

            var wfh = await _context.WFHRequests.Include(w => w.User).FirstOrDefaultAsync(w => w.WFHId == dto.WFHId);
            if (wfh == null) return ApiNotFound("WFH request not found");

            if (wfh.Status != "Pending") return ApiBadRequest($"Request is already {wfh.Status}");

            wfh.Status           = dto.Action;
            wfh.ApprovedByUserId = GetCurrentUserId();
            wfh.ApprovedOn       = DateTime.Now;
            wfh.RejectionReason  = dto.Action == "Rejected" ? dto.RejectionReason : null;

            await _context.SaveChangesAsync();

            return ApiOk($"WFH request {dto.Action} successfully", new
            {
                wfhId        = wfh.WFHId,
                employeeName = wfh.User?.UserName,
                wfhDate      = wfh.WFHDate.ToString("dd-MMM-yyyy"),
                status       = wfh.Status,
                actionOn     = wfh.ApprovedOn?.ToString("dd-MMM-yyyy HH:mm:ss")
            });
        }

        [HttpGet("all")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllWFHRequests(
            [FromQuery] string status = "all",
            [FromQuery] int? month = null,
            [FromQuery] int? year = null)
        {
            var query = _context.WFHRequests.Include(w => w.User).AsQueryable();

            if (status != "all") query = query.Where(w => w.Status == status);
            if (month.HasValue)  query = query.Where(w => w.WFHDate.Month == month.Value);
            if (year.HasValue)   query = query.Where(w => w.WFHDate.Year == year.Value);

            var requests = await query.OrderByDescending(w => w.CreatedOn).ToListAsync();

            var result = requests.Select(w => new WFHResponseDto
            {
                WFHId           = w.WFHId,
                UserId          = w.UserId,
                UserName        = w.User?.UserName ?? "",
                Role            = w.User?.Role ?? "",
                WFHDate         = w.WFHDate.ToString("dd-MMM-yyyy"),
                Reason          = w.Reason,
                Status          = w.Status,
                ApprovedOn      = w.ApprovedOn?.ToString("dd-MMM-yyyy HH:mm"),
                RejectionReason = w.RejectionReason,
                CreatedOn       = w.CreatedOn.ToString("dd-MMM-yyyy HH:mm:ss")
            }).ToList();

            return ApiOk("WFH requests retrieved", result);
        }
    }
}