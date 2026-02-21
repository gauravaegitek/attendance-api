using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace attendance_api.DTOs
{
    // ─── Employee Score Response ──────────────────────────────────────
    public class EmployeeScoreDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalWorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int WFHDays { get; set; }
        public int AbsentDays { get; set; }
        public decimal AttendancePercentage { get; set; }
        public decimal AverageWorkingHours { get; set; }
        public decimal PerformanceScore { get; set; }
        public string Grade { get; set; } = string.Empty;
    }

    // ─── Performance Review Request ───────────────────────────────────
    public class PerformanceReviewRequestDto
    {
        [Required(ErrorMessage = "UserId is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Month is required")]
        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12")]
        public int Month { get; set; }

        [Required(ErrorMessage = "Year is required")]
        public int Year { get; set; }

        [Range(0, 100, ErrorMessage = "Manual score must be between 0 and 100")]
        public decimal? ManualScore { get; set; }

        [MaxLength(1000)]
        public string? Comments { get; set; }
    }

    // ─── Performance Review Response ──────────────────────────────────
    public class PerformanceReviewResponseDto
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal AttendanceScore { get; set; }
        public decimal? ManualScore { get; set; }
        public decimal FinalScore { get; set; }
        public string Grade { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public string CreatedOn { get; set; } = string.Empty;
    }

    // ─── Department Ranking ───────────────────────────────────────────
    public class DepartmentRankingDto
    {
        public string Department { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public List<EmployeeRankDto> Rankings { get; set; } = new();
    }

    public class EmployeeRankDto
    {
        public int Rank { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public decimal PerformanceScore { get; set; }
        public string Grade { get; set; } = string.Empty;
        public int PresentDays { get; set; }
        public int WFHDays { get; set; }
        public decimal AttendancePercentage { get; set; }
    }
}