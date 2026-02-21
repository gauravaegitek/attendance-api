using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace attendance_api.DTOs
{
    // ─── Submit WFH Request ───────────────────────────────────────────
    public class WFHRequestDto
    {
        [Required(ErrorMessage = "WFH date is required")]
        public DateTime WFHDate { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        [MaxLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string Reason { get; set; } = string.Empty;
    }

    // ─── Admin: Approve / Reject ──────────────────────────────────────
    public class WFHApprovalDto
    {
        [Required]
        public int WFHId { get; set; }

        [Required(ErrorMessage = "Action is required (Approved or Rejected)")]
        public string Action { get; set; } = string.Empty;

        public string? RejectionReason { get; set; }
    }

    // ─── WFH Response ─────────────────────────────────────────────────
    public class WFHResponseDto
    {
        public int WFHId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string WFHDate { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }
        public string CreatedOn { get; set; } = string.Empty;
    }

    // ─── WFH Monthly Summary ──────────────────────────────────────────
    public class WFHSummaryDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalWFHDays { get; set; }
        public int ApprovedDays { get; set; }
        public int PendingDays { get; set; }
        public int RejectedDays { get; set; }
        public List<WFHResponseDto> Details { get; set; } = new();
    }
}