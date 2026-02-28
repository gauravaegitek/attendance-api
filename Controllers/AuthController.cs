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
//     public class AuthController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;
//         private readonly IJwtService _jwtService;

//         public AuthController(ApplicationDbContext context, IJwtService jwtService)
//         {
//             _context = context;
//             _jwtService = jwtService;
//         }

//         [HttpPost("register")]
//         public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Register([FromBody] RegisterDto dto)
//         {
//             try
//             {
//                 var email = dto.Email.Trim().ToLower();

//                 if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
//                 {
//                     return BadRequest(new ApiResponse<LoginResponseDto>
//                     {
//                         Success = false,
//                         Message = "Email already exists"
//                     });
//                 }

//                 var validRoles = new[] { "admin", "manager", "supervisor", "developer", "tester", "hr", "employee" };
//                 var role = dto.Role.Trim().ToLower();

//                 if (!validRoles.Contains(role))
//                 {
//                     return BadRequest(new ApiResponse<LoginResponseDto>
//                     {
//                         Success = false,
//                         Message = "Invalid role. Role must be one of: admin, manager, supervisor, developer, tester, hr, employee"
//                     });
//                 }

//                 var roleId = dto.RoleId;
//                 if (roleId == 0)
//                 {
//                     var roleRecord = await _context.Roles
//                         .FirstOrDefaultAsync(r => r.RoleName.ToLower() == role);
//                     if (roleRecord != null)
//                         roleId = roleRecord.RoleId;
//                 }

//                 var user = new User
//                 {
//                     UserName = dto.UserName.Trim(),
//                     Email = email,
//                     PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
//                     ConfirmPassword = dto.Password,
//                     Role = role,
//                     RoleId = roleId,
//                     CreatedOn = DateTime.Now,
//                     IsActive = true
//                 };

//                 _context.Users.Add(user);
//                 await _context.SaveChangesAsync();

//                 return Ok(new ApiResponse<LoginResponseDto>
//                 {
//                     Success = true,
//                     Message = "Registration successful",
//                     Data = new LoginResponseDto
//                     {
//                         UserId = user.UserId,
//                         UserName = user.UserName,
//                         Email = user.Email,
//                         Role = user.Role,
//                         Token = "",
//                         Message = "Welcome! Please login to continue."
//                     }
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<LoginResponseDto>
//                 {
//                     Success = false,
//                     Message = "Registration failed",
//                     Errors = new List<string> { ex.Message }
//                 });
//             }
//         }

//         [HttpPost("login")]
//         public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto dto)
//         {
//             try
//             {
//                 var email = dto.Email.Trim().ToLower();

//                 var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

//                 if (user == null)
//                 {
//                     return Unauthorized(new ApiResponse<LoginResponseDto>
//                     {
//                         Success = false,
//                         Message = "Invalid email or password"
//                     });
//                 }

//                 if (!user.IsActive)
//                 {
//                     return Unauthorized(new ApiResponse<LoginResponseDto>
//                     {
//                         Success = false,
//                         Message = "User account is deactivated. Please contact admin."
//                     });
//                 }

//                 if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
//                 {
//                     return Unauthorized(new ApiResponse<LoginResponseDto>
//                     {
//                         Success = false,
//                         Message = "Invalid email or password"
//                     });
//                 }

//                 var deviceId = dto.DeviceId?.Trim() ?? "";
//                 if (!string.IsNullOrEmpty(user.DeviceId) && user.DeviceId != deviceId)
//                 {
//                     return StatusCode(403, new ApiResponse<LoginResponseDto>
//                     {
//                         Success = false,
//                         Message = "This account is already logged in on another device. Please clear device ID to login from this device.",
//                         Errors = new List<string> { "DEVICE_MISMATCH" }
//                     });
//                 }

//                 user.DeviceId = deviceId;
//                 user.LastSeen = DateTime.Now;
//                 await _context.SaveChangesAsync();

//                 var token = _jwtService.GenerateToken(user);

//                 user.CurrentToken = token;
//                 await _context.SaveChangesAsync();

//                 return Ok(new ApiResponse<LoginResponseDto>
//                 {
//                     Success = true,
//                     Message = "Login successful",
//                     Data = new LoginResponseDto
//                     {
//                         UserId = user.UserId,
//                         UserName = user.UserName,
//                         Email = user.Email,
//                         Role = user.Role,
//                         Token = token,
//                         Message = "Welcome back!"
//                     }
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<LoginResponseDto>
//                 {
//                     Success = false,
//                     Message = "Login failed",
//                     Errors = new List<string> { ex.Message }
//                 });
//             }
//         }

//         // ✅ Clear Device — Sirf Admin
//         [Authorize]
//         [HttpPost("cleardevice")]
//         public async Task<ActionResult<ApiResponse<string>>> ClearDevice([FromBody] ClearDeviceDto dto)
//         {
//             try
//             {
//                 // ✅ Manual role check with custom message
//                 var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
//                 if (userRole?.ToLower() != "admin")
//                 {
//                     return StatusCode(403, new ApiResponse<string>
//                     {
//                         Success = false,
//                         Message = "Access denied. Only admin can clear device ID.",
//                         Errors = new List<string> { "UNAUTHORIZED_ROLE" }
//                     });
//                 }

//                 var user = await _context.Users.FindAsync(dto.UserId);

//                 if (user == null)
//                 {
//                     return NotFound(new ApiResponse<string>
//                     {
//                         Success = false,
//                         Message = "User not found"
//                     });
//                 }

//                 user.DeviceId = null;
//                 user.CurrentToken = null;

//                 await _context.SaveChangesAsync();

//                 return Ok(new ApiResponse<string>
//                 {
//                     Success = true,
//                     Message = "Device ID cleared successfully. You can now login from a different device.",
//                     Data = "Device cleared"
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<string>
//                 {
//                     Success = false,
//                     Message = "Failed to clear device ID",
//                     Errors = new List<string> { ex.Message }
//                 });
//             }
//         }

//         [Authorize]
//         [HttpPost("logout")]
//         public async Task<ActionResult<ApiResponse<string>>> Logout()
//         {
//             try
//             {
//                 var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
//                 if (userIdClaim == null)
//                 {
//                     return Unauthorized(new ApiResponse<string>
//                     {
//                         Success = false,
//                         Message = "Invalid token"
//                     });
//                 }

//                 var userId = int.Parse(userIdClaim.Value);
//                 var user = await _context.Users.FindAsync(userId);

//                 if (user != null)
//                 {
//                     user.LastSeen = DateTime.Now;
//                     user.CurrentToken = null;
//                     await _context.SaveChangesAsync();
//                 }

//                 return Ok(new ApiResponse<string>
//                 {
//                     Success = true,
//                     Message = "Logout successful",
//                     Data = "Logged out"
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<string>
//                 {
//                     Success = false,
//                     Message = "Logout failed",
//                     Errors = new List<string> { ex.Message }
//                 });
//             }
//         }

//         [Authorize(Roles = "admin")]
//         [HttpGet("users")]
//         public async Task<ActionResult<ApiResponse<List<UserListDto>>>> GetAllUsers()
//         {
//             try
//             {
//                 var users = await _context.Users
//                     .Select(u => new UserListDto
//                     {
//                         UserId = u.UserId,
//                         UserName = u.UserName,
//                         Email = u.Email,
//                         Role = u.Role,
//                         DeviceId = u.DeviceId,
//                         LastSeen = u.LastSeen,
//                         CreatedOn = u.CreatedOn,
//                         IsActive = u.IsActive
//                     })
//                     .ToListAsync();

//                 return Ok(new ApiResponse<List<UserListDto>>
//                 {
//                     Success = true,
//                     Message = "Users retrieved successfully",
//                     Data = users
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<List<UserListDto>>
//                 {
//                     Success = false,
//                     Message = "Failed to retrieve users",
//                     Errors = new List<string> { ex.Message }
//                 });
//             }
//         }

//         // ✅ User Active/Inactive Toggle — Sirf Admin
//         [Authorize(Roles = "admin")]
//         [HttpPut("toggle-status/{userId}")]
//         public async Task<ActionResult<ApiResponse<string>>> ToggleUserStatus(int userId)
//         {
//             try
//             {
//                 var user = await _context.Users.FindAsync(userId);

//                 if (user == null)
//                 {
//                     return NotFound(new ApiResponse<string>
//                     {
//                         Success = false,
//                         Message = "User not found"
//                     });
//                 }

//                 user.IsActive = !user.IsActive;

//                 if (!user.IsActive)
//                 {
//                     user.CurrentToken = null;
//                     user.DeviceId = null;
//                 }

//                 await _context.SaveChangesAsync();

//                 return Ok(new ApiResponse<string>
//                 {
//                     Success = true,
//                     Message = user.IsActive
//                         ? $"User '{user.UserName}' activated successfully."
//                         : $"User '{user.UserName}' deactivated successfully.",
//                     Data = user.IsActive ? "active" : "inactive"
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<string>
//                 {
//                     Success = false,
//                     Message = "Failed to update user status",
//                     Errors = new List<string> { ex.Message }
//                 });
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
    public class AuthController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthController(ApplicationDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var email = dto.Email.Trim().ToLower();

                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
                    return ApiBadRequest("Email already exists");

                var validRoles = new[] { "admin", "manager", "supervisor", "developer", "tester", "hr", "employee" };
                var role = dto.Role.Trim().ToLower();

                if (!validRoles.Contains(role))
                    return ApiBadRequest("Invalid role. Role must be one of: admin, manager, supervisor, developer, tester, hr, employee");

                var roleId = dto.RoleId;
                if (roleId == 0)
                {
                    var roleRecord = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == role);
                    if (roleRecord != null) roleId = roleRecord.RoleId;
                }

                var user = new User
                {
                    UserName        = dto.UserName.Trim(),
                    Email           = email,
                    PasswordHash    = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    ConfirmPassword = dto.Password,
                    Role            = role,
                    RoleId          = roleId,
                    CreatedOn       = DateTime.Now,
                    IsActive        = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return ApiOk("Registration successful", new LoginResponseDto
                {
                    UserId   = user.UserId,
                    UserName = user.UserName,
                    Email    = user.Email,
                    Role     = user.Role,
                    Token    = "",
                    Message  = "Welcome! Please login to continue."
                });
            }
            catch (Exception ex) { return ApiServerError("Registration failed", ex); }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto dto)
        {
            try
            {
                var email = dto.Email.Trim().ToLower();
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

                if (user == null)
                    return ApiUnauthorized("Invalid email or password");

                if (!user.IsActive)
                    return ApiUnauthorized("User account is deactivated. Please contact admin.");

                if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                    return ApiUnauthorized("Invalid email or password");

                var deviceId = dto.DeviceId?.Trim() ?? "";
                if (!string.IsNullOrEmpty(user.DeviceId) && user.DeviceId != deviceId)
                    return StatusCode(403, new ApiResponse<object>
                    {
                        Message = "This account is already logged in on another device. Please clear device ID to login from this device.",
                        Success = false,
                        Errors  = new List<string> { "DEVICE_MISMATCH" }
                    });

                user.DeviceId = deviceId;
                user.LastSeen = DateTime.Now;
                await _context.SaveChangesAsync();

                var token = _jwtService.GenerateToken(user);
                user.CurrentToken = token;
                await _context.SaveChangesAsync();

                return ApiOk("Login successful", new LoginResponseDto
                {
                    UserId   = user.UserId,
                    UserName = user.UserName,
                    Email    = user.Email,
                    Role     = user.Role,
                    Token    = token,
                    Message  = "Welcome back!"
                });
            }
            catch (Exception ex) { return ApiServerError("Login failed", ex); }
        }

        [Authorize]
        [HttpPost("cleardevice")]
        public async Task<ActionResult<ApiResponse<string>>> ClearDevice([FromBody] ClearDeviceDto dto)
        {
            try
            {
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                if (userRole?.ToLower() != "admin")
                    return ApiForbidden("Access denied. Only admin can clear device ID.");

                var user = await _context.Users.FindAsync(dto.UserId);
                if (user == null) return ApiNotFound("User not found");

                user.DeviceId     = null;
                user.CurrentToken = null;
                await _context.SaveChangesAsync();

                return ApiOk("Device ID cleared successfully. You can now login from a different device.", "Device cleared");
            }
            catch (Exception ex) { return ApiServerError("Failed to clear device ID", ex); }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<string>>> Logout()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return ApiUnauthorized("Invalid token");

                var userId = int.Parse(userIdClaim.Value);
                var user = await _context.Users.FindAsync(userId);

                if (user != null)
                {
                    user.LastSeen     = DateTime.Now;
                    user.CurrentToken = null;
                    await _context.SaveChangesAsync();
                }

                return ApiOk("Logout successful", "Logged out");
            }
            catch (Exception ex) { return ApiServerError("Logout failed", ex); }
        }

        [Authorize(Roles = "admin")]
        [HttpGet("users")]
        public async Task<ActionResult<ApiResponse<List<UserListDto>>>> GetAllUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new UserListDto
                    {
                        UserId    = u.UserId,
                        UserName  = u.UserName,
                        Email     = u.Email,
                        Role      = u.Role,
                        DeviceId  = u.DeviceId,
                        LastSeen  = u.LastSeen,
                        CreatedOn = u.CreatedOn,
                        IsActive  = u.IsActive
                    })
                    .ToListAsync();

                return ApiOk("Users retrieved successfully", users);
            }
            catch (Exception ex) { return ApiServerError("Failed to retrieve users", ex); }
        }

        [Authorize(Roles = "admin")]
        [HttpPut("toggle-status/{userId}")]
        public async Task<ActionResult<ApiResponse<string>>> ToggleUserStatus(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return ApiNotFound("User not found");

                user.IsActive = !user.IsActive;

                if (!user.IsActive)
                {
                    user.CurrentToken = null;
                    user.DeviceId     = null;
                }

                await _context.SaveChangesAsync();

                var msg = user.IsActive
                    ? $"User '{user.UserName}' activated successfully."
                    : $"User '{user.UserName}' deactivated successfully.";

                return ApiOk(msg, user.IsActive ? "active" : "inactive");
            }
            catch (Exception ex) { return ApiServerError("Failed to update user status", ex); }
        }
    }
}