// // ======================= Controllers/DocumentController.cs =======================
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
//     public class DocumentController : ControllerBase
//     {
//         private readonly ApplicationDbContext _db;
//         private readonly IWebHostEnvironment _env;

//         // ─── Role constants (same pattern as AssetController) ─────────────
//         private const string AdminRole   = "Admin";
//         private const string AdminLower  = "admin";
//         private const string AdminPolicy = "Admin,admin";

//         // ─── Allowed file types ───────────────────────────────────────────
//         private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
//         private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

//         public DocumentController(ApplicationDbContext db, IWebHostEnvironment env)
//         {
//             _db  = db;
//             _env = env;
//         }

//         // ─── Helpers ──────────────────────────────────────────────────────
//         private int GetCurrentUserId()
//         {
//             var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
//             return int.TryParse(claim, out var id) ? id : 0;
//         }

//         private bool IsAdmin() =>
//             User.IsInRole(AdminRole) || User.IsInRole(AdminLower);

//         // ════════════════════════════════════════════════════════════════
//         //  1. POST /api/Document/upload
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("upload")]
//         [Consumes("multipart/form-data")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         [ProducesResponseType(StatusCodes.Status400BadRequest)]
//         [ProducesResponseType(StatusCodes.Status404NotFound)]
//         public async Task<IActionResult> UploadDocument([FromForm] DocumentUploadRequest request)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             if (request.File == null || request.File.Length == 0)
//                 return BadRequest(new { message = "No file provided.", success = false });

//             var ext = Path.GetExtension(request.File.FileName).ToLowerInvariant();
//             if (!AllowedExtensions.Contains(ext))
//                 return BadRequest(new
//                 {
//                     message = $"File type '{ext}' is not allowed. Allowed: {string.Join(", ", AllowedExtensions)}",
//                     success = false
//                 });

//             if (request.File.Length > MaxFileSize)
//                 return BadRequest(new { message = "File size must not exceed 5MB.", success = false });

//             var employee = await _db.Users.FindAsync(request.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found or inactive.", success = false });

//             // ─── Save file to disk ────────────────────────────────────────
//             var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "documents");
//             Directory.CreateDirectory(uploadsFolder);

//             var uniqueFileName = $"{Guid.NewGuid()}{ext}";
//             var filePath       = Path.Combine(uploadsFolder, uniqueFileName);

//             using (var stream = new FileStream(filePath, FileMode.Create))
//                 await request.File.CopyToAsync(stream);

//             var relativePath = Path.Combine("uploads", "documents", uniqueFileName);

//             var document = new EmployeeDocument
//             {
//                 EmployeeId       = request.EmployeeId,
//                 DocumentType     = request.DocumentType,
//                 Description      = request.Description,
//                 FileName         = request.File.FileName,
//                 FilePath         = relativePath,
//                 FileSize         = request.File.Length,
//                 FileExtension    = ext,
//                 UploadedByUserId = GetCurrentUserId(),
//                 UploadedAt       = DateTime.UtcNow,
//                 IsDeleted        = false
//             };

//             _db.EmployeeDocuments.Add(document);
//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = "Document uploaded successfully.",
//                 success = true,
//                 data    = MapDocumentResponse(document, employee.UserName, null)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  2. GET /api/Document/list
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("list")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> GetDocumentList([FromQuery] DocumentListQuery q)
//         {
//             var query = _db.EmployeeDocuments
//                 .Include(d => d.Employee)
//                 .Include(d => d.UploadedByUser)
//                 .Where(d => !d.IsDeleted)
//                 .AsQueryable();

//             // Non-admin: sirf apne documents dekh sakta hai
//             if (!IsAdmin())
//             {
//                 var currentUserId = GetCurrentUserId();
//                 query = query.Where(d => d.EmployeeId == currentUserId);
//             }
//             else
//             {
//                 if (q.EmployeeId.HasValue)
//                     query = query.Where(d => d.EmployeeId == q.EmployeeId.Value);

//                 if (!string.IsNullOrEmpty(q.DocumentType))
//                     query = query.Where(d => d.DocumentType == q.DocumentType);
//             }

//             // Upload date filter
//             if (q.UploadedFrom.HasValue)
//                 query = query.Where(d => d.UploadedAt >= q.UploadedFrom.Value.Date);
//             if (q.UploadedTo.HasValue)
//                 query = query.Where(d => d.UploadedAt <= q.UploadedTo.Value.Date.AddDays(1).AddTicks(-1));

//             var documents = await query.OrderByDescending(d => d.UploadedAt).ToListAsync();

//             return Ok(new
//             {
//                 message    = "Documents fetched successfully.",
//                 success    = true,
//                 totalCount = documents.Count,
//                 filter = new
//                 {
//                     uploadedFrom = q.UploadedFrom?.ToString("yyyy-MM-dd"),
//                     uploadedTo   = q.UploadedTo?.ToString("yyyy-MM-dd")
//                 },
//                 data = documents.Select(d =>
//                     MapDocumentResponse(d, d.Employee?.UserName, d.UploadedByUser?.UserName))
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  3. DELETE /api/Document/remove  — Admin only
//         // ════════════════════════════════════════════════════════════════
//         [HttpDelete("remove")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         [ProducesResponseType(StatusCodes.Status404NotFound)]
//         public async Task<IActionResult> RemoveDocument([FromQuery] int documentId)
//         {
//             if (documentId <= 0)
//                 return BadRequest(new { message = "Invalid document ID.", success = false });

//             var document = await _db.EmployeeDocuments
//                 .FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted);

//             if (document == null)
//                 return NotFound(new { message = $"Document with ID {documentId} not found.", success = false });

//             // Soft delete — same pattern as Asset IsActive = false
//             document.IsDeleted = true;

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Document '{document.FileName}' deleted successfully.",
//                 success = true
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  4. GET /api/Document/summary  — Admin only
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("summary")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> GetDocumentSummary(
//             [FromQuery] DateTime? fromDate = null,
//             [FromQuery] DateTime? toDate   = null)
//         {
//             var query = _db.EmployeeDocuments.Where(d => !d.IsDeleted).AsQueryable();

//             if (fromDate.HasValue)
//                 query = query.Where(d => d.UploadedAt.Date >= fromDate.Value.Date);
//             if (toDate.HasValue)
//                 query = query.Where(d => d.UploadedAt.Date <= toDate.Value.Date);

//             var total = await query.CountAsync();

//             var byType = await query
//                 .GroupBy(d => d.DocumentType)
//                 .Select(g => new DocumentTypeCount { DocumentType = g.Key, Count = g.Count() })
//                 .OrderByDescending(x => x.Count)
//                 .ToListAsync();

//             return Ok(new
//             {
//                 message = "Document summary fetched successfully.",
//                 success = true,
//                 filter = new
//                 {
//                     fromDate = fromDate?.ToString("yyyy-MM-dd"),
//                     toDate   = toDate?.ToString("yyyy-MM-dd")
//                 },
//                 data = new DocumentSummaryResponse
//                 {
//                     Total  = total,
//                     ByType = byType
//                 }
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  Private Helper
//         // ════════════════════════════════════════════════════════════════
//         private static DocumentResponse MapDocumentResponse(
//             EmployeeDocument d,
//             string? employeeName,
//             string? uploadedByName) => new DocumentResponse
//         {
//             DocumentId          = d.DocumentId,
//             EmployeeId          = d.EmployeeId,
//             EmployeeName        = employeeName,
//             DocumentType        = d.DocumentType,
//             Description         = d.Description,
//             FileName            = d.FileName,
//             FilePath            = d.FilePath,
//             FileSize            = d.FileSize,
//             FileExtension       = d.FileExtension,
//             UploadedAt          = d.UploadedAt,
//             UploadedByUserId    = d.UploadedByUserId,
//             UploadedByUserName  = uploadedByName
//         };
//     }
// }









// // ======================= Controllers/DocumentController.cs =======================
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
//     public class DocumentController : ControllerBase
//     {
//         private readonly ApplicationDbContext _db;
//         private readonly IWebHostEnvironment _env;

//         // ─── Constants ────────────────────────────────────────────────────
//         private const string AdminRole   = "Admin";
//         private const string AdminLower  = "admin";
//         private const string AdminPolicy = "Admin,admin";

//         private static readonly string[] AllowedExtensions =
//             { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
//         private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

//         private const string StatusPending  = "pending";
//         private const string StatusApproved = "approved";
//         private const string StatusRejected = "rejected";

//         public DocumentController(ApplicationDbContext db, IWebHostEnvironment env)
//         {
//             _db  = db;
//             _env = env;
//         }

//         // ─── Helpers ──────────────────────────────────────────────────────
//         private int GetCurrentUserId()
//         {
//             var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
//             return int.TryParse(claim, out var id) ? id : 0;
//         }

//         private bool IsAdmin() =>
//             User.IsInRole(AdminRole) || User.IsInRole(AdminLower);

//         // ════════════════════════════════════════════════════════════════
//         //  1. POST /api/Document/upload
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("upload")]
//         [Consumes("multipart/form-data")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         [ProducesResponseType(StatusCodes.Status400BadRequest)]
//         [ProducesResponseType(StatusCodes.Status404NotFound)]
//         public async Task<IActionResult> UploadDocument([FromForm] DocumentUploadRequest request)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             if (request.File == null || request.File.Length == 0)
//                 return BadRequest(new { message = "No file provided.", success = false });

//             var ext = Path.GetExtension(request.File.FileName).ToLowerInvariant();
//             if (!AllowedExtensions.Contains(ext))
//                 return BadRequest(new
//                 {
//                     message = $"File type '{ext}' not allowed. Allowed: {string.Join(", ", AllowedExtensions)}",
//                     success = false
//                 });

//             if (request.File.Length > MaxFileSize)
//                 return BadRequest(new { message = "File size must not exceed 5MB.", success = false });

//             var employee = await _db.Users.FindAsync(request.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found or inactive.", success = false });

//             var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "documents");
//             Directory.CreateDirectory(uploadsFolder);

//             var uniqueFileName = $"{Guid.NewGuid()}{ext}";
//             var filePath       = Path.Combine(uploadsFolder, uniqueFileName);

//             using (var stream = new FileStream(filePath, FileMode.Create))
//                 await request.File.CopyToAsync(stream);

//             var document = new EmployeeDocument
//             {
//                 EmployeeId       = request.EmployeeId,
//                 DocumentType     = request.DocumentType,
//                 Description      = request.Description,
//                 FileName         = request.File.FileName,
//                 FilePath         = Path.Combine("uploads", "documents", uniqueFileName),
//                 FileSize         = request.File.Length,
//                 FileExtension    = ext,
//                 UploadedByUserId = GetCurrentUserId(),
//                 UploadedAt       = DateTime.UtcNow,
//                 VerifyStatus     = StatusPending,   // ← Fix: explicit set
//                 IsDeleted        = false
//             };

//             _db.EmployeeDocuments.Add(document);
//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = "Document uploaded successfully.",
//                 success = true,
//                 data    = MapDocumentResponse(document, employee.UserName, null)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  2. GET /api/Document/list
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("list")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> GetDocumentList([FromQuery] DocumentListQuery q)
//         {
//             var query = _db.EmployeeDocuments
//                 .Include(d => d.Employee)
//                 .Include(d => d.UploadedByUser)
//                 .Where(d => !d.IsDeleted)
//                 .AsQueryable();

//             if (!IsAdmin())
//             {
//                 var currentUserId = GetCurrentUserId();
//                 query = query.Where(d => d.EmployeeId == currentUserId);
//             }
//             else
//             {
//                 if (q.EmployeeId.HasValue)
//                     query = query.Where(d => d.EmployeeId == q.EmployeeId.Value);

//                 if (!string.IsNullOrEmpty(q.DocumentType))
//                     query = query.Where(d => d.DocumentType == q.DocumentType);

//                 if (!string.IsNullOrEmpty(q.VerifyStatus))
//                     query = query.Where(d => d.VerifyStatus == q.VerifyStatus.ToLower());
//             }

//             if (q.UploadedFrom.HasValue)
//                 query = query.Where(d => d.UploadedAt >= q.UploadedFrom.Value.Date);
//             if (q.UploadedTo.HasValue)
//                 query = query.Where(d => d.UploadedAt <= q.UploadedTo.Value.Date.AddDays(1).AddTicks(-1));

//             var documents = await query.OrderByDescending(d => d.UploadedAt).ToListAsync();

//             return Ok(new
//             {
//                 message    = "Documents fetched successfully.",
//                 success    = true,
//                 totalCount = documents.Count,
//                 filter = new
//                 {
//                     uploadedFrom = q.UploadedFrom?.ToString("yyyy-MM-dd"),
//                     uploadedTo   = q.UploadedTo?.ToString("yyyy-MM-dd"),
//                     verifyStatus = q.VerifyStatus
//                 },
//                 data = documents.Select(d =>
//                     MapDocumentResponse(d, d.Employee?.UserName, d.UploadedByUser?.UserName))
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  3. GET /api/Document/download?documentId=1
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("download")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         [ProducesResponseType(StatusCodes.Status403Forbidden)]
//         [ProducesResponseType(StatusCodes.Status404NotFound)]
//         public async Task<IActionResult> DownloadDocument([FromQuery] int documentId)
//         {
//             if (documentId <= 0)
//                 return BadRequest(new { message = "Invalid document ID.", success = false });

//             var document = await _db.EmployeeDocuments
//                 .FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted);

//             if (document == null)
//                 return NotFound(new { message = "Document not found.", success = false });

//             if (!IsAdmin() && document.EmployeeId != GetCurrentUserId())
//                 return Forbid();

//             var fullPath = Path.Combine(_env.WebRootPath ?? "wwwroot", document.FilePath);
//             if (!System.IO.File.Exists(fullPath))
//                 return NotFound(new { message = "File not found on server.", success = false });

//             var mimeType = document.FileExtension switch
//             {
//                 ".pdf"  => "application/pdf",
//                 ".jpg"  => "image/jpeg",
//                 ".jpeg" => "image/jpeg",
//                 ".png"  => "image/png",
//                 ".doc"  => "application/msword",
//                 ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
//                 _       => "application/octet-stream"
//             };

//             var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
//             return File(fileBytes, mimeType, document.FileName);
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  4. PUT /api/Document/update
//         // ════════════════════════════════════════════════════════════════
//         [HttpPut("update")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         [ProducesResponseType(StatusCodes.Status403Forbidden)]
//         [ProducesResponseType(StatusCodes.Status404NotFound)]
//         public async Task<IActionResult> UpdateDocument([FromBody] DocumentUpdateRequest request)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             var document = await _db.EmployeeDocuments
//                 .Include(d => d.Employee)
//                 .FirstOrDefaultAsync(d => d.DocumentId == request.DocumentId && !d.IsDeleted);

//             if (document == null)
//                 return NotFound(new { message = "Document not found.", success = false });

//             if (!IsAdmin() && document.EmployeeId != GetCurrentUserId())
//                 return Forbid();

//             if (!string.IsNullOrWhiteSpace(request.DocumentType))
//                 document.DocumentType = request.DocumentType;

//             if (request.Description != null)
//                 document.Description = request.Description;

//             if (!string.IsNullOrWhiteSpace(request.FileName))
//                 document.FileName = request.FileName;

//             // Update ke baad re-verify
//             document.VerifyStatus  = StatusPending;
//             document.VerifiedBy    = null;
//             document.VerifiedAt    = null;
//             document.VerifyRemarks = null;

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = "Document updated successfully.",
//                 success = true,
//                 data    = MapDocumentResponse(document, document.Employee?.UserName, null)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  5. DELETE /api/Document/remove  — Admin only
//         // ════════════════════════════════════════════════════════════════
//         [HttpDelete("remove")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         [ProducesResponseType(StatusCodes.Status404NotFound)]
//         public async Task<IActionResult> RemoveDocument([FromQuery] int documentId)
//         {
//             if (documentId <= 0)
//                 return BadRequest(new { message = "Invalid document ID.", success = false });

//             var document = await _db.EmployeeDocuments
//                 .FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted);

//             if (document == null)
//                 return NotFound(new { message = $"Document with ID {documentId} not found.", success = false });

//             document.IsDeleted = true;
//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Document '{document.FileName}' deleted successfully.",
//                 success = true
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  6. PUT /api/Document/restore  — Admin only
//         // ════════════════════════════════════════════════════════════════
//         [HttpPut("restore")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         [ProducesResponseType(StatusCodes.Status404NotFound)]
//         public async Task<IActionResult> RestoreDocument([FromQuery] int documentId)
//         {
//             if (documentId <= 0)
//                 return BadRequest(new { message = "Invalid document ID.", success = false });

//             var document = await _db.EmployeeDocuments
//                 .Include(d => d.Employee)
//                 .FirstOrDefaultAsync(d => d.DocumentId == documentId && d.IsDeleted);

//             if (document == null)
//                 return NotFound(new { message = $"Deleted document with ID {documentId} not found.", success = false });

//             document.IsDeleted    = false;
//             document.VerifyStatus = StatusPending;

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Document '{document.FileName}' restored successfully.",
//                 success = true,
//                 data    = MapDocumentResponse(document, document.Employee?.UserName, null)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  7. PUT /api/Document/verify  — Admin only
//         // ════════════════════════════════════════════════════════════════
//         [HttpPut("verify")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         [ProducesResponseType(StatusCodes.Status400BadRequest)]
//         [ProducesResponseType(StatusCodes.Status404NotFound)]
//         public async Task<IActionResult> VerifyDocument([FromBody] DocumentVerifyRequest request)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             var allowedStatuses = new[] { StatusApproved, StatusRejected };
//             if (!allowedStatuses.Contains(request.Status.ToLower()))
//                 return BadRequest(new
//                 {
//                     message = $"Invalid status. Allowed: {string.Join(", ", allowedStatuses)}",
//                     success = false
//                 });

//             var document = await _db.EmployeeDocuments
//                 .Include(d => d.Employee)
//                 .FirstOrDefaultAsync(d => d.DocumentId == request.DocumentId && !d.IsDeleted);

//             if (document == null)
//                 return NotFound(new { message = "Document not found.", success = false });

//             if (document.VerifyStatus != StatusPending)
//                 return BadRequest(new
//                 {
//                     message = $"Document is already '{document.VerifyStatus}'. Only pending documents can be verified.",
//                     success = false
//                 });

//             document.VerifyStatus  = request.Status.ToLower();
//             document.VerifiedBy    = GetCurrentUserId();
//             document.VerifiedAt    = DateTime.UtcNow;
//             document.VerifyRemarks = request.Remarks;

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Document '{document.FileName}' marked as '{document.VerifyStatus}'.",
//                 success = true,
//                 data    = MapDocumentResponse(document, document.Employee?.UserName, null)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  8. GET /api/Document/summary  — Admin only
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("summary")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> GetDocumentSummary(
//             [FromQuery] DateTime? fromDate = null,
//             [FromQuery] DateTime? toDate   = null)
//         {
//             var query = _db.EmployeeDocuments.Where(d => !d.IsDeleted).AsQueryable();

//             if (fromDate.HasValue)
//                 query = query.Where(d => d.UploadedAt.Date >= fromDate.Value.Date);
//             if (toDate.HasValue)
//                 query = query.Where(d => d.UploadedAt.Date <= toDate.Value.Date);

//             var total    = await query.CountAsync();
//             var pending  = await query.CountAsync(d => d.VerifyStatus == StatusPending);   // ← Fix
//             var approved = await query.CountAsync(d => d.VerifyStatus == StatusApproved);  // ← Fix
//             var rejected = await query.CountAsync(d => d.VerifyStatus == StatusRejected);  // ← Fix

//             var byType = await query
//                 .GroupBy(d => d.DocumentType)
//                 .Select(g => new DocumentTypeCount { DocumentType = g.Key, Count = g.Count() })
//                 .OrderByDescending(x => x.Count)
//                 .ToListAsync();

//             return Ok(new
//             {
//                 message = "Document summary fetched successfully.",
//                 success = true,
//                 filter = new
//                 {
//                     fromDate = fromDate?.ToString("yyyy-MM-dd"),
//                     toDate   = toDate?.ToString("yyyy-MM-dd")
//                 },
//                 data = new DocumentSummaryResponse
//                 {
//                     Total    = total,
//                     Pending  = pending,    // ← Fix
//                     Approved = approved,   // ← Fix
//                     Rejected = rejected,   // ← Fix
//                     ByType   = byType
//                 }
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  Private Helper
//         // ════════════════════════════════════════════════════════════════
//         private static DocumentResponse MapDocumentResponse(
//             EmployeeDocument d,
//             string? employeeName,
//             string? uploadedByName) => new DocumentResponse
//         {
//             DocumentId         = d.DocumentId,
//             EmployeeId         = d.EmployeeId,
//             EmployeeName       = employeeName,
//             DocumentType       = d.DocumentType,
//             Description        = d.Description,
//             FileName           = d.FileName,
//             FilePath           = d.FilePath,
//             FileSize           = d.FileSize,
//             FileExtension      = d.FileExtension,
//             UploadedAt         = d.UploadedAt,
//             UploadedByUserId   = d.UploadedByUserId,
//             UploadedByUserName = uploadedByName,
//             VerifyStatus       = d.VerifyStatus,    // ← Fix
//             VerifiedAt         = d.VerifiedAt,      // ← Fix
//             VerifyRemarks      = d.VerifyRemarks    // ← Fix
//         };
//     }
// }







// // ======================= Controllers/DocumentController.cs =======================
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
//     public class DocumentController : ControllerBase
//     {
//         private readonly ApplicationDbContext _db;
//         private readonly IWebHostEnvironment _env;

//         // ─── Constants ────────────────────────────────────────────────────
//         private const string AdminRole   = "Admin";
//         private const string AdminLower  = "admin";
//         private const string AdminPolicy = "Admin,admin";

//         private static readonly string[] AllowedExtensions =
//             { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
//         private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

//         private const string StatusPending  = "pending";
//         private const string StatusApproved = "approved";
//         private const string StatusRejected = "rejected";

//         public DocumentController(ApplicationDbContext db, IWebHostEnvironment env)
//         {
//             _db  = db;
//             _env = env;
//         }

//         // ─── Helpers ──────────────────────────────────────────────────────
//         private int GetCurrentUserId()
//         {
//             var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
//             return int.TryParse(claim, out var id) ? id : 0;
//         }

//         private bool IsAdmin() =>
//             User.IsInRole(AdminRole) || User.IsInRole(AdminLower);

//         // ════════════════════════════════════════════════════════════════
//         //  1. POST /api/Document/upload
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("upload")]
//         [Consumes("multipart/form-data")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         [ProducesResponseType(StatusCodes.Status400BadRequest)]
//         [ProducesResponseType(StatusCodes.Status404NotFound)]
//         public async Task<IActionResult> UploadDocument([FromForm] DocumentUploadRequest request)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             if (request.File == null || request.File.Length == 0)
//                 return BadRequest(new { message = "No file provided.", success = false });

//             var ext = Path.GetExtension(request.File.FileName).ToLowerInvariant();
//             if (!AllowedExtensions.Contains(ext))
//                 return BadRequest(new
//                 {
//                     message = $"File type '{ext}' not allowed. Allowed: {string.Join(", ", AllowedExtensions)}",
//                     success = false
//                 });

//             if (request.File.Length > MaxFileSize)
//                 return BadRequest(new { message = "File size must not exceed 5MB.", success = false });

//             var employee = await _db.Users.FindAsync(request.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found or inactive.", success = false });

//             var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "documents");
//             Directory.CreateDirectory(uploadsFolder);

//             var uniqueFileName = $"{Guid.NewGuid()}{ext}";
//             var filePath       = Path.Combine(uploadsFolder, uniqueFileName);

//             using (var stream = new FileStream(filePath, FileMode.Create))
//                 await request.File.CopyToAsync(stream);

//             var document = new EmployeeDocument
//             {
//                 EmployeeId       = request.EmployeeId,
//                 DocumentType     = request.DocumentType,
//                 Description      = request.Description,
//                 FileName         = request.File.FileName,
//                 FilePath         = Path.Combine("uploads", "documents", uniqueFileName),
//                 FileSize         = request.File.Length,
//                 FileExtension    = ext,
//                 UploadedByUserId = GetCurrentUserId(),
//                 UploadedAt       = DateTime.UtcNow,
//                 VerifyStatus     = StatusPending,
//                 IsDeleted        = false
//             };

//             _db.EmployeeDocuments.Add(document);
//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = "Document uploaded successfully.",
//                 success = true,
//                 data    = MapDocumentResponse(document, employee.UserName, null)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  2. GET /api/Document/list?employeeId=17
//         //  EmployeeId required — bina ID ke data nahi aayega
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("list")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> GetDocumentList([FromQuery] DocumentListQuery q)
//         {
//             // EmployeeId nahi diya → empty response
//             if (!q.EmployeeId.HasValue || q.EmployeeId.Value <= 0)
//                 return Ok(new
//                 {
//                     message    = "Please provide EmployeeId to get documents.",
//                     success    = false,
//                     totalCount = 0,
//                     data       = Array.Empty<object>()
//                 });

//             // Non-admin: sirf apna ID use kar sakta hai
//             if (!IsAdmin() && q.EmployeeId.Value != GetCurrentUserId())
//                 return Forbid();

//             var documents = await _db.EmployeeDocuments
//                 .Include(d => d.Employee)
//                 .Include(d => d.UploadedByUser)
//                 .Where(d => !d.IsDeleted && d.EmployeeId == q.EmployeeId.Value)
//                 .OrderByDescending(d => d.UploadedAt)
//                 .ToListAsync();

//             return Ok(new
//             {
//                 message    = "Documents fetched successfully.",
//                 success    = true,
//                 totalCount = documents.Count,
//                 data       = documents.Select(d =>
//                     MapDocumentResponse(d, d.Employee?.UserName, d.UploadedByUser?.UserName))
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  3. GET /api/Document/download?documentId=1
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("download")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         [ProducesResponseType(StatusCodes.Status403Forbidden)]
//         [ProducesResponseType(StatusCodes.Status404NotFound)]
//         public async Task<IActionResult> DownloadDocument([FromQuery] int documentId)
//         {
//             if (documentId <= 0)
//                 return BadRequest(new { message = "Invalid document ID.", success = false });

//             var document = await _db.EmployeeDocuments
//                 .FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted);

//             if (document == null)
//                 return NotFound(new { message = "Document not found.", success = false });

//             if (!IsAdmin() && document.EmployeeId != GetCurrentUserId())
//                 return Forbid();

//             var fullPath = Path.Combine(_env.WebRootPath ?? "wwwroot", document.FilePath);
//             if (!System.IO.File.Exists(fullPath))
//                 return NotFound(new { message = "File not found on server.", success = false });

//             var mimeType = document.FileExtension switch
//             {
//                 ".pdf"  => "application/pdf",
//                 ".jpg"  => "image/jpeg",
//                 ".jpeg" => "image/jpeg",
//                 ".png"  => "image/png",
//                 ".doc"  => "application/msword",
//                 ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
//                 _       => "application/octet-stream"
//             };

//             var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
//             return File(fileBytes, mimeType, document.FileName);
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  4. PUT /api/Document/update
//         // ════════════════════════════════════════════════════════════════
//         [HttpPut("update")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         [ProducesResponseType(StatusCodes.Status403Forbidden)]
//         [ProducesResponseType(StatusCodes.Status404NotFound)]
//         public async Task<IActionResult> UpdateDocument([FromBody] DocumentUpdateRequest request)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             var document = await _db.EmployeeDocuments
//                 .Include(d => d.Employee)
//                 .FirstOrDefaultAsync(d => d.DocumentId == request.DocumentId && !d.IsDeleted);

//             if (document == null)
//                 return NotFound(new { message = "Document not found.", success = false });

//             if (!IsAdmin() && document.EmployeeId != GetCurrentUserId())
//                 return Forbid();

//             if (!string.IsNullOrWhiteSpace(request.DocumentType))
//                 document.DocumentType = request.DocumentType;

//             if (request.Description != null)
//                 document.Description = request.Description;

//             if (!string.IsNullOrWhiteSpace(request.FileName))
//                 document.FileName = request.FileName;

//             // Update ke baad re-verify
//             document.VerifyStatus  = StatusPending;
//             document.VerifiedBy    = null;
//             document.VerifiedAt    = null;
//             document.VerifyRemarks = null;

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = "Document updated successfully.",
//                 success = true,
//                 data    = MapDocumentResponse(document, document.Employee?.UserName, null)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  5. DELETE /api/Document/remove  — Admin only
//         // ════════════════════════════════════════════════════════════════
//         [HttpDelete("remove")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         [ProducesResponseType(StatusCodes.Status404NotFound)]
//         public async Task<IActionResult> RemoveDocument([FromQuery] int documentId)
//         {
//             if (documentId <= 0)
//                 return BadRequest(new { message = "Invalid document ID.", success = false });

//             var document = await _db.EmployeeDocuments
//                 .FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted);

//             if (document == null)
//                 return NotFound(new { message = $"Document with ID {documentId} not found.", success = false });

//             document.IsDeleted = true;
//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Document '{document.FileName}' deleted successfully.",
//                 success = true
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  6. PUT /api/Document/restore  — Admin only
//         // ════════════════════════════════════════════════════════════════
//         [HttpPut("restore")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         [ProducesResponseType(StatusCodes.Status404NotFound)]
//         public async Task<IActionResult> RestoreDocument([FromQuery] int documentId)
//         {
//             if (documentId <= 0)
//                 return BadRequest(new { message = "Invalid document ID.", success = false });

//             var document = await _db.EmployeeDocuments
//                 .Include(d => d.Employee)
//                 .FirstOrDefaultAsync(d => d.DocumentId == documentId && d.IsDeleted);

//             if (document == null)
//                 return NotFound(new { message = $"Deleted document with ID {documentId} not found.", success = false });

//             document.IsDeleted    = false;
//             document.VerifyStatus = StatusPending;

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Document '{document.FileName}' restored successfully.",
//                 success = true,
//                 data    = MapDocumentResponse(document, document.Employee?.UserName, null)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  7. PUT /api/Document/verify  — Admin only
//         // ════════════════════════════════════════════════════════════════
//         [HttpPut("verify")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         [ProducesResponseType(StatusCodes.Status400BadRequest)]
//         [ProducesResponseType(StatusCodes.Status404NotFound)]
//         public async Task<IActionResult> VerifyDocument([FromBody] DocumentVerifyRequest request)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             var allowedStatuses = new[] { StatusApproved, StatusRejected };
//             if (!allowedStatuses.Contains(request.Status.ToLower()))
//                 return BadRequest(new
//                 {
//                     message = $"Invalid status. Allowed: {string.Join(", ", allowedStatuses)}",
//                     success = false
//                 });

//             var document = await _db.EmployeeDocuments
//                 .Include(d => d.Employee)
//                 .FirstOrDefaultAsync(d => d.DocumentId == request.DocumentId && !d.IsDeleted);

//             if (document == null)
//                 return NotFound(new { message = "Document not found.", success = false });

//             if (document.VerifyStatus != StatusPending)
//                 return BadRequest(new
//                 {
//                     message = $"Document is already '{document.VerifyStatus}'. Only pending documents can be verified.",
//                     success = false
//                 });

//             document.VerifyStatus  = request.Status.ToLower();
//             document.VerifiedBy    = GetCurrentUserId();
//             document.VerifiedAt    = DateTime.UtcNow;
//             document.VerifyRemarks = request.Remarks;

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Document '{document.FileName}' marked as '{document.VerifyStatus}'.",
//                 success = true,
//                 data    = MapDocumentResponse(document, document.Employee?.UserName, null)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  8. GET /api/Document/summary  — Admin only
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("summary")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> GetDocumentSummary(
//             [FromQuery] DateTime? fromDate = null,
//             [FromQuery] DateTime? toDate   = null)
//         {
//             var query = _db.EmployeeDocuments.Where(d => !d.IsDeleted).AsQueryable();

//             if (fromDate.HasValue)
//                 query = query.Where(d => d.UploadedAt.Date >= fromDate.Value.Date);
//             if (toDate.HasValue)
//                 query = query.Where(d => d.UploadedAt.Date <= toDate.Value.Date);

//             var total    = await query.CountAsync();
//             var pending  = await query.CountAsync(d => d.VerifyStatus == StatusPending);
//             var approved = await query.CountAsync(d => d.VerifyStatus == StatusApproved);
//             var rejected = await query.CountAsync(d => d.VerifyStatus == StatusRejected);

//             var byType = await query
//                 .GroupBy(d => d.DocumentType)
//                 .Select(g => new DocumentTypeCount { DocumentType = g.Key, Count = g.Count() })
//                 .OrderByDescending(x => x.Count)
//                 .ToListAsync();

//             return Ok(new
//             {
//                 message = "Document summary fetched successfully.",
//                 success = true,
//                 filter = new
//                 {
//                     fromDate = fromDate?.ToString("yyyy-MM-dd"),
//                     toDate   = toDate?.ToString("yyyy-MM-dd")
//                 },
//                 data = new DocumentSummaryResponse
//                 {
//                     Total    = total,
//                     Pending  = pending,
//                     Approved = approved,
//                     Rejected = rejected,
//                     ByType   = byType
//                 }
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  Private Helper
//         // ════════════════════════════════════════════════════════════════
//         private static DocumentResponse MapDocumentResponse(
//             EmployeeDocument d,
//             string? employeeName,
//             string? uploadedByName) => new DocumentResponse
//         {
//             DocumentId         = d.DocumentId,
//             EmployeeId         = d.EmployeeId,
//             EmployeeName       = employeeName,
//             DocumentType       = d.DocumentType,
//             Description        = d.Description,
//             FileName           = d.FileName,
//             FilePath           = d.FilePath,
//             FileSize           = d.FileSize,
//             FileExtension      = d.FileExtension,
//             UploadedAt         = d.UploadedAt,
//             UploadedByUserId   = d.UploadedByUserId,
//             UploadedByUserName = uploadedByName,
//             VerifyStatus       = d.VerifyStatus,
//             VerifiedAt         = d.VerifiedAt,
//             VerifyRemarks      = d.VerifyRemarks
//         };
//     }
// }









// ======================= Controllers/DocumentController.cs =======================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using attendance_api.Data;
using attendance_api.DTOs;
using attendance_api.Models;
using attendance_api.Services;
using System.Security.Claims;

namespace attendance_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly IDocumentPdfService _pdfService;

        // ─── Constants ────────────────────────────────────────────────────
        private const string AdminRole   = "Admin";
        private const string AdminLower  = "admin";
        private const string AdminPolicy = "Admin,admin";

        private static readonly string[] AllowedExtensions =
            { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        private const string StatusPending  = "pending";
        private const string StatusApproved = "approved";
        private const string StatusRejected = "rejected";

        public DocumentController(ApplicationDbContext db, IWebHostEnvironment env, IDocumentPdfService pdfService)
        {
            _db         = db;
            _env        = env;
            _pdfService = pdfService;
        }

        // ─── Helpers ──────────────────────────────────────────────────────
        private int GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : 0;
        }

        private bool IsAdmin() =>
            User.IsInRole(AdminRole) || User.IsInRole(AdminLower);

        // ════════════════════════════════════════════════════════════════
        //  1. POST /api/Document/upload
        // ════════════════════════════════════════════════════════════════
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        // [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadDocument([FromForm] DocumentUploadRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

            if (request.File == null || request.File.Length == 0)
                return BadRequest(new { message = "No file provided.", success = false });

            var ext = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return BadRequest(new
                {
                    message = $"File type '{ext}' not allowed. Allowed: {string.Join(", ", AllowedExtensions)}",
                    success = false
                });

            if (request.File.Length > MaxFileSize)
                return BadRequest(new { message = "File size must not exceed 5MB.", success = false });

            var employee = await _db.Users.FindAsync(request.EmployeeId);
            if (employee == null || !employee.IsActive)
                return NotFound(new { message = "Employee not found or inactive.", success = false });

            try
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "documents");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{ext}";
                var filePath       = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await request.File.CopyToAsync(stream);

                var document = new EmployeeDocument
                {
                    EmployeeId       = request.EmployeeId,
                    DocumentType     = request.DocumentType,
                    Description      = request.Description,
                    FileName         = request.File.FileName,
                    FilePath         = Path.Combine("uploads", "documents", uniqueFileName),
                    FileSize         = request.File.Length,
                    FileExtension    = ext,
                    UploadedByUserId = GetCurrentUserId(),
                    UploadedAt       = DateTime.UtcNow,
                    VerifyStatus     = StatusPending,
                    IsDeleted        = false
                };

                _db.EmployeeDocuments.Add(document);
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    message = "Document uploaded successfully.",
                    success = true,
                    data    = MapDocumentResponse(document, employee.UserName, null)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Upload failed: {ex.Message}", success = false });
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  2. GET /api/Document/list?employeeId=17
        // ════════════════════════════════════════════════════════════════
        [HttpGet("list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDocumentList([FromQuery] DocumentListQuery q)
        {
            if (!q.EmployeeId.HasValue || q.EmployeeId.Value <= 0)
                return Ok(new
                {
                    message    = "Please provide EmployeeId to get documents.",
                    success    = false,
                    totalCount = 0,
                    data       = Array.Empty<object>()
                });

            if (!IsAdmin() && q.EmployeeId.Value != GetCurrentUserId())
                return Forbid();

            var documents = await _db.EmployeeDocuments
                .Include(d => d.Employee)
                .Include(d => d.UploadedByUser)
                .Where(d => !d.IsDeleted && d.EmployeeId == q.EmployeeId.Value)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            return Ok(new
            {
                message    = "Documents fetched successfully.",
                success    = true,
                totalCount = documents.Count,
                data       = documents.Select(d =>
                    MapDocumentResponse(d, d.Employee?.UserName, d.UploadedByUser?.UserName))
            });
        }

        // ════════════════════════════════════════════════════════════════
        //  3. GET /api/Document/download-pdf?employeeId=17
        //  PDF report — employee ke sare documents ki list
        // ════════════════════════════════════════════════════════════════
        [HttpGet("download-pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        // [ProducesResponseType(StatusCodes.Status403Forbidden)]
        // [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadDocumentsPdf([FromQuery] int employeeId)
        {
            if (employeeId <= 0)
                return BadRequest(new { message = "Invalid employee ID.", success = false });

            if (!IsAdmin() && employeeId != GetCurrentUserId())
                return Forbid();

            var employee = await _db.Users.FindAsync(employeeId);
            if (employee == null || !employee.IsActive)
                return NotFound(new { message = "Employee not found.", success = false });

            var documents = await _db.EmployeeDocuments
                .Where(d => !d.IsDeleted && d.EmployeeId == employeeId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            try
            {
                var pdfBytes = _pdfService.GenerateDocumentListPdf(documents, employee.UserName);
                var fileName = $"Documents_{employee.UserName}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"PDF generation failed: {ex.Message}", success = false });
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  4. DELETE /api/Document/remove  — Admin only
        // ════════════════════════════════════════════════════════════════
        [HttpDelete("remove")]
        [Authorize(Roles = AdminPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        // [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveDocument([FromQuery] int documentId)
        {
            if (documentId <= 0)
                return BadRequest(new { message = "Invalid document ID.", success = false });

            var document = await _db.EmployeeDocuments
                .FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted);

            if (document == null)
                return NotFound(new { message = $"Document with ID {documentId} not found.", success = false });

            document.IsDeleted = true;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Document '{document.FileName}' deleted successfully.",
                success = true
            });
        }

        // ════════════════════════════════════════════════════════════════
        //  5. PUT /api/Document/verify  — Admin only
        // ════════════════════════════════════════════════════════════════
        [HttpPut("verify")]
        [Authorize(Roles = AdminPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        // [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VerifyDocument([FromBody] DocumentVerifyRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

            var allowedStatuses = new[] { StatusApproved, StatusRejected };
            if (!allowedStatuses.Contains(request.Status.ToLower()))
                return BadRequest(new
                {
                    message = $"Invalid status. Allowed: {string.Join(", ", allowedStatuses)}",
                    success = false
                });

            var document = await _db.EmployeeDocuments
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(d => d.DocumentId == request.DocumentId && !d.IsDeleted);

            if (document == null)
                return NotFound(new { message = "Document not found.", success = false });

            if (document.VerifyStatus != StatusPending)
                return BadRequest(new
                {
                    message = $"Document is already '{document.VerifyStatus}'. Only pending documents can be verified.",
                    success = false
                });

            document.VerifyStatus  = request.Status.ToLower();
            document.VerifiedBy    = GetCurrentUserId();
            document.VerifiedAt    = DateTime.UtcNow;
            document.VerifyRemarks = request.Remarks;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Document '{document.FileName}' marked as '{document.VerifyStatus}'.",
                success = true,
                data    = MapDocumentResponse(document, document.Employee?.UserName, null)
            });
        }

        // ════════════════════════════════════════════════════════════════
        //  6. GET /api/Document/summary  — Admin only
        // ════════════════════════════════════════════════════════════════
        [HttpGet("summary")]
        [Authorize(Roles = AdminPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDocumentSummary(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate   = null)
        {
            var query = _db.EmployeeDocuments.Where(d => !d.IsDeleted).AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(d => d.UploadedAt.Date >= fromDate.Value.Date);
            if (toDate.HasValue)
                query = query.Where(d => d.UploadedAt.Date <= toDate.Value.Date);

            var total    = await query.CountAsync();
            var pending  = await query.CountAsync(d => d.VerifyStatus == StatusPending);
            var approved = await query.CountAsync(d => d.VerifyStatus == StatusApproved);
            var rejected = await query.CountAsync(d => d.VerifyStatus == StatusRejected);

            var byType = await query
                .GroupBy(d => d.DocumentType)
                .Select(g => new DocumentTypeCount { DocumentType = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return Ok(new
            {
                message = "Document summary fetched successfully.",
                success = true,
                filter = new
                {
                    fromDate = fromDate?.ToString("yyyy-MM-dd"),
                    toDate   = toDate?.ToString("yyyy-MM-dd")
                },
                data = new DocumentSummaryResponse
                {
                    Total    = total,
                    Pending  = pending,
                    Approved = approved,
                    Rejected = rejected,
                    ByType   = byType
                }
            });
        }

        // ════════════════════════════════════════════════════════════════
        //  Private Helper
        // ════════════════════════════════════════════════════════════════
        private static DocumentResponse MapDocumentResponse(
            EmployeeDocument d,
            string? employeeName,
            string? uploadedByName) => new DocumentResponse
        {
            DocumentId         = d.DocumentId,
            EmployeeId         = d.EmployeeId,
            EmployeeName       = employeeName,
            DocumentType       = d.DocumentType,
            Description        = d.Description,
            FileName           = d.FileName,
            FilePath           = d.FilePath,
            FileSize           = d.FileSize,
            FileExtension      = d.FileExtension,
            UploadedAt         = d.UploadedAt,
            UploadedByUserId   = d.UploadedByUserId,
            UploadedByUserName = uploadedByName,
            VerifyStatus       = d.VerifyStatus,
            VerifiedAt         = d.VerifiedAt,
            VerifyRemarks      = d.VerifyRemarks
        };
    }
}