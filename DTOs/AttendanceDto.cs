using System.ComponentModel.DataAnnotations;

namespace attendance_api.DTOs
{
    public class MarkInDto
{
    public int UserId { get; set; }

    [Required]
    public DateTime AttendanceDate { get; set; }

    public TimeSpan? InTime { get; set; }

    [Required]
    public decimal Latitude { get; set; }

    [Required]
    public decimal Longitude { get; set; }

    [Required]
    public string LocationAddress { get; set; } = string.Empty;

    // Selfie is now conditional based on role
    public IFormFile? SelfieImage { get; set; }

    [Required(ErrorMessage = "Biometric data is required")]
    public string BiometricData { get; set; } = string.Empty;
}

    public class MarkOutDto
{
    public int UserId { get; set; }

    [Required]
    public DateTime AttendanceDate { get; set; }

    public TimeSpan? OutTime { get; set; }

    [Required]
    public decimal Latitude { get; set; }

    [Required]
    public decimal Longitude { get; set; }

    [Required]
    public string LocationAddress { get; set; } = string.Empty;

    // Selfie is now conditional based on role
    public IFormFile? SelfieImage { get; set; }

    [Required(ErrorMessage = "Biometric data is required")]
    public string BiometricData { get; set; } = string.Empty;
}

    public class AttendanceSummaryDto
    {
        public int AttendanceId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime AttendanceDate { get; set; }
        public string? InTime { get; set; }
        public string? OutTime { get; set; }
        public string? InLocation { get; set; }
        public string? OutLocation { get; set; }
        public decimal? TotalHours { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class UserSummaryRequestDto
    {
        // UserId will be auto-populated from JWT token
        public int UserId { get; set; }

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }
    }

    public class AdminSummaryRequestDto
    {
        // UserId will be auto-populated from JWT token (for tracking who requested)
        public int UserId { get; set; }

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }
    }

    // public class ExportUserSummaryDto
    // {
    //     // UserId will be auto-populated from JWT token
    //     public int UserId { get; set; }

    //     [Required]
    //     public DateTime FromDate { get; set; }
        
    //     [Required]
    //     public DateTime ToDate { get; set; }
    // }

    public class ExportAdminSummaryDto
    {
        // UserId will be auto-populated from JWT token
        public int? UserId { get; set; }
        
        [Required]
        public string Role { get; set; } = string.Empty;
        
        [Required]
        public DateTime FromDate { get; set; }
        
        [Required]
        public DateTime ToDate { get; set; }
        
        public string FilterType { get; set; } = "all";
    }
}
