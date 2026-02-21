using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace attendance_api.Models
{
    [Table("WFHRequests")]
    public class WFHRequest
    {
        [Key]
        [Column("wfhid")]
        public int WFHId { get; set; }

        [Required]
        [Column("userid")]
        public int UserId { get; set; }

        [Required]
        [Column("wfhdate")]
        public DateTime WFHDate { get; set; }

        [Required]
        [Column("reason")]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Column("status")]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        [Column("approvedbyuserid")]
        public int? ApprovedByUserId { get; set; }

        [Column("approvedon")]
        public DateTime? ApprovedOn { get; set; }

        [Column("rejectionreason")]
        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        [Column("createdon")]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}