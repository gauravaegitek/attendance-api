//isme 5 api h 

// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using attendance_api.Data;
// using attendance_api.DTOs;
// using attendance_api.Models;
// using System.Security.Claims;

// namespace attendance_api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     [Authorize]
//     public class AssetController : ControllerBase
//     {
//         private readonly ApplicationDbContext _db;

//         public AssetController(ApplicationDbContext db)
//         {
//             _db = db;
//         }

//         // ─── Helper: Current User Id ──────────────────────────────────────
//         private int GetCurrentUserId()
//         {
//             var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
//             return int.TryParse(claim, out var id) ? id : 0;
//         }

//         private bool IsAdmin() =>
//             User.IsInRole("admin") || User.IsInRole("Admin");

//         // ─── POST /api/Asset/assign ───────────────────────────────────────
//         /// <summary>Employee ko asset assign karo (admin only)</summary>
//         [HttpPost("assign")]
//         [Authorize(Roles = "admin,Admin")]
//         public async Task<IActionResult> AssignAsset([FromBody] AssignAssetRequest request)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request", success = false, errors = ModelState });

//             var asset = await _db.Assets.FindAsync(request.AssetId);
//             if (asset == null || !asset.IsActive)
//                 return NotFound(new { message = "Asset not found", success = false });

//             if (asset.Status == "assigned")
//                 return BadRequest(new
//                 {
//                     message = $"Asset is already assigned to user ID {asset.AssignedToUserId}. Please return it first.",
//                     success = false
//                 });

//             var user = await _db.Users.FindAsync(request.AssignedToUserId);
//             if (user == null || !user.IsActive)
//                 return NotFound(new { message = "User not found or inactive", success = false });

//             var adminId = GetCurrentUserId();

//             // ─── Update Asset ─────────────────────────────────────────────
//             asset.Status             = "assigned";
//             asset.AssignedToUserId   = request.AssignedToUserId;
//             asset.AssignedDate       = DateTime.Now;
//             asset.ExpectedReturnDate = request.ExpectedReturnDate;
//             asset.AssignmentNote     = request.AssignmentNote;
//             asset.ReturnedDate       = null;
//             asset.ReturnNote         = null;
//             asset.ReturnCondition    = null;
//             asset.UpdatedOn          = DateTime.Now;

//             // ─── Add History ──────────────────────────────────────────────
//             _db.AssetHistories.Add(new AssetHistory
//             {
//                 AssetId       = asset.AssetId,
//                 UserId        = request.AssignedToUserId,
//                 Action        = "assigned",
//                 Note          = request.AssignmentNote,
//                 ActionDate    = DateTime.Now,
//                 ActionByUserId = adminId
//             });

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Asset '{asset.AssetName}' assigned to {user.UserName} successfully.",
//                 success = true,
//                 data = MapAssetResponse(asset, user.UserName)
//             });
//         }

//         // ─── GET /api/Asset/list ──────────────────────────────────────────
//         /// <summary>All assets inventory list (admin = all, employee = own assets)</summary>
//         [HttpGet("list")]
//         public async Task<IActionResult> GetAssetList(
//             [FromQuery] string? status   = null,   // available / assigned / maintenance / retired
//             [FromQuery] string? assetType = null,  // laptop / phone / tablet
//             [FromQuery] int?   userId   = null)
//         {
//             var query = _db.Assets
//                 .Include(a => a.AssignedToUser)
//                 .Where(a => a.IsActive)
//                 .AsQueryable();

//             // Non-admin: sirf apne assigned assets
//             if (!IsAdmin())
//             {
//                 var currentUserId = GetCurrentUserId();
//                 query = query.Where(a => a.AssignedToUserId == currentUserId);
//             }
//             else
//             {
//                 // Admin filters
//                 if (!string.IsNullOrEmpty(status))
//                     query = query.Where(a => a.Status == status.ToLower());

//                 if (!string.IsNullOrEmpty(assetType))
//                     query = query.Where(a => a.AssetType == assetType.ToLower());

//                 if (userId.HasValue)
//                     query = query.Where(a => a.AssignedToUserId == userId.Value);
//             }

//             var assets = await query
//                 .OrderByDescending(a => a.CreatedOn)
//                 .ToListAsync();

//             var result = assets.Select(a => MapAssetResponse(a, a.AssignedToUser?.UserName));

//             return Ok(new
//             {
//                 message = "Asset list fetched successfully.",
//                 success = true,
//                 totalCount = assets.Count,
//                 data = result
//             });
//         }

//         // ─── PUT /api/Asset/return ────────────────────────────────────────
//         /// <summary>Asset return karo (admin only)</summary>
//         [HttpPut("return")]
//         [Authorize(Roles = "admin,Admin")]
//         public async Task<IActionResult> ReturnAsset([FromBody] ReturnAssetRequest request)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request", success = false });

//             var asset = await _db.Assets
//                 .Include(a => a.AssignedToUser)
//                 .FirstOrDefaultAsync(a => a.AssetId == request.AssetId && a.IsActive);

//             if (asset == null)
//                 return NotFound(new { message = "Asset not found", success = false });

//             if (asset.Status != "assigned")
//                 return BadRequest(new { message = "Asset is not currently assigned.", success = false });

//             var adminId = GetCurrentUserId();
//             var previousUserId = asset.AssignedToUserId;

//             // ─── Update Asset ─────────────────────────────────────────────
//             asset.Status           = "available";
//             asset.ReturnedDate     = DateTime.Now;
//             asset.ReturnNote       = request.ReturnNote;
//             asset.ReturnCondition  = request.ReturnCondition;
//             asset.AssignedToUserId = null;
//             asset.AssignedDate     = null;
//             asset.ExpectedReturnDate = null;
//             asset.UpdatedOn        = DateTime.Now;

//             // ─── Add History ──────────────────────────────────────────────
//             _db.AssetHistories.Add(new AssetHistory
//             {
//                 AssetId        = asset.AssetId,
//                 UserId         = previousUserId,
//                 Action         = "returned",
//                 Note           = request.ReturnNote,
//                 Condition      = request.ReturnCondition,
//                 ActionDate     = DateTime.Now,
//                 ActionByUserId = adminId
//             });

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Asset '{asset.AssetName}' returned successfully.",
//                 success = true,
//                 data = MapAssetResponse(asset, null)
//             });
//         }

//         // ─── GET /api/Asset/history ───────────────────────────────────────
//         /// <summary>Asset assignment history (admin = all, employee = own)</summary>
//         [HttpGet("history")]
//         public async Task<IActionResult> GetAssetHistory(
//             [FromQuery] int?    assetId = null,
//             [FromQuery] int?    userId  = null,
//             [FromQuery] string? action  = null,    // assigned / returned
//             [FromQuery] int     page    = 1,
//             [FromQuery] int     pageSize = 20)
//         {
//             var query = _db.AssetHistories
//                 .Include(h => h.Asset)
//                 .Include(h => h.User)
//                 .AsQueryable();

//             // Non-admin: sirf apni history
//             if (!IsAdmin())
//             {
//                 var currentUserId = GetCurrentUserId();
//                 query = query.Where(h => h.UserId == currentUserId);
//             }
//             else
//             {
//                 if (assetId.HasValue)
//                     query = query.Where(h => h.AssetId == assetId.Value);

//                 if (userId.HasValue)
//                     query = query.Where(h => h.UserId == userId.Value);
//             }

//             if (!string.IsNullOrEmpty(action))
//                 query = query.Where(h => h.Action == action.ToLower());

//             var total = await query.CountAsync();

//             var histories = await query
//                 .OrderByDescending(h => h.ActionDate)
//                 .Skip((page - 1) * pageSize)
//                 .Take(pageSize)
//                 .ToListAsync();

//             // Load actionBy user names separately (avoid complex join)
//             var adminIds = histories
//                 .Where(h => h.ActionByUserId.HasValue)
//                 .Select(h => h.ActionByUserId!.Value)
//                 .Distinct()
//                 .ToList();

//             var adminNames = await _db.Users
//                 .Where(u => adminIds.Contains(u.UserId))
//                 .ToDictionaryAsync(u => u.UserId, u => u.UserName);

//             var result = histories.Select(h => new AssetHistoryResponse
//             {
//                 HistoryId       = h.HistoryId,
//                 AssetId         = h.AssetId,
//                 AssetName       = h.Asset?.AssetName,
//                 AssetType       = h.Asset?.AssetType,
//                 UserId          = h.UserId,
//                 UserName        = h.User?.UserName,
//                 Action          = h.Action,
//                 Note            = h.Note,
//                 Condition       = h.Condition,
//                 ActionDate      = h.ActionDate,
//                 ActionByUserName = h.ActionByUserId.HasValue
//                                     ? adminNames.GetValueOrDefault(h.ActionByUserId.Value)
//                                     : null
//             });

//             return Ok(new
//             {
//                 message = "Asset history fetched successfully.",
//                 success = true,
//                 totalCount  = total,
//                 page,
//                 pageSize,
//                 data = result
//             });
//         }

//         // ─── POST /api/Asset/add ──────────────────────────────────────────
//         /// <summary>Inventory mein naya asset add karo (admin only)</summary>
//         [HttpPost("add")]
//         [Authorize(Roles = "admin,Admin")]
//         public async Task<IActionResult> AddAsset([FromBody] CreateAssetRequest request)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request", success = false, errors = ModelState });

//             // AssetCode unique check
//             if (!string.IsNullOrEmpty(request.AssetCode))
//             {
//                 var exists = await _db.Assets.AnyAsync(a => a.AssetCode == request.AssetCode && a.IsActive);
//                 if (exists)
//                     return Conflict(new { message = $"Asset code '{request.AssetCode}' already exists.", success = false });
//             }

//             var asset = new Asset
//             {
//                 AssetName      = request.AssetName,
//                 AssetType      = request.AssetType.ToLower(),
//                 AssetCode      = request.AssetCode,
//                 SerialNumber   = request.SerialNumber,
//                 Brand          = request.Brand,
//                 Model          = request.Model,
//                 Description    = request.Description,
//                 Status         = "available",
//                 CreatedByUserId = GetCurrentUserId(),
//                 CreatedOn      = DateTime.Now,
//                 IsActive       = true
//             };

//             _db.Assets.Add(asset);
//             await _db.SaveChangesAsync();

//             return CreatedAtAction(nameof(GetAssetList), new { }, new
//             {
//                 message = $"Asset '{asset.AssetName}' added to inventory.",
//                 success = true,
//                 data = MapAssetResponse(asset, null)
//             });
//         }

//         // ─── Helper: Map to Response ──────────────────────────────────────
//         private static AssetResponse MapAssetResponse(Asset a, string? assignedUserName) =>
//             new AssetResponse
//             {
//                 AssetId            = a.AssetId,
//                 AssetName          = a.AssetName,
//                 AssetType          = a.AssetType,
//                 AssetCode          = a.AssetCode,
//                 SerialNumber       = a.SerialNumber,
//                 Brand              = a.Brand,
//                 Model              = a.Model,
//                 Description        = a.Description,
//                 Status             = a.Status,
//                 AssignedToUserId   = a.AssignedToUserId,
//                 AssignedToUserName = assignedUserName,
//                 AssignedDate       = a.AssignedDate,
//                 ExpectedReturnDate = a.ExpectedReturnDate,
//                 AssignmentNote     = a.AssignmentNote,
//                 ReturnedDate       = a.ReturnedDate,
//                 ReturnNote         = a.ReturnNote,
//                 ReturnCondition    = a.ReturnCondition,
//                 CreatedOn          = a.CreatedOn,
//                 IsActive           = a.IsActive
//             };
//     }
// }










// isme 10 api h 


// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using attendance_api.Data;
// using attendance_api.DTOs;
// using attendance_api.Models;
// using System.Security.Claims;

// namespace attendance_api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     [Authorize]
//     public class AssetController : ControllerBase
//     {
//         private readonly ApplicationDbContext _db;

//         public AssetController(ApplicationDbContext db)
//         {
//             _db = db;
//         }

//         // ─── Helpers ──────────────────────────────────────────────────────
//         private int GetCurrentUserId()
//         {
//             var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
//             return int.TryParse(claim, out var id) ? id : 0;
//         }

//         private bool IsAdmin() =>
//             User.IsInRole("admin") || User.IsInRole("Admin");

//         // ════════════════════════════════════════════════════════════════
//         //  1. POST /api/Asset/add — Inventory mein naya asset add karo
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("add")]
//         [Authorize(Roles = "admin,Admin")]
//         public async Task<IActionResult> AddAsset([FromBody] CreateAssetRequest request)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request", success = false, errors = ModelState });

//             if (!string.IsNullOrEmpty(request.AssetCode))
//             {
//                 var exists = await _db.Assets.AnyAsync(a => a.AssetCode == request.AssetCode && a.IsActive);
//                 if (exists)
//                     return Conflict(new { message = $"Asset code '{request.AssetCode}' already exists.", success = false });
//             }

//             var asset = new Asset
//             {
//                 AssetName       = request.AssetName,
//                 AssetType       = request.AssetType.ToLower(),
//                 AssetCode       = request.AssetCode,
//                 SerialNumber    = request.SerialNumber,
//                 Brand           = request.Brand,
//                 Model           = request.Model,
//                 Description     = request.Description,
//                 Status          = "available",
//                 CreatedByUserId = GetCurrentUserId(),
//                 CreatedOn       = DateTime.Now,
//                 IsActive        = true
//             };

//             _db.Assets.Add(asset);
//             await _db.SaveChangesAsync();

//             return CreatedAtAction(nameof(GetAssetById), new { id = asset.AssetId }, new
//             {
//                 message = $"Asset '{asset.AssetName}' added to inventory.",
//                 success = true,
//                 data    = MapAssetResponse(asset, null)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  2. GET /api/Asset/list — Inventory list
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("list")]
//         public async Task<IActionResult> GetAssetList(
//             [FromQuery] string? status    = null,
//             [FromQuery] string? assetType = null,
//             [FromQuery] int?    userId    = null)
//         {
//             var query = _db.Assets
//                 .Include(a => a.AssignedToUser)
//                 .Where(a => a.IsActive)
//                 .AsQueryable();

//             if (!IsAdmin())
//             {
//                 var currentUserId = GetCurrentUserId();
//                 query = query.Where(a => a.AssignedToUserId == currentUserId);
//             }
//             else
//             {
//                 if (!string.IsNullOrEmpty(status))
//                     query = query.Where(a => a.Status == status.ToLower());

//                 if (!string.IsNullOrEmpty(assetType))
//                     query = query.Where(a => a.AssetType == assetType.ToLower());

//                 if (userId.HasValue)
//                     query = query.Where(a => a.AssignedToUserId == userId.Value);
//             }

//             var assets = await query.OrderByDescending(a => a.CreatedOn).ToListAsync();

//             return Ok(new
//             {
//                 message    = "Asset list fetched successfully.",
//                 success    = true,
//                 totalCount = assets.Count,
//                 data       = assets.Select(a => MapAssetResponse(a, a.AssignedToUser?.UserName))
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  3. GET /api/Asset/{id} — Single asset detail  ✅ NEW
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("{id:int}")]
//         public async Task<IActionResult> GetAssetById(int id)
//         {
//             var asset = await _db.Assets
//                 .Include(a => a.AssignedToUser)
//                 .Include(a => a.CreatedByUser)
//                 .FirstOrDefaultAsync(a => a.AssetId == id && a.IsActive);

//             if (asset == null)
//                 return NotFound(new { message = "Asset not found.", success = false });

//             // Employee: sirf apna asset dekh sakta hai
//             if (!IsAdmin() && asset.AssignedToUserId != GetCurrentUserId())
//                 return Forbid();

//             // Latest history bhi attach karo
//             var latestHistory = await _db.AssetHistories
//                 .Where(h => h.AssetId == id)
//                 .OrderByDescending(h => h.ActionDate)
//                 .Take(5)
//                 .Include(h => h.User)
//                 .ToListAsync();

//             var adminIds = latestHistory
//                 .Where(h => h.ActionByUserId.HasValue)
//                 .Select(h => h.ActionByUserId!.Value)
//                 .Distinct().ToList();

//             var adminNames = await _db.Users
//                 .Where(u => adminIds.Contains(u.UserId))
//                 .ToDictionaryAsync(u => u.UserId, u => u.UserName);

//             return Ok(new
//             {
//                 message = "Asset detail fetched successfully.",
//                 success = true,
//                 data    = new
//                 {
//                     asset          = MapAssetResponse(asset, asset.AssignedToUser?.UserName),
//                     createdBy      = asset.CreatedByUser?.UserName,
//                     recentHistory  = latestHistory.Select(h => new AssetHistoryResponse
//                     {
//                         HistoryId        = h.HistoryId,
//                         AssetId          = h.AssetId,
//                         AssetName        = asset.AssetName,
//                         AssetType        = asset.AssetType,
//                         UserId           = h.UserId,
//                         UserName         = h.User?.UserName,
//                         Action           = h.Action,
//                         Note             = h.Note,
//                         Condition        = h.Condition,
//                         ActionDate       = h.ActionDate,
//                         ActionByUserName = h.ActionByUserId.HasValue
//                                             ? adminNames.GetValueOrDefault(h.ActionByUserId.Value)
//                                             : null
//                     })
//                 }
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  4. PUT /api/Asset/update/{id} — Asset info edit  ✅ NEW
//         // ════════════════════════════════════════════════════════════════
//         [HttpPut("update/{id:int}")]
//         [Authorize(Roles = "admin,Admin")]
//         public async Task<IActionResult> UpdateAsset(int id, [FromBody] UpdateAssetRequest request)
//         {
//             var asset = await _db.Assets.FindAsync(id);
//             if (asset == null || !asset.IsActive)
//                 return NotFound(new { message = "Asset not found.", success = false });

//             // AssetCode duplicate check (ignore self)
//             if (!string.IsNullOrEmpty(request.AssetCode) && request.AssetCode != asset.AssetCode)
//             {
//                 var duplicate = await _db.Assets.AnyAsync(a =>
//                     a.AssetCode == request.AssetCode && a.AssetId != id && a.IsActive);
//                 if (duplicate)
//                     return Conflict(new { message = $"Asset code '{request.AssetCode}' already exists.", success = false });
//             }

//             // Sirf jo fields aaye hain woh update karo (PATCH style)
//             if (!string.IsNullOrEmpty(request.AssetName))    asset.AssetName    = request.AssetName;
//             if (!string.IsNullOrEmpty(request.AssetType))    asset.AssetType    = request.AssetType.ToLower();
//             if (request.AssetCode    != null)                 asset.AssetCode    = request.AssetCode;
//             if (request.SerialNumber != null)                 asset.SerialNumber = request.SerialNumber;
//             if (request.Brand        != null)                 asset.Brand        = request.Brand;
//             if (request.Model        != null)                 asset.Model        = request.Model;
//             if (request.Description  != null)                 asset.Description  = request.Description;

//             asset.UpdatedOn = DateTime.Now;

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Asset '{asset.AssetName}' updated successfully.",
//                 success = true,
//                 data    = MapAssetResponse(asset, null)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  5. POST /api/Asset/assign — Employee ko asset assign karo
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("assign")]
//         [Authorize(Roles = "admin,Admin")]
//         public async Task<IActionResult> AssignAsset([FromBody] AssignAssetRequest request)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request", success = false, errors = ModelState });

//             var asset = await _db.Assets.FindAsync(request.AssetId);
//             if (asset == null || !asset.IsActive)
//                 return NotFound(new { message = "Asset not found.", success = false });

//             if (asset.Status == "assigned")
//                 return BadRequest(new
//                 {
//                     message = $"Asset already assigned to user ID {asset.AssignedToUserId}. Return it first.",
//                     success = false
//                 });

//             if (asset.Status is "maintenance" or "retired")
//                 return BadRequest(new { message = $"Asset is in '{asset.Status}' state. Cannot assign.", success = false });

//             var user = await _db.Users.FindAsync(request.AssignedToUserId);
//             if (user == null || !user.IsActive)
//                 return NotFound(new { message = "User not found or inactive.", success = false });

//             var adminId = GetCurrentUserId();

//             asset.Status             = "assigned";
//             asset.AssignedToUserId   = request.AssignedToUserId;
//             asset.AssignedDate       = DateTime.Now;
//             asset.ExpectedReturnDate = request.ExpectedReturnDate;
//             asset.AssignmentNote     = request.AssignmentNote;
//             asset.ReturnedDate       = null;
//             asset.ReturnNote         = null;
//             asset.ReturnCondition    = null;
//             asset.UpdatedOn          = DateTime.Now;

//             _db.AssetHistories.Add(new AssetHistory
//             {
//                 AssetId        = asset.AssetId,
//                 UserId         = request.AssignedToUserId,
//                 Action         = "assigned",
//                 Note           = request.AssignmentNote,
//                 ActionDate     = DateTime.Now,
//                 ActionByUserId = adminId
//             });

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Asset '{asset.AssetName}' assigned to {user.UserName} successfully.",
//                 success = true,
//                 data    = MapAssetResponse(asset, user.UserName)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  6. PUT /api/Asset/return — Asset return karo
//         // ════════════════════════════════════════════════════════════════
//         [HttpPut("return")]
//         [Authorize(Roles = "admin,Admin")]
//         public async Task<IActionResult> ReturnAsset([FromBody] ReturnAssetRequest request)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request", success = false });

//             var asset = await _db.Assets
//                 .Include(a => a.AssignedToUser)
//                 .FirstOrDefaultAsync(a => a.AssetId == request.AssetId && a.IsActive);

//             if (asset == null)
//                 return NotFound(new { message = "Asset not found.", success = false });

//             if (asset.Status != "assigned")
//                 return BadRequest(new { message = "Asset is not currently assigned.", success = false });

//             var adminId        = GetCurrentUserId();
//             var previousUserId = asset.AssignedToUserId;

//             asset.Status             = "available";
//             asset.ReturnedDate       = DateTime.Now;
//             asset.ReturnNote         = request.ReturnNote;
//             asset.ReturnCondition    = request.ReturnCondition;
//             asset.AssignedToUserId   = null;
//             asset.AssignedDate       = null;
//             asset.ExpectedReturnDate = null;
//             asset.UpdatedOn          = DateTime.Now;

//             _db.AssetHistories.Add(new AssetHistory
//             {
//                 AssetId        = asset.AssetId,
//                 UserId         = previousUserId,
//                 Action         = "returned",
//                 Note           = request.ReturnNote,
//                 Condition      = request.ReturnCondition,
//                 ActionDate     = DateTime.Now,
//                 ActionByUserId = adminId
//             });

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Asset '{asset.AssetName}' returned successfully.",
//                 success = true,
//                 data    = MapAssetResponse(asset, null)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  7. PUT /api/Asset/status/{id} — Maintenance / Retired  ✅ NEW
//         // ════════════════════════════════════════════════════════════════
//         [HttpPut("status/{id:int}")]
//         [Authorize(Roles = "admin,Admin")]
//         public async Task<IActionResult> ChangeAssetStatus(int id, [FromBody] ChangeAssetStatusRequest request)
//         {
//             var allowed = new[] { "available", "maintenance", "retired" };
//             if (!allowed.Contains(request.Status.ToLower()))
//                 return BadRequest(new
//                 {
//                     message = "Invalid status. Allowed: available, maintenance, retired.",
//                     success = false
//                 });

//             var asset = await _db.Assets.FindAsync(id);
//             if (asset == null || !asset.IsActive)
//                 return NotFound(new { message = "Asset not found.", success = false });

//             if (asset.Status == "assigned")
//                 return BadRequest(new { message = "Asset is currently assigned. Return it before changing status.", success = false });

//             var oldStatus = asset.Status;
//             asset.Status    = request.Status.ToLower();
//             asset.UpdatedOn = DateTime.Now;

//             _db.AssetHistories.Add(new AssetHistory
//             {
//                 AssetId        = asset.AssetId,
//                 UserId         = null,
//                 Action         = $"status_changed",
//                 Note           = $"{oldStatus} → {asset.Status}. {request.Note}".Trim(' ', '.'),
//                 ActionDate     = DateTime.Now,
//                 ActionByUserId = GetCurrentUserId()
//             });

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Asset status changed to '{asset.Status}' successfully.",
//                 success = true,
//                 data    = MapAssetResponse(asset, null)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  8. DELETE /api/Asset/{id} — Soft Delete  ✅ NEW
//         // ════════════════════════════════════════════════════════════════
//         [HttpDelete("{id:int}")]
//         [Authorize(Roles = "admin,Admin")]
//         public async Task<IActionResult> DeleteAsset(int id)
//         {
//             var asset = await _db.Assets.FindAsync(id);
//             if (asset == null || !asset.IsActive)
//                 return NotFound(new { message = "Asset not found.", success = false });

//             if (asset.Status == "assigned")
//                 return BadRequest(new { message = "Cannot delete an assigned asset. Return it first.", success = false });

//             asset.IsActive  = false;
//             asset.UpdatedOn = DateTime.Now;

//             _db.AssetHistories.Add(new AssetHistory
//             {
//                 AssetId        = asset.AssetId,
//                 UserId         = null,
//                 Action         = "deleted",
//                 Note           = "Asset soft deleted from inventory.",
//                 ActionDate     = DateTime.Now,
//                 ActionByUserId = GetCurrentUserId()
//             });

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Asset '{asset.AssetName}' deleted from inventory.",
//                 success = true
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  9. GET /api/Asset/summary — Dashboard stats  ✅ NEW
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("summary")]
//         [Authorize(Roles = "admin,Admin")]
//         public async Task<IActionResult> GetAssetSummary()
//         {
//             var assets = await _db.Assets
//                 .Where(a => a.IsActive)
//                 .ToListAsync();

//             var byType = assets
//                 .GroupBy(a => a.AssetType)
//                 .Select(g => new AssetTypeCount
//                 {
//                     AssetType = g.Key,
//                     Count     = g.Count()
//                 })
//                 .OrderByDescending(x => x.Count)
//                 .ToList();

//             var summary = new AssetSummaryResponse
//             {
//                 Total       = assets.Count,
//                 Available   = assets.Count(a => a.Status == "available"),
//                 Assigned    = assets.Count(a => a.Status == "assigned"),
//                 Maintenance = assets.Count(a => a.Status == "maintenance"),
//                 Retired     = assets.Count(a => a.Status == "retired"),
//                 ByType      = byType
//             };

//             return Ok(new
//             {
//                 message = "Asset summary fetched successfully.",
//                 success = true,
//                 data    = summary
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  10. GET /api/Asset/history — Assignment history
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("history")]
//         public async Task<IActionResult> GetAssetHistory(
//             [FromQuery] int?    assetId  = null,
//             [FromQuery] int?    userId   = null,
//             [FromQuery] string? action   = null,
//             [FromQuery] int     page     = 1,
//             [FromQuery] int     pageSize = 20)
//         {
//             var query = _db.AssetHistories
//                 .Include(h => h.Asset)
//                 .Include(h => h.User)
//                 .AsQueryable();

//             if (!IsAdmin())
//             {
//                 var currentUserId = GetCurrentUserId();
//                 query = query.Where(h => h.UserId == currentUserId);
//             }
//             else
//             {
//                 if (assetId.HasValue) query = query.Where(h => h.AssetId == assetId.Value);
//                 if (userId.HasValue)  query = query.Where(h => h.UserId  == userId.Value);
//             }

//             if (!string.IsNullOrEmpty(action))
//                 query = query.Where(h => h.Action == action.ToLower());

//             var total     = await query.CountAsync();
//             var histories = await query
//                 .OrderByDescending(h => h.ActionDate)
//                 .Skip((page - 1) * pageSize)
//                 .Take(pageSize)
//                 .ToListAsync();

//             var adminIds = histories
//                 .Where(h => h.ActionByUserId.HasValue)
//                 .Select(h => h.ActionByUserId!.Value)
//                 .Distinct().ToList();

//             var adminNames = await _db.Users
//                 .Where(u => adminIds.Contains(u.UserId))
//                 .ToDictionaryAsync(u => u.UserId, u => u.UserName);

//             var result = histories.Select(h => new AssetHistoryResponse
//             {
//                 HistoryId        = h.HistoryId,
//                 AssetId          = h.AssetId,
//                 AssetName        = h.Asset?.AssetName,
//                 AssetType        = h.Asset?.AssetType,
//                 UserId           = h.UserId,
//                 UserName         = h.User?.UserName,
//                 Action           = h.Action,
//                 Note             = h.Note,
//                 Condition        = h.Condition,
//                 ActionDate       = h.ActionDate,
//                 ActionByUserName = h.ActionByUserId.HasValue
//                                     ? adminNames.GetValueOrDefault(h.ActionByUserId.Value)
//                                     : null
//             });

//             return Ok(new
//             {
//                 message    = "Asset history fetched successfully.",
//                 success    = true,
//                 totalCount = total,
//                 page,
//                 pageSize,
//                 data = result
//             });
//         }

//         // ─── Helper: Map to Response ──────────────────────────────────────
//         private static AssetResponse MapAssetResponse(Asset a, string? assignedUserName) =>
//             new AssetResponse
//             {
//                 AssetId            = a.AssetId,
//                 AssetName          = a.AssetName,
//                 AssetType          = a.AssetType,
//                 AssetCode          = a.AssetCode,
//                 SerialNumber       = a.SerialNumber,
//                 Brand              = a.Brand,
//                 Model              = a.Model,
//                 Description        = a.Description,
//                 Status             = a.Status,
//                 AssignedToUserId   = a.AssignedToUserId,
//                 AssignedToUserName = assignedUserName,
//                 AssignedDate       = a.AssignedDate,
//                 ExpectedReturnDate = a.ExpectedReturnDate,
//                 AssignmentNote     = a.AssignmentNote,
//                 ReturnedDate       = a.ReturnedDate,
//                 ReturnNote         = a.ReturnNote,
//                 ReturnCondition    = a.ReturnCondition,
//                 CreatedOn          = a.CreatedOn,
//                 IsActive           = a.IsActive
//             };
//     }
// }














// isme 6 api h 

// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using attendance_api.Data;
// using attendance_api.DTOs;
// using attendance_api.Models;
// using System.Security.Claims;

// namespace attendance_api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     [Authorize]
//     public class AssetController : ControllerBase
//     {
//         private readonly ApplicationDbContext _db;

//         public AssetController(ApplicationDbContext db)
//         {
//             _db = db;
//         }

//         // ─── Helpers ──────────────────────────────────────────────────────
//         private int GetCurrentUserId()
//         {
//             var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
//             return int.TryParse(claim, out var id) ? id : 0;
//         }

//         private bool IsAdmin() =>
//             User.IsInRole("admin") || User.IsInRole("Admin");

//         // ════════════════════════════════════════════════════════════════
//         //  1. POST /api/Asset/add
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("add")]
//         [Authorize(Roles = "admin,Admin")]
//         public async Task<IActionResult> AddAsset([FromBody] CreateAssetRequest request)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             if (!string.IsNullOrEmpty(request.AssetCode))
//             {
//                 var exists = await _db.Assets.AnyAsync(a => a.AssetCode == request.AssetCode && a.IsActive);
//                 if (exists)
//                     return Conflict(new { message = $"Asset code '{request.AssetCode}' already exists.", success = false });
//             }

//             var asset = new Asset
//             {
//                 AssetName       = request.AssetName,
//                 AssetType       = request.AssetType.ToLower(),
//                 AssetCode       = request.AssetCode,
//                 SerialNumber    = request.SerialNumber,
//                 Brand           = request.Brand,
//                 Model           = request.Model,
//                 Description     = request.Description,
//                 Status          = "available",
//                 CreatedByUserId = GetCurrentUserId(),
//                 CreatedOn       = DateTime.Now,
//                 IsActive        = true
//             };

//             _db.Assets.Add(asset);
//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Asset '{asset.AssetName}' added to inventory.",
//                 success = true,
//                 data    = MapAssetResponse(asset, null)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  2. POST /api/Asset/assign
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("assign")]
//         [Authorize(Roles = "admin,Admin")]
//         public async Task<IActionResult> AssignAsset([FromBody] AssignAssetRequest request)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             var asset = await _db.Assets.FindAsync(request.AssetId);
//             if (asset == null || !asset.IsActive)
//                 return NotFound(new { message = "Asset not found.", success = false });

//             if (asset.Status == "assigned")
//                 return BadRequest(new
//                 {
//                     message = $"Asset already assigned to user ID {asset.AssignedToUserId}. Return it first.",
//                     success = false
//                 });

//             var user = await _db.Users.FindAsync(request.AssignedToUserId);
//             if (user == null || !user.IsActive)
//                 return NotFound(new { message = "User not found or inactive.", success = false });

//             var adminId = GetCurrentUserId();

//             asset.Status             = "assigned";
//             asset.AssignedToUserId   = request.AssignedToUserId;
//             asset.AssignedDate       = DateTime.Now;
//             asset.ExpectedReturnDate = request.ExpectedReturnDate;
//             asset.AssignmentNote     = request.AssignmentNote;
//             asset.ReturnedDate       = null;
//             asset.ReturnNote         = null;
//             asset.ReturnCondition    = null;
//             asset.UpdatedOn          = DateTime.Now;

//             _db.AssetHistories.Add(new AssetHistory
//             {
//                 AssetId        = asset.AssetId,
//                 UserId         = request.AssignedToUserId,
//                 Action         = "assigned",
//                 Note           = request.AssignmentNote,
//                 ActionDate     = DateTime.Now,
//                 ActionByUserId = adminId
//             });

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Asset '{asset.AssetName}' assigned to {user.UserName} successfully.",
//                 success = true,
//                 data    = MapAssetResponse(asset, user.UserName)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  3. PUT /api/Asset/return
//         // ════════════════════════════════════════════════════════════════
//         [HttpPut("return")]
//         [Authorize(Roles = "admin,Admin")]
//         public async Task<IActionResult> ReturnAsset([FromBody] ReturnAssetRequest request)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false });

//             var asset = await _db.Assets
//                 .Include(a => a.AssignedToUser)
//                 .FirstOrDefaultAsync(a => a.AssetId == request.AssetId && a.IsActive);

//             if (asset == null)
//                 return NotFound(new { message = "Asset not found.", success = false });

//             if (asset.Status != "assigned")
//                 return BadRequest(new { message = "Asset is not currently assigned.", success = false });

//             var previousUserId = asset.AssignedToUserId;
//             var adminId        = GetCurrentUserId();

//             asset.Status             = "available";
//             asset.ReturnedDate       = DateTime.Now;
//             asset.ReturnNote         = request.ReturnNote;
//             asset.ReturnCondition    = request.ReturnCondition;
//             asset.AssignedToUserId   = null;
//             asset.AssignedDate       = null;
//             asset.ExpectedReturnDate = null;
//             asset.UpdatedOn          = DateTime.Now;

//             _db.AssetHistories.Add(new AssetHistory
//             {
//                 AssetId        = asset.AssetId,
//                 UserId         = previousUserId,
//                 Action         = "returned",
//                 Note           = request.ReturnNote,
//                 Condition      = request.ReturnCondition,
//                 ActionDate     = DateTime.Now,
//                 ActionByUserId = adminId
//             });

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Asset '{asset.AssetName}' returned successfully.",
//                 success = true,
//                 data    = MapAssetResponse(asset, null)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  4. GET /api/Asset/list
//         //     ✅ fromDate / toDate — AssignedDate ke basis pe filter
//         //
//         //     ?fromDate=2026-01-01
//         //     ?toDate=2026-03-31
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("list")]
//         public async Task<IActionResult> GetAssetList(
//             [FromQuery] string?   status    = null,
//             [FromQuery] string?   assetType = null,
//             [FromQuery] int?      userId    = null,
//             [FromQuery] DateTime? fromDate  = null,   // ✅ NEW
//             [FromQuery] DateTime? toDate    = null)   // ✅ NEW
//         {
//             var query = _db.Assets
//                 .Include(a => a.AssignedToUser)
//                 .Where(a => a.IsActive)
//                 .AsQueryable();

//             if (!IsAdmin())
//             {
//                 var currentUserId = GetCurrentUserId();
//                 query = query.Where(a => a.AssignedToUserId == currentUserId);
//             }
//             else
//             {
//                 if (!string.IsNullOrEmpty(status))
//                     query = query.Where(a => a.Status == status.ToLower());

//                 if (!string.IsNullOrEmpty(assetType))
//                     query = query.Where(a => a.AssetType == assetType.ToLower());

//                 if (userId.HasValue)
//                     query = query.Where(a => a.AssignedToUserId == userId.Value);
//             }

//             // ✅ Date filter — AssignedDate ke basis pe
//             if (fromDate.HasValue)
//                 query = query.Where(a => a.AssignedDate >= fromDate.Value.Date);

//             if (toDate.HasValue)
//                 query = query.Where(a => a.AssignedDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));

//             var assets = await query.OrderByDescending(a => a.CreatedOn).ToListAsync();

//             return Ok(new
//             {
//                 message    = "Asset list fetched successfully.",
//                 success    = true,
//                 totalCount = assets.Count,
//                 filter = new
//                 {
//                     fromDate = fromDate?.ToString("yyyy-MM-dd"),
//                     toDate   = toDate?.ToString("yyyy-MM-dd")
//                 },
//                 data = assets.Select(a => MapAssetResponse(a, a.AssignedToUser?.UserName))
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  5. GET /api/Asset/history
//         //     ✅ fromDate / toDate — ActionDate ke basis pe filter
//         //
//         //     ?fromDate=2026-01-01
//         //     ?toDate=2026-03-31
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("history")]
//         public async Task<IActionResult> GetAssetHistory(
//             [FromQuery] int?      assetId  = null,
//             [FromQuery] int?      userId   = null,
//             [FromQuery] string?   action   = null,
//             [FromQuery] DateTime? fromDate = null,    // ✅ NEW
//             [FromQuery] DateTime? toDate   = null,    // ✅ NEW
//             [FromQuery] int       page     = 1,
//             [FromQuery] int       pageSize = 20)
//         {
//             var query = _db.AssetHistories
//                 .Include(h => h.Asset)
//                 .Include(h => h.User)
//                 .AsQueryable();

//             if (!IsAdmin())
//             {
//                 var currentUserId = GetCurrentUserId();
//                 query = query.Where(h => h.UserId == currentUserId);
//             }
//             else
//             {
//                 if (assetId.HasValue) query = query.Where(h => h.AssetId == assetId.Value);
//                 if (userId.HasValue)  query = query.Where(h => h.UserId  == userId.Value);
//             }

//             if (!string.IsNullOrEmpty(action))
//                 query = query.Where(h => h.Action == action.ToLower());

//             // ✅ Date filter — ActionDate ke basis pe
//             if (fromDate.HasValue)
//                 query = query.Where(h => h.ActionDate >= fromDate.Value.Date);

//             if (toDate.HasValue)
//                 query = query.Where(h => h.ActionDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));

//             var total = await query.CountAsync();

//             var histories = await query
//                 .OrderByDescending(h => h.ActionDate)
//                 .Skip((page - 1) * pageSize)
//                 .Take(pageSize)
//                 .ToListAsync();

//             var adminIds = histories
//                 .Where(h => h.ActionByUserId.HasValue)
//                 .Select(h => h.ActionByUserId!.Value)
//                 .Distinct().ToList();

//             var adminNames = await _db.Users
//                 .Where(u => adminIds.Contains(u.UserId))
//                 .ToDictionaryAsync(u => u.UserId, u => u.UserName);

//             var result = histories.Select(h => new AssetHistoryResponse
//             {
//                 HistoryId        = h.HistoryId,
//                 AssetId          = h.AssetId,
//                 AssetName        = h.Asset?.AssetName,
//                 AssetType        = h.Asset?.AssetType,
//                 UserId           = h.UserId,
//                 UserName         = h.User?.UserName,
//                 Action           = h.Action,
//                 Note             = h.Note,
//                 Condition        = h.Condition,
//                 ActionDate       = h.ActionDate,
//                 ActionByUserName = h.ActionByUserId.HasValue
//                                     ? adminNames.GetValueOrDefault(h.ActionByUserId.Value)
//                                     : null
//             });

//             return Ok(new
//             {
//                 message    = "Asset history fetched successfully.",
//                 success    = true,
//                 totalCount = total,
//                 page,
//                 pageSize,
//                 filter = new
//                 {
//                     fromDate = fromDate?.ToString("yyyy-MM-dd"),
//                     toDate   = toDate?.ToString("yyyy-MM-dd")
//                 },
//                 data = result
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  6. GET /api/Asset/summary
//         //     ✅ fromDate / toDate — AssignedDate ke basis pe filter
//         //
//         //     ?fromDate=2026-01-01
//         //     ?toDate=2026-03-31
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("summary")]
//         [Authorize(Roles = "admin,Admin")]
//         public async Task<IActionResult> GetAssetSummary(
//             [FromQuery] DateTime? fromDate = null,    // ✅ NEW
//             [FromQuery] DateTime? toDate   = null)    // ✅ NEW
//         {
//             // Base: saare active assets
//             var allAssets = await _db.Assets
//                 .Where(a => a.IsActive)
//                 .ToListAsync();

//             // Date filter wale assets (assigned wale ke liye)
//             var filteredAssets = allAssets.AsEnumerable();

//             if (fromDate.HasValue)
//                 filteredAssets = filteredAssets.Where(a =>
//                     a.AssignedDate.HasValue && a.AssignedDate.Value.Date >= fromDate.Value.Date);

//             if (toDate.HasValue)
//                 filteredAssets = filteredAssets.Where(a =>
//                     a.AssignedDate.HasValue && a.AssignedDate.Value.Date <= toDate.Value.Date);

//             var filtered = filteredAssets.ToList();

//             // Date filter laga ho toh filtered data ka summary
//             // Nahi laga toh poora inventory ka summary
//             var summarySource = (fromDate.HasValue || toDate.HasValue) ? filtered : allAssets;

//             var summary = new AssetSummaryResponse
//             {
//                 Total     = summarySource.Count,
//                 Available = summarySource.Count(a => a.Status == "available"),
//                 Assigned  = summarySource.Count(a => a.Status == "assigned"),
//                 ByType    = summarySource
//                     .GroupBy(a => a.AssetType)
//                     .Select(g => new AssetTypeCount
//                     {
//                         AssetType = g.Key,
//                         Count     = g.Count()
//                     })
//                     .OrderByDescending(x => x.Count)
//                     .ToList()
//             };

//             return Ok(new
//             {
//                 message = "Asset summary fetched successfully.",
//                 success = true,
//                 filter = new
//                 {
//                     fromDate = fromDate?.ToString("yyyy-MM-dd"),
//                     toDate   = toDate?.ToString("yyyy-MM-dd")
//                 },
//                 data = summary
//             });
//         }

//         // ─── Helper: Map to Response ──────────────────────────────────────
//         private static AssetResponse MapAssetResponse(Asset a, string? assignedUserName) =>
//             new AssetResponse
//             {
//                 AssetId            = a.AssetId,
//                 AssetName          = a.AssetName,
//                 AssetType          = a.AssetType,
//                 AssetCode          = a.AssetCode,
//                 SerialNumber       = a.SerialNumber,
//                 Brand              = a.Brand,
//                 Model              = a.Model,
//                 Description        = a.Description,
//                 Status             = a.Status,
//                 AssignedToUserId   = a.AssignedToUserId,
//                 AssignedToUserName = assignedUserName,
//                 AssignedDate       = a.AssignedDate,
//                 ExpectedReturnDate = a.ExpectedReturnDate,
//                 AssignmentNote     = a.AssignmentNote,
//                 ReturnedDate       = a.ReturnedDate,
//                 ReturnNote         = a.ReturnNote,
//                 ReturnCondition    = a.ReturnCondition,
//                 CreatedOn          = a.CreatedOn
//             };
//     }
// }














// ======================= Controllers/AssetController.cs (FULL UPDATED - NO NEW TABLE) =======================
// ✅ Maintenance data will be stored in dbo.Assets (same table)
// ✅ Logs will be stored in dbo.AssetHistories (same history table)
// ❌ No AssetMaintenance / No _db.AssetMaintenances

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using attendance_api.Data;
using attendance_api.DTOs;
using attendance_api.Models;
using System.Security.Claims;

namespace attendance_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AssetController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public AssetController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ─── Helpers ──────────────────────────────────────────────────────
        private int GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : 0;
        }

        private bool IsAdmin() =>
            User.IsInRole("admin") || User.IsInRole("Admin");

        // ════════════════════════════════════════════════════════════════
        //  1. POST /api/Asset/add
        // ════════════════════════════════════════════════════════════════
        [HttpPost("add")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<IActionResult> AddAsset([FromBody] CreateAssetRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

            if (!string.IsNullOrEmpty(request.AssetCode))
            {
                var exists = await _db.Assets.AnyAsync(a => a.AssetCode == request.AssetCode && a.IsActive);
                if (exists)
                    return Conflict(new { message = $"Asset code '{request.AssetCode}' already exists.", success = false });
            }

            var asset = new Asset
            {
                AssetName = request.AssetName,
                AssetType = request.AssetType.ToLower(),
                AssetCode = request.AssetCode,
                SerialNumber = request.SerialNumber,
                Brand = request.Brand,
                Model = request.Model,
                Description = request.Description,
                Status = "available",
                CreatedByUserId = GetCurrentUserId(),
                CreatedOn = DateTime.Now,
                IsActive = true
            };

            _db.Assets.Add(asset);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Asset '{asset.AssetName}' added to inventory.",
                success = true,
                data = MapAssetResponse(asset, null)
            });
        }

        // ════════════════════════════════════════════════════════════════
        //  2. POST /api/Asset/assign
        // ════════════════════════════════════════════════════════════════
        [HttpPost("assign")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<IActionResult> AssignAsset([FromBody] AssignAssetRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

            var asset = await _db.Assets.FindAsync(request.AssetId);
            if (asset == null || !asset.IsActive)
                return NotFound(new { message = "Asset not found.", success = false });

            if (asset.Status == "assigned")
                return BadRequest(new
                {
                    message = $"Asset already assigned to user ID {asset.AssignedToUserId}. Return it first.",
                    success = false
                });

            if (asset.Status == "maintenance")
                return BadRequest(new { message = "Asset is in maintenance. Cannot assign now.", success = false });

            var user = await _db.Users.FindAsync(request.AssignedToUserId);
            if (user == null || !user.IsActive)
                return NotFound(new { message = "User not found or inactive.", success = false });

            var adminId = GetCurrentUserId();

            asset.Status = "assigned";
            asset.AssignedToUserId = request.AssignedToUserId;
            asset.AssignedDate = DateTime.Now;
            asset.ExpectedReturnDate = request.ExpectedReturnDate;
            asset.AssignmentNote = request.AssignmentNote;
            asset.ReturnedDate = null;
            asset.ReturnNote = null;
            asset.ReturnCondition = null;
            asset.UpdatedOn = DateTime.Now;

            _db.AssetHistories.Add(new AssetHistory
            {
                AssetId = asset.AssetId,
                UserId = request.AssignedToUserId,
                Action = "assigned",
                Note = request.AssignmentNote,
                ActionDate = DateTime.Now,
                ActionByUserId = adminId
            });

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Asset '{asset.AssetName}' assigned to {user.UserName} successfully.",
                success = true,
                data = MapAssetResponse(asset, user.UserName)
            });
        }

        // ════════════════════════════════════════════════════════════════
        //  3. PUT /api/Asset/return
        // ════════════════════════════════════════════════════════════════
        [HttpPut("return")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<IActionResult> ReturnAsset([FromBody] ReturnAssetRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request.", success = false });

            var asset = await _db.Assets
                .Include(a => a.AssignedToUser)
                .FirstOrDefaultAsync(a => a.AssetId == request.AssetId && a.IsActive);

            if (asset == null)
                return NotFound(new { message = "Asset not found.", success = false });

            if (asset.Status != "assigned")
                return BadRequest(new { message = "Asset is not currently assigned.", success = false });

            var previousUserId = asset.AssignedToUserId;
            var adminId = GetCurrentUserId();

            asset.Status = "available";
            asset.ReturnedDate = DateTime.Now;
            asset.ReturnNote = request.ReturnNote;
            asset.ReturnCondition = request.ReturnCondition;
            asset.AssignedToUserId = null;
            asset.AssignedDate = null;
            asset.ExpectedReturnDate = null;
            asset.UpdatedOn = DateTime.Now;

            _db.AssetHistories.Add(new AssetHistory
            {
                AssetId = asset.AssetId,
                UserId = previousUserId,
                Action = "returned",
                Note = request.ReturnNote,
                Condition = request.ReturnCondition,
                ActionDate = DateTime.Now,
                ActionByUserId = adminId
            });

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Asset '{asset.AssetName}' returned successfully.",
                success = true,
                data = MapAssetResponse(asset, null)
            });
        }

        // ════════════════════════════════════════════════════════════════
        //  4. GET /api/Asset/list
        // ════════════════════════════════════════════════════════════════
        [HttpGet("list")]
        public async Task<IActionResult> GetAssetList(
            [FromQuery] string? status = null,
            [FromQuery] string? assetType = null,
            [FromQuery] int? userId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var query = _db.Assets
                .Include(a => a.AssignedToUser)
                .Where(a => a.IsActive)
                .AsQueryable();

            if (!IsAdmin())
            {
                var currentUserId = GetCurrentUserId();
                query = query.Where(a => a.AssignedToUserId == currentUserId);
            }
            else
            {
                if (!string.IsNullOrEmpty(status))
                    query = query.Where(a => a.Status == status.ToLower());

                if (!string.IsNullOrEmpty(assetType))
                    query = query.Where(a => a.AssetType == assetType.ToLower());

                if (userId.HasValue)
                    query = query.Where(a => a.AssignedToUserId == userId.Value);
            }

            if (fromDate.HasValue)
                query = query.Where(a => a.AssignedDate >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(a => a.AssignedDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));

            var assets = await query.OrderByDescending(a => a.CreatedOn).ToListAsync();

            return Ok(new
            {
                message = "Asset list fetched successfully.",
                success = true,
                totalCount = assets.Count,
                filter = new
                {
                    fromDate = fromDate?.ToString("yyyy-MM-dd"),
                    toDate = toDate?.ToString("yyyy-MM-dd")
                },
                data = assets.Select(a => MapAssetResponse(a, a.AssignedToUser?.UserName))
            });
        }

        // ════════════════════════════════════════════════════════════════
        //  5. GET /api/Asset/history
        // ════════════════════════════════════════════════════════════════
        [HttpGet("history")]
        public async Task<IActionResult> GetAssetHistory(
            [FromQuery] int? assetId = null,
            [FromQuery] int? userId = null,
            [FromQuery] string? action = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _db.AssetHistories
                .Include(h => h.Asset)
                .Include(h => h.User)
                .AsQueryable();

            if (!IsAdmin())
            {
                var currentUserId = GetCurrentUserId();
                query = query.Where(h => h.UserId == currentUserId);
            }
            else
            {
                if (assetId.HasValue) query = query.Where(h => h.AssetId == assetId.Value);
                if (userId.HasValue) query = query.Where(h => h.UserId == userId.Value);
            }

            if (!string.IsNullOrEmpty(action))
                query = query.Where(h => h.Action == action.ToLower());

            if (fromDate.HasValue)
                query = query.Where(h => h.ActionDate >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(h => h.ActionDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));

            var total = await query.CountAsync();

            var histories = await query
                .OrderByDescending(h => h.ActionDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var adminIds = histories
                .Where(h => h.ActionByUserId.HasValue)
                .Select(h => h.ActionByUserId!.Value)
                .Distinct().ToList();

            var adminNames = await _db.Users
                .Where(u => adminIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => u.UserName);

            var result = histories.Select(h => new AssetHistoryResponse
            {
                HistoryId = h.HistoryId,
                AssetId = h.AssetId,
                AssetName = h.Asset?.AssetName,
                AssetType = h.Asset?.AssetType,
                UserId = h.UserId,
                UserName = h.User?.UserName,
                Action = h.Action,
                Note = h.Note,
                Condition = h.Condition,
                ActionDate = h.ActionDate,
                ActionByUserName = h.ActionByUserId.HasValue
                                    ? adminNames.GetValueOrDefault(h.ActionByUserId.Value)
                                    : null
            });

            return Ok(new
            {
                message = "Asset history fetched successfully.",
                success = true,
                totalCount = total,
                page,
                pageSize,
                filter = new
                {
                    fromDate = fromDate?.ToString("yyyy-MM-dd"),
                    toDate = toDate?.ToString("yyyy-MM-dd")
                },
                data = result
            });
        }

        // ════════════════════════════════════════════════════════════════
        //  6. GET /api/Asset/summary
        // ════════════════════════════════════════════════════════════════
        [HttpGet("summary")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<IActionResult> GetAssetSummary(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var allAssets = await _db.Assets
                .Where(a => a.IsActive)
                .ToListAsync();

            var filteredAssets = allAssets.AsEnumerable();

            if (fromDate.HasValue)
                filteredAssets = filteredAssets.Where(a =>
                    a.AssignedDate.HasValue && a.AssignedDate.Value.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                filteredAssets = filteredAssets.Where(a =>
                    a.AssignedDate.HasValue && a.AssignedDate.Value.Date <= toDate.Value.Date);

            var filtered = filteredAssets.ToList();
            var summarySource = (fromDate.HasValue || toDate.HasValue) ? filtered : allAssets;

            var summary = new AssetSummaryResponse
            {
                Total = summarySource.Count,
                Available = summarySource.Count(a => a.Status == "available"),
                Assigned = summarySource.Count(a => a.Status == "assigned"),
                ByType = summarySource
                    .GroupBy(a => a.AssetType)
                    .Select(g => new AssetTypeCount { AssetType = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList()
            };

            return Ok(new
            {
                message = "Asset summary fetched successfully.",
                success = true,
                filter = new
                {
                    fromDate = fromDate?.ToString("yyyy-MM-dd"),
                    toDate = toDate?.ToString("yyyy-MM-dd")
                },
                data = summary
            });
        }

        // ════════════════════════════════════════════════════════════════
        //  7. POST /api/Asset/maintenance/start   (NO NEW TABLE)
        // ════════════════════════════════════════════════════════════════
        [HttpPost("maintenance/start")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<IActionResult> StartMaintenance([FromBody] StartMaintenanceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

            var asset = await _db.Assets
                .Include(a => a.AssignedToUser)
                .FirstOrDefaultAsync(a => a.AssetId == request.AssetId && a.IsActive);

            if (asset == null)
                return NotFound(new { message = "Asset not found.", success = false });

            if (asset.Status == "assigned")
                return BadRequest(new { message = "Asset is assigned. Please return it before maintenance.", success = false });

            if (asset.Status == "maintenance")
                return BadRequest(new { message = "Asset is already in maintenance.", success = false });

            var adminId = GetCurrentUserId();

            // ✅ Update same Assets table
            asset.Status = "maintenance";
            asset.UpdatedOn = DateTime.Now;

            // These columns must exist in dbo.Assets (as you added via SQL)
            asset.MaintenanceType = request.MaintenanceType?.ToLower() ?? "repair";
            asset.MaintenanceVendorName = request.VendorName;
            asset.MaintenanceTicketNo = request.TicketNo;
            asset.MaintenanceIssue = request.IssueDescription;
            asset.MaintenanceStartDate = DateTime.Now;
            asset.MaintenanceEndDate = null;
            asset.MaintenanceCost = null;
            asset.MaintenanceResolution = null;
            asset.MaintenanceCreatedByUserId = adminId;
            asset.MaintenanceCompletedByUserId = null;

            // ✅ Log in AssetHistories
            _db.AssetHistories.Add(new AssetHistory
            {
                AssetId = asset.AssetId,
                UserId = null,
                Action = "maintenance_started",
                Note = request.IssueDescription,
                ActionDate = DateTime.Now,
                ActionByUserId = adminId
            });

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Maintenance started for asset '{asset.AssetName}'.",
                success = true,
                data = MapMaintenanceFromAsset(asset)
            });
        }

        // ════════════════════════════════════════════════════════════════
        //  8. PUT /api/Asset/maintenance/complete (NO NEW TABLE)
        // ════════════════════════════════════════════════════════════════
        [HttpPut("maintenance/complete")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<IActionResult> CompleteMaintenance([FromBody] CompleteMaintenanceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

            // ✅ Here MaintenanceId is NOT available because no table.
            // So we treat request.MaintenanceId as AssetId OR you can change DTO.
            // Best: use AssetId in request. If you already use MaintenanceId in frontend, keep it as AssetId.
            var assetId = request.MaintenanceId; // <-- IMPORTANT: treating MaintenanceId as AssetId

            var asset = await _db.Assets.FirstOrDefaultAsync(a => a.AssetId == assetId && a.IsActive);
            if (asset == null)
                return NotFound(new { message = "Asset not found.", success = false });

            if (asset.Status != "maintenance")
                return BadRequest(new { message = "Asset is not in maintenance.", success = false });

            var adminId = GetCurrentUserId();

            asset.Status = "available";
            asset.UpdatedOn = DateTime.Now;

            asset.MaintenanceEndDate = DateTime.Now;
            asset.MaintenanceCost = request.Cost;
            asset.MaintenanceResolution = request.ResolutionNote;
            asset.MaintenanceCompletedByUserId = adminId;

            _db.AssetHistories.Add(new AssetHistory
            {
                AssetId = asset.AssetId,
                UserId = null,
                Action = "maintenance_completed",
                Note = request.ResolutionNote,
                ActionDate = DateTime.Now,
                ActionByUserId = adminId
            });

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Maintenance completed for asset '{asset.AssetName}'.",
                success = true,
                data = MapMaintenanceFromAsset(asset)
            });
        }

        // ════════════════════════════════════════════════════════════════
        //  9. GET /api/Asset/maintenance/list  (NO NEW TABLE)
        //     List assets which have maintenance info (or in maintenance)
        // ════════════════════════════════════════════════════════════════
        [HttpGet("maintenance/list")]
        public async Task<IActionResult> GetMaintenanceList(
            [FromQuery] int? assetId = null,
            [FromQuery] string? status = null,          // open => maintenance , completed => available with enddate, etc.
            [FromQuery] DateTime? fromDate = null,      // MaintenanceStartDate
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _db.Assets
                .Where(a => a.IsActive)
                .AsQueryable();

            if (!IsAdmin())
            {
                var currentUserId = GetCurrentUserId();
                query = query.Where(a => a.AssignedToUserId == currentUserId);
            }

            // only those having maintenance start date
            query = query.Where(a => a.MaintenanceStartDate != null);

            if (assetId.HasValue)
                query = query.Where(a => a.AssetId == assetId.Value);

            // status mapping:
            // open => Assets.Status == maintenance
            // completed => MaintenanceEndDate != null
            if (!string.IsNullOrEmpty(status))
            {
                var s = status.ToLower();
                if (s == "open")
                    query = query.Where(a => a.Status == "maintenance");
                else if (s == "completed")
                    query = query.Where(a => a.MaintenanceEndDate != null);
            }

            if (fromDate.HasValue)
                query = query.Where(a => a.MaintenanceStartDate >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(a => a.MaintenanceStartDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(a => a.MaintenanceStartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                message = "Maintenance list fetched successfully.",
                success = true,
                totalCount = total,
                page,
                pageSize,
                filter = new
                {
                    fromDate = fromDate?.ToString("yyyy-MM-dd"),
                    toDate = toDate?.ToString("yyyy-MM-dd")
                },
                data = data.Select(MapMaintenanceFromAsset)
            });
        }

        // ════════════════════════════════════════════════════════════════
        //  10. GET /api/Asset/maintenance/history?assetId=1
        //     Maintenance events from dbo.AssetHistories only
        // ════════════════════════════════════════════════════════════════
        [HttpGet("maintenance/history")]
        public async Task<IActionResult> GetMaintenanceHistory([FromQuery] int assetId)
        {
            var asset = await _db.Assets.FirstOrDefaultAsync(a => a.AssetId == assetId && a.IsActive);
            if (asset == null)
                return NotFound(new { message = "Asset not found.", success = false });

            if (!IsAdmin())
            {
                var currentUserId = GetCurrentUserId();
                if (asset.AssignedToUserId != currentUserId)
                    return Forbid();
            }

            var list = await _db.AssetHistories
                .Where(h => h.AssetId == assetId &&
                            (h.Action == "maintenance_started" || h.Action == "maintenance_completed"))
                .OrderByDescending(h => h.ActionDate)
                .ToListAsync();

            return Ok(new
            {
                message = "Maintenance history fetched successfully.",
                success = true,
                totalCount = list.Count,
                data = list.Select(h => new
                {
                    h.HistoryId,
                    h.AssetId,
                    h.Action,
                    h.Note,
                    h.ActionDate,
                    h.ActionByUserId
                })
            });
        }

        // ─── Helper: Map to Asset Response ────────────────────────────────
        private static AssetResponse MapAssetResponse(Asset a, string? assignedUserName) =>
            new AssetResponse
            {
                AssetId = a.AssetId,
                AssetName = a.AssetName,
                AssetType = a.AssetType,
                AssetCode = a.AssetCode,
                SerialNumber = a.SerialNumber,
                Brand = a.Brand,
                Model = a.Model,
                Description = a.Description,
                Status = a.Status,
                AssignedToUserId = a.AssignedToUserId,
                AssignedToUserName = assignedUserName,
                AssignedDate = a.AssignedDate,
                ExpectedReturnDate = a.ExpectedReturnDate,
                AssignmentNote = a.AssignmentNote,
                ReturnedDate = a.ReturnedDate,
                ReturnNote = a.ReturnNote,
                ReturnCondition = a.ReturnCondition,
                CreatedOn = a.CreatedOn
            };

        // ─── Helper: Map Maintenance from Asset (NO TABLE) ────────────────
        private static MaintenanceResponse MapMaintenanceFromAsset(Asset a) =>
            new MaintenanceResponse
            {
                // ✅ No maintenance table, so we use AssetId as MaintenanceId for API consistency
                MaintenanceId = a.AssetId,
                AssetId = a.AssetId,
                AssetName = a.AssetName,
                AssetType = a.AssetType,

                MaintenanceType = a.MaintenanceType ?? "",
                VendorName = a.MaintenanceVendorName,
                TicketNo = a.MaintenanceTicketNo,
                IssueDescription = a.MaintenanceIssue,

                StartDate = a.MaintenanceStartDate ?? a.CreatedOn,
                EndDate = a.MaintenanceEndDate,

                // open/completed mapping
                Status = a.Status == "maintenance" ? "open" : (a.MaintenanceEndDate != null ? "completed" : "open"),
                Cost = a.MaintenanceCost,
                ResolutionNote = a.MaintenanceResolution
            };
    }
}