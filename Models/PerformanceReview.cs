using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace attendance_api.Models
{
    [Table("PerformanceReviews")]
    public class PerformanceReview
    {
        [Key]
        [Column("reviewid")]
        public int ReviewId { get; set; }

        [Required]
        [Column("userid")]
        public int UserId { get; set; }

        [Required]
        [Column("reviewmonth")]
        public int ReviewMonth { get; set; }

        [Required]
        [Column("reviewyear")]
        public int ReviewYear { get; set; }

        [Column("attendancescore", TypeName = "decimal(5,2)")]
        public decimal AttendanceScore { get; set; }

        [Column("manualscore", TypeName = "decimal(5,2)")]
        public decimal? ManualScore { get; set; }

        [Column("finalscore", TypeName = "decimal(5,2)")]
        public decimal FinalScore { get; set; }

        [Column("grade")]
        [MaxLength(5)]
        public string Grade { get; set; } = "C";

        [Column("reviewercomments")]
        [MaxLength(1000)]
        public string? ReviewerComments { get; set; }

        [Column("reviewedbyuserid")]
        public int? ReviewedByUserId { get; set; }

        [Column("createdon")]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}