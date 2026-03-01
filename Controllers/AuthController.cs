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
//     public class AuthController : BaseController
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
//                     return ApiBadRequest("Email already exists");

//                 var validRoles = new[] { "admin", "manager", "supervisor", "developer", "tester", "hr", "employee" };
//                 var role = dto.Role.Trim().ToLower();

//                 if (!validRoles.Contains(role))
//                     return ApiBadRequest("Invalid role. Role must be one of: admin, manager, supervisor, developer, tester, hr, employee");

//                 var roleId = dto.RoleId;
//                 if (roleId == 0)
//                 {
//                     var roleRecord = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == role);
//                     if (roleRecord != null) roleId = roleRecord.RoleId;
//                 }

//                 var user = new User
//                 {
//                     UserName        = dto.UserName.Trim(),
//                     Email           = email,
//                     PasswordHash    = BCrypt.Net.BCrypt.HashPassword(dto.Password),
//                     ConfirmPassword = dto.Password,
//                     Role            = role,
//                     RoleId          = roleId,
//                     CreatedOn       = DateTime.Now,
//                     IsActive        = true
//                 };

//                 _context.Users.Add(user);
//                 await _context.SaveChangesAsync();

//                 return ApiOk("Registration successful", new LoginResponseDto
//                 {
//                     UserId   = user.UserId,
//                     UserName = user.UserName,
//                     Email    = user.Email,
//                     Role     = user.Role,
//                     Token    = "",
//                     Message  = "Welcome! Please login to continue."
//                 });
//             }
//             catch (Exception ex) { return ApiServerError("Registration failed", ex); }
//         }

//         [HttpPost("login")]
//         public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto dto)
//         {
//             try
//             {
//                 var email = dto.Email.Trim().ToLower();
//                 var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

//                 if (user == null)
//                     return ApiUnauthorized("Invalid email or password");

//                 if (!user.IsActive)
//                     return ApiUnauthorized("User account is deactivated. Please contact admin.");

//                 if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
//                     return ApiUnauthorized("Invalid email or password");

//                 var deviceId = dto.DeviceId?.Trim() ?? "";
//                 if (!string.IsNullOrEmpty(user.DeviceId) && user.DeviceId != deviceId)
//                     return StatusCode(403, new ApiResponse<object>
//                     {
//                         Message = "This account is already logged in on another device. Please clear device ID to login from this device.",
//                         Success = false,
//                         Errors  = new List<string> { "DEVICE_MISMATCH" }
//                     });

//                 user.DeviceId = deviceId;
//                 user.LastSeen = DateTime.Now;
//                 await _context.SaveChangesAsync();

//                 var token = _jwtService.GenerateToken(user);
//                 user.CurrentToken = token;
//                 await _context.SaveChangesAsync();

//                 return ApiOk("Login successful", new LoginResponseDto
//                 {
//                     UserId   = user.UserId,
//                     UserName = user.UserName,
//                     Email    = user.Email,
//                     Role     = user.Role,
//                     Token    = token,
//                     Message  = "Welcome back!"
//                 });
//             }
//             catch (Exception ex) { return ApiServerError("Login failed", ex); }
//         }

//         [Authorize]
//         [HttpPost("cleardevice")]
//         public async Task<ActionResult<ApiResponse<string>>> ClearDevice([FromBody] ClearDeviceDto dto)
//         {
//             try
//             {
//                 var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
//                 if (userRole?.ToLower() != "admin")
//                     return ApiForbidden("Access denied. Only admin can clear device ID.");

//                 var user = await _context.Users.FindAsync(dto.UserId);
//                 if (user == null) return ApiNotFound("User not found");

//                 user.DeviceId     = null;
//                 user.CurrentToken = null;
//                 await _context.SaveChangesAsync();

//                 return ApiOk("Device ID cleared successfully. You can now login from a different device.", "Device cleared");
//             }
//             catch (Exception ex) { return ApiServerError("Failed to clear device ID", ex); }
//         }

//         [Authorize]
//         [HttpPost("logout")]
//         public async Task<ActionResult<ApiResponse<string>>> Logout()
//         {
//             try
//             {
//                 var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
//                 if (userIdClaim == null) return ApiUnauthorized("Invalid token");

//                 var userId = int.Parse(userIdClaim.Value);
//                 var user = await _context.Users.FindAsync(userId);

//                 if (user != null)
//                 {
//                     user.LastSeen     = DateTime.Now;
//                     user.CurrentToken = null;
//                     await _context.SaveChangesAsync();
//                 }

//                 return ApiOk("Logout successful", "Logged out");
//             }
//             catch (Exception ex) { return ApiServerError("Logout failed", ex); }
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
//                         UserId    = u.UserId,
//                         UserName  = u.UserName,
//                         Email     = u.Email,
//                         Role      = u.Role,
//                         DeviceId  = u.DeviceId,
//                         LastSeen  = u.LastSeen,
//                         CreatedOn = u.CreatedOn,
//                         IsActive  = u.IsActive
//                     })
//                     .ToListAsync();

//                 return ApiOk("Users retrieved successfully", users);
//             }
//             catch (Exception ex) { return ApiServerError("Failed to retrieve users", ex); }
//         }

//         [Authorize(Roles = "admin")]
//         [HttpPut("toggle-status/{userId}")]
//         public async Task<ActionResult<ApiResponse<string>>> ToggleUserStatus(int userId)
//         {
//             try
//             {
//                 var user = await _context.Users.FindAsync(userId);
//                 if (user == null) return ApiNotFound("User not found");

//                 user.IsActive = !user.IsActive;

//                 if (!user.IsActive)
//                 {
//                     user.CurrentToken = null;
//                     user.DeviceId     = null;
//                 }

//                 await _context.SaveChangesAsync();

//                 var msg = user.IsActive
//                     ? $"User '{user.UserName}' activated successfully."
//                     : $"User '{user.UserName}' deactivated successfully.";

//                 return ApiOk(msg, user.IsActive ? "active" : "inactive");
//             }
//             catch (Exception ex) { return ApiServerError("Failed to update user status", ex); }
//         }
//     }
// }










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
//     public class AuthController : BaseController
//     {
//         private readonly ApplicationDbContext _context;
//         private readonly IJwtService _jwtService;

//         public AuthController(ApplicationDbContext context, IJwtService jwtService)
//         {
//             _context  = context;
//             _jwtService = jwtService;
//         }

//         // ────────────────────────────────────────────────
//         // REGISTER  (no changes)
//         // ────────────────────────────────────────────────
//         [HttpPost("register")]
//         public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Register([FromBody] RegisterDto dto)
//         {
//             try
//             {
//                 var email = dto.Email.Trim().ToLower();

//                 if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
//                     return ApiBadRequest("Email already exists");

//                 var validRoles = new[] { "admin", "manager", "supervisor", "developer", "tester", "hr", "employee" };
//                 var role       = dto.Role.Trim().ToLower();

//                 if (!validRoles.Contains(role))
//                     return ApiBadRequest("Invalid role. Role must be one of: admin, manager, supervisor, developer, tester, hr, employee");

//                 var roleId = dto.RoleId;
//                 if (roleId == 0)
//                 {
//                     var roleRecord = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == role);
//                     if (roleRecord != null) roleId = roleRecord.RoleId;
//                 }

//                 var user = new User
//                 {
//                     UserName        = dto.UserName.Trim(),
//                     Email           = email,
//                     PasswordHash    = BCrypt.Net.BCrypt.HashPassword(dto.Password),
//                     ConfirmPassword = dto.Password,
//                     Role            = role,
//                     RoleId          = roleId,
//                     CreatedOn       = DateTime.Now,
//                     IsActive        = true
//                 };

//                 _context.Users.Add(user);
//                 await _context.SaveChangesAsync();

//                 return ApiOk("Registration successful", new LoginResponseDto
//                 {
//                     UserId   = user.UserId,
//                     UserName = user.UserName,
//                     Email    = user.Email,
//                     Role     = user.Role,
//                     Token    = "",
//                     Message  = "Welcome! Please login to continue."
//                 });
//             }
//             catch (Exception ex) { return ApiServerError("Registration failed", ex); }
//         }

//         // ────────────────────────────────────────────────
//         // LOGIN  ✅ UPDATED — Login history INSERT added
//         // ────────────────────────────────────────────────
//         // [HttpPost("login")]
//         // public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto dto)
//         // {
//         //     try
//         //     {
//         //         var email = dto.Email.Trim().ToLower();
//         //         var user  = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

//         //         if (user == null)
//         //             return ApiUnauthorized("Invalid email or password");

//         //         if (!user.IsActive)
//         //             return ApiUnauthorized("User account is deactivated. Please contact admin.");

//         //         if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
//         //             return ApiUnauthorized("Invalid email or password");

//         //         var deviceId = dto.DeviceId?.Trim() ?? "";
//         //         if (!string.IsNullOrEmpty(user.DeviceId) && user.DeviceId != deviceId)
//         //             return StatusCode(403, new ApiResponse<object>
//         //             {
//         //                 Message = "This account is already logged in on another device. Please clear device ID to login from this device.",
//         //                 Success = false,
//         //                 Errors  = new List<string> { "DEVICE_MISMATCH" }
//         //             });

//         //         var now       = DateTime.Now;              // ← ✅ NEW
//         //         user.DeviceId = deviceId;
//         //         user.LastSeen = now;
//         //         await _context.SaveChangesAsync();

//         //         var token        = _jwtService.GenerateToken(user);
//         //         user.CurrentToken = token;
//         //         await _context.SaveChangesAsync();

//         //         // ── ✅ NEW: Login History row INSERT ─────
//         //         var loginHistory = new UserLoginHistory
//         //         {
//         //             UserId     = user.UserId,
//         //             UserName   = user.UserName,
//         //             LoginDate  = now.Date,
//         //             LoginTime  = now.TimeOfDay,
//         //             DeviceType = dto.DeviceType?.Trim(),   // Mobile / Desktop / Web
//         //             DeviceName = dto.DeviceName?.Trim(),   // Samsung S21 / Chrome on Windows
//         //             CreatedAt  = now,
//         //             UpdatedAt  = now
//         //         };
//         //         _context.UserLoginHistories.Add(loginHistory);
//         //         await _context.SaveChangesAsync();
//         //         // ─────────────────────────────────────────

//         //         return ApiOk("Login successful", new LoginResponseDto
//         //         {
//         //             UserId         = user.UserId,
//         //             UserName       = user.UserName,
//         //             Email          = user.Email,
//         //             Role           = user.Role,
//         //             Token          = token,
//         //             Message        = "Welcome back!",
//         //             LoginHistoryId = loginHistory.Id       // ← ✅ NEW: Flutter side save karega
//         //         });
//         //     }
//         //     catch (Exception ex) { return ApiServerError("Login failed", ex); }
//         // }



//             [HttpPost("login")]
//             public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto dto)
//             {
//                 try
//                 {
//                     var email = (dto.Email ?? "").Trim().ToLower();
//                     var user  = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

//                     if (user == null)
//                         return ApiUnauthorized("Invalid email or password");

//                     if (!user.IsActive)
//                         return ApiUnauthorized("User account is deactivated. Please contact admin.");

//                     if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
//                         return ApiUnauthorized("Invalid email or password");

//                     var deviceId = dto.DeviceId?.Trim() ?? "";

//                     if (!string.IsNullOrEmpty(user.DeviceId) && user.DeviceId != deviceId)
//                         return StatusCode(403, new ApiResponse<object>
//                         {
//                             Message = "This account is already logged in on another device. Please clear device ID to login from this device.",
//                             Success = false,
//                             Errors  = new List<string> { "DEVICE_MISMATCH" }
//                         });

//                     var now       = DateTime.Now;
//                     var loginTime = new TimeSpan(now.Hour, now.Minute, 0); // ✅ only HH:mm (seconds = 0)

//                     // Update user device/session details
//                     user.DeviceId  = deviceId;
//                     user.LastSeen  = now;
//                     await _context.SaveChangesAsync();

//                     // Create token and store current token
//                     var token         = _jwtService.GenerateToken(user);
//                     user.CurrentToken = token;
//                     await _context.SaveChangesAsync();

//                     // ── ✅ Login History row INSERT ─────
//                     var loginHistory = new UserLoginHistory
//                     {
//                         UserId     = user.UserId,
//                         UserName   = user.UserName,
//                         LoginDate  = now.Date,
//                         LoginTime  = loginTime,               // ✅ only HH:mm
//                         DeviceType = dto.DeviceType?.Trim(),  // Mobile / Desktop / Web
//                         DeviceName = dto.DeviceName?.Trim(),  // Samsung S21 / Chrome on Windows
//                         CreatedAt  = now,
//                         UpdatedAt  = now
//                     };

//                     _context.UserLoginHistories.Add(loginHistory);
//                     await _context.SaveChangesAsync();
//                     // ───────────────────────────────────

//                     return ApiOk("Login successful", new LoginResponseDto
//                     {
//                         UserId         = user.UserId,
//                         UserName       = user.UserName,
//                         Email          = user.Email,
//                         Role           = user.Role,
//                         Token          = token,
//                         Message        = "Welcome back!",
//                         LoginHistoryId = loginHistory.Id
//                     });
//                 }
//                 catch (Exception ex)
//                 {
//                     return ApiServerError("Login failed", ex);
//                 }
//             }
//         // ────────────────────────────────────────────────
//         // LOGOUT  ✅ UPDATED — Login history UPDATE added
//         // ────────────────────────────────────────────────
//         // [Authorize]
//         // [HttpPost("logout")]
//         // public async Task<ActionResult<ApiResponse<string>>> Logout([FromBody] LogoutDto dto)  // ← ✅ [FromBody] LogoutDto added
//         // {
//         //     try
//         //     {
//         //         var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
//         //         if (userIdClaim == null) return ApiUnauthorized("Invalid token");

//         //         var userId = int.Parse(userIdClaim.Value);
//         //         var user   = await _context.Users.FindAsync(userId);
//         //         var now    = DateTime.Now;                 // ← ✅ NEW

//         //         if (user != null)
//         //         {
//         //             user.LastSeen     = now;
//         //             user.CurrentToken = null;
//         //             await _context.SaveChangesAsync();
//         //         }

//         //         // ── ✅ NEW: Login History row UPDATE (logout time) ──
//         //         if (dto.LoginHistoryId > 0)
//         //         {
//         //             var history = await _context.UserLoginHistories.FindAsync(dto.LoginHistoryId);
//         //             if (history != null)
//         //             {
//         //                 history.LogoutDate = now.Date;
//         //                 history.LogoutTime = now.TimeOfDay;
//         //                 history.UpdatedAt  = now;
//         //                 await _context.SaveChangesAsync();
//         //             }
//         //         }
//         //         // ───────────────────────────────────────────────────

//         //         return ApiOk("Logout successful", "Logged out");
//         //     }
//         //     catch (Exception ex) { return ApiServerError("Logout failed", ex); }
//         // }



//             // [Authorize]
//             // [HttpPost("logout")]
//             // public async Task<ActionResult<ApiResponse<string>>> Logout([FromBody] LogoutDto dto)
//             // {
//             //     try
//             //     {
//             //         var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
//             //         if (userIdClaim == null) return ApiUnauthorized("Invalid token");

//             //         var userId = int.Parse(userIdClaim.Value);
//             //         var user   = await _context.Users.FindAsync(userId);
//             //         var now    = DateTime.Now;

//             //         if (user != null)
//             //         {
//             //             user.LastSeen     = now;
//             //             user.CurrentToken = null;
//             //             await _context.SaveChangesAsync();
//             //         }

//             //         // ── Step 1: LoginHistoryId se try karo ──────────────────
//             //         bool updated = false;

//             //         if (dto != null && dto.LoginHistoryId > 0)
//             //         {
//             //             var history = await _context.UserLoginHistories
//             //                 .FindAsync(dto.LoginHistoryId);

//             //             if (history != null)
//             //             {
//             //                 history.LogoutDate = now.Date;
//             //                 history.LogoutTime = now.TimeOfDay;
//             //                 history.UpdatedAt  = now;
//             //                 await _context.SaveChangesAsync();
//             //                 updated = true;
//             //             }
//             //         }

//             //         // ── Step 2: Fallback — userId se latest active session ──
//             //         if (!updated)
//             //         {
//             //             var latestHistory = await _context.UserLoginHistories
//             //                 .Where(h => h.UserId == userId && h.LogoutDate == null)
//             //                 .OrderByDescending(h => h.CreatedAt)
//             //                 .FirstOrDefaultAsync();

//             //             if (latestHistory != null)
//             //             {
//             //                 latestHistory.LogoutDate = now.Date;
//             //                 latestHistory.LogoutTime = now.TimeOfDay;
//             //                 latestHistory.UpdatedAt  = now;
//             //                 await _context.SaveChangesAsync();
//             //             }
//             //         }

//             //         return ApiOk("Logout successful", "Logged out");
//             //     }
//             //     catch (Exception ex) { return ApiServerError("Logout failed", ex); }
//             // }


//                 [Authorize]
//                 [HttpPost("logout")]
//                 public async Task<ActionResult<ApiResponse<string>>> Logout([FromBody] LogoutDto dto)
//                 {
//                     try
//                     {
//                         var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
//                         if (userIdClaim == null) return ApiUnauthorized("Invalid token");

//                         var userId = int.Parse(userIdClaim.Value);
//                         var user   = await _context.Users.FindAsync(userId);

//                         var now        = DateTime.Now;
//                         var logoutTime = new TimeSpan(now.Hour, now.Minute, 0); // ✅ only HH:mm (seconds = 0)

//                         if (user != null)
//                         {
//                             user.LastSeen     = now;
//                             user.CurrentToken = null;
//                             await _context.SaveChangesAsync();
//                         }

//                         // ── Step 1: LoginHistoryId se try karo ──────────────────
//                         bool updated = false;

//                         if (dto != null && dto.LoginHistoryId > 0)
//                         {
//                             var history = await _context.UserLoginHistories.FindAsync(dto.LoginHistoryId);

//                             if (history != null)
//                             {
//                                 history.LogoutDate = now.Date;
//                                 history.LogoutTime = logoutTime;  // ✅ only HH:mm
//                                 history.UpdatedAt  = now;

//                                 await _context.SaveChangesAsync();
//                                 updated = true;
//                             }
//                         }

//                         // ── Step 2: Fallback — userId se latest active session ──
//                         if (!updated)
//                         {
//                             var latestHistory = await _context.UserLoginHistories
//                                 .Where(h => h.UserId == userId && h.LogoutDate == null)
//                                 .OrderByDescending(h => h.CreatedAt)
//                                 .FirstOrDefaultAsync();

//                             if (latestHistory != null)
//                             {
//                                 latestHistory.LogoutDate = now.Date;
//                                 latestHistory.LogoutTime = logoutTime; // ✅ only HH:mm
//                                 latestHistory.UpdatedAt  = now;

//                                 await _context.SaveChangesAsync();
//                             }
//                         }

//                         return ApiOk("Logout successful", "Logged out");
//                     }
//                     catch (Exception ex)
//                     {
//                         return ApiServerError("Logout failed", ex);
//                     }
//                 }
//         // ────────────────────────────────────────────────
//         // CLEAR DEVICE  (no changes)
//         // ────────────────────────────────────────────────
//         [Authorize]
//         [HttpPost("cleardevice")]
//         public async Task<ActionResult<ApiResponse<string>>> ClearDevice([FromBody] ClearDeviceDto dto)
//         {
//             try
//             {
//                 var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
//                 if (userRole?.ToLower() != "admin")
//                     return ApiForbidden("Access denied. Only admin can clear device ID.");

//                 var user = await _context.Users.FindAsync(dto.UserId);
//                 if (user == null) return ApiNotFound("User not found");

//                 user.DeviceId     = null;
//                 user.CurrentToken = null;
//                 await _context.SaveChangesAsync();

//                 return ApiOk("Device ID cleared successfully. You can now login from a different device.", "Device cleared");
//             }
//             catch (Exception ex) { return ApiServerError("Failed to clear device ID", ex); }
//         }

//         // ────────────────────────────────────────────────
//         // GET ALL USERS  (no changes)
//         // ────────────────────────────────────────────────
//         [Authorize(Roles = "admin")]
//         [HttpGet("users")]
//         public async Task<ActionResult<ApiResponse<List<UserListDto>>>> GetAllUsers()
//         {
//             try
//             {
//                 var users = await _context.Users
//                     .Select(u => new UserListDto
//                     {
//                         UserId    = u.UserId,
//                         UserName  = u.UserName,
//                         Email     = u.Email,
//                         Role      = u.Role,
//                         DeviceId  = u.DeviceId,
//                         LastSeen  = u.LastSeen,
//                         CreatedOn = u.CreatedOn,
//                         IsActive  = u.IsActive
//                     })
//                     .ToListAsync();

//                 return ApiOk("Users retrieved successfully", users);
//             }
//             catch (Exception ex) { return ApiServerError("Failed to retrieve users", ex); }
//         }

//         // ────────────────────────────────────────────────
//         // TOGGLE STATUS  (no changes)
//         // ────────────────────────────────────────────────
//         [Authorize(Roles = "admin")]
//         [HttpPut("toggle-status/{userId}")]
//         public async Task<ActionResult<ApiResponse<string>>> ToggleUserStatus(int userId)
//         {
//             try
//             {
//                 var user = await _context.Users.FindAsync(userId);
//                 if (user == null) return ApiNotFound("User not found");

//                 user.IsActive = !user.IsActive;

//                 if (!user.IsActive)
//                 {
//                     user.CurrentToken = null;
//                     user.DeviceId     = null;
//                 }

//                 await _context.SaveChangesAsync();

//                 var msg = user.IsActive
//                     ? $"User '{user.UserName}' activated successfully."
//                     : $"User '{user.UserName}' deactivated successfully.";

//                 return ApiOk(msg, user.IsActive ? "active" : "inactive");
//             }
//             catch (Exception ex) { return ApiServerError("Failed to update user status", ex); }
//         }
//     }
// }
















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
//     public class AuthController : BaseController
//     {
//         private readonly ApplicationDbContext _context;
//         private readonly IJwtService _jwtService;

//         public AuthController(ApplicationDbContext context, IJwtService jwtService)
//         {
//             _context = context;
//             _jwtService = jwtService;
//         }

//         // ✅ helper: show only HH:mm for DateTime? fields
//         private static string? ToHHmm(DateTime? dt)
//             => dt.HasValue ? dt.Value.ToString("HH:mm") : null;

//         // ✅ helper: show only HH:mm for TimeSpan? fields
//         private static string? ToHHmm(TimeSpan? ts)
//             => ts.HasValue ? ts.Value.ToString(@"hh\:mm") : null;

//         // ────────────────────────────────────────────────
//         // REGISTER
//         // ────────────────────────────────────────────────
//         [HttpPost("register")]
//         public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Register([FromBody] RegisterDto dto)
//         {
//             try
//             {
//                 var email = (dto.Email ?? "").Trim().ToLower();

//                 if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
//                     return ApiBadRequest("Email already exists");

//                 var validRoles = new[] { "admin", "manager", "supervisor", "developer", "tester", "hr", "employee" };
//                 var role = (dto.Role ?? "").Trim().ToLower();

//                 if (!validRoles.Contains(role))
//                     return ApiBadRequest("Invalid role. Role must be one of: admin, manager, supervisor, developer, tester, hr, employee");

//                 var roleId = dto.RoleId;
//                 if (roleId == 0)
//                 {
//                     var roleRecord = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == role);
//                     if (roleRecord != null) roleId = roleRecord.RoleId;
//                 }

//                 var now = DateTime.Now;

//                 var user = new User
//                 {
//                     UserName = (dto.UserName ?? "").Trim(),
//                     Email = email,
//                     PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
//                     ConfirmPassword = dto.Password,
//                     Role = role,
//                     RoleId = roleId,
//                     CreatedOn = now, // ✅ keep full datetime in DB
//                     IsActive = true
//                 };

//                 _context.Users.Add(user);
//                 await _context.SaveChangesAsync();

//                 return ApiOk("Registration successful", new LoginResponseDto
//                 {
//                     UserId = user.UserId,
//                     UserName = user.UserName,
//                     Email = user.Email,
//                     Role = user.Role,
//                     Token = "",
//                     Message = "Welcome! Please login to continue."
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return ApiServerError("Registration failed", ex);
//             }
//         }

//         // ────────────────────────────────────────────────
//         // LOGIN ✅ inserts login history, returns token
//         // ────────────────────────────────────────────────
//         [HttpPost("login")]
//         public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto dto)
//         {
//             try
//             {
//                 var email = (dto.Email ?? "").Trim().ToLower();
//                 var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

//                 if (user == null) return ApiUnauthorized("Invalid email or password");
//                 if (!user.IsActive) return ApiUnauthorized("User account is deactivated. Please contact admin.");
//                 if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) return ApiUnauthorized("Invalid email or password");

//                 var deviceId = dto.DeviceId?.Trim() ?? "";

//                 if (!string.IsNullOrEmpty(user.DeviceId) && user.DeviceId != deviceId)
//                     return StatusCode(403, new ApiResponse<object>
//                     {
//                         Message = "This account is already logged in on another device. Please clear device ID to login from this device.",
//                         Success = false,
//                         Errors = new List<string> { "DEVICE_MISMATCH" }
//                     });

//                 var now = DateTime.Now;

//                 // ✅ user table: keep full datetime in DB
//                 user.DeviceId = deviceId;
//                 user.LastSeen = now;
//                 await _context.SaveChangesAsync();

//                 // token
//                 var token = _jwtService.GenerateToken(user);
//                 user.CurrentToken = token;
//                 await _context.SaveChangesAsync();

//                 // ✅ login history time only (HH:mm stored as TIME(0) / TimeSpan)
//                 var loginTime = new TimeSpan(now.Hour, now.Minute, 0);

//                 var loginHistory = new UserLoginHistory
//                 {
//                     UserId = user.UserId,
//                     UserName = user.UserName,
//                     LoginDate = now.Date,
//                     LoginTime = loginTime,
//                     DeviceType = dto.DeviceType?.Trim(),
//                     DeviceName = dto.DeviceName?.Trim(),
//                     CreatedAt = now,  // full datetime (audit)
//                     UpdatedAt = now
//                 };

//                 _context.UserLoginHistories.Add(loginHistory);
//                 await _context.SaveChangesAsync();

//                 // ✅ IMPORTANT: LoginHistoryId is NOT returned to client
//                 return ApiOk("Login successful", new LoginResponseDto
//                 {
//                     UserId = user.UserId,
//                     UserName = user.UserName,
//                     Email = user.Email,
//                     Role = user.Role,
//                     Token = token,
//                     Message = "Welcome back!"
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return ApiServerError("Login failed", ex);
//             }
//         }

//         // ────────────────────────────────────────────────
//         // LOGOUT ✅ only by userId from JWT (no request body)
//         // ────────────────────────────────────────────────
//         [Authorize]
//         [HttpPost("logout")]
//         public async Task<ActionResult<ApiResponse<string>>> Logout()
//         {
//             try
//             {
//                 var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
//                 if (userIdClaim == null) return ApiUnauthorized("Invalid token");

//                 var userId = int.Parse(userIdClaim.Value);
//                 var user = await _context.Users.FindAsync(userId);

//                 var now = DateTime.Now;
//                 var logoutTime = new TimeSpan(now.Hour, now.Minute, 0);

//                 // ✅ Update user token session
//                 if (user != null)
//                 {
//                     user.LastSeen = now;
//                     user.CurrentToken = null;

//                     // If you want logout to allow login from another device without admin:
//                     // user.DeviceId = null;

//                     await _context.SaveChangesAsync();
//                 }

//                 // ✅ Update latest active login history for this user
//                 var latestHistory = await _context.UserLoginHistories
//                     .Where(h => h.UserId == userId && h.LogoutDate == null)
//                     .OrderByDescending(h => h.CreatedAt)
//                     .FirstOrDefaultAsync();

//                 if (latestHistory != null)
//                 {
//                     latestHistory.LogoutDate = now.Date;
//                     latestHistory.LogoutTime = logoutTime;
//                     latestHistory.UpdatedAt = now;
//                     await _context.SaveChangesAsync();
//                 }

//                 return ApiOk("Logout successful", "Logged out");
//             }
//             catch (Exception ex)
//             {
//                 return ApiServerError("Logout failed", ex);
//             }
//         }

//         // ────────────────────────────────────────────────
//         // CLEAR DEVICE (no change)
//         // ────────────────────────────────────────────────
//         [Authorize]
//         [HttpPost("cleardevice")]
//         public async Task<ActionResult<ApiResponse<string>>> ClearDevice([FromBody] ClearDeviceDto dto)
//         {
//             try
//             {
//                 var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
//                 if (userRole?.ToLower() != "admin")
//                     return ApiForbidden("Access denied. Only admin can clear device ID.");

//                 var user = await _context.Users.FindAsync(dto.UserId);
//                 if (user == null) return ApiNotFound("User not found");

//                 user.DeviceId = null;
//                 user.CurrentToken = null;
//                 await _context.SaveChangesAsync();

//                 return ApiOk("Device ID cleared successfully. You can now login from a different device.", "Device cleared");
//             }
//             catch (Exception ex)
//             {
//                 return ApiServerError("Failed to clear device ID", ex);
//             }
//         }

//         // ────────────────────────────────────────────────
//         // GET ALL USERS ✅ returns only HH:mm (DISPLAY)
//         // ────────────────────────────────────────────────
//         [Authorize(Roles = "admin")]
//         [HttpGet("users")]
//         public async Task<ActionResult<ApiResponse<object>>> GetAllUsers()
//         {
//             try
//             {
//                 var users = await _context.Users.ToListAsync();

//                 var result = users.Select(u => new
//                 {
//                     u.UserId,
//                     u.UserName,
//                     u.Email,
//                     u.Role,
//                     u.DeviceId,
//                     LastSeen = ToHHmm(u.LastSeen),
//                     CreatedOn = ToHHmm(u.CreatedOn),
//                     u.IsActive
//                 });

//                 return ApiOk("Users retrieved successfully", result);
//             }
//             catch (Exception ex)
//             {
//                 return ApiServerError("Failed to retrieve users", ex);
//             }
//         }

//         // ────────────────────────────────────────────────
//         // GET LOGIN HISTORY ✅ show HH:mm for times
//         // ────────────────────────────────────────────────
//         // [Authorize(Roles = "admin")]
//         // [HttpGet("login-history")]
//         // public async Task<ActionResult<ApiResponse<object>>> GetLoginHistory([FromQuery] int userId)
//         // {
//         //     try
//         //     {
//         //         var q = _context.UserLoginHistories.AsQueryable();
//         //         if (userId > 0) q = q.Where(x => x.UserId == userId);

//         //         var list = await q
//         //             .OrderByDescending(x => x.CreatedAt)
//         //             .Take(200)
//         //             .ToListAsync();

//         //         var result = list.Select(h => new
//         //         {
//         //             h.Id,
//         //             h.UserId,
//         //             h.UserName,
//         //             LoginDate = h.LoginDate,
//         //             LoginTime = ToHHmm(h.LoginTime),
//         //             LogoutDate = h.LogoutDate,
//         //             LogoutTime = ToHHmm(h.LogoutTime),
//         //             h.DeviceType,
//         //             h.DeviceName,
//         //             CreatedAt = h.CreatedAt,
//         //             UpdatedAt = h.UpdatedAt
//         //         });

//         //         return ApiOk("Login history retrieved successfully", result);
//         //     }
//         //     catch (Exception ex)
//         //     {
//         //         return ApiServerError("Failed to retrieve login history", ex);
//         //     }
//         // }

//         // // ────────────────────────────────────────────────
//         // // TOGGLE STATUS
//         // // ────────────────────────────────────────────────
//         // [Authorize(Roles = "admin")]
//         // [HttpPut("toggle-status/{userId}")]
//         // public async Task<ActionResult<ApiResponse<string>>> ToggleUserStatus(int userId)
//         // {
//         //     try
//         //     {
//         //         var user = await _context.Users.FindAsync(userId);
//         //         if (user == null) return ApiNotFound("User not found");

//         //         user.IsActive = !user.IsActive;

//         //         if (!user.IsActive)
//         //         {
//         //             user.CurrentToken = null;
//         //             user.DeviceId = null;
//         //         }

//         //         await _context.SaveChangesAsync();

//         //         var msg = user.IsActive
//         //             ? $"User '{user.UserName}' activated successfully."
//         //             : $"User '{user.UserName}' deactivated successfully.";

//         //         return ApiOk(msg, user.IsActive ? "active" : "inactive");
//         //     }
//         //     catch (Exception ex)
//         //     {
//         //         return ApiServerError("Failed to update user status", ex);
//         //     }
//         // }
//     }
// }









// // AuthController.cs  ✅ FULL CODE (Only DeviceType, no DeviceName)

// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
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
//     public class AuthController : BaseController
//     {
//         private readonly ApplicationDbContext _context;
//         private readonly IJwtService _jwtService;

//         public AuthController(ApplicationDbContext context, IJwtService jwtService)
//         {
//             _context = context;
//             _jwtService = jwtService;
//         }

//         // ✅ helper: show only HH:mm for DateTime? fields
//         private static string? ToHHmm(DateTime? dt)
//             => dt.HasValue ? dt.Value.ToString("HH:mm") : null;

//         // ✅ helper: show only HH:mm for TimeSpan? fields
//         private static string? ToHHmm(TimeSpan? ts)
//             => ts.HasValue ? ts.Value.ToString(@"hh\:mm") : null;

//         // ────────────────────────────────────────────────
//         // REGISTER
//         // ────────────────────────────────────────────────
//         [HttpPost("register")]
//         public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Register([FromBody] RegisterDto dto)
//         {
//             try
//             {
//                 var email = (dto.Email ?? "").Trim().ToLower();

//                 if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
//                     return ApiBadRequest("Email already exists");

//                 var validRoles = new[] { "admin", "manager", "supervisor", "developer", "tester", "hr", "employee" };
//                 var role = (dto.Role ?? "").Trim().ToLower();

//                 if (!validRoles.Contains(role))
//                     return ApiBadRequest("Invalid role. Role must be one of: admin, manager, supervisor, developer, tester, hr, employee");

//                 var roleId = dto.RoleId;
//                 if (roleId == 0)
//                 {
//                     var roleRecord = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == role);
//                     if (roleRecord != null) roleId = roleRecord.RoleId;
//                 }

//                 var now = DateTime.Now;

//                 var user = new User
//                 {
//                     UserName = (dto.UserName ?? "").Trim(),
//                     Email = email,
//                     PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
//                     ConfirmPassword = dto.Password,
//                     Role = role,
//                     RoleId = roleId,
//                     CreatedOn = now,
//                     IsActive = true,

//                     // ✅ DeviceType will remain null on register
//                     DeviceType = null
//                 };

//                 _context.Users.Add(user);
//                 await _context.SaveChangesAsync();

//                 return ApiOk("Registration successful", new LoginResponseDto
//                 {
//                     UserId = user.UserId,
//                     UserName = user.UserName,
//                     Email = user.Email,
//                     Role = user.Role,
//                     Token = "",
//                     Message = "Welcome! Please login to continue.",
//                     DeviceType = user.DeviceType
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return ApiServerError("Registration failed", ex);
//             }
//         }

//         // ────────────────────────────────────────────────
//         // LOGIN ✅ inserts login history, returns token
//         // ────────────────────────────────────────────────
//         [HttpPost("login")]
//         public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto dto)
//         {
//             try
//             {
//                 var email = (dto.Email ?? "").Trim().ToLower();
//                 var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

//                 if (user == null) return ApiUnauthorized("Invalid email or password");
//                 if (!user.IsActive) return ApiUnauthorized("User account is deactivated. Please contact admin.");
//                 if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
//                     return ApiUnauthorized("Invalid email or password");

//                 var deviceId = dto.DeviceId?.Trim() ?? "";
//                 var deviceType = dto.DeviceType?.Trim(); // ✅ ONLY THIS

//                 // ✅ enforce single-device binding
//                 if (!string.IsNullOrEmpty(user.DeviceId) && user.DeviceId != deviceId)
//                 {
//                     return StatusCode(403, new ApiResponse<object>
//                     {
//                         Message = "This account is already logged in on another device. Please clear device ID to login from this device.",
//                         Success = false,
//                         Errors = new List<string> { "DEVICE_MISMATCH" }
//                     });
//                 }

//                 var now = DateTime.Now;

//                 // ✅ update user current session/device
//                 user.DeviceId = deviceId;
//                 user.DeviceType = deviceType;
//                 user.LastSeen = now;
//                 await _context.SaveChangesAsync();

//                 // token
//                 var token = _jwtService.GenerateToken(user);
//                 user.CurrentToken = token;
//                 await _context.SaveChangesAsync();

//                 // ✅ login history time only (HH:mm stored as TIME(0) / TimeSpan)
//                 var loginTime = new TimeSpan(now.Hour, now.Minute, 0);

//                 var loginHistory = new UserLoginHistory
//                 {
//                     UserId = user.UserId,
//                     UserName = user.UserName,
//                     LoginDate = now.Date,
//                     LoginTime = loginTime,
//                     DeviceType = deviceType, // ✅ stored here also
//                     CreatedAt = now,
//                     UpdatedAt = now
//                 };

//                 _context.UserLoginHistories.Add(loginHistory);
//                 await _context.SaveChangesAsync();

//                 // ✅ response includes DeviceType
//                 return ApiOk("Login successful", new LoginResponseDto
//                 {
//                     UserId = user.UserId,
//                     UserName = user.UserName,
//                     Email = user.Email,
//                     Role = user.Role,
//                     Token = token,
//                     Message = "Welcome back!",
//                     DeviceType = user.DeviceType
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return ApiServerError("Login failed", ex);
//             }
//         }

//         // ────────────────────────────────────────────────
//         // LOGOUT ✅ only by userId from JWT (no request body)
//         // ────────────────────────────────────────────────
//         [Authorize]
//         [HttpPost("logout")]
//         public async Task<ActionResult<ApiResponse<string>>> Logout()
//         {
//             try
//             {
//                 var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
//                 if (userIdClaim == null) return ApiUnauthorized("Invalid token");

//                 var userId = int.Parse(userIdClaim.Value);
//                 var user = await _context.Users.FindAsync(userId);

//                 var now = DateTime.Now;
//                 var logoutTime = new TimeSpan(now.Hour, now.Minute, 0);

//                 // ✅ Update user token session
//                 if (user != null)
//                 {
//                     user.LastSeen = now;
//                     user.CurrentToken = null;

//                     // OPTIONAL:
//                     // If you want logout to allow login from another device without admin:
//                     // user.DeviceId = null;
//                     // user.DeviceType = null;

//                     await _context.SaveChangesAsync();
//                 }

//                 // ✅ Update latest active login history for this user
//                 var latestHistory = await _context.UserLoginHistories
//                     .Where(h => h.UserId == userId && h.LogoutDate == null)
//                     .OrderByDescending(h => h.CreatedAt)
//                     .FirstOrDefaultAsync();

//                 if (latestHistory != null)
//                 {
//                     latestHistory.LogoutDate = now.Date;
//                     latestHistory.LogoutTime = logoutTime;
//                     latestHistory.UpdatedAt = now;
//                     await _context.SaveChangesAsync();
//                 }

//                 return ApiOk("Logout successful", "Logged out");
//             }
//             catch (Exception ex)
//             {
//                 return ApiServerError("Logout failed", ex);
//             }
//         }

//         // ────────────────────────────────────────────────
//         // CLEAR DEVICE ✅ only admin
//         // ────────────────────────────────────────────────
//         [Authorize]
//         [HttpPost("cleardevice")]
//         public async Task<ActionResult<ApiResponse<string>>> ClearDevice([FromBody] ClearDeviceDto dto)
//         {
//             try
//             {
//                 var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
//                 if (userRole?.ToLower() != "admin")
//                     return ApiForbidden("Access denied. Only admin can clear device ID.");

//                 var user = await _context.Users.FindAsync(dto.UserId);
//                 if (user == null) return ApiNotFound("User not found");

//                 user.DeviceId = null;
//                 user.DeviceType = null;
//                 user.CurrentToken = null;

//                 await _context.SaveChangesAsync();

//                 return ApiOk("Device ID cleared successfully. You can now login from a different device.", "Device cleared");
//             }
//             catch (Exception ex)
//             {
//                 return ApiServerError("Failed to clear device ID", ex);
//             }
//         }

//         // ────────────────────────────────────────────────
//         // GET ALL USERS ✅ returns only HH:mm (DISPLAY)
//         // ────────────────────────────────────────────────
//         [Authorize(Roles = "admin")]
//         [HttpGet("users")]
//         public async Task<ActionResult<ApiResponse<object>>> GetAllUsers()
//         {
//             try
//             {
//                 var users = await _context.Users.ToListAsync();

//                 var result = users.Select(u => new
//                 {
//                     u.UserId,
//                     u.UserName,
//                     u.Email,
//                     u.Role,
//                     u.DeviceId,
//                     u.DeviceType,
//                     LastSeen = ToHHmm(u.LastSeen),
//                     CreatedOn = ToHHmm(u.CreatedOn),
//                     u.IsActive
//                 });

//                 return ApiOk("Users retrieved successfully", result);
//             }
//             catch (Exception ex)
//             {
//                 return ApiServerError("Failed to retrieve users", ex);
//             }
//         }
//     }
// }










// AuthController.cs  ✅ FULL CODE

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
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

        private static string? ToHHmm(DateTime? dt)
            => dt.HasValue ? dt.Value.ToString("HH:mm") : null;

        private static string? ToHHmm(TimeSpan? ts)
            => ts.HasValue ? ts.Value.ToString(@"hh\:mm") : null;

        // ────────────────────────────────────────────────
        // REGISTER
        // ────────────────────────────────────────────────
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var email = (dto.Email ?? "").Trim().ToLower();

                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
                    return ApiBadRequest("Email already exists");

                var validRoles = new[] { "admin", "manager", "supervisor", "developer", "tester", "hr", "employee" };
                var role = (dto.Role ?? "").Trim().ToLower();

                if (!validRoles.Contains(role))
                    return ApiBadRequest("Invalid role. Role must be one of: admin, manager, supervisor, developer, tester, hr, employee");

                var roleId = dto.RoleId;
                if (roleId == 0)
                {
                    var roleRecord = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == role);
                    if (roleRecord != null) roleId = roleRecord.RoleId;
                }

                var now = DateTime.Now;
                var user = new User
                {
                    UserName        = (dto.UserName ?? "").Trim(),
                    Email           = email,
                    PasswordHash    = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    ConfirmPassword = dto.Password,
                    Role            = role,
                    RoleId          = roleId,
                    CreatedOn       = now,
                    IsActive        = true,
                    DeviceType      = null
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return ApiOk("Registration successful", new LoginResponseDto
                {
                    UserId     = user.UserId,
                    UserName   = user.UserName,
                    Email      = user.Email,
                    Role       = user.Role,
                    Token      = "",
                    Message    = "Welcome! Please login to continue.",
                    DeviceType = user.DeviceType
                });
            }
            catch (Exception ex) { return ApiServerError("Registration failed", ex); }
        }

        // ────────────────────────────────────────────────
        // LOGIN ✅ TokenExpiresAt save hoga
        // ────────────────────────────────────────────────
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto dto)
        {
            try
            {
                var email = (dto.Email ?? "").Trim().ToLower();
                var user  = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

                if (user == null) return ApiUnauthorized("Invalid email or password");
                if (!user.IsActive) return ApiUnauthorized("User account is deactivated. Please contact admin.");
                if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                    return ApiUnauthorized("Invalid email or password");

                var deviceId   = dto.DeviceId?.Trim() ?? "";
                var deviceType = dto.DeviceType?.Trim();

                if (!string.IsNullOrEmpty(user.DeviceId) && user.DeviceId != deviceId)
                {
                    return StatusCode(403, new ApiResponse<object>
                    {
                        Message = "This account is already logged in on another device. Please clear device ID to login from this device.",
                        Success = false,
                        Errors  = new List<string> { "DEVICE_MISMATCH" }
                    });
                }

                var now = DateTime.Now;

                user.DeviceId   = deviceId;
                user.DeviceType = deviceType;
                user.LastSeen   = now;
                await _context.SaveChangesAsync();

                var token = _jwtService.GenerateToken(user);
                user.CurrentToken = token;
                await _context.SaveChangesAsync();

                // ✅ JWT se TokenExpiresAt nikalo
                var jwtToken       = new JwtSecurityTokenHandler().ReadJwtToken(token);
                var tokenExpiresAt = jwtToken.ValidTo.ToLocalTime();

                var loginHistory = new UserLoginHistory
                {
                    UserId         = user.UserId,
                    UserName       = user.UserName,
                    LoginDate      = now.Date,
                    LoginTime      = new TimeSpan(now.Hour, now.Minute, now.Second),
                    DeviceType     = deviceType,
                    TokenExpiresAt = tokenExpiresAt,   // ✅ NEW
                    CreatedAt      = now,
                    UpdatedAt      = now
                };

                _context.UserLoginHistories.Add(loginHistory);
                await _context.SaveChangesAsync();

                return ApiOk("Login successful", new LoginResponseDto
                {
                    UserId     = user.UserId,
                    UserName   = user.UserName,
                    Email      = user.Email,
                    Role       = user.Role,
                    Token      = token,
                    Message    = "Welcome back!",
                    DeviceType = user.DeviceType
                });
            }
            catch (Exception ex) { return ApiServerError("Login failed", ex); }
        }

        // ────────────────────────────────────────────────
        // LOGOUT ✅ TotalMinutes + LogoutReason save hoga
        // ────────────────────────────────────────────────
        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<string>>> Logout()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return ApiUnauthorized("Invalid token");

                var userId = int.Parse(userIdClaim.Value);
                var user   = await _context.Users.FindAsync(userId);

                var now = DateTime.Now;

                if (user != null)
                {
                    user.LastSeen     = now;
                    user.CurrentToken = null;
                    await _context.SaveChangesAsync();
                }

                // ✅ Active session dhundo
                var latestHistory = await _context.UserLoginHistories
                    .Where(h => h.UserId == userId && h.LogoutDate == null)
                    .OrderByDescending(h => h.CreatedAt)
                    .FirstOrDefaultAsync();

                if (latestHistory != null)
                {
                    latestHistory.LogoutDate   = now.Date;
                    latestHistory.LogoutTime   = new TimeSpan(now.Hour, now.Minute, now.Second);
                    latestHistory.LogoutReason = "manual";   // ✅ NEW
                    latestHistory.UpdatedAt    = now;

                    // ✅ TotalMinutes calculate karo
                    if (latestHistory.LoginDate.HasValue && latestHistory.LoginTime.HasValue)
                    {
                        var loginDt = latestHistory.LoginDate.Value
                                        .Add(latestHistory.LoginTime.Value);
                        latestHistory.TotalMinutes = (int)(now - loginDt).TotalMinutes;
                    }

                    await _context.SaveChangesAsync();
                }

                return ApiOk("Logout successful", "Logged out");
            }
            catch (Exception ex) { return ApiServerError("Logout failed", ex); }
        }

        // ────────────────────────────────────────────────
        // CLEAR DEVICE ✅ LogoutReason = "device_cleared"
        // ────────────────────────────────────────────────
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

                var now = DateTime.Now;

                // ✅ Active session close karo
                var activeSession = await _context.UserLoginHistories
                    .Where(h => h.UserId == dto.UserId && h.LogoutDate == null)
                    .OrderByDescending(h => h.CreatedAt)
                    .FirstOrDefaultAsync();

                if (activeSession != null)
                {
                    activeSession.LogoutDate   = now.Date;
                    activeSession.LogoutTime   = new TimeSpan(now.Hour, now.Minute, now.Second);
                    activeSession.LogoutReason = "device_cleared";   // ✅ NEW
                    activeSession.UpdatedAt    = now;

                    if (activeSession.LoginDate.HasValue && activeSession.LoginTime.HasValue)
                    {
                        var loginDt = activeSession.LoginDate.Value
                                        .Add(activeSession.LoginTime.Value);
                        activeSession.TotalMinutes = (int)(now - loginDt).TotalMinutes;
                    }
                }

                user.DeviceId     = null;
                user.DeviceType   = null;
                user.CurrentToken = null;

                await _context.SaveChangesAsync();

                return ApiOk("Device cleared and session logged out.", "Device cleared");
            }
            catch (Exception ex) { return ApiServerError("Failed to clear device ID", ex); }
        }

        // ────────────────────────────────────────────────
        // GET ALL USERS
        // ────────────────────────────────────────────────
        [Authorize(Roles = "admin")]
        [HttpGet("users")]
        public async Task<ActionResult<ApiResponse<object>>> GetAllUsers()
        {
            try
            {
                var users  = await _context.Users.ToListAsync();
                var result = users.Select(u => new
                {
                    u.UserId,
                    u.UserName,
                    u.Email,
                    u.Role,
                    u.DeviceId,
                    u.DeviceType,
                    LastSeen  = ToHHmm(u.LastSeen),
                    CreatedOn = ToHHmm(u.CreatedOn),
                    u.IsActive
                });

                return ApiOk("Users retrieved successfully", result);
            }
            catch (Exception ex) { return ApiServerError("Failed to retrieve users", ex); }
        }
    }
}