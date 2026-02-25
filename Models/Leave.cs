using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace attendance_api.Models
{
    public class Leave
    {
        [Key]
        public int LeaveId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        public string LeaveType { get; set; } = string.Empty;
        // casual, sick, earned, halfday, unpaid

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        public int TotalDays { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;

        // pending, approved, rejected
        public string Status { get; set; } = "pending";

        public string? AdminRemark { get; set; }

        public int? ApprovedByUserId { get; set; }

        public DateTime? ApprovedOn { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}