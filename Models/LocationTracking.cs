using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace attendance_api.Models
{
    public class LocationTracking
    {
        [Key]
        public int TrackingId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        // checkin ke time ki location
        public double? CheckInLatitude { get; set; }
        public double? CheckInLongitude { get; set; }
        public string? CheckInAddress { get; set; }

        public DateTime? CheckInTime { get; set; }

        // checkout ke time ki location
        public double? CheckOutLatitude { get; set; }
        public double? CheckOutLongitude { get; set; }
        public string? CheckOutAddress { get; set; }

        public DateTime? CheckOutTime { get; set; }

        // Total time on field (minutes)
        public int? TotalTimeMinutes { get; set; }

        // Client visit details
        public bool IsClientVisit { get; set; } = false;

        public string? ClientName { get; set; }
        public string? ClientAddress { get; set; }
        public double? ClientLatitude { get; set; }
        public double? ClientLongitude { get; set; }

        // Purpose of visit
        public string? VisitPurpose { get; set; }

        // What was discussed / requirement taken
        public string? MeetingNotes { get; set; }

        // Outcome
        public string? Outcome { get; set; }

        // visit, office, wfh
        public string WorkType { get; set; } = "office";

        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}