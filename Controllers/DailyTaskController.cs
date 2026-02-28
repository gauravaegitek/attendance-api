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
//     public class DailyTaskController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;

//         public DailyTaskController(ApplicationDbContext context)
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

//         // ✅ 1. Task Add karo (User)
//         [HttpPost("add")]
//         public async Task<ActionResult<ApiResponse<DailyTaskResponseDto>>> AddTask([FromBody] AddDailyTaskDto dto)
//         {
//             try
//             {
//                 var userId = GetCurrentUserId();
//                 if (userId == 0)
//                     return Unauthorized(new ApiResponse<DailyTaskResponseDto> { Success = false, Message = "Invalid token" });

//                 var validStatuses = new[] { "todo", "inprogress", "completed", "pending" };
//                 if (!validStatuses.Contains(dto.Status.ToLower()))
//                     return BadRequest(new ApiResponse<DailyTaskResponseDto> { Success = false, Message = "Status must be: todo, inprogress, completed, pending" });

//                 if (dto.HoursSpent < 0 || dto.HoursSpent > 24)
//                     return BadRequest(new ApiResponse<DailyTaskResponseDto> { Success = false, Message = "HoursSpent must be between 0 and 24" });

//                 var task = new DailyTask
//                 {
//                     UserId = userId,
//                     TaskDate = dto.TaskDate.Date,
//                     TaskTitle = dto.TaskTitle.Trim(),
//                     TaskDescription = dto.TaskDescription?.Trim(),
//                     ProjectName = dto.ProjectName?.Trim(),
//                     Status = dto.Status.ToLower(),
//                     HoursSpent = dto.HoursSpent,
//                     Remarks = dto.Remarks?.Trim(),
//                     CreatedOn = DateTime.Now
//                 };

//                 _context.DailyTasks.Add(task);
//                 await _context.SaveChangesAsync();

//                 var user = await _context.Users.FindAsync(userId);

//                 return Ok(new ApiResponse<DailyTaskResponseDto>
//                 {
//                     Success = true,
//                     Message = "Task added successfully",
//                     Data = MapToDto(task, user!)
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<DailyTaskResponseDto> { Success = false, Message = "Failed to add task", Errors = new List<string> { ex.Message } });
//             }
//         }

//         // ✅ 2. Task Update karo (User — sirf apna)
//         [HttpPut("update/{taskId}")]
//         public async Task<ActionResult<ApiResponse<DailyTaskResponseDto>>> UpdateTask(int taskId, [FromBody] UpdateDailyTaskDto dto)
//         {
//             try
//             {
//                 var userId = GetCurrentUserId();
//                 var task = await _context.DailyTasks.Include(t => t.User).FirstOrDefaultAsync(t => t.TaskId == taskId);

//                 if (task == null)
//                     return NotFound(new ApiResponse<DailyTaskResponseDto> { Success = false, Message = "Task not found" });

//                 if (task.UserId != userId)
//                     return StatusCode(403, new ApiResponse<DailyTaskResponseDto> { Success = false, Message = "You can only update your own tasks" });

//                 if (!string.IsNullOrEmpty(dto.TaskTitle)) task.TaskTitle = dto.TaskTitle.Trim();
//                 if (dto.TaskDescription != null) task.TaskDescription = dto.TaskDescription.Trim();
//                 if (dto.ProjectName != null) task.ProjectName = dto.ProjectName.Trim();
//                 if (!string.IsNullOrEmpty(dto.Status)) task.Status = dto.Status.ToLower();
//                 if (dto.HoursSpent.HasValue) task.HoursSpent = dto.HoursSpent.Value;
//                 if (dto.Remarks != null) task.Remarks = dto.Remarks.Trim();

//                 task.UpdatedOn = DateTime.Now;
//                 await _context.SaveChangesAsync();

//                 return Ok(new ApiResponse<DailyTaskResponseDto>
//                 {
//                     Success = true,
//                     Message = "Task updated successfully",
//                     Data = MapToDto(task, task.User!)
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<DailyTaskResponseDto> { Success = false, Message = "Failed", Errors = new List<string> { ex.Message } });
//             }
//         }

//         // ✅ 3. Task Delete (User — sirf apna)
//         [HttpDelete("delete/{taskId}")]
//         public async Task<ActionResult<ApiResponse<string>>> DeleteTask(int taskId)
//         {
//             try
//             {
//                 var userId = GetCurrentUserId();
//                 var role = GetCurrentUserRole();

//                 var task = await _context.DailyTasks.FindAsync(taskId);
//                 if (task == null)
//                     return NotFound(new ApiResponse<string> { Success = false, Message = "Task not found" });

//                 if (task.UserId != userId && role != "admin")
//                     return StatusCode(403, new ApiResponse<string> { Success = false, Message = "You can only delete your own tasks" });

//                 _context.DailyTasks.Remove(task);
//                 await _context.SaveChangesAsync();

//                 return Ok(new ApiResponse<string> { Success = true, Message = "Task deleted", Data = "deleted" });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Failed", Errors = new List<string> { ex.Message } });
//             }
//         }

//         // ✅ 4. My Tasks — Date filter ke saath
//         [HttpGet("my")]
//         public async Task<ActionResult<ApiResponse<List<DailyTaskResponseDto>>>> GetMyTasks(
//             [FromQuery] DateTime? date = null,
//             [FromQuery] DateTime? fromDate = null,
//             [FromQuery] DateTime? toDate = null,
//             [FromQuery] string? status = null)
//         {
//             try
//             {
//                 var userId = GetCurrentUserId();

//                 var query = _context.DailyTasks
//                     .Include(t => t.User)
//                     .Where(t => t.UserId == userId);

//                 if (date.HasValue)
//                     query = query.Where(t => t.TaskDate.Date == date.Value.Date);
//                 else
//                 {
//                     if (fromDate.HasValue) query = query.Where(t => t.TaskDate >= fromDate.Value.Date);
//                     if (toDate.HasValue) query = query.Where(t => t.TaskDate <= toDate.Value.Date);
//                 }

//                 if (!string.IsNullOrEmpty(status))
//                     query = query.Where(t => t.Status.ToLower() == status.ToLower());

//                 var tasks = await query.OrderByDescending(t => t.TaskDate).ThenByDescending(t => t.CreatedOn).ToListAsync();

//                 return Ok(new ApiResponse<List<DailyTaskResponseDto>>
//                 {
//                     Success = true,
//                     Message = $"Total {tasks.Count} tasks",
//                     Data = tasks.Select(t => MapToDto(t, t.User!)).ToList()
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<List<DailyTaskResponseDto>> { Success = false, Message = "Failed", Errors = new List<string> { ex.Message } });
//             }
//         }

//         // ✅ 5. My Today's Summary
//         [HttpGet("my/today")]
//         public async Task<ActionResult<ApiResponse<DailyTaskSummaryDto>>> GetMyTodaySummary()
//         {
//             try
//             {
//                 var userId = GetCurrentUserId();
//                 var user = await _context.Users.FindAsync(userId);

//                 var tasks = await _context.DailyTasks
//                     .Include(t => t.User)
//                     .Where(t => t.UserId == userId && t.TaskDate.Date == DateTime.Today)
//                     .OrderByDescending(t => t.CreatedOn)
//                     .ToListAsync();

//                 var summary = new DailyTaskSummaryDto
//                 {
//                     UserId = userId,
//                     UserName = user?.UserName ?? "",
//                     TaskDate = DateTime.Today,
//                     TotalTasks = tasks.Count,
//                     TotalHours = tasks.Sum(t => t.HoursSpent),
//                     Tasks = tasks.Select(t => MapToDto(t, t.User!)).ToList()
//                 };

//                 return Ok(new ApiResponse<DailyTaskSummaryDto> { Success = true, Message = "Today's summary", Data = summary });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<DailyTaskSummaryDto> { Success = false, Message = "Failed", Errors = new List<string> { ex.Message } });
//             }
//         }

//         // ✅ 6. All Users Ke Tasks — Admin/Manager
//         [HttpGet("all")]
//         public async Task<ActionResult<ApiResponse<List<DailyTaskResponseDto>>>> GetAllTasks(
//             [FromQuery] int? userId = null,
//             [FromQuery] DateTime? date = null,
//             [FromQuery] DateTime? fromDate = null,
//             [FromQuery] DateTime? toDate = null,
//             [FromQuery] string? status = null)
//         {
//             try
//             {
//                 var role = GetCurrentUserRole();
//                 if (role != "admin" && role != "manager" && role != "hr")
//                     return StatusCode(403, new ApiResponse<List<DailyTaskResponseDto>> { Success = false, Message = "Access denied." });

//                 var query = _context.DailyTasks.Include(t => t.User).AsQueryable();

//                 if (userId.HasValue)
//                     query = query.Where(t => t.UserId == userId.Value);

//                 if (date.HasValue)
//                     query = query.Where(t => t.TaskDate.Date == date.Value.Date);
//                 else
//                 {
//                     if (fromDate.HasValue) query = query.Where(t => t.TaskDate >= fromDate.Value.Date);
//                     if (toDate.HasValue) query = query.Where(t => t.TaskDate <= toDate.Value.Date);
//                 }

//                 if (!string.IsNullOrEmpty(status))
//                     query = query.Where(t => t.Status.ToLower() == status.ToLower());

//                 var tasks = await query.OrderByDescending(t => t.TaskDate).ThenByDescending(t => t.CreatedOn).ToListAsync();

//                 return Ok(new ApiResponse<List<DailyTaskResponseDto>>
//                 {
//                     Success = true,
//                     Message = $"Total {tasks.Count} tasks",
//                     Data = tasks.Select(t => MapToDto(t, t.User!)).ToList()
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<List<DailyTaskResponseDto>> { Success = false, Message = "Failed", Errors = new List<string> { ex.Message } });
//             }
//         }

//         // ✅ 7. All Users Ki Today's Summary — Admin
//         [HttpGet("all/today")]
//         public async Task<ActionResult<ApiResponse<List<DailyTaskSummaryDto>>>> GetAllTodaySummary()
//         {
//             try
//             {
//                 var role = GetCurrentUserRole();
//                 if (role != "admin" && role != "manager" && role != "hr")
//                     return StatusCode(403, new ApiResponse<List<DailyTaskSummaryDto>> { Success = false, Message = "Access denied." });

//                 var tasks = await _context.DailyTasks
//                     .Include(t => t.User)
//                     .Where(t => t.TaskDate.Date == DateTime.Today)
//                     .ToListAsync();

//                 var grouped = tasks
//                     .GroupBy(t => t.UserId)
//                     .Select(g => new DailyTaskSummaryDto
//                     {
//                         UserId = g.Key,
//                         UserName = g.First().User?.UserName ?? "",
//                         TaskDate = DateTime.Today,
//                         TotalTasks = g.Count(),
//                         TotalHours = g.Sum(t => t.HoursSpent),
//                         Tasks = g.Select(t => MapToDto(t, t.User!)).ToList()
//                     })
//                     .ToList();

//                 return Ok(new ApiResponse<List<DailyTaskSummaryDto>>
//                 {
//                     Success = true,
//                     Message = $"Today's summary — {grouped.Count} users",
//                     Data = grouped
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<List<DailyTaskSummaryDto>> { Success = false, Message = "Failed", Errors = new List<string> { ex.Message } });
//             }
//         }

//         private DailyTaskResponseDto MapToDto(DailyTask t, User user)
//         {
//             return new DailyTaskResponseDto
//             {
//                 TaskId = t.TaskId,
//                 UserId = t.UserId,
//                 UserName = user.UserName,
//                 Email = user.Email,
//                 Role = user.Role,
//                 TaskDate = t.TaskDate,
//                 TaskTitle = t.TaskTitle,
//                 TaskDescription = t.TaskDescription,
//                 ProjectName = t.ProjectName,
//                 Status = t.Status,
//                 HoursSpent = t.HoursSpent,
//                 Remarks = t.Remarks,
//                 CreatedOn = t.CreatedOn,
//                 UpdatedOn = t.UpdatedOn
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
    public class DailyTaskController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public DailyTaskController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("add")]
        public async Task<ActionResult<ApiResponse<DailyTaskResponseDto>>> AddTask([FromBody] AddDailyTaskDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0) return ApiUnauthorized("Invalid token");

                var validStatuses = new[] { "todo", "inprogress", "completed", "pending" };
                if (!validStatuses.Contains(dto.Status.ToLower()))
                    return ApiBadRequest("Status must be: todo, inprogress, completed, pending");

                if (dto.HoursSpent < 0 || dto.HoursSpent > 24)
                    return ApiBadRequest("HoursSpent must be between 0 and 24");

                var task = new DailyTask
                {
                    UserId          = userId,
                    TaskDate        = dto.TaskDate.Date,
                    TaskTitle       = dto.TaskTitle.Trim(),
                    TaskDescription = dto.TaskDescription?.Trim(),
                    ProjectName     = dto.ProjectName?.Trim(),
                    Status          = dto.Status.ToLower(),
                    HoursSpent      = dto.HoursSpent,
                    Remarks         = dto.Remarks?.Trim(),
                    CreatedOn       = DateTime.Now
                };

                _context.DailyTasks.Add(task);
                await _context.SaveChangesAsync();

                var user = await _context.Users.FindAsync(userId);
                return ApiOk("Task added successfully", MapToDto(task, user!));
            }
            catch (Exception ex) { return ApiServerError("Failed to add task", ex); }
        }

        [HttpPut("update/{taskId}")]
        public async Task<ActionResult<ApiResponse<DailyTaskResponseDto>>> UpdateTask(int taskId, [FromBody] UpdateDailyTaskDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var task = await _context.DailyTasks.Include(t => t.User).FirstOrDefaultAsync(t => t.TaskId == taskId);

                if (task == null) return ApiNotFound("Task not found");
                if (task.UserId != userId) return ApiForbidden("You can only update your own tasks");

                if (!string.IsNullOrEmpty(dto.TaskTitle)) task.TaskTitle       = dto.TaskTitle.Trim();
                if (dto.TaskDescription != null)           task.TaskDescription = dto.TaskDescription.Trim();
                if (dto.ProjectName != null)               task.ProjectName     = dto.ProjectName.Trim();
                if (!string.IsNullOrEmpty(dto.Status))     task.Status          = dto.Status.ToLower();
                if (dto.HoursSpent.HasValue)               task.HoursSpent      = dto.HoursSpent.Value;
                if (dto.Remarks != null)                   task.Remarks         = dto.Remarks.Trim();

                task.UpdatedOn = DateTime.Now;
                await _context.SaveChangesAsync();

                return ApiOk("Task updated successfully", MapToDto(task, task.User!));
            }
            catch (Exception ex) { return ApiServerError("Failed to update task", ex); }
        }

        [HttpDelete("delete/{taskId}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteTask(int taskId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var role   = GetCurrentUserRole();

                var task = await _context.DailyTasks.FindAsync(taskId);
                if (task == null) return ApiNotFound("Task not found");

                if (task.UserId != userId && role != "admin")
                    return ApiForbidden("You can only delete your own tasks");

                _context.DailyTasks.Remove(task);
                await _context.SaveChangesAsync();

                return ApiOk("Task deleted successfully", "deleted");
            }
            catch (Exception ex) { return ApiServerError("Failed to delete task", ex); }
        }

        [HttpGet("my")]
        public async Task<ActionResult<ApiResponse<List<DailyTaskResponseDto>>>> GetMyTasks(
            [FromQuery] DateTime? date = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? status = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var query = _context.DailyTasks.Include(t => t.User).Where(t => t.UserId == userId);

                if (date.HasValue)
                    query = query.Where(t => t.TaskDate.Date == date.Value.Date);
                else
                {
                    if (fromDate.HasValue) query = query.Where(t => t.TaskDate >= fromDate.Value.Date);
                    if (toDate.HasValue)   query = query.Where(t => t.TaskDate <= toDate.Value.Date);
                }

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(t => t.Status.ToLower() == status.ToLower());

                var tasks = await query.OrderByDescending(t => t.TaskDate).ThenByDescending(t => t.CreatedOn).ToListAsync();

                return ApiOk($"Total {tasks.Count} tasks", tasks.Select(t => MapToDto(t, t.User!)).ToList());
            }
            catch (Exception ex) { return ApiServerError("Failed to retrieve tasks", ex); }
        }

        [HttpGet("my/today")]
        public async Task<ActionResult<ApiResponse<DailyTaskSummaryDto>>> GetMyTodaySummary()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);

                var tasks = await _context.DailyTasks
                    .Include(t => t.User)
                    .Where(t => t.UserId == userId && t.TaskDate.Date == DateTime.Today)
                    .OrderByDescending(t => t.CreatedOn)
                    .ToListAsync();

                var summary = new DailyTaskSummaryDto
                {
                    UserId     = userId,
                    UserName   = user?.UserName ?? "",
                    TaskDate   = DateTime.Today,
                    TotalTasks = tasks.Count,
                    TotalHours = tasks.Sum(t => t.HoursSpent),
                    Tasks      = tasks.Select(t => MapToDto(t, t.User!)).ToList()
                };

                return ApiOk("Today's summary", summary);
            }
            catch (Exception ex) { return ApiServerError("Failed to retrieve today's summary", ex); }
        }

        [HttpGet("all")]
        public async Task<ActionResult<ApiResponse<List<DailyTaskResponseDto>>>> GetAllTasks(
            [FromQuery] int? userId = null,
            [FromQuery] DateTime? date = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? status = null)
        {
            try
            {
                var role = GetCurrentUserRole();
                if (role != "admin" && role != "manager" && role != "hr")
                    return ApiForbidden("Access denied.");

                var query = _context.DailyTasks.Include(t => t.User).AsQueryable();

                if (userId.HasValue) query = query.Where(t => t.UserId == userId.Value);

                if (date.HasValue)
                    query = query.Where(t => t.TaskDate.Date == date.Value.Date);
                else
                {
                    if (fromDate.HasValue) query = query.Where(t => t.TaskDate >= fromDate.Value.Date);
                    if (toDate.HasValue)   query = query.Where(t => t.TaskDate <= toDate.Value.Date);
                }

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(t => t.Status.ToLower() == status.ToLower());

                var tasks = await query.OrderByDescending(t => t.TaskDate).ThenByDescending(t => t.CreatedOn).ToListAsync();

                return ApiOk($"Total {tasks.Count} tasks", tasks.Select(t => MapToDto(t, t.User!)).ToList());
            }
            catch (Exception ex) { return ApiServerError("Failed to retrieve tasks", ex); }
        }

        [HttpGet("all/today")]
        public async Task<ActionResult<ApiResponse<List<DailyTaskSummaryDto>>>> GetAllTodaySummary()
        {
            try
            {
                var role = GetCurrentUserRole();
                if (role != "admin" && role != "manager" && role != "hr")
                    return ApiForbidden("Access denied.");

                var tasks = await _context.DailyTasks
                    .Include(t => t.User)
                    .Where(t => t.TaskDate.Date == DateTime.Today)
                    .ToListAsync();

                var grouped = tasks
                    .GroupBy(t => t.UserId)
                    .Select(g => new DailyTaskSummaryDto
                    {
                        UserId     = g.Key,
                        UserName   = g.First().User?.UserName ?? "",
                        TaskDate   = DateTime.Today,
                        TotalTasks = g.Count(),
                        TotalHours = g.Sum(t => t.HoursSpent),
                        Tasks      = g.Select(t => MapToDto(t, t.User!)).ToList()
                    }).ToList();

                return ApiOk($"Today's summary — {grouped.Count} users", grouped);
            }
            catch (Exception ex) { return ApiServerError("Failed to retrieve today's summary", ex); }
        }

        private DailyTaskResponseDto MapToDto(DailyTask t, User user) => new()
        {
            TaskId          = t.TaskId,
            UserId          = t.UserId,
            UserName        = user.UserName,
            Email           = user.Email,
            Role            = user.Role,
            TaskDate        = t.TaskDate,
            TaskTitle       = t.TaskTitle,
            TaskDescription = t.TaskDescription,
            ProjectName     = t.ProjectName,
            Status          = t.Status,
            HoursSpent      = t.HoursSpent,
            Remarks         = t.Remarks,
            CreatedOn       = t.CreatedOn,
            UpdatedOn       = t.UpdatedOn
        };
    }
}