namespace attendance_api.DTOs
{
    // Task add karne ke liye
    public class AddDailyTaskDto
    {
        public DateTime TaskDate { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string? TaskDescription { get; set; }
        public string? ProjectName { get; set; }
        public string Status { get; set; } = "completed";
        public decimal HoursSpent { get; set; }
        public string? Remarks { get; set; }
    }

    // Task update karne ke liye
    public class UpdateDailyTaskDto
    {
        public string? TaskTitle { get; set; }
        public string? TaskDescription { get; set; }
        public string? ProjectName { get; set; }
        public string? Status { get; set; }
        public decimal? HoursSpent { get; set; }
        public string? Remarks { get; set; }
    }

    // Response DTO
    public class DailyTaskResponseDto
    {
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime TaskDate { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string? TaskDescription { get; set; }
        public string? ProjectName { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal HoursSpent { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    // Summary DTO — ek din ka total
    public class DailyTaskSummaryDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime TaskDate { get; set; }
        public int TotalTasks { get; set; }
        public decimal TotalHours { get; set; }
        public List<DailyTaskResponseDto> Tasks { get; set; } = new();
    }
}