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
//     public class ProfileController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;
//         private readonly IWebHostEnvironment _env;

//         public ProfileController(ApplicationDbContext context, IWebHostEnvironment env)
//         {
//             _context = context;
//             _env     = env;
//         }

//         // ─── Helper: UserId from JWT ──────────────────────────────────────
//         private int GetUserId()
//         {
//             var claim = User.FindFirst(ClaimTypes.NameIdentifier);
//             return claim != null ? int.Parse(claim.Value) : 0;
//         }

//         // ─── Helper: Map User entity → ProfileDto ────────────────────────
//         private static UserProfileDto MapToDto(User u) => new()
//         {
//             UserId           = u.UserId,
//             UserName         = u.UserName,
//             Email            = u.Email,
//             Role             = u.Role,
//             Phone            = u.Phone,
//             Department       = u.Department,
//             Designation      = u.Designation,
//             ProfilePhoto     = u.ProfilePhoto,
//             DateOfBirth      = u.DateOfBirth?.ToString("dd-MMM-yyyy"),
//             Address          = u.Address,
//             EmergencyContact = u.EmergencyContact,
//             JoiningDate      = u.CreatedOn.ToString("dd-MMM-yyyy"),
//             IsActive         = u.IsActive,
//             LastSeen         = u.LastSeen?.ToString("dd-MMM-yyyy HH:mm:ss")
//         };

//         // ─────────────────────────────────────────────────────────────────
//         // GET /api/Profile  — Employee: Get own profile
//         // ─────────────────────────────────────────────────────────────────
//         [HttpGet]
//         public async Task<IActionResult> GetProfile()
//         {
//             var user = await _context.Users.FindAsync(GetUserId());
//             if (user == null)
//                 return NotFound(new { success = false, message = "User not found" });

//             return Ok(new { success = true, message = "Profile retrieved", data = MapToDto(user) });
//         }

//         // ─────────────────────────────────────────────────────────────────
//         // GET /api/Profile/{userId}  — Admin: Get any user's profile
//         // ─────────────────────────────────────────────────────────────────
//         [HttpGet("{userId:int}")]
//         [Authorize(Roles = "admin")]
//         public async Task<IActionResult> GetUserProfile(int userId)
//         {
//             var user = await _context.Users.FindAsync(userId);
//             if (user == null)
//                 return NotFound(new { success = false, message = "User not found" });

//             return Ok(new { success = true, message = "Profile retrieved", data = MapToDto(user) });
//         }

//         // ─────────────────────────────────────────────────────────────────
//         // GET /api/Profile/all  — Admin: Get all users profiles
//         // ─────────────────────────────────────────────────────────────────
//         [HttpGet("all")]
//         [Authorize(Roles = "admin")]
//         public async Task<IActionResult> GetAllProfiles(
//             [FromQuery] bool onlyActive = true,
//             [FromQuery] string? role = null)
//         {
//             var query = _context.Users.AsQueryable();

//             if (onlyActive)
//                 query = query.Where(u => u.IsActive);

//             if (!string.IsNullOrEmpty(role))
//                 query = query.Where(u => u.Role.ToLower() == role.ToLower());

//             var users = await query.OrderBy(u => u.UserName).ToListAsync();
//             var result = users.Select(MapToDto).ToList();

//             return Ok(new { success = true, message = "Profiles retrieved", totalCount = result.Count, data = result });
//         }

//         // ─────────────────────────────────────────────────────────────────
//         // PUT /api/Profile  — Employee: Update own profile
//         // Accepts: multipart/form-data (for optional photo upload)
//         // ─────────────────────────────────────────────────────────────────
//         [HttpPut]
//         [Consumes("multipart/form-data")]
//         public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto dto)
//         {
//             var user = await _context.Users.FindAsync(GetUserId());
//             if (user == null)
//                 return NotFound(new { success = false, message = "User not found" });

//             // Update text fields only if provided
//             if (!string.IsNullOrWhiteSpace(dto.UserName))        user.UserName        = dto.UserName;
//             if (!string.IsNullOrWhiteSpace(dto.Phone))           user.Phone           = dto.Phone;
//             if (!string.IsNullOrWhiteSpace(dto.Department))      user.Department      = dto.Department;
//             if (!string.IsNullOrWhiteSpace(dto.Designation))     user.Designation     = dto.Designation;
//             if (dto.DateOfBirth.HasValue)                         user.DateOfBirth     = dto.DateOfBirth;
//             if (!string.IsNullOrWhiteSpace(dto.Address))         user.Address         = dto.Address;
//             if (!string.IsNullOrWhiteSpace(dto.EmergencyContact))user.EmergencyContact = dto.EmergencyContact;

//             // Handle profile photo upload
//             if (dto.ProfilePhoto != null && dto.ProfilePhoto.Length > 0)
//             {
//                 var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
//                 if (!allowedTypes.Contains(dto.ProfilePhoto.ContentType.ToLower()))
//                     return BadRequest(new { success = false, message = "Only JPG/PNG files allowed for profile photo" });

//                 if (dto.ProfilePhoto.Length > 1 * 1024 * 1024) // 1 MB limit
//                     return BadRequest(new { success = false, message = "Profile photo must be less than 1 MB" });

//                 var uploadDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "profiles");
//                 if (!Directory.Exists(uploadDir))
//                     Directory.CreateDirectory(uploadDir);

//                 // Delete old photo if exists
//                 if (!string.IsNullOrEmpty(user.ProfilePhoto))
//                 {
//                     var oldFilePath = Path.Combine(_env.WebRootPath ?? "wwwroot",
//                         user.ProfilePhoto.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
//                     if (System.IO.File.Exists(oldFilePath))
//                         System.IO.File.Delete(oldFilePath);
//                 }

//                 var ext      = Path.GetExtension(dto.ProfilePhoto.FileName);
//                 var fileName = $"profile_{user.UserId}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
//                 var filePath = Path.Combine(uploadDir, fileName);

//                 using (var stream = new FileStream(filePath, FileMode.Create))
//                     await dto.ProfilePhoto.CopyToAsync(stream);

//                 user.ProfilePhoto = $"/uploads/profiles/{fileName}";
//             }

//             await _context.SaveChangesAsync();

//             return Ok(new { success = true, message = "Profile updated successfully", data = MapToDto(user) });
//         }

//         // ─────────────────────────────────────────────────────────────────
//         // DELETE /api/Profile/photo  — Employee: Remove profile photo
//         // ─────────────────────────────────────────────────────────────────
//         [HttpDelete("photo")]
//         public async Task<IActionResult> RemoveProfilePhoto()
//         {
//             var user = await _context.Users.FindAsync(GetUserId());
//             if (user == null)
//                 return NotFound(new { success = false, message = "User not found" });

//             if (string.IsNullOrEmpty(user.ProfilePhoto))
//                 return BadRequest(new { success = false, message = "No profile photo to remove" });

//             // Delete file from disk
//             var filePath = Path.Combine(_env.WebRootPath ?? "wwwroot",
//                 user.ProfilePhoto.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
//             if (System.IO.File.Exists(filePath))
//                 System.IO.File.Delete(filePath);

//             user.ProfilePhoto = null;
//             await _context.SaveChangesAsync();

//             return Ok(new { success = true, message = "Profile photo removed successfully" });
//         }

//         // ─────────────────────────────────────────────────────────────────
//         // POST /api/Profile/changepassword  — Employee: Change own password
//         // ─────────────────────────────────────────────────────────────────
//         [HttpPost("changepassword")]
//         public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

//             if (dto.NewPassword != dto.ConfirmPassword)
//                 return BadRequest(new { success = false, message = "New password and confirm password do not match" });

//             var user = await _context.Users.FindAsync(GetUserId());
//             if (user == null)
//                 return NotFound(new { success = false, message = "User not found" });

//             // Verify current password
//             if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
//                 return BadRequest(new { success = false, message = "Current password is incorrect" });

//             // New password must be different
//             if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
//                 return BadRequest(new { success = false, message = "New password must be different from current password" });

//             // ✅ FIX: passwordhash mein bcrypt hash + confirmpassword mein plain text
//             user.PasswordHash    = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
//             user.ConfirmPassword = dto.ConfirmPassword; // plain text save
//             await _context.SaveChangesAsync();

//             return Ok(new { success = true, message = "Password changed successfully" });
//         }
//     }
// }











using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using attendance_api.Data;
using attendance_api.DTOs;
using attendance_api.Models;

namespace attendance_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProfileController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env     = env;
        }

        private static UserProfileDto MapToDto(User u) => new()
        {
            UserId           = u.UserId,
            UserName         = u.UserName,
            Email            = u.Email,
            Role             = u.Role,
            Phone            = u.Phone,
            Department       = u.Department,
            Designation      = u.Designation,
            ProfilePhoto     = u.ProfilePhoto,
            DateOfBirth      = u.DateOfBirth?.ToString("dd-MMM-yyyy"),
            Address          = u.Address,
            EmergencyContact = u.EmergencyContact,
            JoiningDate      = u.CreatedOn.ToString("dd-MMM-yyyy"),
            IsActive         = u.IsActive,
            LastSeen         = u.LastSeen?.ToString("dd-MMM-yyyy HH:mm:ss")
        };

        // GET /api/Profile — Employee: Get own profile
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var user = await _context.Users.FindAsync(GetCurrentUserId());
            if (user == null) return ApiNotFound("User not found");

            return ApiOk("Profile retrieved", MapToDto(user));
        }

        // GET /api/Profile/{userId} — Admin: Get any user's profile
        [HttpGet("{userId:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetUserProfile(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return ApiNotFound("User not found");

            return ApiOk("Profile retrieved", MapToDto(user));
        }

        // GET /api/Profile/all — Admin: Get all users profiles
        [HttpGet("all")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllProfiles(
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? role    = null)
        {
            var query = _context.Users.AsQueryable();

            if (onlyActive)
                query = query.Where(u => u.IsActive);

            if (!string.IsNullOrEmpty(role))
                query = query.Where(u => u.Role.ToLower() == role.ToLower());

            var users  = await query.OrderBy(u => u.UserName).ToListAsync();
            var result = users.Select(MapToDto).ToList();

            return ApiOk($"Profiles retrieved — total {result.Count}", result);
        }

        // PUT /api/Profile — Employee: Update own profile
        [HttpPut]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto dto)
        {
            var user = await _context.Users.FindAsync(GetCurrentUserId());
            if (user == null) return ApiNotFound("User not found");

            if (!string.IsNullOrWhiteSpace(dto.UserName))         user.UserName         = dto.UserName;
            if (!string.IsNullOrWhiteSpace(dto.Phone))            user.Phone            = dto.Phone;
            if (!string.IsNullOrWhiteSpace(dto.Department))       user.Department       = dto.Department;
            if (!string.IsNullOrWhiteSpace(dto.Designation))      user.Designation      = dto.Designation;
            if (dto.DateOfBirth.HasValue)                          user.DateOfBirth      = dto.DateOfBirth;
            if (!string.IsNullOrWhiteSpace(dto.Address))          user.Address          = dto.Address;
            if (!string.IsNullOrWhiteSpace(dto.EmergencyContact)) user.EmergencyContact = dto.EmergencyContact;

            if (dto.ProfilePhoto != null && dto.ProfilePhoto.Length > 0)
            {
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
                if (!allowedTypes.Contains(dto.ProfilePhoto.ContentType.ToLower()))
                    return ApiBadRequest("Only JPG/PNG files allowed for profile photo");

                if (dto.ProfilePhoto.Length > 1 * 1024 * 1024)
                    return ApiBadRequest("Profile photo must be less than 1 MB");

                var uploadDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "profiles");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                if (!string.IsNullOrEmpty(user.ProfilePhoto))
                {
                    var oldFilePath = Path.Combine(_env.WebRootPath ?? "wwwroot",
                        user.ProfilePhoto.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                }

                var ext      = Path.GetExtension(dto.ProfilePhoto.FileName);
                var fileName = $"profile_{user.UserId}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await dto.ProfilePhoto.CopyToAsync(stream);

                user.ProfilePhoto = $"/uploads/profiles/{fileName}";
            }

            await _context.SaveChangesAsync();
            return ApiOk("Profile updated successfully", MapToDto(user));
        }

        // DELETE /api/Profile/photo — Employee: Remove profile photo
        [HttpDelete("photo")]
        public async Task<IActionResult> RemoveProfilePhoto()
        {
            var user = await _context.Users.FindAsync(GetCurrentUserId());
            if (user == null) return ApiNotFound("User not found");

            if (string.IsNullOrEmpty(user.ProfilePhoto))
                return ApiBadRequest("No profile photo to remove");

            var filePath = Path.Combine(_env.WebRootPath ?? "wwwroot",
                user.ProfilePhoto.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

            user.ProfilePhoto = null;
            await _context.SaveChangesAsync();

            return ApiOk("Profile photo removed successfully");
        }

        // POST /api/Profile/changepassword — Employee: Change own password
        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return ApiBadRequest("Invalid data");

            if (dto.NewPassword != dto.ConfirmPassword)
                return ApiBadRequest("New password and confirm password do not match");

            var user = await _context.Users.FindAsync(GetCurrentUserId());
            if (user == null) return ApiNotFound("User not found");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                return ApiBadRequest("Current password is incorrect");

            if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
                return ApiBadRequest("New password must be different from current password");

            user.PasswordHash    = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.ConfirmPassword = dto.ConfirmPassword;
            await _context.SaveChangesAsync();

            return ApiOk("Password changed successfully");
        }
    }
}