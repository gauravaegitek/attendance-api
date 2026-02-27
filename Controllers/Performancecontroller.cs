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
//     public class PerformanceController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;

//         public PerformanceController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         // ─── Helper: UserId from JWT ──────────────────────────────────────
//         private int GetUserId()
//         {
//             var claim = User.FindFirst(ClaimTypes.NameIdentifier);
//             return claim != null ? int.Parse(claim.Value) : 0;
//         }

//         // ─────────────────────────────────────────────────────────────────
//         // PRIVATE: Calculate attendance-based performance score (0-100)
//         //
//         // Score Formula:
//         //   Attendance %      → 60% weightage  (max 60 points)
//         //   Avg Working Hours → 30% weightage  (max 30 points, target = 8h)
//         //   WFH ratio         → 10% weightage  (max 10 points)
//         //
//         // Grade:
//         //   A = 85+  |  B = 70+  |  C = 50+  |  D = below 50
//         // ─────────────────────────────────────────────────────────────────
//         private async Task<EmployeeScoreDto> CalculateScore(int userId, int month, int year)
//         {
//             var user = await _context.Users.FindAsync(userId)
//                        ?? throw new Exception("User not found");

//             // Total working days (Mon-Fri, excluding holidays)
//             var firstDay = new DateTime(year, month, 1);
//             var lastDay  = firstDay.AddMonths(1).AddDays(-1);

//             var holidays = await _context.Holidays
//                 .Where(h => h.HolidayDate.Month == month && h.HolidayDate.Year == year && h.IsActive)
//                 .Select(h => h.HolidayDate.Date)
//                 .ToListAsync();

//             int totalWorkingDays = 0;
//             for (var d = firstDay; d <= lastDay; d = d.AddDays(1))
//                 if (d.DayOfWeek != DayOfWeek.Saturday &&
//                     d.DayOfWeek != DayOfWeek.Sunday &&
//                     !holidays.Contains(d.Date))
//                     totalWorkingDays++;

//             // Attendance records for this user in this month
//             var attendances = await _context.Attendances
//                 .Where(a => a.UserId == userId
//                          && a.AttendanceDate.Month == month
//                          && a.AttendanceDate.Year == year)
//                 .ToListAsync();

//             int presentDays = attendances.Count(a => a.InTime != null);

//             // Approved WFH days
//             int wfhDays = await _context.WFHRequests
//                 .CountAsync(w => w.UserId == userId
//                               && w.WFHDate.Month == month
//                               && w.WFHDate.Year == year
//                               && w.Status == "Approved");

//             int absentDays = Math.Max(0, totalWorkingDays - presentDays - wfhDays);

//             decimal attendancePct = totalWorkingDays > 0
//                 ? Math.Round((decimal)(presentDays + wfhDays) / totalWorkingDays * 100, 2)
//                 : 0;

//             // Average working hours (only complete days with out-time)
//             var completedDays = attendances
//                 .Where(a => a.TotalHours != null && a.TotalHours > 0)
//                 .ToList();

//             decimal avgHours = completedDays.Count > 0
//                 ? Math.Round((decimal)completedDays.Average(a => (double)a.TotalHours!.Value), 2)
//                 : 0;

//             // Score calculation
//             decimal attendanceScore = attendancePct * 0.60m;                         // max 60
//             decimal hoursScore      = Math.Min(avgHours / 8m * 30m, 30m);            // max 30
//             decimal wfhScore        = totalWorkingDays > 0
//                 ? Math.Min((decimal)wfhDays / totalWorkingDays * 100m * 0.10m, 10m)  // max 10
//                 : 0;

//             decimal finalScore = Math.Round(attendanceScore + hoursScore + wfhScore, 2);

//             string grade = finalScore >= 85 ? "A" :
//                            finalScore >= 70 ? "B" :
//                            finalScore >= 50 ? "C" : "D";

//             return new EmployeeScoreDto
//             {
//                 UserId                = userId,
//                 UserName              = user.UserName,
//                 Role                  = user.Role,
//                 Department            = string.IsNullOrEmpty(user.Department) ? user.Role : user.Department,
//                 Month                 = month,
//                 Year                  = year,
//                 TotalWorkingDays      = totalWorkingDays,
//                 PresentDays           = presentDays,
//                 WFHDays               = wfhDays,
//                 AbsentDays            = absentDays,
//                 AttendancePercentage  = attendancePct,
//                 AverageWorkingHours   = avgHours,
//                 PerformanceScore      = finalScore,
//                 Grade                 = grade
//             };
//         }

//         // ─────────────────────────────────────────────────────────────────
//         // GET /api/Performance/employeescore?month=2&year=2026
//         // Employee: own score  |  Admin: can pass userId param
//         // ─────────────────────────────────────────────────────────────────
//         [HttpGet("employeescore")]
//         public async Task<IActionResult> GetEmployeeScore(
//             [FromQuery] int month,
//             [FromQuery] int year,
//             [FromQuery] int? userId = null)
//         {
//             if (month < 1 || month > 12)
//                 return BadRequest(new { success = false, message = "Invalid month. Use 1-12" });

//             var loggedInUserId = GetUserId();
//             bool isAdmin       = User.IsInRole("admin");

//             int targetUserId = (isAdmin && userId.HasValue) ? userId.Value : loggedInUserId;

//             try
//             {
//                 var score = await CalculateScore(targetUserId, month, year);
//                 return Ok(new { success = true, message = "Performance score calculated", data = score });
//             }
//             catch (Exception ex)
//             {
//                 return NotFound(new { success = false, message = ex.Message });
//             }
//         }

//         // ─────────────────────────────────────────────────────────────────
//         // POST /api/Performance/review  — Admin: Submit monthly review
//         //
//         // Final Score = (AttendanceScore * 70%) + (ManualScore * 30%)
//         // If no ManualScore, FinalScore = AttendanceScore only
//         // ─────────────────────────────────────────────────────────────────
//         [HttpPost("review")]
//         [Authorize(Roles = "admin")]
//         public async Task<IActionResult> SubmitPerformanceReview([FromBody] PerformanceReviewRequestDto dto)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

//             var user = await _context.Users.FindAsync(dto.UserId);
//             if (user == null)
//                 return NotFound(new { success = false, message = "User not found" });

//             // Calculate attendance score
//             EmployeeScoreDto scoreData;
//             try { scoreData = await CalculateScore(dto.UserId, dto.Month, dto.Year); }
//             catch { scoreData = new EmployeeScoreDto { PerformanceScore = 0 }; }

//             decimal attendanceScore = scoreData.PerformanceScore;

//             decimal finalScore = dto.ManualScore.HasValue
//                 ? Math.Round((attendanceScore * 0.70m) + (dto.ManualScore.Value * 0.30m), 2)
//                 : attendanceScore;

//             string grade = finalScore >= 85 ? "A" :
//                            finalScore >= 70 ? "B" :
//                            finalScore >= 50 ? "C" : "D";

//             int adminId = GetUserId();

//             // Check if review already exists for this month/year
//             var existingReview = await _context.PerformanceReviews
//                 .FirstOrDefaultAsync(r => r.UserId == dto.UserId
//                                        && r.ReviewMonth == dto.Month
//                                        && r.ReviewYear == dto.Year);

//             if (existingReview != null)
//             {
//                 existingReview.AttendanceScore  = attendanceScore;
//                 existingReview.ManualScore      = dto.ManualScore;
//                 existingReview.FinalScore       = finalScore;
//                 existingReview.Grade            = grade;
//                 existingReview.ReviewerComments = dto.Comments;
//                 existingReview.ReviewedByUserId = adminId;
//             }
//             else
//             {
//                 _context.PerformanceReviews.Add(new PerformanceReview
//                 {
//                     UserId          = dto.UserId,
//                     ReviewMonth     = dto.Month,
//                     ReviewYear      = dto.Year,
//                     AttendanceScore = attendanceScore,
//                     ManualScore     = dto.ManualScore,
//                     FinalScore      = finalScore,
//                     Grade           = grade,
//                     ReviewerComments = dto.Comments,
//                     ReviewedByUserId = adminId,
//                     CreatedOn       = DateTime.Now
//                 });
//             }

//             await _context.SaveChangesAsync();

//             return Ok(new
//             {
//                 success = true,
//                 message = existingReview != null ? "Review updated successfully" : "Review submitted successfully",
//                 data    = new PerformanceReviewResponseDto
//                 {
//                     UserId          = dto.UserId,
//                     UserName        = user.UserName,
//                     Role            = user.Role,
//                     Month           = dto.Month,
//                     Year            = dto.Year,
//                     AttendanceScore = attendanceScore,
//                     ManualScore     = dto.ManualScore,
//                     FinalScore      = finalScore,
//                     Grade           = grade,
//                     Comments        = dto.Comments,
//                     CreatedOn       = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")
//                 }
//             });
//         }

//         // ─────────────────────────────────────────────────────────────────
//         // GET /api/Performance/ranking?month=2&year=2026&department=developer
//         // Admin: Department-wise employee ranking by performance score
//         // department param optional — if empty, returns all departments
//         // ─────────────────────────────────────────────────────────────────
//         [HttpGet("ranking")]
//         [Authorize(Roles = "admin")]
//         public async Task<IActionResult> GetDepartmentRanking(
//             [FromQuery] int month,
//             [FromQuery] int year,
//             [FromQuery] string? department = null)
//         {
//             if (month < 1 || month > 12)
//                 return BadRequest(new { success = false, message = "Invalid month. Use 1-12" });

//             var usersQuery = _context.Users.Where(u => u.IsActive);

//             if (!string.IsNullOrEmpty(department))
//                 usersQuery = usersQuery.Where(u => u.Role.ToLower() == department.ToLower());

//             var users = await usersQuery.ToListAsync();

//             if (!users.Any())
//                 return NotFound(new { success = false, message = "No users found" });

//             // Calculate score for each user
//             var scoreList = new List<EmployeeScoreDto>();
//             foreach (var u in users)
//             {
//                 try
//                 {
//                     var score = await CalculateScore(u.UserId, month, year);
//                     scoreList.Add(score);
//                 }
//                 catch { /* skip users with errors */ }
//             }

//             // Group by department and rank within each department
//             var rankingsByDept = scoreList
//                 .GroupBy(s => s.Department)
//                 .Select(g =>
//                 {
//                     var ranked = g.OrderByDescending(s => s.PerformanceScore)
//                                   .Select((s, idx) => new EmployeeRankDto
//                                   {
//                                       Rank                = idx + 1,
//                                       UserId              = s.UserId,
//                                       UserName            = s.UserName,
//                                       Role                = s.Role,
//                                       PerformanceScore    = s.PerformanceScore,
//                                       Grade               = s.Grade,
//                                       PresentDays         = s.PresentDays,
//                                       WFHDays             = s.WFHDays,
//                                       AttendancePercentage = s.AttendancePercentage
//                                   }).ToList();

//                     return new DepartmentRankingDto
//                     {
//                         Department = g.Key,
//                         Month      = month,
//                         Year       = year,
//                         Rankings   = ranked
//                     };
//                 })
//                 .OrderBy(d => d.Department)
//                 .ToList();

//             return Ok(new
//             {
//                 success        = true,
//                 message        = "Rankings retrieved successfully",
//                 totalEmployees = scoreList.Count,
//                 data           = rankingsByDept
//             });
//         }

//         // ─────────────────────────────────────────────────────────────────
//         // GET /api/Performance/reviews?month=2&year=2026
//         // Admin: All submitted performance reviews
//         // ─────────────────────────────────────────────────────────────────
//         [HttpGet("reviews")]
//         [Authorize(Roles = "admin")]
//         public async Task<IActionResult> GetAllReviews([FromQuery] int month, [FromQuery] int year)
//         {
//             if (month < 1 || month > 12)
//                 return BadRequest(new { success = false, message = "Invalid month. Use 1-12" });

//             var reviews = await _context.PerformanceReviews
//                 .Include(r => r.User)
//                 .Where(r => r.ReviewMonth == month && r.ReviewYear == year)
//                 .OrderByDescending(r => r.FinalScore)
//                 .ToListAsync();

//             var result = reviews.Select(r => new PerformanceReviewResponseDto
//             {
//                 ReviewId        = r.ReviewId,
//                 UserId          = r.UserId,
//                 UserName        = r.User?.UserName ?? "",
//                 Role            = r.User?.Role ?? "",
//                 Month           = r.ReviewMonth,
//                 Year            = r.ReviewYear,
//                 AttendanceScore = r.AttendanceScore,
//                 ManualScore     = r.ManualScore,
//                 FinalScore      = r.FinalScore,
//                 Grade           = r.Grade,
//                 Comments        = r.ReviewerComments,
//                 CreatedOn       = r.CreatedOn.ToString("dd-MMM-yyyy HH:mm:ss")
//             }).ToList();

//             return Ok(new { success = true, message = "Reviews retrieved", totalCount = result.Count, data = result });
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
    public class PerformanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PerformanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ─── Helper: UserId from JWT ──────────────────────────────────────
        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        // ─────────────────────────────────────────────────────────────────
        // PRIVATE: Calculate attendance-based performance score (0-100)
        //
        // Score Formula:
        //   Attendance %      → 60% weightage  (max 60 points)
        //   Avg Working Hours → 30% weightage  (max 30 points, target = 8h)
        //   WFH ratio         → 10% weightage  (max 10 points)
        //
        // Grade:
        //   A = 85+  |  B = 70+  |  C = 50+  |  D = below 50
        // ─────────────────────────────────────────────────────────────────
        private async Task<EmployeeScoreDto> CalculateScore(int userId, int month, int year)
        {
            var user = await _context.Users.FindAsync(userId)
                       ?? throw new Exception("User not found");

            // Total working days (Mon-Fri, excluding holidays)
            var firstDay = new DateTime(year, month, 1);
            var lastDay  = firstDay.AddMonths(1).AddDays(-1);

            var holidays = await _context.Holidays
                .Where(h => h.HolidayDate.Month == month && h.HolidayDate.Year == year && h.IsActive)
                .Select(h => h.HolidayDate.Date)
                .ToListAsync();

            int totalWorkingDays = 0;
            for (var d = firstDay; d <= lastDay; d = d.AddDays(1))
                if (d.DayOfWeek != DayOfWeek.Saturday &&
                    d.DayOfWeek != DayOfWeek.Sunday &&
                    !holidays.Contains(d.Date))
                    totalWorkingDays++;

            // Attendance records for this user in this month
            var attendances = await _context.Attendances
                .Where(a => a.UserId == userId
                         && a.AttendanceDate.Month == month
                         && a.AttendanceDate.Year == year)
                .ToListAsync();

            int presentDays = attendances.Count(a => a.InTime != null);

            // Approved WFH days
            int wfhDays = await _context.WFHRequests
                .CountAsync(w => w.UserId == userId
                              && w.WFHDate.Month == month
                              && w.WFHDate.Year == year
                              && w.Status == "Approved");

            int absentDays = Math.Max(0, totalWorkingDays - presentDays - wfhDays);

            decimal attendancePct = totalWorkingDays > 0
                ? Math.Round((decimal)(presentDays + wfhDays) / totalWorkingDays * 100, 2)
                : 0;

            // Average working hours (only complete days with out-time)
            var completedDays = attendances
                .Where(a => a.TotalHours != null && a.TotalHours > 0)
                .ToList();

            decimal avgHours = completedDays.Count > 0
                ? Math.Round((decimal)completedDays.Average(a => (double)a.TotalHours!.Value), 2)
                : 0;

            // Score calculation
            decimal attendanceScore = attendancePct * 0.60m;                         // max 60
            decimal hoursScore      = Math.Min(avgHours / 8m * 30m, 30m);            // max 30
            decimal wfhScore        = totalWorkingDays > 0
                ? Math.Min((decimal)wfhDays / totalWorkingDays * 100m * 0.10m, 10m)  // max 10
                : 0;

            decimal finalScore = Math.Round(attendanceScore + hoursScore + wfhScore, 2);

            string grade = finalScore >= 85 ? "A" :
                           finalScore >= 70 ? "B" :
                           finalScore >= 50 ? "C" : "D";

            return new EmployeeScoreDto
            {
                UserId                = userId,
                UserName              = user.UserName,
                Role                  = user.Role,
                Department            = string.IsNullOrEmpty(user.Department) ? user.Role : user.Department,
                Month                 = month,
                Year                  = year,
                TotalWorkingDays      = totalWorkingDays,
                PresentDays           = presentDays,
                WFHDays               = wfhDays,
                AbsentDays            = absentDays,
                AttendancePercentage  = attendancePct,
                AverageWorkingHours   = avgHours,
                PerformanceScore      = finalScore,
                Grade                 = grade
            };
        }

        // ─────────────────────────────────────────────────────────────────
        // GET /api/Performance/employeescore?month=2&year=2026
        // Employee: own score  |  Admin: can pass userId param
        // ─────────────────────────────────────────────────────────────────
        [HttpGet("employeescore")]
        public async Task<IActionResult> GetEmployeeScore(
            [FromQuery] int month,
            [FromQuery] int year,
            [FromQuery] int? userId = null)
        {
            if (month < 1 || month > 12)
                return BadRequest(new { success = false, message = "Invalid month. Use 1-12" });

            var loggedInUserId = GetUserId();
            bool isAdmin       = User.IsInRole("admin");

            int targetUserId = (isAdmin && userId.HasValue) ? userId.Value : loggedInUserId;

            try
            {
                var score = await CalculateScore(targetUserId, month, year);
                return Ok(new { success = true, message = "Performance score calculated", data = score });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // GET /api/Performance/myreviews?month=2&year=2026
        // Employee: View own performance reviews submitted by admin
        // month & year are optional — if not passed, returns all reviews
        // ─────────────────────────────────────────────────────────────────
        [HttpGet("myreviews")]
        public async Task<IActionResult> GetMyReviews(
            [FromQuery] int? month = null,
            [FromQuery] int? year = null)
        {
            var userId = GetUserId();

            var query = _context.PerformanceReviews
                .Where(r => r.UserId == userId);

            if (month.HasValue && month >= 1 && month <= 12)
                query = query.Where(r => r.ReviewMonth == month.Value);

            if (year.HasValue)
                query = query.Where(r => r.ReviewYear == year.Value);

            var reviews = await query
                .OrderByDescending(r => r.ReviewYear)
                .ThenByDescending(r => r.ReviewMonth)
                .ToListAsync();

            if (!reviews.Any())
                return NotFound(new { success = false, message = "No reviews found for this period" });

            var result = reviews.Select(r => new PerformanceReviewResponseDto
            {
                ReviewId        = r.ReviewId,
                UserId          = r.UserId,
                Month           = r.ReviewMonth,
                Year            = r.ReviewYear,
                AttendanceScore = r.AttendanceScore,
                ManualScore     = r.ManualScore,
                FinalScore      = r.FinalScore,
                Grade           = r.Grade,
                Comments        = r.ReviewerComments,
                CreatedOn       = r.CreatedOn.ToString("dd-MMM-yyyy HH:mm:ss")
            }).ToList();

            return Ok(new
            {
                success    = true,
                message    = "Your performance reviews",
                totalCount = result.Count,
                data       = result
            });
        }

        // ─────────────────────────────────────────────────────────────────
        // POST /api/Performance/review  — Admin: Submit monthly review
        //
        // Final Score = (AttendanceScore * 70%) + (ManualScore * 30%)
        // If no ManualScore, FinalScore = AttendanceScore only
        // ─────────────────────────────────────────────────────────────────
        [HttpPost("review")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> SubmitPerformanceReview([FromBody] PerformanceReviewRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                return NotFound(new { success = false, message = "User not found" });

            // Calculate attendance score
            EmployeeScoreDto scoreData;
            try { scoreData = await CalculateScore(dto.UserId, dto.Month, dto.Year); }
            catch { scoreData = new EmployeeScoreDto { PerformanceScore = 0 }; }

            decimal attendanceScore = scoreData.PerformanceScore;

            decimal finalScore = dto.ManualScore.HasValue
                ? Math.Round((attendanceScore * 0.70m) + (dto.ManualScore.Value * 0.30m), 2)
                : attendanceScore;

            string grade = finalScore >= 85 ? "A" :
                           finalScore >= 70 ? "B" :
                           finalScore >= 50 ? "C" : "D";

            int adminId = GetUserId();

            // Check if review already exists for this month/year
            var existingReview = await _context.PerformanceReviews
                .FirstOrDefaultAsync(r => r.UserId == dto.UserId
                                       && r.ReviewMonth == dto.Month
                                       && r.ReviewYear == dto.Year);

            if (existingReview != null)
            {
                existingReview.AttendanceScore  = attendanceScore;
                existingReview.ManualScore      = dto.ManualScore;
                existingReview.FinalScore       = finalScore;
                existingReview.Grade            = grade;
                existingReview.ReviewerComments = dto.Comments;
                existingReview.ReviewedByUserId = adminId;
            }
            else
            {
                _context.PerformanceReviews.Add(new PerformanceReview
                {
                    UserId           = dto.UserId,
                    ReviewMonth      = dto.Month,
                    ReviewYear       = dto.Year,
                    AttendanceScore  = attendanceScore,
                    ManualScore      = dto.ManualScore,
                    FinalScore       = finalScore,
                    Grade            = grade,
                    ReviewerComments = dto.Comments,
                    ReviewedByUserId = adminId,
                    CreatedOn        = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = existingReview != null ? "Review updated successfully" : "Review submitted successfully",
                data    = new PerformanceReviewResponseDto
                {
                    UserId          = dto.UserId,
                    UserName        = user.UserName,
                    Role            = user.Role,
                    Month           = dto.Month,
                    Year            = dto.Year,
                    AttendanceScore = attendanceScore,
                    ManualScore     = dto.ManualScore,
                    FinalScore      = finalScore,
                    Grade           = grade,
                    Comments        = dto.Comments,
                    CreatedOn       = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")
                }
            });
        }

        // ─────────────────────────────────────────────────────────────────
        // GET /api/Performance/ranking?month=2&year=2026&department=developer
        // Admin: Department-wise employee ranking by performance score
        // department param optional — if empty, returns all departments
        // ─────────────────────────────────────────────────────────────────
        [HttpGet("ranking")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetDepartmentRanking(
            [FromQuery] int month,
            [FromQuery] int year,
            [FromQuery] string? department = null)
        {
            if (month < 1 || month > 12)
                return BadRequest(new { success = false, message = "Invalid month. Use 1-12" });

            var usersQuery = _context.Users.Where(u => u.IsActive);

            if (!string.IsNullOrEmpty(department))
                usersQuery = usersQuery.Where(u => u.Role.ToLower() == department.ToLower());

            var users = await usersQuery.ToListAsync();

            if (!users.Any())
                return NotFound(new { success = false, message = "No users found" });

            // Calculate score for each user
            var scoreList = new List<EmployeeScoreDto>();
            foreach (var u in users)
            {
                try
                {
                    var score = await CalculateScore(u.UserId, month, year);
                    scoreList.Add(score);
                }
                catch { /* skip users with errors */ }
            }

            // Group by department and rank within each department
            var rankingsByDept = scoreList
                .GroupBy(s => s.Department)
                .Select(g =>
                {
                    var ranked = g.OrderByDescending(s => s.PerformanceScore)
                                  .Select((s, idx) => new EmployeeRankDto
                                  {
                                      Rank                 = idx + 1,
                                      UserId               = s.UserId,
                                      UserName             = s.UserName,
                                      Role                 = s.Role,
                                      PerformanceScore     = s.PerformanceScore,
                                      Grade                = s.Grade,
                                      PresentDays          = s.PresentDays,
                                      WFHDays              = s.WFHDays,
                                      AttendancePercentage = s.AttendancePercentage
                                  }).ToList();

                    return new DepartmentRankingDto
                    {
                        Department = g.Key,
                        Month      = month,
                        Year       = year,
                        Rankings   = ranked
                    };
                })
                .OrderBy(d => d.Department)
                .ToList();

            return Ok(new
            {
                success        = true,
                message        = "Rankings retrieved successfully",
                totalEmployees = scoreList.Count,
                data           = rankingsByDept
            });
        }

        // ─────────────────────────────────────────────────────────────────
        // GET /api/Performance/reviews?month=2&year=2026
        // Admin: All submitted performance reviews
        // ─────────────────────────────────────────────────────────────────
        [HttpGet("reviews")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllReviews([FromQuery] int month, [FromQuery] int year)
        {
            if (month < 1 || month > 12)
                return BadRequest(new { success = false, message = "Invalid month. Use 1-12" });

            var reviews = await _context.PerformanceReviews
                .Include(r => r.User)
                .Where(r => r.ReviewMonth == month && r.ReviewYear == year)
                .OrderByDescending(r => r.FinalScore)
                .ToListAsync();

            var result = reviews.Select(r => new PerformanceReviewResponseDto
            {
                ReviewId        = r.ReviewId,
                UserId          = r.UserId,
                UserName        = r.User?.UserName ?? "",
                Role            = r.User?.Role ?? "",
                Month           = r.ReviewMonth,
                Year            = r.ReviewYear,
                AttendanceScore = r.AttendanceScore,
                ManualScore     = r.ManualScore,
                FinalScore      = r.FinalScore,
                Grade           = r.Grade,
                Comments        = r.ReviewerComments,
                CreatedOn       = r.CreatedOn.ToString("dd-MMM-yyyy HH:mm:ss")
            }).ToList();

            return Ok(new { success = true, message = "Reviews retrieved", totalCount = result.Count, data = result });
        }
    }
}