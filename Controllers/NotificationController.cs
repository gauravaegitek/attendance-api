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
    public class NotificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Sabhi notifications fetch karo (apni)
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetMyNotifications()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var userId = int.Parse(userIdClaim!.Value);

                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Select(n => new NotificationDto
                    {
                        NotificationId = n.NotificationId,
                        Title = n.Title,
                        Message = n.Message,
                        Type = n.Type,
                        IsRead = n.IsRead,
                        CreatedAt = n.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<NotificationDto>>
                {
                    Success = true,
                    Message = "Notifications fetched successfully",
                    Data = notifications
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<NotificationDto>>
                {
                    Success = false,
                    Message = "Failed to fetch notifications",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // ✅ Notification read mark karo
        [HttpPut("read/{notificationId}")]
        public async Task<ActionResult<ApiResponse<string>>> MarkAsRead(int notificationId)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var userId = int.Parse(userIdClaim!.Value);

                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

                if (notification == null)
                    return NotFound(new ApiResponse<string> { Success = false, Message = "Notification not found" });

                notification.IsRead = true;
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string> { Success = true, Message = "Marked as read", Data = "read" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Failed to mark as read",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // ✅ Saari notifications read mark karo
        [HttpPut("read-all")]
        public async Task<ActionResult<ApiResponse<string>>> MarkAllAsRead()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var userId = int.Parse(userIdClaim!.Value);

                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .ToListAsync();

                notifications.ForEach(n => n.IsRead = true);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string> { Success = true, Message = "All notifications marked as read" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Failed",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // ✅ Unread count
        [HttpGet("unread-count")]
        public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var userId = int.Parse(userIdClaim!.Value);

                var count = await _context.Notifications
                    .CountAsync(n => n.UserId == userId && !n.IsRead);

                return Ok(new ApiResponse<int> { Success = true, Message = "Unread count", Data = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<int>
                {
                    Success = false,
                    Message = "Failed",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // ✅ Admin — Notification send karo kisi user ko
        [Authorize(Roles = "admin")]
        [HttpPost("send")]
        public async Task<ActionResult<ApiResponse<string>>> SendNotification([FromBody] SendNotificationDto dto)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = dto.UserId,
                    Title = dto.Title,
                    Message = dto.Message,
                    Type = dto.Type, // "alert" or "reminder"
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string> { Success = true, Message = "Notification sent successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Failed to send notification",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}