namespace attendance_api.DTOs
{
    // Apply leave karne ke liye
    public class ApplyLeaveDto
    {
        public string LeaveType { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    // Admin approve/reject karne ke liye
    public class LeaveActionDto
    {
        public string Status { get; set; } = string.Empty; // approved / rejected
        public string? AdminRemark { get; set; }
    }

    // Response DTO
    public class LeaveResponseDto
    {
        public int LeaveId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string LeaveType { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalDays { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AdminRemark { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}