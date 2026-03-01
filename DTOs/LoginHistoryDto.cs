namespace attendance_api.DTOs
{
    // ─── Logout ke liye ───────────────────────────────────────
    // (LoginDto & LoginResponseDto AuthDto.cs me already hain)
    // ─────────────────────────────────────────────────────────

    public class LogoutDto
    {
        public int LoginHistoryId { get; set; }
    }

    public class LoginHistoryDto
    {
        public int       Id         { get; set; }
        public int       UserId     { get; set; }
        public string    UserName   { get; set; } = string.Empty;
        public DateTime? LoginDate  { get; set; }
        public string?   LoginTime  { get; set; }
        public string?   DeviceType { get; set; }
        public string?   DeviceName { get; set; }
        public DateTime? LogoutDate { get; set; }
        public string?   LogoutTime { get; set; }
        public bool      IsActive   { get; set; }
        // ✅ NEW
        public int?      TotalMinutes  { get; set; }
        public string?   TotalDuration { get; set; }
        public string?   LogoutReason  { get; set; }
        public string?   SessionStatus { get; set; }

        public DateTime  CreatedAt  { get; set; }
        public DateTime  UpdatedAt  { get; set; }
    }

    public class LoginHistoryFilterDto
    {
        public int?      UserId     { get; set; }
        public DateTime? FromDate   { get; set; }
        public DateTime? ToDate     { get; set; }
        public string?   DeviceType { get; set; }
        public bool?     IsActive   { get; set; }
    }
}