namespace attendance_api.DTOs
{
    // Check-in karne ke liye
    public class LocationCheckInDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }
        public string WorkType { get; set; } = "office"; // office, visit, wfh
    }

    // Check-out karne ke liye
    public class LocationCheckOutDto
    {
        public int TrackingId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }

        // Client visit details (optional)
        public bool IsClientVisit { get; set; } = false;
        public string? ClientName { get; set; }
        public string? ClientAddress { get; set; }
        public double? ClientLatitude { get; set; }
        public double? ClientLongitude { get; set; }
        public string? VisitPurpose { get; set; }
        public string? MeetingNotes { get; set; }
        public string? Outcome { get; set; }
    }

    // Response DTO
    public class LocationTrackingResponseDto
    {
        public int TrackingId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public double? CheckInLatitude { get; set; }
        public double? CheckInLongitude { get; set; }
        public string? CheckInAddress { get; set; }
        public DateTime? CheckInTime { get; set; }

        public double? CheckOutLatitude { get; set; }
        public double? CheckOutLongitude { get; set; }
        public string? CheckOutAddress { get; set; }
        public DateTime? CheckOutTime { get; set; }

        public int? TotalTimeMinutes { get; set; }
        public string? TotalTimeFormatted { get; set; } // "2hr 30min"

        public bool IsClientVisit { get; set; }
        public string? ClientName { get; set; }
        public string? ClientAddress { get; set; }
        public string? VisitPurpose { get; set; }
        public string? MeetingNotes { get; set; }
        public string? Outcome { get; set; }

        public string WorkType { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
    }
}