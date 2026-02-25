using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using attendance_api.Data;
using attendance_api.DTOs;
using attendance_api.Models;
using System.Security.Claims;

namespace attendance_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LeaveController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LeaveController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Helper — current logged in user ka ID
        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value?.ToLower() ?? "";
        }

        // ✅ 1. Leave Apply karo (User)
        [HttpPost("apply")]
        public async Task<ActionResult<ApiResponse<LeaveResponseDto>>> ApplyLeave([FromBody] ApplyLeaveDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                    return Unauthorized(new ApiResponse<LeaveResponseDto> { Success = false, Message = "Invalid token" });

                if (dto.FromDate > dto.ToDate)
                    return BadRequest(new ApiResponse<LeaveResponseDto> { Success = false, Message = "FromDate cannot be greater than ToDate" });

                var validTypes = new[] { "casual", "sick", "earned", "halfday", "unpaid" };
                if (!validTypes.Contains(dto.LeaveType.ToLower()))
                    return BadRequest(new ApiResponse<LeaveResponseDto> { Success = false, Message = "Invalid leave type. Use: casual, sick, earned, halfday, unpaid" });

                int totalDays = (int)(dto.ToDate.Date - dto.FromDate.Date).TotalDays + 1;
                if (dto.LeaveType.ToLower() == "halfday") totalDays = 1;

                var leave = new Leave
                {
                    UserId = userId,
                    LeaveType = dto.LeaveType.ToLower(),
                    FromDate = dto.FromDate.Date,
                    ToDate = dto.ToDate.Date,
                    TotalDays = totalDays,
                    Reason = dto.Reason.Trim(),
                    Status = "pending",
                    CreatedOn = DateTime.Now
                };

                _context.Leaves.Add(leave);
                await _context.SaveChangesAsync();

                var user = await _context.Users.FindAsync(userId);

                return Ok(new ApiResponse<LeaveResponseDto>
                {
                    Success = true,
                    Message = "Leave applied successfully",
                    Data = MapLeaveToDto(leave, user!)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<LeaveResponseDto> { Success = false, Message = "Failed to apply leave", Errors = new List<string> { ex.Message } });
            }
        }

        // ✅ 2. My Leaves — Current user ki saari leaves
        [HttpGet("my")]
        public async Task<ActionResult<ApiResponse<List<LeaveResponseDto>>>> GetMyLeaves(
            [FromQuery] string? status = null,
            [FromQuery] int? year = null)
        {
            try
            {
                var userId = GetCurrentUserId();

                var query = _context.Leaves
                    .Include(l => l.User)
                    .Where(l => l.UserId == userId);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(l => l.Status.ToLower() == status.ToLower());

                if (year.HasValue)
                    query = query.Where(l => l.FromDate.Year == year.Value);

                var leaves = await query
                    .OrderByDescending(l => l.CreatedOn)
                    .ToListAsync();

                var result = leaves.Select(l => MapLeaveToDto(l, l.User!)).ToList();

                return Ok(new ApiResponse<List<LeaveResponseDto>> { Success = true, Message = "Leaves retrieved", Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<LeaveResponseDto>> { Success = false, Message = "Failed", Errors = new List<string> { ex.Message } });
            }
        }

        // ✅ 3. All Users Ki Leaves — Admin Only
        [HttpGet("all")]
        public async Task<ActionResult<ApiResponse<List<LeaveResponseDto>>>> GetAllLeaves(
            [FromQuery] string? status = null,
            [FromQuery] int? userId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var role = GetCurrentUserRole();
                if (role != "admin" && role != "hr" && role != "manager")
                    return StatusCode(403, new ApiResponse<List<LeaveResponseDto>> { Success = false, Message = "Access denied." });

                var query = _context.Leaves.Include(l => l.User).AsQueryable();

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(l => l.Status.ToLower() == status.ToLower());

                if (userId.HasValue)
                    query = query.Where(l => l.UserId == userId.Value);

                if (fromDate.HasValue)
                    query = query.Where(l => l.FromDate >= fromDate.Value.Date);

                if (toDate.HasValue)
                    query = query.Where(l => l.ToDate <= toDate.Value.Date);

                var leaves = await query.OrderByDescending(l => l.CreatedOn).ToListAsync();
                var result = leaves.Select(l => MapLeaveToDto(l, l.User!)).ToList();

                return Ok(new ApiResponse<List<LeaveResponseDto>> { Success = true, Message = $"Total {result.Count} leaves found", Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<LeaveResponseDto>> { Success = false, Message = "Failed", Errors = new List<string> { ex.Message } });
            }
        }

        // ✅ 4. Leave Approve / Reject — Admin Only
        [HttpPut("action/{leaveId}")]
        public async Task<ActionResult<ApiResponse<LeaveResponseDto>>> LeaveAction(int leaveId, [FromBody] LeaveActionDto dto)
        {
            try
            {
                var role = GetCurrentUserRole();
                if (role != "admin" && role != "hr")
                    return StatusCode(403, new ApiResponse<LeaveResponseDto> { Success = false, Message = "Access denied. Only admin/hr can approve or reject." });

                var validStatuses = new[] { "approved", "rejected" };
                if (!validStatuses.Contains(dto.Status.ToLower()))
                    return BadRequest(new ApiResponse<LeaveResponseDto> { Success = false, Message = "Status must be: approved or rejected" });

                var leave = await _context.Leaves.Include(l => l.User).FirstOrDefaultAsync(l => l.LeaveId == leaveId);

                if (leave == null)
                    return NotFound(new ApiResponse<LeaveResponseDto> { Success = false, Message = "Leave not found" });

                if (leave.Status != "pending")
                    return BadRequest(new ApiResponse<LeaveResponseDto> { Success = false, Message = $"Leave is already {leave.Status}" });

                leave.Status = dto.Status.ToLower();
                leave.AdminRemark = dto.AdminRemark;
                leave.ApprovedByUserId = GetCurrentUserId();
                leave.ApprovedOn = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<LeaveResponseDto>
                {
                    Success = true,
                    Message = $"Leave {leave.Status} successfully",
                    Data = MapLeaveToDto(leave, leave.User!)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<LeaveResponseDto> { Success = false, Message = "Failed", Errors = new List<string> { ex.Message } });
            }
        }

        // ✅ 5. Leave Cancel — User apni pending leave cancel kar sakta hai
        [HttpDelete("cancel/{leaveId}")]
        public async Task<ActionResult<ApiResponse<string>>> CancelLeave(int leaveId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var leave = await _context.Leaves.FindAsync(leaveId);

                if (leave == null)
                    return NotFound(new ApiResponse<string> { Success = false, Message = "Leave not found" });

                if (leave.UserId != userId)
                    return StatusCode(403, new ApiResponse<string> { Success = false, Message = "You can only cancel your own leave" });

                if (leave.Status != "pending")
                    return BadRequest(new ApiResponse<string> { Success = false, Message = $"Cannot cancel leave which is already {leave.Status}" });

                _context.Leaves.Remove(leave);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string> { Success = true, Message = "Leave cancelled successfully", Data = "cancelled" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Failed", Errors = new List<string> { ex.Message } });
            }
        }

        // ✅ Helper: Map to DTO
        private LeaveResponseDto MapLeaveToDto(Leave leave, User user)
        {
            return new LeaveResponseDto
            {
                LeaveId = leave.LeaveId,
                UserId = leave.UserId,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role,
                LeaveType = leave.LeaveType,
                FromDate = leave.FromDate,
                ToDate = leave.ToDate,
                TotalDays = leave.TotalDays,
                Reason = leave.Reason,
                Status = leave.Status,
                AdminRemark = leave.AdminRemark,
                ApprovedOn = leave.ApprovedOn,
                CreatedOn = leave.CreatedOn
            };
        }
    }
}