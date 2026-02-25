using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace attendance_api.Models
{
    public class DailyTask
    {
        [Key]
        public int TaskId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        public DateTime TaskDate { get; set; }

        [Required]
        public string TaskTitle { get; set; } = string.Empty;

        public string? TaskDescription { get; set; }

        public string? ProjectName { get; set; }

        // todo, inprogress, completed, pending
        public string Status { get; set; } = "completed";

        // Time spent in hours (e.g. 2.5 = 2hr 30min)
        public decimal HoursSpent { get; set; }

        public string? Remarks { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public DateTime? UpdatedOn { get; set; }
    }
}