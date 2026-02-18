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
//         public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Register(RegisterDto dto)
//         {
//             try
//             {
//                 // Check if email already exists
//                 if (await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
//                 {
//                     return BadRequest(new ApiResponse<LoginResponseDto>
//                     {
//                         Success = false,
//                         Message = "Email already exists"
//                     });
//                 }

//                 // Validate role (must be lowercase)
//                 var validRoles = new[] { "admin", "manager", "supervisor", "developer", "tester", "hr", "employee" };
//                 if (!validRoles.Contains(dto.Role.ToLower()))
//                 {
//                     return BadRequest(new ApiResponse<LoginResponseDto>
//                     {
//                         Success = false,
//                         Message = "Invalid role. Role must be one of: admin, manager, supervisor, developer, tester, hr, employee"
//                     });
//                 }

//                 // Create new user
//                 var user = new User
//                 {
//                     UserName = dto.UserName,
//                     Email = dto.Email.ToLower(),
//                     PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
//                     Role = dto.Role.ToLower(),
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
//                         // Token = null, // No token on registration
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
//         public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login(LoginDto dto)
//         {
//             try
//             {
//                 // Find user by email
//                 var user = await _context.Users
//                     .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

//                 if (user == null)
//                 {
//                     return Unauthorized(new ApiResponse<LoginResponseDto>
//                     {
//                         Success = false,
//                         Message = "Invalid email or password"
//                     });
//                 }

//                 // Check if user is active
//                 if (!user.IsActive)
//                 {
//                     return Unauthorized(new ApiResponse<LoginResponseDto>
//                     {
//                         Success = false,
//                         Message = "User account is deactivated. Please contact admin."
//                     });
//                 }

//                 // Verify password
//                 if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
//                 {
//                     return Unauthorized(new ApiResponse<LoginResponseDto>
//                     {
//                         Success = false,
//                         Message = "Invalid email or password"
//                     });
//                 }

//                 // Check device binding
//                 if (!string.IsNullOrEmpty(user.DeviceId) && user.DeviceId != dto.DeviceId)
//                 {
//                     return StatusCode(403, new ApiResponse<LoginResponseDto>
//                     {
//                         Success = false,
//                         Message = "This account is already logged in on another device. Please clear device ID to login from this device.",
//                         Errors = new List<string> { "DEVICE_MISMATCH" }
//                     });
//                 }

//                 // Update device ID and last seen
//                 user.DeviceId = dto.DeviceId;
//                 user.LastSeen = DateTime.Now;
//                 await _context.SaveChangesAsync();

//                 // Generate token
//                 var token = _jwtService.GenerateToken(user);

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

//         [HttpPost("cleardevice")]
//         public async Task<ActionResult<ApiResponse<string>>> ClearDevice(ClearDeviceDto dto)
//         {
//             try
//             {
//                 var user = await _context.Users.FindAsync(dto.UserId);

//                 if (user == null)
//                 {
//                     return NotFound(new ApiResponse<string>
//                     {
//                         Success = false,
//                         Message = "User not found"
//                     });
//                 }

//                 // Clear device ID
//                 user.DeviceId = null;
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
//                     // Clear device ID on logout
//                     // user.DeviceId = null;
//                     user.LastSeen = DateTime.Now;
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
//         public async Task<ActionResult<ApiResponse<List<User>>>> GetAllUsers()
//         {
//             try
//             {
//                 var users = await _context.Users
//                     .Select(u => new User
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

//                 return Ok(new ApiResponse<List<User>>
//                 {
//                     Success = true,
//                     Message = "Users retrieved successfully",
//                     Data = users
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<List<User>>
//                 {
//                     Success = false,
//                     Message = "Failed to retrieve users",
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

//                 // Check if email already exists
//                 if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
//                 {
//                     return BadRequest(new ApiResponse<LoginResponseDto>
//                     {
//                         Success = false,
//                         Message = "Email already exists"
//                     });
//                 }

//                 // Validate role (must be lowercase)
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

//                 // Create new user
//                 var user = new User
//                 {
//                     UserName = dto.UserName.Trim(),
//                     Email = email,
//                     PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
//                     Role = role,
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

//                 // Find user by email
//                 var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

//                 if (user == null)
//                 {
//                     return Unauthorized(new ApiResponse<LoginResponseDto>
//                     {
//                         Success = false,
//                         Message = "Invalid email or password"
//                     });
//                 }

//                 // Check if user is active
//                 if (!user.IsActive)
//                 {
//                     return Unauthorized(new ApiResponse<LoginResponseDto>
//                     {
//                         Success = false,
//                         Message = "User account is deactivated. Please contact admin."
//                     });
//                 }

//                 // Verify password
//                 if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
//                 {
//                     return Unauthorized(new ApiResponse<LoginResponseDto>
//                     {
//                         Success = false,
//                         Message = "Invalid email or password"
//                     });
//                 }

//                 // Device binding (optional)
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

//                 // Update device ID and last seen
//                 user.DeviceId = deviceId;
//                 user.LastSeen = DateTime.Now;
//                 await _context.SaveChangesAsync();

//                 // Generate token
//                 var token = _jwtService.GenerateToken(user);

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

//         [HttpPost("cleardevice")]
//         public async Task<ActionResult<ApiResponse<string>>> ClearDevice([FromBody] ClearDeviceDto dto)
//         {
//             try
//             {
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
//                     // If you want: user.DeviceId = null;
//                     user.LastSeen = DateTime.Now;
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
    public class AuthController : ControllerBase
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

                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
                {
                    return BadRequest(new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Email already exists"
                    });
                }

                // Validate role (must be lowercase)
                var validRoles = new[] { "admin", "manager", "supervisor", "developer", "tester", "hr", "employee" };
                var role = dto.Role.Trim().ToLower();

                if (!validRoles.Contains(role))
                {
                    return BadRequest(new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Invalid role. Role must be one of: admin, manager, supervisor, developer, tester, hr, employee"
                    });
                }

                // If RoleId not provided, fetch from DB by role name
                var roleId = dto.RoleId;
                if (roleId == 0)
                {
                    var roleRecord = await _context.Roles
                        .FirstOrDefaultAsync(r => r.RoleName.ToLower() == role);
                    if (roleRecord != null)
                        roleId = roleRecord.RoleId;
                }

                // Create new user
                var user = new User
                {
                    UserName = dto.UserName.Trim(),
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    ConfirmPassword = dto.Password, // ✅ plain password save hoga
                    Role = role,
                    RoleId = roleId,
                    CreatedOn = DateTime.Now,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<LoginResponseDto>
                {
                    Success = true,
                    Message = "Registration successful",
                    Data = new LoginResponseDto
                    {
                        UserId = user.UserId,
                        UserName = user.UserName,
                        Email = user.Email,
                        Role = user.Role,
                        Token = "",
                        Message = "Welcome! Please login to continue."
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = "Registration failed",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto dto)
        {
            try
            {
                var email = dto.Email.Trim().ToLower();

                // Find user by email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

                if (user == null)
                {
                    return Unauthorized(new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    });
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    return Unauthorized(new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "User account is deactivated. Please contact admin."
                    });
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                {
                    return Unauthorized(new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    });
                }

                // Device binding
                var deviceId = dto.DeviceId?.Trim() ?? "";
                if (!string.IsNullOrEmpty(user.DeviceId) && user.DeviceId != deviceId)
                {
                    return StatusCode(403, new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "This account is already logged in on another device. Please clear device ID to login from this device.",
                        Errors = new List<string> { "DEVICE_MISMATCH" }
                    });
                }

                // Update device ID and last seen
                user.DeviceId = deviceId;
                user.LastSeen = DateTime.Now;
                await _context.SaveChangesAsync();

                // Generate token
                var token = _jwtService.GenerateToken(user);

                return Ok(new ApiResponse<LoginResponseDto>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = new LoginResponseDto
                    {
                        UserId = user.UserId,
                        UserName = user.UserName,
                        Email = user.Email,
                        Role = user.Role,
                        Token = token,
                        Message = "Welcome back!"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = "Login failed",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost("cleardevice")]
        public async Task<ActionResult<ApiResponse<string>>> ClearDevice([FromBody] ClearDeviceDto dto)
        {
            try
            {
                var user = await _context.Users.FindAsync(dto.UserId);

                if (user == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                user.DeviceId = null;
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Device ID cleared successfully. You can now login from a different device.",
                    Data = "Device cleared"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Failed to clear device ID",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<string>>> Logout()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Invalid token"
                    });
                }

                var userId = int.Parse(userIdClaim.Value);
                var user = await _context.Users.FindAsync(userId);

                if (user != null)
                {
                    user.LastSeen = DateTime.Now;
                    await _context.SaveChangesAsync();
                }

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Logout successful",
                    Data = "Logged out"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Logout failed",
                    Errors = new List<string> { ex.Message }
                });
            }
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
                        UserId = u.UserId,
                        UserName = u.UserName,
                        Email = u.Email,
                        Role = u.Role,
                        DeviceId = u.DeviceId,
                        LastSeen = u.LastSeen,
                        CreatedOn = u.CreatedOn,
                        IsActive = u.IsActive
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<UserListDto>>
                {
                    Success = true,
                    Message = "Users retrieved successfully",
                    Data = users
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<UserListDto>>
                {
                    Success = false,
                    Message = "Failed to retrieve users",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}