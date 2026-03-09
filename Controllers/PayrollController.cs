// // ======================= Controllers/PayrollController.cs =======================
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using attendance_api.Data;
// using attendance_api.DTOs;
// using attendance_api.Models;
// using attendance_api.Services;
// using System.Security.Claims;

// namespace attendance_api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     [Authorize]
//     public class PayrollController : ControllerBase
//     {
//         private readonly ApplicationDbContext _db;
//         private readonly IPayrollExportService _exportService;

//         private const string AdminRole   = "Admin";
//         private const string AdminLower  = "admin";
//         private const string AdminPolicy = "Admin,admin";

//         public PayrollController(ApplicationDbContext db, IPayrollExportService exportService)
//         {
//             _db            = db;
//             _exportService = exportService;
//         }

//         private int  GetCurrentUserId() =>
//             int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

//         private bool IsAdmin() =>
//             User.IsInRole(AdminRole) || User.IsInRole(AdminLower);

//         // ════════════════════════════════════════════════════════════════
//         //  1. GET /api/Payroll/calculate
//         //  Attendance se salary calculate karke PayrollRecord save karta hai
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("calculate")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         // [ProducesResponseType(StatusCodes.Status404NotFound)]
//         public async Task<IActionResult> CalculatePayroll([FromQuery] PayrollCalculateRequest req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             // Non-admin sirf apna payroll dekh sakta hai
//             if (!IsAdmin() && req.EmployeeId != GetCurrentUserId())
//                 return Forbid();

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found or inactive.", success = false });

//             // ── Attendance records fetch ───────────────────────────────
//             var from = new DateTime(req.Year, req.Month, 1);
//             var to   = from.AddMonths(1).AddDays(-1);

//             var attendanceList = await _db.Attendances
//                 .Where(a => a.UserId == req.EmployeeId
//                          && a.AttendanceDate >= from
//                          && a.AttendanceDate <= to)
//                 .ToListAsync();

//             // ── Working days (Mon–Sat, exclude Sundays) ───────────────
//             int totalWorkingDays = 0;
//             for (var d = from; d <= to; d = d.AddDays(1))
//                 if (d.DayOfWeek != DayOfWeek.Sunday)
//                     totalWorkingDays++;

//             int presentDays = attendanceList.Count;
//             int absentDays  = Math.Max(0, totalWorkingDays - presentDays);

//             // ── Late days calculation ──────────────────────────────────
//             var officeCutoff = TimeSpan.Parse(req.OfficeStartTime)
//                                .Add(TimeSpan.FromMinutes(req.GraceMinutes));

//             int lateDays = attendanceList.Count(a =>
//             {
//                 // InTime is TimeSpan? — null check karo
//                 if (a.InTime == null) return false;
//                 return a.InTime.Value > officeCutoff;
//             });

//             // ── Salary math ───────────────────────────────────────────
//             var perDaySalary    = totalWorkingDays > 0
//                                   ? req.BasicSalary / totalWorkingDays
//                                   : 0;

//             var absentDeduction = perDaySalary * absentDays;
//             var lateDeduction   = perDaySalary * (req.LateDeductionPercent / 100m) * lateDays;

//             // Existing manual deduction (POST /deduction se set hua ho toh)
//             var existing = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             var manualDeduction       = existing?.ManualDeduction       ?? 0;
//             var manualDeductionReason = existing?.ManualDeductionReason ?? null;

//             var totalDeduction = absentDeduction + lateDeduction + manualDeduction;
//             var netSalary      = req.BasicSalary - totalDeduction;
//             if (netSalary < 0) netSalary = 0;

//             // ── Save / Update ─────────────────────────────────────────
//             if (existing != null)
//             {
//                 existing.BasicSalary      = req.BasicSalary;
//                 existing.TotalWorkingDays = totalWorkingDays;
//                 existing.PresentDays      = presentDays;
//                 existing.AbsentDays       = absentDays;
//                 existing.LateDays         = lateDays;
//                 existing.PerDaySalary     = perDaySalary;
//                 existing.AbsentDeduction  = absentDeduction;
//                 existing.LateDeduction    = lateDeduction;
//                 existing.TotalDeduction   = totalDeduction;
//                 existing.NetSalary        = netSalary;
//                 existing.GeneratedAt      = DateTime.UtcNow;
//                 existing.GeneratedBy      = GetCurrentUserId();
//             }
//             else
//             {
//                 existing = new PayrollRecord
//                 {
//                     EmployeeId            = req.EmployeeId,
//                     Month                 = req.Month,
//                     Year                  = req.Year,
//                     BasicSalary           = req.BasicSalary,
//                     TotalWorkingDays      = totalWorkingDays,
//                     PresentDays           = presentDays,
//                     AbsentDays            = absentDays,
//                     LateDays              = lateDays,
//                     PerDaySalary          = perDaySalary,
//                     AbsentDeduction       = absentDeduction,
//                     LateDeduction         = lateDeduction,
//                     ManualDeduction       = manualDeduction,
//                     ManualDeductionReason = manualDeductionReason,
//                     TotalDeduction        = totalDeduction,
//                     NetSalary             = netSalary,
//                     Status                = "draft",
//                     GeneratedAt           = DateTime.UtcNow,
//                     GeneratedBy           = GetCurrentUserId()
//                 };
//                 _db.PayrollRecords.Add(existing);
//             }

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = "Payroll calculated successfully.",
//                 success = true,
//                 data    = MapPayrollResponse(existing, employee)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  2. POST /api/Payroll/deduction  — Admin only
//         //  Manual deduction add/update karo (late/absent ke alawa)
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("deduction")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         // [ProducesResponseType(StatusCodes.Status404NotFound)]
//         public async Task<IActionResult> AddDeduction([FromBody] PayrollDeductionRequest req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found or inactive.", success = false });

//             var payroll = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             if (payroll == null)
//                 return NotFound(new
//                 {
//                     message = $"No payroll record found for {employee.UserName} for {req.Month}/{req.Year}. Please calculate first.",
//                     success = false
//                 });

//             if (payroll.Status == "paid")
//                 return BadRequest(new { message = "Cannot modify a paid payroll.", success = false });

//             // Update manual deduction
//             payroll.ManualDeduction       = req.Amount;
//             payroll.ManualDeductionReason = req.Reason;
//             payroll.TotalDeduction        = payroll.AbsentDeduction + payroll.LateDeduction + req.Amount;
//             payroll.NetSalary             = Math.Max(0, payroll.BasicSalary - payroll.TotalDeduction);

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Deduction of ₹{req.Amount} added successfully. Net salary updated to ₹{payroll.NetSalary:N2}.",
//                 success = true,
//                 data    = MapPayrollResponse(payroll, employee)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  3. GET /api/Payroll/slip
//         //  Payslip JSON response (PDF ke liye download-pdf use karo)
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("slip")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         // [ProducesResponseType(StatusCodes.Status404NotFound)]
//         public async Task<IActionResult> GetPayslip([FromQuery] PayrollSlipQuery req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             if (!IsAdmin() && req.EmployeeId != GetCurrentUserId())
//                 return Forbid();

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found.", success = false });

//             var payroll = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             if (payroll == null)
//                 return NotFound(new
//                 {
//                     message = $"No payroll record for {employee.UserName} for {req.Month}/{req.Year}.",
//                     success = false
//                 });

//             return Ok(new
//             {
//                 message = "Payslip fetched successfully.",
//                 success = true,
//                 data    = MapPayrollResponse(payroll, employee)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  3b. GET /api/Payroll/slip/download-pdf
//         //  Payslip PDF download
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("slip/download-pdf")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         // [ProducesResponseType(StatusCodes.Status404NotFound)]
//         public async Task<IActionResult> DownloadPayslipPdf([FromQuery] PayrollSlipQuery req)
//         {
//             if (!IsAdmin() && req.EmployeeId != GetCurrentUserId())
//                 return Forbid();

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found.", success = false });

//             var payroll = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             if (payroll == null)
//                 return NotFound(new { message = "Payroll record not found. Please calculate first.", success = false });

//             try
//             {
//                 var pdfBytes = _exportService.GeneratePayslipPdf(
//                     payroll,
//                     employee.UserName,
//                     employee.Department ?? "-",
//                     employee.Designation ?? "-");

//                 var fileName = $"Payslip_{employee.UserName}_{payroll.Month:D2}_{payroll.Year}.pdf";
//                 return File(pdfBytes, "application/pdf", fileName);
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new { message = $"PDF generation failed: {ex.Message}", success = false });
//             }
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  4. POST /api/Payroll/export  — Admin only
//         //  Payroll export — PDF ya Excel
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("export")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         // [ProducesResponseType(StatusCodes.Status400BadRequest)]
//         public async Task<IActionResult> ExportPayroll([FromBody] PayrollExportRequest req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             var allowedFormats = new[] { "pdf", "excel" };
//             if (!allowedFormats.Contains(req.Format.ToLower()))
//                 return BadRequest(new { message = "Format must be 'pdf' or 'excel'.", success = false });

//             var query = _db.PayrollRecords
//                 .Include(p => p.Employee)
//                 .Where(p => !p.IsDeleted && p.Month == req.Month && p.Year == req.Year);

//             if (req.EmployeeId.HasValue && req.EmployeeId.Value > 0)
//                 query = query.Where(p => p.EmployeeId == req.EmployeeId.Value);

//             var records = await query.OrderBy(p => p.Employee!.UserName).ToListAsync();

//             if (!records.Any())
//                 return NotFound(new
//                 {
//                     message = $"No payroll records found for {req.Month}/{req.Year}.",
//                     success = false
//                 });

//             var exportData = records
//                 .Select(p => (Payroll: p, EmployeeName: p.Employee?.UserName ?? "Unknown"))
//                 .ToList();

//             try
//             {
//                 if (req.Format.ToLower() == "excel")
//                 {
//                     var excelBytes = _exportService.GeneratePayrollExcel(exportData, req.Month, req.Year);
//                     var fileName   = $"Payroll_{req.Month:D2}_{req.Year}.xlsx";
//                     return File(excelBytes,
//                         "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
//                         fileName);
//                 }
//                 else
//                 {
//                     var pdfBytes = _exportService.GeneratePayrollPdf(exportData, req.Month, req.Year);
//                     var fileName = $"Payroll_{req.Month:D2}_{req.Year}.pdf";
//                     return File(pdfBytes, "application/pdf", fileName);
//                 }
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new { message = $"Export failed: {ex.Message}", success = false });
//             }
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  Private Helper
//         // ════════════════════════════════════════════════════════════════
//         private static PayrollResponse MapPayrollResponse(PayrollRecord p, User employee) =>
//             new PayrollResponse
//             {
//                 PayrollId             = p.PayrollId,
//                 EmployeeId            = p.EmployeeId,
//                 EmployeeName          = employee.UserName,
//                 Department            = employee.Department,
//                 Designation           = employee.Designation,
//                 Month                 = p.Month,
//                 Year                  = p.Year,
//                 MonthName             = new DateTime(p.Year, p.Month, 1).ToString("MMMM"),
//                 BasicSalary           = p.BasicSalary,
//                 TotalWorkingDays      = p.TotalWorkingDays,
//                 PresentDays           = p.PresentDays,
//                 AbsentDays            = p.AbsentDays,
//                 LateDays              = p.LateDays,
//                 PerDaySalary          = p.PerDaySalary,
//                 AbsentDeduction       = p.AbsentDeduction,
//                 LateDeduction         = p.LateDeduction,
//                 ManualDeduction       = p.ManualDeduction,
//                 ManualDeductionReason = p.ManualDeductionReason,
//                 TotalDeduction        = p.TotalDeduction,
//                 NetSalary             = p.NetSalary,
//                 Status                = p.Status,
//                 GeneratedAt           = p.GeneratedAt,
//                 Remarks               = p.Remarks
//             };
//     }
// }










// // ======================= Controllers/PayrollController.cs =======================
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using attendance_api.Data;
// using attendance_api.DTOs;
// using attendance_api.Models;
// using attendance_api.Services;
// using System.Security.Claims;

// namespace attendance_api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     [Authorize]
//     public class PayrollController : ControllerBase
//     {
//         private readonly ApplicationDbContext _db;
//         private readonly IPayrollExportService _exportService;

//         private const string AdminRole   = "Admin";
//         private const string AdminLower  = "admin";
//         private const string AdminPolicy = "Admin,admin";

//         // ── Fixed office rules ─────────────────────────────────────────────
//         // Office hours : 10:00 AM – 6:00 PM
//         // Late cutoff  : 10:15 AM → ½ day deduction per late entry
//         // Off days     : Saturday & Sunday
//         private static readonly TimeSpan LateCutoff = new TimeSpan(10, 15, 0);

//         public PayrollController(ApplicationDbContext db, IPayrollExportService exportService)
//         {
//             _db            = db;
//             _exportService = exportService;
//         }

//         private int  GetCurrentUserId() =>
//             int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

//         private bool IsAdmin() =>
//             User.IsInRole(AdminRole) || User.IsInRole(AdminLower);

//         // ════════════════════════════════════════════════════════════════
//         //  1. GET /api/Payroll/calculate  — Admin only
//         //  • Only allowed after the requested month is fully complete
//         //  • Auto-fetches attendance from DB
//         //  • Working days = Mon–Fri (Sat & Sun off)
//         //  • Calendar-accurate days per month (28 / 30 / 31)
//         //  • Late rule = InTime after 10:15 AM → 0.5 day deducted
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("calculate")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> CalculatePayroll([FromQuery] PayrollCalculateRequest req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             // Guard: only calculate after month is over
//             var lastDayOfMonth = new DateTime(req.Year, req.Month,
//                 DateTime.DaysInMonth(req.Year, req.Month));

//             if (DateTime.Today <= lastDayOfMonth)
//                 return BadRequest(new
//                 {
//                     message = $"Payroll for {lastDayOfMonth:MMMM yyyy} can only be calculated " +
//                               $"after the month is complete (after {lastDayOfMonth:dd-MMM-yyyy}).",
//                     success = false
//                 });

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found or inactive.", success = false });

//             var from = new DateTime(req.Year, req.Month, 1);
//             var to   = lastDayOfMonth;

//             // Working days: Mon–Fri only
//             int totalWorkingDays = 0;
//             for (var d = from; d <= to; d = d.AddDays(1))
//                 if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
//                     totalWorkingDays++;

//             // Auto-fetch attendance
//             var attendanceList = await _db.Attendances
//                 .Where(a => a.UserId == req.EmployeeId
//                          && a.AttendanceDate >= from
//                          && a.AttendanceDate <= to)
//                 .ToListAsync();

//             int presentDays = attendanceList.Count;
//             int absentDays  = Math.Max(0, totalWorkingDays - presentDays);

//             // Late = InTime after 10:15 AM
//             int lateDays = attendanceList.Count(a =>
//                 a.InTime.HasValue && a.InTime.Value > LateCutoff);

//             // Salary calculation
//             decimal perDaySalary    = totalWorkingDays > 0 ? req.BasicSalary / totalWorkingDays : 0;
//             decimal absentDeduction = perDaySalary * absentDays;
//             decimal lateDeduction   = perDaySalary * 0.5m * lateDays;  // ½ day per late

//             // Carry over any existing manual deduction
//             var existing = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             decimal manualDeduction       = existing?.ManualDeduction       ?? 0;
//             string? manualDeductionReason = existing?.ManualDeductionReason ?? null;

//             decimal totalDeduction = absentDeduction + lateDeduction + manualDeduction;
//             decimal netSalary      = Math.Max(0, req.BasicSalary - totalDeduction);

//             if (existing != null)
//             {
//                 if (existing.Status == "approved" || existing.Status == "paid")
//                     return BadRequest(new
//                     {
//                         message = $"Payroll is already '{existing.Status}'. Recalculation not allowed.",
//                         success = false
//                     });

//                 existing.BasicSalary      = req.BasicSalary;
//                 existing.TotalWorkingDays = totalWorkingDays;
//                 existing.PresentDays      = presentDays;
//                 existing.AbsentDays       = absentDays;
//                 existing.LateDays         = lateDays;
//                 existing.PerDaySalary     = perDaySalary;
//                 existing.AbsentDeduction  = absentDeduction;
//                 existing.LateDeduction    = lateDeduction;
//                 existing.TotalDeduction   = totalDeduction;
//                 existing.NetSalary        = netSalary;
//                 existing.GeneratedAt      = DateTime.UtcNow;
//                 existing.GeneratedBy      = GetCurrentUserId();
//             }
//             else
//             {
//                 existing = new PayrollRecord
//                 {
//                     EmployeeId            = req.EmployeeId,
//                     Month                 = req.Month,
//                     Year                  = req.Year,
//                     BasicSalary           = req.BasicSalary,
//                     TotalWorkingDays      = totalWorkingDays,
//                     PresentDays           = presentDays,
//                     AbsentDays            = absentDays,
//                     LateDays              = lateDays,
//                     PerDaySalary          = perDaySalary,
//                     AbsentDeduction       = absentDeduction,
//                     LateDeduction         = lateDeduction,
//                     ManualDeduction       = manualDeduction,
//                     ManualDeductionReason = manualDeductionReason,
//                     TotalDeduction        = totalDeduction,
//                     NetSalary             = netSalary,
//                     Status                = "draft",
//                     GeneratedAt           = DateTime.UtcNow,
//                     GeneratedBy           = GetCurrentUserId()
//                 };
//                 _db.PayrollRecords.Add(existing);
//             }

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = "Payroll calculated successfully.",
//                 success = true,
//                 data    = await MapPayrollResponseAsync(existing, employee)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  2. POST /api/Payroll/deduction  — Admin only
//         //  Manual deduction (optional) — only on draft payrolls
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("deduction")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> AddDeduction([FromBody] PayrollDeductionRequest req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found or inactive.", success = false });

//             var payroll = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             if (payroll == null)
//                 return NotFound(new
//                 {
//                     message = $"No payroll record for {employee.UserName} ({req.Month}/{req.Year}). Calculate first.",
//                     success = false
//                 });

//             if (payroll.Status == "paid")
//                 return BadRequest(new { message = "Cannot modify a paid payroll.", success = false });

//             if (payroll.Status == "approved")
//                 return BadRequest(new { message = "Payroll is approved. Unapprove first to modify.", success = false });

//             payroll.ManualDeduction       = req.Amount;
//             payroll.ManualDeductionReason = req.Reason;
//             payroll.TotalDeduction        = payroll.AbsentDeduction + payroll.LateDeduction + req.Amount;
//             payroll.NetSalary             = Math.Max(0, payroll.BasicSalary - payroll.TotalDeduction);

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Deduction ₹{req.Amount} added. Net salary: ₹{payroll.NetSalary:N2}.",
//                 success = true,
//                 data    = await MapPayrollResponseAsync(payroll, employee)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  3. POST /api/Payroll/approve  — Admin only
//         //  draft → approved
//         //  Employee ko payslip tab dikhegi jab admin approve kare
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("approve")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> ApprovePayroll([FromBody] PayrollApproveRequest req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found or inactive.", success = false });

//             var payroll = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             if (payroll == null)
//                 return NotFound(new
//                 {
//                     message = $"No payroll record for {employee.UserName} ({req.Month}/{req.Year}). Calculate first.",
//                     success = false
//                 });

//             if (payroll.Status == "paid")
//                 return BadRequest(new { message = "Payroll is already marked as paid.", success = false });

//             if (payroll.Status == "approved")
//                 return BadRequest(new { message = "Payroll is already approved.", success = false });

//             payroll.Status     = "approved";
//             payroll.ApprovedBy = GetCurrentUserId();
//             payroll.ApprovedAt = DateTime.UtcNow;

//             if (!string.IsNullOrWhiteSpace(req.Remarks))
//                 payroll.Remarks = req.Remarks;

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Payroll approved for {employee.UserName} ({req.Month}/{req.Year}).",
//                 success = true,
//                 data    = await MapPayrollResponseAsync(payroll, employee)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  4. POST /api/Payroll/markpaid  — Admin only
//         //  approved → paid
//         //  Salary disburse hone ke baad mark karo
//         //  Tabhi employee PDF download kar sakta hai
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("markpaid")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> MarkPaid([FromBody] PayrollMarkPaidRequest req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found or inactive.", success = false });

//             var payroll = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             if (payroll == null)
//                 return NotFound(new { message = "Payroll record not found.", success = false });

//             if (payroll.Status == "paid")
//                 return BadRequest(new { message = "Salary is already marked as paid.", success = false });

//             if (payroll.Status != "approved")
//                 return BadRequest(new
//                 {
//                     message = "Payroll must be approved before marking as paid.",
//                     success = false
//                 });

//             payroll.Status = "paid";
//             payroll.PaidBy = GetCurrentUserId();
//             payroll.PaidAt = DateTime.UtcNow;

//             if (!string.IsNullOrWhiteSpace(req.Remarks))
//                 payroll.Remarks = req.Remarks;

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Salary marked as paid for {employee.UserName} ({req.Month}/{req.Year}).",
//                 success = true,
//                 data    = await MapPayrollResponseAsync(payroll, employee)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  5. GET /api/Payroll/slip
//         //  Employee: only visible when approved or paid
//         //  Admin: visible at all statuses
//         //  PaymentStatus: "Pay Completed" or "Pending"
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("slip")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> GetPayslip([FromQuery] PayrollSlipQuery req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             if (!IsAdmin() && req.EmployeeId != GetCurrentUserId())
//                 return Forbid();

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found.", success = false });

//             var payroll = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             if (payroll == null)
//                 return NotFound(new
//                 {
//                     message = $"No payroll record for {req.Month}/{req.Year}.",
//                     success = false
//                 });

//             // Employee: draft payroll not visible
//             if (!IsAdmin() && payroll.Status == "draft")
//                 return Ok(new
//                 {
//                     message = "Your payslip is being processed. It will be visible once admin approves it.",
//                     success = false,
//                     data    = (object?)null
//                 });

//             return Ok(new
//             {
//                 message = "Payslip fetched successfully.",
//                 success = true,
//                 data    = await MapPayrollResponseAsync(payroll, employee)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  6. GET /api/Payroll/slip/download-pdf
//         //  PDF download only when status is "paid"
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("slip/download-pdf")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> DownloadPayslipPdf([FromQuery] PayrollSlipQuery req)
//         {
//             if (!IsAdmin() && req.EmployeeId != GetCurrentUserId())
//                 return Forbid();

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found.", success = false });

//             var payroll = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             if (payroll == null)
//                 return NotFound(new { message = "Payroll record not found. Calculate first.", success = false });

//             if (payroll.Status != "paid")
//                 return BadRequest(new
//                 {
//                     message = payroll.Status == "draft"
//                         ? "Payslip is not approved yet. PDF not available."
//                         : "Salary payment is pending. PDF available once salary is disbursed.",
//                     success = false
//                 });

//             try
//             {
//                 string approvedByName = "-";
//                 if (payroll.ApprovedBy.HasValue)
//                 {
//                     var approver = await _db.Users.FindAsync(payroll.ApprovedBy.Value);
//                     approvedByName = approver?.UserName ?? "-";
//                 }

//                 string paidByName = "-";
//                 if (payroll.PaidBy.HasValue)
//                 {
//                     var paidBy = await _db.Users.FindAsync(payroll.PaidBy.Value);
//                     paidByName = paidBy?.UserName ?? "-";
//                 }

//                 var pdfBytes = _exportService.GeneratePayslipPdf(
//                     payroll,
//                     employee.UserName,
//                     employee.Department  ?? "-",
//                     employee.Designation ?? "-",
//                     approvedByName,
//                     paidByName);

//                 var fileName = $"Payslip_{employee.UserName}_{payroll.Month:D2}_{payroll.Year}.pdf";
//                 return File(pdfBytes, "application/pdf", fileName);
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new { message = $"PDF generation failed: {ex.Message}", success = false });
//             }
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  7. POST /api/Payroll/export  — Admin only
//         //  employeeId null/0 = all employees
//         //  Excel is protected (read-only)
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("export")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> ExportPayroll([FromBody] PayrollExportRequest req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             var allowedFormats = new[] { "pdf", "excel" };
//             if (!allowedFormats.Contains(req.Format.ToLower()))
//                 return BadRequest(new { message = "Format must be 'pdf' or 'excel'.", success = false });

//             var query = _db.PayrollRecords
//                 .Include(p => p.Employee)
//                 .Where(p => !p.IsDeleted && p.Month == req.Month && p.Year == req.Year);

//             if (req.EmployeeId.HasValue && req.EmployeeId.Value > 0)
//                 query = query.Where(p => p.EmployeeId == req.EmployeeId.Value);

//             var records = await query.OrderBy(p => p.Employee!.UserName).ToListAsync();

//             if (!records.Any())
//                 return NotFound(new
//                 {
//                     message = $"No payroll records found for {req.Month}/{req.Year}.",
//                     success = false
//                 });

//             var exportData = records
//                 .Select(p => (Payroll: p, EmployeeName: p.Employee?.UserName ?? "Unknown"))
//                 .ToList();

//             try
//             {
//                 if (req.Format.ToLower() == "excel")
//                 {
//                     var excelBytes = _exportService.GeneratePayrollExcel(exportData, req.Month, req.Year);
//                     var fileName   = $"Payroll_{req.Month:D2}_{req.Year}.xlsx";
//                     return File(excelBytes,
//                         "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
//                         fileName);
//                 }
//                 else
//                 {
//                     var pdfBytes = _exportService.GeneratePayrollPdf(exportData, req.Month, req.Year);
//                     var fileName = $"Payroll_{req.Month:D2}_{req.Year}.pdf";
//                     return File(pdfBytes, "application/pdf", fileName);
//                 }
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new { message = $"Export failed: {ex.Message}", success = false });
//             }
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  Private Helper
//         // ════════════════════════════════════════════════════════════════
//         private async Task<PayrollResponse> MapPayrollResponseAsync(PayrollRecord p, User employee)
//         {
//             string? approvedByName = null;
//             if (p.ApprovedBy.HasValue)
//             {
//                 var approver = await _db.Users.FindAsync(p.ApprovedBy.Value);
//                 approvedByName = approver?.UserName;
//             }

//             string? paidByName = null;
//             if (p.PaidBy.HasValue)
//             {
//                 var paidBy = await _db.Users.FindAsync(p.PaidBy.Value);
//                 paidByName = paidBy?.UserName;
//             }

//             return new PayrollResponse
//             {
//                 PayrollId             = p.PayrollId,
//                 EmployeeId            = p.EmployeeId,
//                 EmployeeName          = employee.UserName,
//                 Department            = employee.Department,
//                 Designation           = employee.Designation,
//                 Month                 = p.Month,
//                 Year                  = p.Year,
//                 MonthName             = new DateTime(p.Year, p.Month, 1).ToString("MMMM"),
//                 BasicSalary           = p.BasicSalary,
//                 TotalWorkingDays      = p.TotalWorkingDays,
//                 PresentDays           = p.PresentDays,
//                 AbsentDays            = p.AbsentDays,
//                 LateDays              = p.LateDays,
//                 PerDaySalary          = p.PerDaySalary,
//                 AbsentDeduction       = p.AbsentDeduction,
//                 LateDeduction         = p.LateDeduction,
//                 ManualDeduction       = p.ManualDeduction,
//                 ManualDeductionReason = p.ManualDeductionReason,
//                 TotalDeduction        = p.TotalDeduction,
//                 NetSalary             = p.NetSalary,
//                 Status                = p.Status,
//                 PaymentStatus         = p.Status == "paid" ? "Pay Completed" : "Pending",
//                 ApprovedAt            = p.ApprovedAt,
//                 ApprovedByName        = approvedByName,
//                 PaidAt                = p.PaidAt,
//                 PaidByName            = paidByName,
//                 GeneratedAt           = p.GeneratedAt,
//                 Remarks               = p.Remarks
//             };
//         }
//     }
// }














// // ======================= Controllers/PayrollController.cs =======================
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using attendance_api.Data;
// using attendance_api.DTOs;
// using attendance_api.Models;
// using attendance_api.Services;
// using System.Security.Claims;

// namespace attendance_api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     [Authorize]
//     public class PayrollController : ControllerBase
//     {
//         private readonly ApplicationDbContext _db;
//         private readonly IPayrollExportService _exportService;

//         private const string AdminRole   = "Admin";
//         private const string AdminLower  = "admin";
//         private const string AdminPolicy = "Admin,admin";

//         // ── Fixed office rules ─────────────────────────────────────────────
//         // Office hours : 10:00 AM – 6:00 PM
//         // Late cutoff  : 10:15 AM → ½ day deduction per late entry
//         // Off days     : Saturday & Sunday
//         private static readonly TimeSpan LateCutoff = new TimeSpan(10, 15, 0);

//         public PayrollController(ApplicationDbContext db, IPayrollExportService exportService)
//         {
//             _db            = db;
//             _exportService = exportService;
//         }

//         private int  GetCurrentUserId() =>
//             int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

//         private bool IsAdmin() =>
//             User.IsInRole(AdminRole) || User.IsInRole(AdminLower);

//         // ════════════════════════════════════════════════════════════════
//         //  1. GET /api/Payroll/calculate  — Admin only
//         //
//         //  Rules:
//         //  • Only allowed after the requested month is fully complete
//         //  • Auto-fetches attendance from DB
//         //  • Working days = Mon–Fri only (Sat & Sun off)
//         //  • Calendar-accurate days per month (28 / 30 / 31)
//         //  • Late rule = InTime after 10:15 AM → 0.5 day deducted
//         //
//         //  Recalculation:
//         //  • If status is "approved" or "paid" → reset to draft
//         //    (clears approval & payment info) and recalculates fresh
//         //  • Manual deduction is preserved during recalculation
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("calculate")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> CalculatePayroll([FromQuery] PayrollCalculateRequest req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             // Guard: only calculate after month is over
//             var lastDayOfMonth = new DateTime(req.Year, req.Month,
//                 DateTime.DaysInMonth(req.Year, req.Month));

//             if (DateTime.Today <= lastDayOfMonth)
//                 return BadRequest(new
//                 {
//                     message = $"Payroll for {lastDayOfMonth:MMMM yyyy} can only be calculated " +
//                               $"after the month is complete (after {lastDayOfMonth:dd-MMM-yyyy}).",
//                     success = false
//                 });

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found or inactive.", success = false });

//             var from = new DateTime(req.Year, req.Month, 1);
//             var to   = lastDayOfMonth;

//             // Working days: Mon–Fri only (Sat & Sun off)
//             int totalWorkingDays = 0;
//             for (var d = from; d <= to; d = d.AddDays(1))
//                 if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
//                     totalWorkingDays++;

//             // Auto-fetch attendance from DB
//             var attendanceList = await _db.Attendances
//                 .Where(a => a.UserId == req.EmployeeId
//                          && a.AttendanceDate >= from
//                          && a.AttendanceDate <= to)
//                 .ToListAsync();

//             // Present days: ALL days including Sat/Sun
//             // If employee works on Sat/Sun → reduces absent days (bonus)
//             // Cap at totalWorkingDays to avoid negative absents
//             int presentDays = Math.Min(attendanceList.Count, totalWorkingDays);

//             int absentDays = Math.Max(0, totalWorkingDays - presentDays);

//             // Late = InTime after 10:15 AM → ½ day deduction each
//             int lateDays = attendanceList.Count(a =>
//                 a.InTime.HasValue && a.InTime.Value > LateCutoff);

//             // Salary calculation
//             decimal perDaySalary    = totalWorkingDays > 0
//                                       ? Math.Round(req.BasicSalary / totalWorkingDays, 2)
//                                       : 0;
//             decimal absentDeduction = Math.Round(perDaySalary * absentDays, 2);
//             decimal lateDeduction   = Math.Round(perDaySalary * 0.5m * lateDays, 2);

//             // Check for existing record
//             var existing = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             // Preserve manual deduction across recalculations
//             decimal manualDeduction       = existing?.ManualDeduction       ?? 0;
//             string? manualDeductionReason = existing?.ManualDeductionReason ?? null;

//             decimal totalDeduction = absentDeduction + lateDeduction + manualDeduction;
//             decimal netSalary      = Math.Max(0, req.BasicSalary - totalDeduction);

//             if (existing != null)
//             {
//                 // If approved or paid → reset to draft for fresh recalculation
//                 if (existing.Status == "approved" || existing.Status == "paid")
//                 {
//                     existing.Status     = "draft";
//                     existing.ApprovedBy = null;
//                     existing.ApprovedAt = null;
//                     existing.PaidBy     = null;
//                     existing.PaidAt     = null;
//                     existing.Remarks    = null;
//                 }

//                 existing.BasicSalary      = req.BasicSalary;
//                 existing.TotalWorkingDays = totalWorkingDays;
//                 existing.PresentDays      = presentDays;
//                 existing.AbsentDays       = absentDays;
//                 existing.LateDays         = lateDays;
//                 existing.PerDaySalary     = perDaySalary;
//                 existing.AbsentDeduction  = absentDeduction;
//                 existing.LateDeduction    = lateDeduction;
//                 existing.ManualDeduction  = manualDeduction;
//                 existing.ManualDeductionReason = manualDeductionReason;
//                 existing.TotalDeduction   = totalDeduction;
//                 existing.NetSalary        = netSalary;
//                 existing.GeneratedAt      = DateTime.UtcNow;
//                 existing.GeneratedBy      = GetCurrentUserId();
//             }
//             else
//             {
//                 existing = new PayrollRecord
//                 {
//                     EmployeeId            = req.EmployeeId,
//                     Month                 = req.Month,
//                     Year                  = req.Year,
//                     BasicSalary           = req.BasicSalary,
//                     TotalWorkingDays      = totalWorkingDays,
//                     PresentDays           = presentDays,
//                     AbsentDays            = absentDays,
//                     LateDays              = lateDays,
//                     PerDaySalary          = perDaySalary,
//                     AbsentDeduction       = absentDeduction,
//                     LateDeduction         = lateDeduction,
//                     ManualDeduction       = manualDeduction,
//                     ManualDeductionReason = manualDeductionReason,
//                     TotalDeduction        = totalDeduction,
//                     NetSalary             = netSalary,
//                     Status                = "draft",
//                     GeneratedAt           = DateTime.UtcNow,
//                     GeneratedBy           = GetCurrentUserId()
//                 };
//                 _db.PayrollRecords.Add(existing);
//             }

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = "Payroll calculated successfully.",
//                 success = true,
//                 data    = await MapPayrollResponseAsync(existing, employee)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  2. POST /api/Payroll/deduction  — Admin only
//         //  Manual deduction (optional) — only on draft payrolls
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("deduction")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> AddDeduction([FromBody] PayrollDeductionRequest req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found or inactive.", success = false });

//             var payroll = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             if (payroll == null)
//                 return NotFound(new
//                 {
//                     message = $"No payroll record for {employee.UserName} ({req.Month}/{req.Year}). Calculate first.",
//                     success = false
//                 });

//             if (payroll.Status == "paid")
//                 return BadRequest(new { message = "Cannot modify a paid payroll. Recalculate to reset.", success = false });

//             if (payroll.Status == "approved")
//                 return BadRequest(new { message = "Payroll is approved. Recalculate to modify.", success = false });

//             payroll.ManualDeduction       = req.Amount;
//             payroll.ManualDeductionReason = req.Reason;
//             payroll.TotalDeduction        = payroll.AbsentDeduction + payroll.LateDeduction + req.Amount;
//             payroll.NetSalary             = Math.Max(0, payroll.BasicSalary - payroll.TotalDeduction);

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Deduction ₹{req.Amount} added. Net salary: ₹{payroll.NetSalary:N2}.",
//                 success = true,
//                 data    = await MapPayrollResponseAsync(payroll, employee)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  3. POST /api/Payroll/approve  — Admin only
//         //  draft → approved
//         //  Employee ko payslip tab dikhegi jab admin approve kare
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("approve")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> ApprovePayroll([FromBody] PayrollApproveRequest req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found or inactive.", success = false });

//             var payroll = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             if (payroll == null)
//                 return NotFound(new
//                 {
//                     message = $"No payroll record for {employee.UserName} ({req.Month}/{req.Year}). Calculate first.",
//                     success = false
//                 });

//             if (payroll.Status == "paid")
//                 return BadRequest(new { message = "Payroll is already paid.", success = false });

//             if (payroll.Status == "approved")
//                 return BadRequest(new { message = "Payroll is already approved.", success = false });

//             payroll.Status     = "approved";
//             payroll.ApprovedBy = GetCurrentUserId();
//             payroll.ApprovedAt = DateTime.UtcNow;

//             if (!string.IsNullOrWhiteSpace(req.Remarks))
//                 payroll.Remarks = req.Remarks;

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Payroll approved for {employee.UserName} ({req.Month}/{req.Year}).",
//                 success = true,
//                 data    = await MapPayrollResponseAsync(payroll, employee)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  4. POST /api/Payroll/markpaid  — Admin only
//         //  approved → paid
//         //  Salary disburse hone ke baad mark karo
//         //  Tabhi employee PDF download kar sakta hai
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("markpaid")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> MarkPaid([FromBody] PayrollMarkPaidRequest req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found or inactive.", success = false });

//             var payroll = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             if (payroll == null)
//                 return NotFound(new { message = "Payroll record not found.", success = false });

//             if (payroll.Status == "paid")
//                 return BadRequest(new { message = "Salary is already marked as paid.", success = false });

//             if (payroll.Status != "approved")
//                 return BadRequest(new
//                 {
//                     message = "Payroll must be approved before marking as paid.",
//                     success = false
//                 });

//             payroll.Status = "paid";
//             payroll.PaidBy = GetCurrentUserId();
//             payroll.PaidAt = DateTime.UtcNow;

//             if (!string.IsNullOrWhiteSpace(req.Remarks))
//                 payroll.Remarks = req.Remarks;

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Salary marked as paid for {employee.UserName} ({req.Month}/{req.Year}).",
//                 success = true,
//                 data    = await MapPayrollResponseAsync(payroll, employee)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  5. GET /api/Payroll/slip
//         //  Employee: only visible when approved or paid
//         //  Admin: visible at all statuses
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("slip")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> GetPayslip([FromQuery] PayrollSlipQuery req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             if (!IsAdmin() && req.EmployeeId != GetCurrentUserId())
//                 return Forbid();

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found.", success = false });

//             var payroll = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             if (payroll == null)
//                 return NotFound(new
//                 {
//                     message = $"No payroll record for {req.Month}/{req.Year}.",
//                     success = false
//                 });

//             // Employee: draft payroll not visible
//             if (!IsAdmin() && payroll.Status == "draft")
//                 return Ok(new
//                 {
//                     message = "Your payslip is being processed. It will be visible once admin approves it.",
//                     success = false,
//                     data    = (object?)null
//                 });

//             return Ok(new
//             {
//                 message = "Payslip fetched successfully.",
//                 success = true,
//                 data    = await MapPayrollResponseAsync(payroll, employee)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  6. GET /api/Payroll/slip/download-pdf
//         //  PDF download only when status is "paid"
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("slip/download-pdf")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> DownloadPayslipPdf([FromQuery] PayrollSlipQuery req)
//         {
//             if (!IsAdmin() && req.EmployeeId != GetCurrentUserId())
//                 return Forbid();

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found.", success = false });

//             var payroll = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             if (payroll == null)
//                 return NotFound(new { message = "Payroll record not found. Calculate first.", success = false });

//             if (payroll.Status != "paid")
//                 return BadRequest(new
//                 {
//                     message = payroll.Status == "draft"
//                         ? "Payslip is not approved yet. PDF not available."
//                         : "Salary payment is pending. PDF available once salary is disbursed.",
//                     success = false
//                 });

//             try
//             {
//                 string approvedByName = "-";
//                 if (payroll.ApprovedBy.HasValue)
//                 {
//                     var approver = await _db.Users.FindAsync(payroll.ApprovedBy.Value);
//                     approvedByName = approver?.UserName ?? "-";
//                 }

//                 string paidByName = "-";
//                 if (payroll.PaidBy.HasValue)
//                 {
//                     var paidBy = await _db.Users.FindAsync(payroll.PaidBy.Value);
//                     paidByName = paidBy?.UserName ?? "-";
//                 }

//                 var pdfBytes = _exportService.GeneratePayslipPdf(
//                     payroll,
//                     employee.UserName,
//                     employee.Department  ?? "-",
//                     employee.Designation ?? "-",
//                     approvedByName,
//                     paidByName);

//                 var fileName = $"Payslip_{employee.UserName}_{payroll.Month:D2}_{payroll.Year}.pdf";
//                 return File(pdfBytes, "application/pdf", fileName);
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new { message = $"PDF generation failed: {ex.Message}", success = false });
//             }
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  7. POST /api/Payroll/export  — Admin only
//         //  employeeId null/0 = all employees
//         //  Excel is protected (read-only)
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("export")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> ExportPayroll([FromBody] PayrollExportRequest req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             var allowedFormats = new[] { "pdf", "excel" };
//             if (!allowedFormats.Contains(req.Format.ToLower()))
//                 return BadRequest(new { message = "Format must be 'pdf' or 'excel'.", success = false });

//             var query = _db.PayrollRecords
//                 .Include(p => p.Employee)
//                 .Where(p => !p.IsDeleted && p.Month == req.Month && p.Year == req.Year);

//             if (req.EmployeeId.HasValue && req.EmployeeId.Value > 0)
//                 query = query.Where(p => p.EmployeeId == req.EmployeeId.Value);

//             var records = await query.OrderBy(p => p.Employee!.UserName).ToListAsync();

//             if (!records.Any())
//                 return NotFound(new
//                 {
//                     message = $"No payroll records found for {req.Month}/{req.Year}.",
//                     success = false
//                 });

//             var exportData = records
//                 .Select(p => (Payroll: p, EmployeeName: p.Employee?.UserName ?? "Unknown"))
//                 .ToList();

//             try
//             {
//                 if (req.Format.ToLower() == "excel")
//                 {
//                     var excelBytes = _exportService.GeneratePayrollExcel(exportData, req.Month, req.Year);
//                     var fileName   = $"Payroll_{req.Month:D2}_{req.Year}.xlsx";
//                     return File(excelBytes,
//                         "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
//                         fileName);
//                 }
//                 else
//                 {
//                     var pdfBytes = _exportService.GeneratePayrollPdf(exportData, req.Month, req.Year);
//                     var fileName = $"Payroll_{req.Month:D2}_{req.Year}.pdf";
//                     return File(pdfBytes, "application/pdf", fileName);
//                 }
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new { message = $"Export failed: {ex.Message}", success = false });
//             }
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  Private Helper
//         // ════════════════════════════════════════════════════════════════
//         private async Task<PayrollResponse> MapPayrollResponseAsync(PayrollRecord p, User employee)
//         {
//             string? approvedByName = null;
//             if (p.ApprovedBy.HasValue)
//             {
//                 var approver = await _db.Users.FindAsync(p.ApprovedBy.Value);
//                 approvedByName = approver?.UserName;
//             }

//             string? paidByName = null;
//             if (p.PaidBy.HasValue)
//             {
//                 var paidBy = await _db.Users.FindAsync(p.PaidBy.Value);
//                 paidByName = paidBy?.UserName;
//             }

//             return new PayrollResponse
//             {
//                 PayrollId             = p.PayrollId,
//                 EmployeeId            = p.EmployeeId,
//                 EmployeeName          = employee.UserName,
//                 Department            = employee.Department,
//                 Designation           = employee.Designation,
//                 Month                 = p.Month,
//                 Year                  = p.Year,
//                 MonthName             = new DateTime(p.Year, p.Month, 1).ToString("MMMM"),
//                 BasicSalary           = p.BasicSalary,
//                 TotalWorkingDays      = p.TotalWorkingDays,
//                 PresentDays           = p.PresentDays,
//                 AbsentDays            = p.AbsentDays,
//                 LateDays              = p.LateDays,
//                 PerDaySalary          = p.PerDaySalary,
//                 AbsentDeduction       = p.AbsentDeduction,
//                 LateDeduction         = p.LateDeduction,
//                 ManualDeduction       = p.ManualDeduction,
//                 ManualDeductionReason = p.ManualDeductionReason,
//                 TotalDeduction        = p.TotalDeduction,
//                 NetSalary             = p.NetSalary,
//                 Status                = p.Status,
//                 PaymentStatus         = p.Status == "paid" ? "Pay Completed" : "Pending",
//                 ApprovedAt            = p.ApprovedAt,
//                 ApprovedByName        = approvedByName,
//                 PaidAt                = p.PaidAt,
//                 PaidByName            = paidByName,
//                 GeneratedAt           = p.GeneratedAt,
//                 Remarks               = p.Remarks
//             };
//         }
//     }
// }















// // ======================= Controllers/PayrollController.cs =======================
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using attendance_api.Data;
// using attendance_api.DTOs;
// using attendance_api.Models;
// using attendance_api.Services;
// using System.Security.Claims;

// namespace attendance_api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     [Authorize]
//     public class PayrollController : ControllerBase
//     {
//         private readonly ApplicationDbContext _db;
//         private readonly IPayrollExportService _exportService;

//         private const string AdminRole   = "Admin";
//         private const string AdminLower  = "admin";
//         private const string AdminPolicy = "Admin,admin";

//         // ── Fixed office rules ─────────────────────────────────────────────
//         // Office hours : 10:00 AM – 6:00 PM
//         // Late cutoff  : 10:15 AM → ½ day deduction per late entry
//         // Off days     : Saturday & Sunday
//         private static readonly TimeSpan LateCutoff = new TimeSpan(10, 15, 0);

//         public PayrollController(ApplicationDbContext db, IPayrollExportService exportService)
//         {
//             _db            = db;
//             _exportService = exportService;
//         }

//         private int  GetCurrentUserId() =>
//             int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

//         private bool IsAdmin() =>
//             User.IsInRole(AdminRole) || User.IsInRole(AdminLower);

//         // ════════════════════════════════════════════════════════════════
//         //  1. GET /api/Payroll/calculate  — Admin only
//         //
//         //  Rules:
//         //  • Only allowed after the requested month is fully complete
//         //  • Auto-fetches attendance from DB
//         //  • Working days = Mon–Fri only (Sat & Sun off)
//         //  • Calendar-accurate days per month (28 / 30 / 31)
//         //  • Late rule = InTime after 10:15 AM → 0.5 day deducted
//         //
//         //  Recalculation:
//         //  • If status is "approved" or "paid" → reset to draft
//         //    (clears approval & payment info) and recalculates fresh
//         //  • Manual deduction is preserved during recalculation
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("calculate")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> CalculatePayroll([FromQuery] PayrollCalculateRequest req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             // Guard: only calculate after month is over
//             var lastDayOfMonth = new DateTime(req.Year, req.Month,
//                 DateTime.DaysInMonth(req.Year, req.Month));

//             if (DateTime.Today <= lastDayOfMonth)
//                 return BadRequest(new
//                 {
//                     message = $"Payroll for {lastDayOfMonth:MMMM yyyy} can only be calculated " +
//                               $"after the month is complete (after {lastDayOfMonth:dd-MMM-yyyy}).",
//                     success = false
//                 });

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found or inactive.", success = false });

//             var from = new DateTime(req.Year, req.Month, 1);
//             var to   = lastDayOfMonth;

//             // Working days: Mon–Fri only (Sat & Sun off)
//             int totalWorkingDays = 0;
//             for (var d = from; d <= to; d = d.AddDays(1))
//                 if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
//                     totalWorkingDays++;

//             // Auto-fetch attendance from DB
//             var attendanceList = await _db.Attendances
//                 .Where(a => a.UserId == req.EmployeeId
//                          && a.AttendanceDate >= from
//                          && a.AttendanceDate <= to)
//                 .ToListAsync();

//             // Present days: ALL days including Sat/Sun
//             // If employee works on Sat/Sun → reduces absent days (bonus)
//             // Cap at totalWorkingDays to avoid negative absents
//             int presentDays = Math.Min(attendanceList.Count, totalWorkingDays);

//             int absentDays = Math.Max(0, totalWorkingDays - presentDays);

//             // Late = InTime after 10:15 AM → ½ day deduction each
//             int lateDays = attendanceList.Count(a =>
//                 a.InTime.HasValue && a.InTime.Value > LateCutoff);

//             // Per Day Salary = Basic ÷ total calendar days in month
//             // Feb(28)=1428.57 | 30-day=1333.33 | 31-day=1290.32
//             int     totalCalendarDays = DateTime.DaysInMonth(req.Year, req.Month);
//             decimal perDaySalary      = Math.Round(req.BasicSalary / totalCalendarDays, 2);
//             decimal absentDeduction   = Math.Round(perDaySalary * absentDays, 2);
//             decimal lateDeduction     = Math.Round(perDaySalary * 0.5m * lateDays, 2);

//             // Check for existing record
//             var existing = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             // Preserve manual deduction across recalculations
//             decimal manualDeduction       = existing?.ManualDeduction       ?? 0;
//             string? manualDeductionReason = existing?.ManualDeductionReason ?? null;

//             decimal totalDeduction = absentDeduction + lateDeduction + manualDeduction;
//             decimal netSalary      = Math.Max(0, req.BasicSalary - totalDeduction);

//             if (existing != null)
//             {
//                 // If approved or paid → reset to draft for fresh recalculation
//                 if (existing.Status == "approved" || existing.Status == "paid")
//                 {
//                     existing.Status     = "draft";
//                     existing.ApprovedBy = null;
//                     existing.ApprovedAt = null;
//                     existing.PaidBy     = null;
//                     existing.PaidAt     = null;
//                     existing.Remarks    = null;
//                 }

//                 existing.BasicSalary      = req.BasicSalary;
//                 existing.TotalWorkingDays = totalWorkingDays;
//                 existing.PresentDays      = presentDays;
//                 existing.AbsentDays       = absentDays;
//                 existing.LateDays         = lateDays;
//                 existing.PerDaySalary     = perDaySalary;
//                 existing.AbsentDeduction  = absentDeduction;
//                 existing.LateDeduction    = lateDeduction;
//                 existing.ManualDeduction  = manualDeduction;
//                 existing.ManualDeductionReason = manualDeductionReason;
//                 existing.TotalDeduction   = totalDeduction;
//                 existing.NetSalary        = netSalary;
//                 existing.GeneratedAt      = DateTime.UtcNow;
//                 existing.GeneratedBy      = GetCurrentUserId();
//             }
//             else
//             {
//                 existing = new PayrollRecord
//                 {
//                     EmployeeId            = req.EmployeeId,
//                     Month                 = req.Month,
//                     Year                  = req.Year,
//                     BasicSalary           = req.BasicSalary,
//                     TotalWorkingDays      = totalWorkingDays,
//                     PresentDays           = presentDays,
//                     AbsentDays            = absentDays,
//                     LateDays              = lateDays,
//                     PerDaySalary          = perDaySalary,
//                     AbsentDeduction       = absentDeduction,
//                     LateDeduction         = lateDeduction,
//                     ManualDeduction       = manualDeduction,
//                     ManualDeductionReason = manualDeductionReason,
//                     TotalDeduction        = totalDeduction,
//                     NetSalary             = netSalary,
//                     Status                = "draft",
//                     GeneratedAt           = DateTime.UtcNow,
//                     GeneratedBy           = GetCurrentUserId()
//                 };
//                 _db.PayrollRecords.Add(existing);
//             }

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = "Payroll calculated successfully.",
//                 success = true,
//                 data    = await MapPayrollResponseAsync(existing, employee)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  2. POST /api/Payroll/deduction  — Admin only
//         //  Manual deduction (optional) — only on draft payrolls
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("deduction")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> AddDeduction([FromBody] PayrollDeductionRequest req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found or inactive.", success = false });

//             var payroll = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             if (payroll == null)
//                 return NotFound(new
//                 {
//                     message = $"No payroll record for {employee.UserName} ({req.Month}/{req.Year}). Calculate first.",
//                     success = false
//                 });

//             if (payroll.Status == "paid")
//                 return BadRequest(new { message = "Cannot modify a paid payroll. Recalculate to reset.", success = false });

//             if (payroll.Status == "approved")
//                 return BadRequest(new { message = "Payroll is approved. Recalculate to modify.", success = false });

//             payroll.ManualDeduction       = req.Amount;
//             payroll.ManualDeductionReason = req.Reason;
//             payroll.TotalDeduction        = payroll.AbsentDeduction + payroll.LateDeduction + req.Amount;
//             payroll.NetSalary             = Math.Max(0, payroll.BasicSalary - payroll.TotalDeduction);

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Deduction ₹{req.Amount} added. Net salary: ₹{payroll.NetSalary:N2}.",
//                 success = true,
//                 data    = await MapPayrollResponseAsync(payroll, employee)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  3. POST /api/Payroll/approve  — Admin only
//         //  draft → approved
//         //  Employee ko payslip tab dikhegi jab admin approve kare
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("approve")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> ApprovePayroll([FromBody] PayrollApproveRequest req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found or inactive.", success = false });

//             var payroll = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             if (payroll == null)
//                 return NotFound(new
//                 {
//                     message = $"No payroll record for {employee.UserName} ({req.Month}/{req.Year}). Calculate first.",
//                     success = false
//                 });

//             if (payroll.Status == "paid")
//                 return BadRequest(new { message = "Payroll is already paid.", success = false });

//             if (payroll.Status == "approved")
//                 return BadRequest(new { message = "Payroll is already approved.", success = false });

//             payroll.Status     = "approved";
//             payroll.ApprovedBy = GetCurrentUserId();
//             payroll.ApprovedAt = DateTime.UtcNow;

//             if (!string.IsNullOrWhiteSpace(req.Remarks))
//                 payroll.Remarks = req.Remarks;

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Payroll approved for {employee.UserName} ({req.Month}/{req.Year}).",
//                 success = true,
//                 data    = await MapPayrollResponseAsync(payroll, employee)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  4. POST /api/Payroll/markpaid  — Admin only
//         //  approved → paid
//         //  Salary disburse hone ke baad mark karo
//         //  Tabhi employee PDF download kar sakta hai
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("markpaid")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> MarkPaid([FromBody] PayrollMarkPaidRequest req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found or inactive.", success = false });

//             var payroll = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             if (payroll == null)
//                 return NotFound(new { message = "Payroll record not found.", success = false });

//             if (payroll.Status == "paid")
//                 return BadRequest(new { message = "Salary is already marked as paid.", success = false });

//             if (payroll.Status != "approved")
//                 return BadRequest(new
//                 {
//                     message = "Payroll must be approved before marking as paid.",
//                     success = false
//                 });

//             payroll.Status = "paid";
//             payroll.PaidBy = GetCurrentUserId();
//             payroll.PaidAt = DateTime.UtcNow;

//             if (!string.IsNullOrWhiteSpace(req.Remarks))
//                 payroll.Remarks = req.Remarks;

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Salary marked as paid for {employee.UserName} ({req.Month}/{req.Year}).",
//                 success = true,
//                 data    = await MapPayrollResponseAsync(payroll, employee)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  5. GET /api/Payroll/slip
//         //  Employee: only visible when approved or paid
//         //  Admin: visible at all statuses
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("slip")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> GetPayslip([FromQuery] PayrollSlipQuery req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             if (!IsAdmin() && req.EmployeeId != GetCurrentUserId())
//                 return Forbid();

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found.", success = false });

//             var payroll = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             if (payroll == null)
//                 return NotFound(new
//                 {
//                     message = $"No payroll record for {req.Month}/{req.Year}.",
//                     success = false
//                 });

//             // Employee: draft payroll not visible
//             if (!IsAdmin() && payroll.Status == "draft")
//                 return Ok(new
//                 {
//                     message = "Your payslip is being processed. It will be visible once admin approves it.",
//                     success = false,
//                     data    = (object?)null
//                 });

//             return Ok(new
//             {
//                 message = "Payslip fetched successfully.",
//                 success = true,
//                 data    = await MapPayrollResponseAsync(payroll, employee)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  6. GET /api/Payroll/slip/download-pdf
//         //  PDF download only when status is "paid"
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("slip/download-pdf")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> DownloadPayslipPdf([FromQuery] PayrollSlipQuery req)
//         {
//             if (!IsAdmin() && req.EmployeeId != GetCurrentUserId())
//                 return Forbid();

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found.", success = false });

//             var payroll = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             if (payroll == null)
//                 return NotFound(new { message = "Payroll record not found. Calculate first.", success = false });

//             if (payroll.Status != "paid")
//                 return BadRequest(new
//                 {
//                     message = payroll.Status == "draft"
//                         ? "Payslip is not approved yet. PDF not available."
//                         : "Salary payment is pending. PDF available once salary is disbursed.",
//                     success = false
//                 });

//             try
//             {
//                 string approvedByName = "-";
//                 if (payroll.ApprovedBy.HasValue)
//                 {
//                     var approver = await _db.Users.FindAsync(payroll.ApprovedBy.Value);
//                     approvedByName = approver?.UserName ?? "-";
//                 }

//                 string paidByName = "-";
//                 if (payroll.PaidBy.HasValue)
//                 {
//                     var paidBy = await _db.Users.FindAsync(payroll.PaidBy.Value);
//                     paidByName = paidBy?.UserName ?? "-";
//                 }

//                 var pdfBytes = _exportService.GeneratePayslipPdf(
//                     payroll,
//                     employee.UserName,
//                     employee.Department  ?? "-",
//                     employee.Designation ?? "-",
//                     approvedByName,
//                     paidByName);

//                 var fileName = $"Payslip_{employee.UserName}_{payroll.Month:D2}_{payroll.Year}.pdf";
//                 return File(pdfBytes, "application/pdf", fileName);
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new { message = $"PDF generation failed: {ex.Message}", success = false });
//             }
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  7. POST /api/Payroll/export  — Admin only
//         //  employeeId null/0 = all employees
//         //  Excel is protected (read-only)
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("export")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> ExportPayroll([FromBody] PayrollExportRequest req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             var allowedFormats = new[] { "pdf", "excel" };
//             if (!allowedFormats.Contains(req.Format.ToLower()))
//                 return BadRequest(new { message = "Format must be 'pdf' or 'excel'.", success = false });

//             var query = _db.PayrollRecords
//                 .Include(p => p.Employee)
//                 .Where(p => !p.IsDeleted && p.Month == req.Month && p.Year == req.Year);

//             if (req.EmployeeId.HasValue && req.EmployeeId.Value > 0)
//                 query = query.Where(p => p.EmployeeId == req.EmployeeId.Value);

//             var records = await query.OrderBy(p => p.Employee!.UserName).ToListAsync();

//             if (!records.Any())
//                 return NotFound(new
//                 {
//                     message = $"No payroll records found for {req.Month}/{req.Year}.",
//                     success = false
//                 });

//             var exportData = records
//                 .Select(p => (Payroll: p, EmployeeName: p.Employee?.UserName ?? "Unknown"))
//                 .ToList();

//             try
//             {
//                 if (req.Format.ToLower() == "excel")
//                 {
//                     var excelBytes = _exportService.GeneratePayrollExcel(exportData, req.Month, req.Year);
//                     var fileName   = $"Payroll_{req.Month:D2}_{req.Year}.xlsx";
//                     return File(excelBytes,
//                         "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
//                         fileName);
//                 }
//                 else
//                 {
//                     var pdfBytes = _exportService.GeneratePayrollPdf(exportData, req.Month, req.Year);
//                     var fileName = $"Payroll_{req.Month:D2}_{req.Year}.pdf";
//                     return File(pdfBytes, "application/pdf", fileName);
//                 }
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new { message = $"Export failed: {ex.Message}", success = false });
//             }
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  Private Helper
//         // ════════════════════════════════════════════════════════════════
//         private async Task<PayrollResponse> MapPayrollResponseAsync(PayrollRecord p, User employee)
//         {
//             string? approvedByName = null;
//             if (p.ApprovedBy.HasValue)
//             {
//                 var approver = await _db.Users.FindAsync(p.ApprovedBy.Value);
//                 approvedByName = approver?.UserName;
//             }

//             string? paidByName = null;
//             if (p.PaidBy.HasValue)
//             {
//                 var paidBy = await _db.Users.FindAsync(p.PaidBy.Value);
//                 paidByName = paidBy?.UserName;
//             }

//             return new PayrollResponse
//             {
//                 PayrollId             = p.PayrollId,
//                 EmployeeId            = p.EmployeeId,
//                 EmployeeName          = employee.UserName,
//                 Department            = employee.Department,
//                 Designation           = employee.Designation,
//                 Month                 = p.Month,
//                 Year                  = p.Year,
//                 MonthName             = new DateTime(p.Year, p.Month, 1).ToString("MMMM"),
//                 BasicSalary           = p.BasicSalary,
//                 TotalWorkingDays      = p.TotalWorkingDays,
//                 PresentDays           = p.PresentDays,
//                 AbsentDays            = p.AbsentDays,
//                 LateDays              = p.LateDays,
//                 PerDaySalary          = p.PerDaySalary,
//                 AbsentDeduction       = p.AbsentDeduction,
//                 LateDeduction         = p.LateDeduction,
//                 ManualDeduction       = p.ManualDeduction,
//                 ManualDeductionReason = p.ManualDeductionReason,
//                 TotalDeduction        = p.TotalDeduction,
//                 NetSalary             = p.NetSalary,
//                 Status                = p.Status,
//                 PaymentStatus         = p.Status == "paid" ? "Pay Completed" : "Pending",
//                 ApprovedAt            = p.ApprovedAt,
//                 ApprovedByName        = approvedByName,
//                 PaidAt                = p.PaidAt,
//                 PaidByName            = paidByName,
//                 GeneratedAt           = p.GeneratedAt,
//                 Remarks               = p.Remarks
//             };
//         }
//     }
// }















// // ======================= Controllers/PayrollController.cs =======================
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using attendance_api.Data;
// using attendance_api.DTOs;
// using attendance_api.Models;
// using attendance_api.Services;
// using System.Security.Claims;

// namespace attendance_api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     [Authorize]
//     public class PayrollController : ControllerBase
//     {
//         private readonly ApplicationDbContext _db;
//         private readonly IPayrollExportService _exportService;

//         private const string AdminRole   = "Admin";
//         private const string AdminLower  = "admin";
//         private const string AdminPolicy = "Admin,admin";

//         // ── Fixed office rules ─────────────────────────────────────────────
//         // Office hours : 10:00 AM – 6:00 PM
//         // Late cutoff  : 10:15 AM → ½ day deduction per late entry
//         // Off days     : Saturday & Sunday
//         private static readonly TimeSpan LateCutoff = new TimeSpan(10, 15, 0);

//         public PayrollController(ApplicationDbContext db, IPayrollExportService exportService)
//         {
//             _db            = db;
//             _exportService = exportService;
//         }

//         private int  GetCurrentUserId() =>
//             int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

//         private bool IsAdmin() =>
//             User.IsInRole(AdminRole) || User.IsInRole(AdminLower);

//         // ════════════════════════════════════════════════════════════════
//         //  1. GET /api/Payroll/calculate  — Admin only
//         //
//         //  Rules:
//         //  • Only allowed after the requested month is fully complete
//         //  • Auto-fetches attendance from DB
//         //  • Working days  = Mon–Fri only (Sat & Sun off)
//         //  • PerDaySalary  = BasicSalary ÷ TotalCalendarDays (28/30/31)
//         //  • Late rule     = InTime after 10:15 AM → 0.5 day deducted
//         //  • Sat/Sun work  = counts as present (reduces absent days)
//         //
//         //  Recalculation:
//         //  • approved/paid → reset to draft, recalculate fresh
//         //  • Manual deduction preserved
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("calculate")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> CalculatePayroll([FromQuery] PayrollCalculateRequest req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             // Guard: only calculate after month is over
//             var lastDayOfMonth = new DateTime(req.Year, req.Month,
//                 DateTime.DaysInMonth(req.Year, req.Month));

//             if (DateTime.Today <= lastDayOfMonth)
//                 return BadRequest(new
//                 {
//                     message = $"Payroll for {lastDayOfMonth:MMMM yyyy} can only be calculated " +
//                               $"after the month is complete (after {lastDayOfMonth:dd-MMM-yyyy}).",
//                     success = false
//                 });

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found or inactive.", success = false });

//             var from = new DateTime(req.Year, req.Month, 1);
//             var to   = lastDayOfMonth;

//             // ── Calendar days: actual days in month ───────────────────────
//             // Feb=28(or 29), Apr/Jun/Sep/Nov=30, rest=31
//             int totalCalendarDays = DateTime.DaysInMonth(req.Year, req.Month);

//             // ── Working days: Mon–Fri only ────────────────────────────────
//             int totalWorkingDays = 0;
//             for (var d = from; d <= to; d = d.AddDays(1))
//                 if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
//                     totalWorkingDays++;

//             // ── Auto-fetch attendance from DB ─────────────────────────────
//             var attendanceList = await _db.Attendances
//                 .Where(a => a.UserId == req.EmployeeId
//                          && a.AttendanceDate >= from
//                          && a.AttendanceDate <= to)
//                 .ToListAsync();

//             // ── Present days ──────────────────────────────────────────────
//             // ALL attendance records count (including Sat/Sun = reduces absent days)
//             // Cap at totalWorkingDays to avoid negative absents
//             int presentDays = Math.Min(attendanceList.Count, totalWorkingDays);
//             int absentDays  = Math.Max(0, totalWorkingDays - presentDays);

//             // ── Late: InTime after 10:15 AM → ½ day deduction ────────────
//             int lateDays = attendanceList.Count(a =>
//                 a.InTime.HasValue && a.InTime.Value > LateCutoff);

//             // ── Salary: PerDaySalary = Basic ÷ calendar days ─────────────
//             // Example: 40000 ÷ 28 = 1428.57 | 40000 ÷ 30 = 1333.33 | 40000 ÷ 31 = 1290.32
//             decimal perDaySalary    = Math.Round(req.BasicSalary / totalCalendarDays, 2);
//             decimal absentDeduction = Math.Round(perDaySalary * absentDays, 2);
//             decimal lateDeduction   = Math.Round(perDaySalary * 0.5m * lateDays, 2);

//             // ── Preserve manual deduction across recalculations ───────────
//             var existing = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             decimal manualDeduction       = existing?.ManualDeduction       ?? 0;
//             string? manualDeductionReason = existing?.ManualDeductionReason ?? null;

//             decimal totalDeduction = absentDeduction + lateDeduction + manualDeduction;
//             decimal netSalary      = Math.Max(0, req.BasicSalary - totalDeduction);

//             if (existing != null)
//             {
//                 // Reset approved/paid to draft for fresh recalculation
//                 if (existing.Status == "approved" || existing.Status == "paid")
//                 {
//                     existing.Status     = "draft";
//                     existing.ApprovedBy = null;
//                     existing.ApprovedAt = null;
//                     existing.PaidBy     = null;
//                     existing.PaidAt     = null;
//                     existing.Remarks    = null;
//                 }

//                 existing.BasicSalary           = req.BasicSalary;
//                 existing.TotalCalendarDays      = totalCalendarDays;   // ← NEW
//                 existing.TotalWorkingDays       = totalWorkingDays;
//                 existing.PresentDays            = presentDays;
//                 existing.AbsentDays             = absentDays;
//                 existing.LateDays               = lateDays;
//                 existing.PerDaySalary           = perDaySalary;
//                 existing.AbsentDeduction        = absentDeduction;
//                 existing.LateDeduction          = lateDeduction;
//                 existing.ManualDeduction        = manualDeduction;
//                 existing.ManualDeductionReason  = manualDeductionReason;
//                 existing.TotalDeduction         = totalDeduction;
//                 existing.NetSalary              = netSalary;
//                 existing.GeneratedAt            = DateTime.UtcNow;
//                 existing.GeneratedBy            = GetCurrentUserId();
//             }
//             else
//             {
//                 existing = new PayrollRecord
//                 {
//                     EmployeeId            = req.EmployeeId,
//                     Month                 = req.Month,
//                     Year                  = req.Year,
//                     BasicSalary           = req.BasicSalary,
//                     TotalCalendarDays     = totalCalendarDays,          // ← NEW
//                     TotalWorkingDays      = totalWorkingDays,
//                     PresentDays           = presentDays,
//                     AbsentDays            = absentDays,
//                     LateDays              = lateDays,
//                     PerDaySalary          = perDaySalary,
//                     AbsentDeduction       = absentDeduction,
//                     LateDeduction         = lateDeduction,
//                     ManualDeduction       = manualDeduction,
//                     ManualDeductionReason = manualDeductionReason,
//                     TotalDeduction        = totalDeduction,
//                     NetSalary             = netSalary,
//                     Status                = "draft",
//                     GeneratedAt           = DateTime.UtcNow,
//                     GeneratedBy           = GetCurrentUserId()
//                 };
//                 _db.PayrollRecords.Add(existing);
//             }

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = "Payroll calculated successfully.",
//                 success = true,
//                 data    = await MapPayrollResponseAsync(existing, employee)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  2. POST /api/Payroll/deduction  — Admin only
//         //  Manual deduction (optional) — only on draft payrolls
//         //  Amount = 0 → removes manual deduction
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("deduction")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> AddDeduction([FromBody] PayrollDeductionRequest req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found or inactive.", success = false });

//             var payroll = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             if (payroll == null)
//                 return NotFound(new
//                 {
//                     message = $"No payroll record for {employee.UserName} ({req.Month}/{req.Year}). Calculate first.",
//                     success = false
//                 });

//             if (payroll.Status == "paid")
//                 return BadRequest(new { message = "Cannot modify a paid payroll. Recalculate to reset.", success = false });

//             if (payroll.Status == "approved")
//                 return BadRequest(new { message = "Payroll is approved. Recalculate to modify.", success = false });

//             payroll.ManualDeduction       = req.Amount;
//             payroll.ManualDeductionReason = req.Reason;
//             payroll.TotalDeduction        = payroll.AbsentDeduction + payroll.LateDeduction + req.Amount;
//             payroll.NetSalary             = Math.Max(0, payroll.BasicSalary - payroll.TotalDeduction);

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = req.Amount == 0
//                     ? $"Manual deduction removed. Net salary: ₹{payroll.NetSalary:N2}."
//                     : $"Deduction ₹{req.Amount} added. Net salary: ₹{payroll.NetSalary:N2}.",
//                 success = true,
//                 data    = await MapPayrollResponseAsync(payroll, employee)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  3. POST /api/Payroll/approve  — Admin only
//         //  draft → approved
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("approve")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> ApprovePayroll([FromBody] PayrollApproveRequest req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found or inactive.", success = false });

//             var payroll = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             if (payroll == null)
//                 return NotFound(new
//                 {
//                     message = $"No payroll record for {employee.UserName} ({req.Month}/{req.Year}). Calculate first.",
//                     success = false
//                 });

//             if (payroll.Status == "paid")
//                 return BadRequest(new { message = "Payroll is already paid.", success = false });

//             if (payroll.Status == "approved")
//                 return BadRequest(new { message = "Payroll is already approved.", success = false });

//             payroll.Status     = "approved";
//             payroll.ApprovedBy = GetCurrentUserId();
//             payroll.ApprovedAt = DateTime.UtcNow;

//             if (!string.IsNullOrWhiteSpace(req.Remarks))
//                 payroll.Remarks = req.Remarks;

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Payroll approved for {employee.UserName} ({req.Month}/{req.Year}).",
//                 success = true,
//                 data    = await MapPayrollResponseAsync(payroll, employee)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  4. POST /api/Payroll/markpaid  — Admin only
//         //  approved → paid
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("markpaid")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> MarkPaid([FromBody] PayrollMarkPaidRequest req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found or inactive.", success = false });

//             var payroll = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             if (payroll == null)
//                 return NotFound(new { message = "Payroll record not found.", success = false });

//             if (payroll.Status == "paid")
//                 return BadRequest(new { message = "Salary is already marked as paid.", success = false });

//             if (payroll.Status != "approved")
//                 return BadRequest(new
//                 {
//                     message = "Payroll must be approved before marking as paid.",
//                     success = false
//                 });

//             payroll.Status = "paid";
//             payroll.PaidBy = GetCurrentUserId();
//             payroll.PaidAt = DateTime.UtcNow;

//             if (!string.IsNullOrWhiteSpace(req.Remarks))
//                 payroll.Remarks = req.Remarks;

//             await _db.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = $"Salary marked as paid for {employee.UserName} ({req.Month}/{req.Year}).",
//                 success = true,
//                 data    = await MapPayrollResponseAsync(payroll, employee)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  5. GET /api/Payroll/slip
//         //  Employee: only visible when approved or paid
//         //  Admin: visible at all statuses
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("slip")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> GetPayslip([FromQuery] PayrollSlipQuery req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             if (!IsAdmin() && req.EmployeeId != GetCurrentUserId())
//                 return Forbid();

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found.", success = false });

//             var payroll = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             if (payroll == null)
//                 return NotFound(new
//                 {
//                     message = $"No payroll record for {req.Month}/{req.Year}.",
//                     success = false
//                 });

//             if (!IsAdmin() && payroll.Status == "draft")
//                 return Ok(new
//                 {
//                     message = "Your payslip is being processed. It will be visible once admin approves it.",
//                     success = false,
//                     data    = (object?)null
//                 });

//             return Ok(new
//             {
//                 message = "Payslip fetched successfully.",
//                 success = true,
//                 data    = await MapPayrollResponseAsync(payroll, employee)
//             });
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  6. GET /api/Payroll/slip/download-pdf
//         //  PDF download only when status is "paid"
//         // ════════════════════════════════════════════════════════════════
//         [HttpGet("slip/download-pdf")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> DownloadPayslipPdf([FromQuery] PayrollSlipQuery req)
//         {
//             if (!IsAdmin() && req.EmployeeId != GetCurrentUserId())
//                 return Forbid();

//             var employee = await _db.Users.FindAsync(req.EmployeeId);
//             if (employee == null || !employee.IsActive)
//                 return NotFound(new { message = "Employee not found.", success = false });

//             var payroll = await _db.PayrollRecords
//                 .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
//                                        && p.Month == req.Month
//                                        && p.Year  == req.Year
//                                        && !p.IsDeleted);

//             if (payroll == null)
//                 return NotFound(new { message = "Payroll record not found. Calculate first.", success = false });

//             if (payroll.Status != "paid")
//                 return BadRequest(new
//                 {
//                     message = payroll.Status == "draft"
//                         ? "Payslip is not approved yet. PDF not available."
//                         : "Salary payment is pending. PDF available once salary is disbursed.",
//                     success = false
//                 });

//             try
//             {
//                 string approvedByName = "-";
//                 if (payroll.ApprovedBy.HasValue)
//                 {
//                     var approver = await _db.Users.FindAsync(payroll.ApprovedBy.Value);
//                     approvedByName = approver?.UserName ?? "-";
//                 }

//                 string paidByName = "-";
//                 if (payroll.PaidBy.HasValue)
//                 {
//                     var paidBy = await _db.Users.FindAsync(payroll.PaidBy.Value);
//                     paidByName = paidBy?.UserName ?? "-";
//                 }

//                 var pdfBytes = _exportService.GeneratePayslipPdf(
//                     payroll,
//                     employee.UserName,
//                     employee.Department  ?? "-",
//                     employee.Designation ?? "-",
//                     approvedByName,
//                     paidByName);

//                 var fileName = $"Payslip_{employee.UserName}_{payroll.Month:D2}_{payroll.Year}.pdf";
//                 return File(pdfBytes, "application/pdf", fileName);
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new { message = $"PDF generation failed: {ex.Message}", success = false });
//             }
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  7. POST /api/Payroll/export  — Admin only
//         //  employeeId null/0 = all employees
//         //  Excel is protected (read-only)
//         // ════════════════════════════════════════════════════════════════
//         [HttpPost("export")]
//         [Authorize(Roles = AdminPolicy)]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         public async Task<IActionResult> ExportPayroll([FromBody] PayrollExportRequest req)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

//             var allowedFormats = new[] { "pdf", "excel" };
//             if (!allowedFormats.Contains(req.Format.ToLower()))
//                 return BadRequest(new { message = "Format must be 'pdf' or 'excel'.", success = false });

//             var query = _db.PayrollRecords
//                 .Include(p => p.Employee)
//                 .Where(p => !p.IsDeleted && p.Month == req.Month && p.Year == req.Year);

//             if (req.EmployeeId.HasValue && req.EmployeeId.Value > 0)
//                 query = query.Where(p => p.EmployeeId == req.EmployeeId.Value);

//             var records = await query.OrderBy(p => p.Employee!.UserName).ToListAsync();

//             if (!records.Any())
//                 return NotFound(new
//                 {
//                     message = $"No payroll records found for {req.Month}/{req.Year}.",
//                     success = false
//                 });

//             var exportData = records
//                 .Select(p => (Payroll: p, EmployeeName: p.Employee?.UserName ?? "Unknown"))
//                 .ToList();

//             try
//             {
//                 if (req.Format.ToLower() == "excel")
//                 {
//                     var excelBytes = _exportService.GeneratePayrollExcel(exportData, req.Month, req.Year);
//                     var fileName   = $"Payroll_{req.Month:D2}_{req.Year}.xlsx";
//                     return File(excelBytes,
//                         "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
//                         fileName);
//                 }
//                 else
//                 {
//                     var pdfBytes = _exportService.GeneratePayrollPdf(exportData, req.Month, req.Year);
//                     var fileName = $"Payroll_{req.Month:D2}_{req.Year}.pdf";
//                     return File(pdfBytes, "application/pdf", fileName);
//                 }
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new { message = $"Export failed: {ex.Message}", success = false });
//             }
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  Private Helper — Map DB record → Response DTO
//         // ════════════════════════════════════════════════════════════════
//         private async Task<PayrollResponse> MapPayrollResponseAsync(PayrollRecord p, User employee)
//         {
//             string? approvedByName = null;
//             if (p.ApprovedBy.HasValue)
//             {
//                 var approver = await _db.Users.FindAsync(p.ApprovedBy.Value);
//                 approvedByName = approver?.UserName;
//             }

//             string? paidByName = null;
//             if (p.PaidBy.HasValue)
//             {
//                 var paidBy = await _db.Users.FindAsync(p.PaidBy.Value);
//                 paidByName = paidBy?.UserName;
//             }

//             return new PayrollResponse
//             {
//                 PayrollId             = p.PayrollId,
//                 EmployeeId            = p.EmployeeId,
//                 EmployeeName          = employee.UserName,
//                 Department            = employee.Department,
//                 Designation           = employee.Designation,
//                 Month                 = p.Month,
//                 Year                  = p.Year,
//                 MonthName             = new DateTime(p.Year, p.Month, 1).ToString("MMMM"),
//                 BasicSalary           = p.BasicSalary,
//                 TotalCalendarDays     = p.TotalCalendarDays,             // ← NEW
//                 TotalWorkingDays      = p.TotalWorkingDays,
//                 PresentDays           = p.PresentDays,
//                 AbsentDays            = p.AbsentDays,
//                 LateDays              = p.LateDays,
//                 PerDaySalary          = p.PerDaySalary,
//                 AbsentDeduction       = p.AbsentDeduction,
//                 LateDeduction         = p.LateDeduction,
//                 ManualDeduction       = p.ManualDeduction,
//                 ManualDeductionReason = p.ManualDeductionReason,
//                 TotalDeduction        = p.TotalDeduction,
//                 NetSalary             = p.NetSalary,
//                 Status                = p.Status,
//                 PaymentStatus         = p.Status == "paid" ? "Pay Completed" : "Pending",
//                 ApprovedAt            = p.ApprovedAt,
//                 ApprovedByName        = approvedByName,
//                 PaidAt                = p.PaidAt,
//                 PaidByName            = paidByName,
//                 GeneratedAt           = p.GeneratedAt,
//                 Remarks               = p.Remarks
//             };
//         }
//     }
// }

















// ======================= Controllers/PayrollController.cs =======================
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
    public class PayrollController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IPayrollExportService _exportService;

        private const string AdminRole   = "Admin";
        private const string AdminLower  = "admin";
        private const string AdminPolicy = "Admin,admin";

        // ── Fixed office rules ─────────────────────────────────────────────
        // Office hours : 10:00 AM – 6:00 PM
        // Off days     : Saturday & Sunday
        // Late cutoff  : Dynamic — sent by frontend in HH:mm format

        public PayrollController(ApplicationDbContext db, IPayrollExportService exportService)
        {
            _db            = db;
            _exportService = exportService;
        }

        private int  GetCurrentUserId() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        private bool IsAdmin() =>
            User.IsInRole(AdminRole) || User.IsInRole(AdminLower);

        // ════════════════════════════════════════════════════════════════
        //  1. GET /api/Payroll/calculate  — Admin only
        //
        //  Rules:
        //  • Only allowed after the requested month is fully complete
        //  • Auto-fetches attendance from DB
        //  • Working days  = Mon–Fri only (Sat & Sun off)
        //  • PerDaySalary  = BasicSalary ÷ TotalCalendarDays (28/30/31)
        //  • Late rule     = InTime after LateCutoff (from request) → 0.5 day deducted
        //  • Sat/Sun work  = counts as present (reduces absent days)
        //
        //  Recalculation:
        //  • approved/paid → reset to draft, recalculate fresh
        //  • Manual deduction preserved
        // ════════════════════════════════════════════════════════════════
        [HttpGet("calculate")]
        [Authorize(Roles = AdminPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CalculatePayroll([FromQuery] PayrollCalculateRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

            // ── Parse LateCutoff from request ─────────────────────────────
            if (!TimeSpan.TryParse(req.LateCutoff, out TimeSpan lateCutoff))
                return BadRequest(new
                {
                    message = "Invalid LateCutoff format. Use HH:mm (e.g. '10:15').",
                    success = false
                });

            // Guard: only calculate after month is over
            var lastDayOfMonth = new DateTime(req.Year, req.Month,
                DateTime.DaysInMonth(req.Year, req.Month));

            if (DateTime.Today <= lastDayOfMonth)
                return BadRequest(new
                {
                    message = $"Payroll for {lastDayOfMonth:MMMM yyyy} can only be calculated " +
                              $"after the month is complete (after {lastDayOfMonth:dd-MMM-yyyy}).",
                    success = false
                });

            var employee = await _db.Users.FindAsync(req.EmployeeId);
            if (employee == null || !employee.IsActive)
                return NotFound(new { message = "Employee not found or inactive.", success = false });

            var from = new DateTime(req.Year, req.Month, 1);
            var to   = lastDayOfMonth;

            // ── Calendar days: actual days in month ───────────────────────
            int totalCalendarDays = DateTime.DaysInMonth(req.Year, req.Month);

            // ── Working days: Mon–Fri only ────────────────────────────────
            int totalWorkingDays = 0;
            for (var d = from; d <= to; d = d.AddDays(1))
                if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                    totalWorkingDays++;

            // ── Auto-fetch attendance from DB ─────────────────────────────
            var attendanceList = await _db.Attendances
                .Where(a => a.UserId == req.EmployeeId
                         && a.AttendanceDate >= from
                         && a.AttendanceDate <= to)
                .ToListAsync();

            // ── Present days ──────────────────────────────────────────────
            // ALL attendance records count (including Sat/Sun = reduces absent days)
            // Cap at totalWorkingDays to avoid negative absents
            int presentDays = Math.Min(attendanceList.Count, totalWorkingDays);
            int absentDays  = Math.Max(0, totalWorkingDays - presentDays);

            // ── Late: InTime after lateCutoff → ½ day deduction ──────────
            int lateDays = attendanceList.Count(a =>
                a.InTime.HasValue && a.InTime.Value > lateCutoff);

            // ── Salary: PerDaySalary = Basic ÷ calendar days ─────────────
            decimal perDaySalary    = Math.Round(req.BasicSalary / totalCalendarDays, 2);
            decimal absentDeduction = Math.Round(perDaySalary * absentDays, 2);
            decimal lateDeduction   = Math.Round(perDaySalary * 0.5m * lateDays, 2);

            // ── Preserve manual deduction across recalculations ───────────
            var existing = await _db.PayrollRecords
                .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
                                       && p.Month == req.Month
                                       && p.Year  == req.Year
                                       && !p.IsDeleted);

            decimal manualDeduction       = existing?.ManualDeduction       ?? 0;
            string? manualDeductionReason = existing?.ManualDeductionReason ?? null;

            decimal totalDeduction = absentDeduction + lateDeduction + manualDeduction;
            decimal netSalary      = Math.Max(0, req.BasicSalary - totalDeduction);

            if (existing != null)
            {
                // Reset approved/paid to draft for fresh recalculation
                if (existing.Status == "approved" || existing.Status == "paid")
                {
                    existing.Status     = "draft";
                    existing.ApprovedBy = null;
                    existing.ApprovedAt = null;
                    existing.PaidBy     = null;
                    existing.PaidAt     = null;
                    existing.Remarks    = null;
                }

                existing.BasicSalary           = req.BasicSalary;
                existing.TotalCalendarDays      = totalCalendarDays;
                existing.TotalWorkingDays       = totalWorkingDays;
                existing.PresentDays            = presentDays;
                existing.AbsentDays             = absentDays;
                existing.LateDays               = lateDays;
                existing.PerDaySalary           = perDaySalary;
                existing.AbsentDeduction        = absentDeduction;
                existing.LateDeduction          = lateDeduction;
                existing.ManualDeduction        = manualDeduction;
                existing.ManualDeductionReason  = manualDeductionReason;
                existing.TotalDeduction         = totalDeduction;
                existing.NetSalary              = netSalary;
                existing.GeneratedAt            = DateTime.UtcNow;
                existing.GeneratedBy            = GetCurrentUserId();
            }
            else
            {
                existing = new PayrollRecord
                {
                    EmployeeId            = req.EmployeeId,
                    Month                 = req.Month,
                    Year                  = req.Year,
                    BasicSalary           = req.BasicSalary,
                    TotalCalendarDays     = totalCalendarDays,
                    TotalWorkingDays      = totalWorkingDays,
                    PresentDays           = presentDays,
                    AbsentDays            = absentDays,
                    LateDays              = lateDays,
                    PerDaySalary          = perDaySalary,
                    AbsentDeduction       = absentDeduction,
                    LateDeduction         = lateDeduction,
                    ManualDeduction       = manualDeduction,
                    ManualDeductionReason = manualDeductionReason,
                    TotalDeduction        = totalDeduction,
                    NetSalary             = netSalary,
                    Status                = "draft",
                    GeneratedAt           = DateTime.UtcNow,
                    GeneratedBy           = GetCurrentUserId()
                };
                _db.PayrollRecords.Add(existing);
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Payroll calculated successfully.",
                success = true,
                data    = await MapPayrollResponseAsync(existing, employee)
            });
        }

        // ════════════════════════════════════════════════════════════════
        //  2. POST /api/Payroll/deduction  — Admin only
        //  Manual deduction (optional) — only on draft payrolls
        //  Amount = 0 → removes manual deduction
        // ════════════════════════════════════════════════════════════════
        [HttpPost("deduction")]
        [Authorize(Roles = AdminPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AddDeduction([FromBody] PayrollDeductionRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

            var employee = await _db.Users.FindAsync(req.EmployeeId);
            if (employee == null || !employee.IsActive)
                return NotFound(new { message = "Employee not found or inactive.", success = false });

            var payroll = await _db.PayrollRecords
                .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
                                       && p.Month == req.Month
                                       && p.Year  == req.Year
                                       && !p.IsDeleted);

            if (payroll == null)
                return NotFound(new
                {
                    message = $"No payroll record for {employee.UserName} ({req.Month}/{req.Year}). Calculate first.",
                    success = false
                });

            if (payroll.Status == "paid")
                return BadRequest(new { message = "Cannot modify a paid payroll. Recalculate to reset.", success = false });

            if (payroll.Status == "approved")
                return BadRequest(new { message = "Payroll is approved. Recalculate to modify.", success = false });

            payroll.ManualDeduction       = req.Amount;
            payroll.ManualDeductionReason = req.Reason;
            payroll.TotalDeduction        = payroll.AbsentDeduction + payroll.LateDeduction + req.Amount;
            payroll.NetSalary             = Math.Max(0, payroll.BasicSalary - payroll.TotalDeduction);

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = req.Amount == 0
                    ? $"Manual deduction removed. Net salary: ₹{payroll.NetSalary:N2}."
                    : $"Deduction ₹{req.Amount} added. Net salary: ₹{payroll.NetSalary:N2}.",
                success = true,
                data    = await MapPayrollResponseAsync(payroll, employee)
            });
        }

        // ════════════════════════════════════════════════════════════════
        //  3. POST /api/Payroll/approve  — Admin only
        //  draft → approved
        // ════════════════════════════════════════════════════════════════
        [HttpPost("approve")]
        [Authorize(Roles = AdminPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ApprovePayroll([FromBody] PayrollApproveRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

            var employee = await _db.Users.FindAsync(req.EmployeeId);
            if (employee == null || !employee.IsActive)
                return NotFound(new { message = "Employee not found or inactive.", success = false });

            var payroll = await _db.PayrollRecords
                .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
                                       && p.Month == req.Month
                                       && p.Year  == req.Year
                                       && !p.IsDeleted);

            if (payroll == null)
                return NotFound(new
                {
                    message = $"No payroll record for {employee.UserName} ({req.Month}/{req.Year}). Calculate first.",
                    success = false
                });

            if (payroll.Status == "paid")
                return BadRequest(new { message = "Payroll is already paid.", success = false });

            if (payroll.Status == "approved")
                return BadRequest(new { message = "Payroll is already approved.", success = false });

            payroll.Status     = "approved";
            payroll.ApprovedBy = GetCurrentUserId();
            payroll.ApprovedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(req.Remarks))
                payroll.Remarks = req.Remarks;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Payroll approved for {employee.UserName} ({req.Month}/{req.Year}).",
                success = true,
                data    = await MapPayrollResponseAsync(payroll, employee)
            });
        }

        // ════════════════════════════════════════════════════════════════
        //  4. POST /api/Payroll/markpaid  — Admin only
        //  approved → paid
        // ════════════════════════════════════════════════════════════════
        [HttpPost("markpaid")]
        [Authorize(Roles = AdminPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkPaid([FromBody] PayrollMarkPaidRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

            var employee = await _db.Users.FindAsync(req.EmployeeId);
            if (employee == null || !employee.IsActive)
                return NotFound(new { message = "Employee not found or inactive.", success = false });

            var payroll = await _db.PayrollRecords
                .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
                                       && p.Month == req.Month
                                       && p.Year  == req.Year
                                       && !p.IsDeleted);

            if (payroll == null)
                return NotFound(new { message = "Payroll record not found.", success = false });

            if (payroll.Status == "paid")
                return BadRequest(new { message = "Salary is already marked as paid.", success = false });

            if (payroll.Status != "approved")
                return BadRequest(new
                {
                    message = "Payroll must be approved before marking as paid.",
                    success = false
                });

            payroll.Status = "paid";
            payroll.PaidBy = GetCurrentUserId();
            payroll.PaidAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(req.Remarks))
                payroll.Remarks = req.Remarks;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Salary marked as paid for {employee.UserName} ({req.Month}/{req.Year}).",
                success = true,
                data    = await MapPayrollResponseAsync(payroll, employee)
            });
        }

        // ════════════════════════════════════════════════════════════════
        //  5. GET /api/Payroll/slip
        //  Employee: only visible when approved or paid
        //  Admin: visible at all statuses
        // ════════════════════════════════════════════════════════════════
        [HttpGet("slip")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPayslip([FromQuery] PayrollSlipQuery req)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

            if (!IsAdmin() && req.EmployeeId != GetCurrentUserId())
                return Forbid();

            var employee = await _db.Users.FindAsync(req.EmployeeId);
            if (employee == null || !employee.IsActive)
                return NotFound(new { message = "Employee not found.", success = false });

            var payroll = await _db.PayrollRecords
                .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
                                       && p.Month == req.Month
                                       && p.Year  == req.Year
                                       && !p.IsDeleted);

            if (payroll == null)
                return NotFound(new
                {
                    message = $"No payroll record for {req.Month}/{req.Year}.",
                    success = false
                });

            if (!IsAdmin() && payroll.Status == "draft")
                return Ok(new
                {
                    message = "Your payslip is being processed. It will be visible once admin approves it.",
                    success = false,
                    data    = (object?)null
                });

            return Ok(new
            {
                message = "Payslip fetched successfully.",
                success = true,
                data    = await MapPayrollResponseAsync(payroll, employee)
            });
        }

        // ════════════════════════════════════════════════════════════════
        //  6. GET /api/Payroll/slip/download-pdf
        //  PDF download only when status is "paid"
        // ════════════════════════════════════════════════════════════════
        [HttpGet("slip/download-pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DownloadPayslipPdf([FromQuery] PayrollSlipQuery req)
        {
            if (!IsAdmin() && req.EmployeeId != GetCurrentUserId())
                return Forbid();

            var employee = await _db.Users.FindAsync(req.EmployeeId);
            if (employee == null || !employee.IsActive)
                return NotFound(new { message = "Employee not found.", success = false });

            var payroll = await _db.PayrollRecords
                .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId
                                       && p.Month == req.Month
                                       && p.Year  == req.Year
                                       && !p.IsDeleted);

            if (payroll == null)
                return NotFound(new { message = "Payroll record not found. Calculate first.", success = false });

            if (payroll.Status != "paid")
                return BadRequest(new
                {
                    message = payroll.Status == "draft"
                        ? "Payslip is not approved yet. PDF not available."
                        : "Salary payment is pending. PDF available once salary is disbursed.",
                    success = false
                });

            try
            {
                string approvedByName = "-";
                if (payroll.ApprovedBy.HasValue)
                {
                    var approver = await _db.Users.FindAsync(payroll.ApprovedBy.Value);
                    approvedByName = approver?.UserName ?? "-";
                }

                string paidByName = "-";
                if (payroll.PaidBy.HasValue)
                {
                    var paidBy = await _db.Users.FindAsync(payroll.PaidBy.Value);
                    paidByName = paidBy?.UserName ?? "-";
                }

                var pdfBytes = _exportService.GeneratePayslipPdf(
                    payroll,
                    employee.UserName,
                    employee.Department  ?? "-",
                    employee.Designation ?? "-",
                    approvedByName,
                    paidByName);

                var fileName = $"Payslip_{employee.UserName}_{payroll.Month:D2}_{payroll.Year}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"PDF generation failed: {ex.Message}", success = false });
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  7. POST /api/Payroll/export  — Admin only
        //  employeeId null/0 = all employees
        //  Excel is protected (read-only)
        // ════════════════════════════════════════════════════════════════
        [HttpPost("export")]
        [Authorize(Roles = AdminPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportPayroll([FromBody] PayrollExportRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request.", success = false, errors = ModelState });

            var allowedFormats = new[] { "pdf", "excel" };
            if (!allowedFormats.Contains(req.Format.ToLower()))
                return BadRequest(new { message = "Format must be 'pdf' or 'excel'.", success = false });

            var query = _db.PayrollRecords
                .Include(p => p.Employee)
                .Where(p => !p.IsDeleted && p.Month == req.Month && p.Year == req.Year);

            if (req.EmployeeId.HasValue && req.EmployeeId.Value > 0)
                query = query.Where(p => p.EmployeeId == req.EmployeeId.Value);

            var records = await query.OrderBy(p => p.Employee!.UserName).ToListAsync();

            if (!records.Any())
                return NotFound(new
                {
                    message = $"No payroll records found for {req.Month}/{req.Year}.",
                    success = false
                });

            var exportData = records
                .Select(p => (Payroll: p, EmployeeName: p.Employee?.UserName ?? "Unknown"))
                .ToList();

            try
            {
                if (req.Format.ToLower() == "excel")
                {
                    var excelBytes = _exportService.GeneratePayrollExcel(exportData, req.Month, req.Year);
                    var fileName   = $"Payroll_{req.Month:D2}_{req.Year}.xlsx";
                    return File(excelBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
                }
                else
                {
                    var pdfBytes = _exportService.GeneratePayrollPdf(exportData, req.Month, req.Year);
                    var fileName = $"Payroll_{req.Month:D2}_{req.Year}.pdf";
                    return File(pdfBytes, "application/pdf", fileName);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Export failed: {ex.Message}", success = false });
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  Private Helper — Map DB record → Response DTO
        // ════════════════════════════════════════════════════════════════
        private async Task<PayrollResponse> MapPayrollResponseAsync(PayrollRecord p, User employee)
        {
            string? approvedByName = null;
            if (p.ApprovedBy.HasValue)
            {
                var approver = await _db.Users.FindAsync(p.ApprovedBy.Value);
                approvedByName = approver?.UserName;
            }

            string? paidByName = null;
            if (p.PaidBy.HasValue)
            {
                var paidBy = await _db.Users.FindAsync(p.PaidBy.Value);
                paidByName = paidBy?.UserName;
            }

            return new PayrollResponse
            {
                PayrollId             = p.PayrollId,
                EmployeeId            = p.EmployeeId,
                EmployeeName          = employee.UserName,
                Department            = employee.Department,
                Designation           = employee.Designation,
                Month                 = p.Month,
                Year                  = p.Year,
                MonthName             = new DateTime(p.Year, p.Month, 1).ToString("MMMM"),
                BasicSalary           = p.BasicSalary,
                TotalCalendarDays     = p.TotalCalendarDays,
                TotalWorkingDays      = p.TotalWorkingDays,
                PresentDays           = p.PresentDays,
                AbsentDays            = p.AbsentDays,
                LateDays              = p.LateDays,
                PerDaySalary          = p.PerDaySalary,
                AbsentDeduction       = p.AbsentDeduction,
                LateDeduction         = p.LateDeduction,
                ManualDeduction       = p.ManualDeduction,
                ManualDeductionReason = p.ManualDeductionReason,
                TotalDeduction        = p.TotalDeduction,
                NetSalary             = p.NetSalary,
                Status                = p.Status,
                PaymentStatus         = p.Status == "paid" ? "Pay Completed" : "Pending",
                ApprovedAt            = p.ApprovedAt,
                ApprovedByName        = approvedByName,
                PaidAt                = p.PaidAt,
                PaidByName            = paidByName,
                GeneratedAt           = p.GeneratedAt,
                Remarks               = p.Remarks
            };
        }
    }
}