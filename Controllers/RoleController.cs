// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using attendance_api.Data;
// using attendance_api.DTOs;
// using attendance_api.Models;

// namespace attendance_api.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     [Authorize(Roles = "admin")]
//     public class RoleController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;

//         public RoleController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         // GET: api/role
//         [HttpGet]
//         public async Task<ActionResult<ApiResponse<List<RoleListDto>>>> GetAllRoles()
//         {
//             try
//             {
//                 var roles = await _context.Roles
//                     .OrderBy(r => r.RoleName)
//                     .Select(r => new RoleListDto
//                     {
//                         RoleId = r.RoleId,
//                         RoleName = r.RoleName,
//                         Description = r.Description,
//                         RequiresSelfie = r.RequiresSelfie,
//                         IsActive = r.IsActive,
//                         CreatedOn = r.CreatedOn
//                     })
//                     .ToListAsync();

//                 return Ok(new ApiResponse<List<RoleListDto>>
//                 {
//                     Success = true,
//                     Message = "Roles retrieved successfully",
//                     Data = roles
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<List<RoleListDto>>
//                 {
//                     Success = false,
//                     Message = "Failed to retrieve roles",
//                     Errors = new List<string> { ex.Message }
//                 });
//             }
//         }

//         // GET: api/role/{id}
//         [HttpGet("{id}")]
//         public async Task<ActionResult<ApiResponse<RoleListDto>>> GetRole(int id)
//         {
//             try
//             {
//                 var role = await _context.Roles.FindAsync(id);

//                 if (role == null)
//                 {
//                     return NotFound(new ApiResponse<RoleListDto>
//                     {
//                         Success = false,
//                         Message = "Role not found"
//                     });
//                 }

//                 var roleDto = new RoleListDto
//                 {
//                     RoleId = role.RoleId,
//                     RoleName = role.RoleName,
//                     Description = role.Description,
//                     RequiresSelfie = role.RequiresSelfie,
//                     IsActive = role.IsActive,
//                     CreatedOn = role.CreatedOn
//                 };

//                 return Ok(new ApiResponse<RoleListDto>
//                 {
//                     Success = true,
//                     Message = "Role retrieved successfully",
//                     Data = roleDto
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<RoleListDto>
//                 {
//                     Success = false,
//                     Message = "Failed to retrieve role",
//                     Errors = new List<string> { ex.Message }
//                 });
//             }
//         }

//         // POST: api/role
//         [HttpPost]
//         public async Task<ActionResult<ApiResponse<RoleListDto>>> CreateRole([FromBody] RoleDto dto)
//         {
//             try
//             {
//                 // Check if role name already exists
//                 var existingRole = await _context.Roles
//                     .FirstOrDefaultAsync(r => r.RoleName.ToLower() == dto.RoleName.ToLower());

//                 if (existingRole != null)
//                 {
//                     return BadRequest(new ApiResponse<RoleListDto>
//                     {
//                         Success = false,
//                         Message = "Role name already exists"
//                     });
//                 }

//                 var role = new Role
//                 {
//                     RoleName = dto.RoleName,
//                     Description = dto.Description,
//                     RequiresSelfie = dto.RequiresSelfie,
//                     IsActive = dto.IsActive,
//                     CreatedOn = DateTime.Now
//                 };

//                 _context.Roles.Add(role);
//                 await _context.SaveChangesAsync();

//                 var responseDto = new RoleListDto
//                 {
//                     RoleId = role.RoleId,
//                     RoleName = role.RoleName,
//                     Description = role.Description,
//                     RequiresSelfie = role.RequiresSelfie,
//                     IsActive = role.IsActive,
//                     CreatedOn = role.CreatedOn
//                 };

//                 return CreatedAtAction(nameof(GetRole), new { id = role.RoleId }, new ApiResponse<RoleListDto>
//                 {
//                     Success = true,
//                     Message = "Role created successfully",
//                     Data = responseDto
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<RoleListDto>
//                 {
//                     Success = false,
//                     Message = "Failed to create role",
//                     Errors = new List<string> { ex.Message }
//                 });
//             }
//         }

//         // PUT: api/role/{id}
//         [HttpPut("{id}")]
//         public async Task<ActionResult<ApiResponse<RoleListDto>>> UpdateRole(int id, [FromBody] RoleDto dto)
//         {
//             try
//             {
//                 var role = await _context.Roles.FindAsync(id);

//                 if (role == null)
//                 {
//                     return NotFound(new ApiResponse<RoleListDto>
//                     {
//                         Success = false,
//                         Message = "Role not found"
//                     });
//                 }

//                 // Check if new role name conflicts with existing role
//                 var existingRole = await _context.Roles
//                     .FirstOrDefaultAsync(r => r.RoleName.ToLower() == dto.RoleName.ToLower() && r.RoleId != id);

//                 if (existingRole != null)
//                 {
//                     return BadRequest(new ApiResponse<RoleListDto>
//                     {
//                         Success = false,
//                         Message = "Role name already exists"
//                     });
//                 }

//                 role.RoleName = dto.RoleName;
//                 role.Description = dto.Description;
//                 role.RequiresSelfie = dto.RequiresSelfie;
//                 role.IsActive = dto.IsActive;

//                 await _context.SaveChangesAsync();

//                 var responseDto = new RoleListDto
//                 {
//                     RoleId = role.RoleId,
//                     RoleName = role.RoleName,
//                     Description = role.Description,
//                     RequiresSelfie = role.RequiresSelfie,
//                     IsActive = role.IsActive,
//                     CreatedOn = role.CreatedOn
//                 };

//                 return Ok(new ApiResponse<RoleListDto>
//                 {
//                     Success = true,
//                     Message = "Role updated successfully",
//                     Data = responseDto
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<RoleListDto>
//                 {
//                     Success = false,
//                     Message = "Failed to update role",
//                     Errors = new List<string> { ex.Message }
//                 });
//             }
//         }

//         // DELETE: api/role/{id}
//         [HttpDelete("{id}")]
//         public async Task<ActionResult<ApiResponse<object>>> DeleteRole(int id)
//         {
//             try
//             {
//                 var role = await _context.Roles.FindAsync(id);

//                 if (role == null)
//                 {
//                     return NotFound(new ApiResponse<object>
//                     {
//                         Success = false,
//                         Message = "Role not found"
//                     });
//                 }

//                 // Check if role is assigned to any users
//                 var usersWithRole = await _context.Users.AnyAsync(u => u.RoleId == id);

//                 if (usersWithRole)
//                 {
//                     return BadRequest(new ApiResponse<object>
//                     {
//                         Success = false,
//                         Message = "Cannot delete role as it is assigned to users"
//                     });
//                 }

//                 _context.Roles.Remove(role);
//                 await _context.SaveChangesAsync();

//                 return Ok(new ApiResponse<object>
//                 {
//                     Success = true,
//                     Message = "Role deleted successfully"
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<object>
//                 {
//                     Success = false,
//                     Message = "Failed to delete role",
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

namespace attendance_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class RoleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/role
        [HttpGet]
        [AllowAnonymous] // ← ONLY CHANGE — register screen ke liye public
        public async Task<ActionResult<ApiResponse<List<RoleListDto>>>> GetAllRoles()
        {
            try
            {
                var roles = await _context.Roles
                    .Where(r => r.IsActive) // ← sirf active roles
                    .OrderBy(r => r.RoleName)
                    .Select(r => new RoleListDto
                    {
                        RoleId = r.RoleId,
                        RoleName = r.RoleName,
                        Description = r.Description,
                        RequiresSelfie = r.RequiresSelfie,
                        IsActive = r.IsActive,
                        CreatedOn = r.CreatedOn
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<RoleListDto>>
                {
                    Success = true,
                    Message = "Roles retrieved successfully",
                    Data = roles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<RoleListDto>>
                {
                    Success = false,
                    Message = "Failed to retrieve roles",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // GET: api/role/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<RoleListDto>>> GetRole(int id)
        {
            try
            {
                var role = await _context.Roles.FindAsync(id);

                if (role == null)
                {
                    return NotFound(new ApiResponse<RoleListDto>
                    {
                        Success = false,
                        Message = "Role not found"
                    });
                }

                var roleDto = new RoleListDto
                {
                    RoleId = role.RoleId,
                    RoleName = role.RoleName,
                    Description = role.Description,
                    RequiresSelfie = role.RequiresSelfie,
                    IsActive = role.IsActive,
                    CreatedOn = role.CreatedOn
                };

                return Ok(new ApiResponse<RoleListDto>
                {
                    Success = true,
                    Message = "Role retrieved successfully",
                    Data = roleDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<RoleListDto>
                {
                    Success = false,
                    Message = "Failed to retrieve role",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // POST: api/role
        [HttpPost]
        public async Task<ActionResult<ApiResponse<RoleListDto>>> CreateRole([FromBody] RoleDto dto)
        {
            try
            {
                var existingRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.RoleName.ToLower() == dto.RoleName.ToLower());

                if (existingRole != null)
                {
                    return BadRequest(new ApiResponse<RoleListDto>
                    {
                        Success = false,
                        Message = "Role name already exists"
                    });
                }

                var role = new Role
                {
                    RoleName = dto.RoleName,
                    Description = dto.Description,
                    RequiresSelfie = dto.RequiresSelfie,
                    IsActive = dto.IsActive,
                    CreatedOn = DateTime.Now
                };

                _context.Roles.Add(role);
                await _context.SaveChangesAsync();

                var responseDto = new RoleListDto
                {
                    RoleId = role.RoleId,
                    RoleName = role.RoleName,
                    Description = role.Description,
                    RequiresSelfie = role.RequiresSelfie,
                    IsActive = role.IsActive,
                    CreatedOn = role.CreatedOn
                };

                return CreatedAtAction(nameof(GetRole), new { id = role.RoleId }, new ApiResponse<RoleListDto>
                {
                    Success = true,
                    Message = "Role created successfully",
                    Data = responseDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<RoleListDto>
                {
                    Success = false,
                    Message = "Failed to create role",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // PUT: api/role/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<RoleListDto>>> UpdateRole(int id, [FromBody] RoleDto dto)
        {
            try
            {
                var role = await _context.Roles.FindAsync(id);

                if (role == null)
                {
                    return NotFound(new ApiResponse<RoleListDto>
                    {
                        Success = false,
                        Message = "Role not found"
                    });
                }

                var existingRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.RoleName.ToLower() == dto.RoleName.ToLower() && r.RoleId != id);

                if (existingRole != null)
                {
                    return BadRequest(new ApiResponse<RoleListDto>
                    {
                        Success = false,
                        Message = "Role name already exists"
                    });
                }

                role.RoleName = dto.RoleName;
                role.Description = dto.Description;
                role.RequiresSelfie = dto.RequiresSelfie;
                role.IsActive = dto.IsActive;

                await _context.SaveChangesAsync();

                var responseDto = new RoleListDto
                {
                    RoleId = role.RoleId,
                    RoleName = role.RoleName,
                    Description = role.Description,
                    RequiresSelfie = role.RequiresSelfie,
                    IsActive = role.IsActive,
                    CreatedOn = role.CreatedOn
                };

                return Ok(new ApiResponse<RoleListDto>
                {
                    Success = true,
                    Message = "Role updated successfully",
                    Data = responseDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<RoleListDto>
                {
                    Success = false,
                    Message = "Failed to update role",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // DELETE: api/role/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteRole(int id)
        {
            try
            {
                var role = await _context.Roles.FindAsync(id);

                if (role == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Role not found"
                    });
                }

                var usersWithRole = await _context.Users.AnyAsync(u => u.RoleId == id);

                if (usersWithRole)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Cannot delete role as it is assigned to users"
                    });
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Role deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to delete role",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}