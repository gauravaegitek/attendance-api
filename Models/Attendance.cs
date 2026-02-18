// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace attendance_api.Models
// {
//     [Table("attendance")]
//     public class Attendance
//     {
//         [Key]
//         [Column("attendanceid")]
//         public int AttendanceId { get; set; }

//         [Required]
//         [Column("userid")]
//         public int UserId { get; set; }

//         [Required]
//         [Column("attendancedate")]
//         public DateTime AttendanceDate { get; set; }

//         [Column("intime")]
//         public TimeSpan? InTime { get; set; }

//         [Column("intimedatetime")]
//         public DateTime? InTimeDateTime { get; set; }

//         [Column("inlatitude")]
//         public decimal? InLatitude { get; set; }

//         [Column("inlongitude")]
//         public decimal? InLongitude { get; set; }

//         [Column("inlocationaddress")]
//         [MaxLength(500)]
//         public string? InLocationAddress { get; set; }

//         [Column("inselfie")]
// [MaxLength(500)]
// public string? InSelfie { get; set; }

// [Column("inbiometric")]
// public string? InBiometric { get; set; }

// [Column("outtime")]
// public TimeSpan? OutTime { get; set; }

//         [Column("outtimedatetime")]
//         public DateTime? OutTimeDateTime { get; set; }

//         [Column("outlatitude")]
//         public decimal? OutLatitude { get; set; }

//         [Column("outlongitude")]
//         public decimal? OutLongitude { get; set; }

//         [Column("outlocationaddress")]
//         [MaxLength(500)]
//         public string? OutLocationAddress { get; set; }

//         [Column("outselfie")]
// [MaxLength(500)]
// public string? OutSelfie { get; set; }

// [Column("outbiometric")]
// public string? OutBiometric { get; set; }

// [Column("totalhours")]
// public decimal? TotalHours { get; set; }

//         [Column("createdon")]
//         public DateTime CreatedOn { get; set; } = DateTime.Now;

//         [Column("updatedon")]
//         public DateTime? UpdatedOn { get; set; }

//         // Navigation property
//         [ForeignKey("UserId")]
//         public User User { get; set; } = null!;
//     }
// }












using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace attendance_api.Models
{
    [Table("attendance")]
    public class Attendance
    {
        [Key]
        [Column("attendanceid")]
        public int AttendanceId { get; set; }

        [Required]
        [Column("userid")]
        public int UserId { get; set; }

        [Required]
        [Column("attendancedate")]
        public DateTime AttendanceDate { get; set; }

        [Column("intime")]
        public TimeSpan? InTime { get; set; }

        [Column("intimedatetime")]
        public DateTime? InTimeDateTime { get; set; }

        [Column("inlatitude")]
        public decimal? InLatitude { get; set; }

        [Column("inlongitude")]
        public decimal? InLongitude { get; set; }

        [Column("inlocationaddress")]
        [MaxLength(500)]
        public string? InLocationAddress { get; set; }

        [Column("inselfie")]
        [MaxLength(500)]
        public string? InSelfie { get; set; }

        [Column("inbiometric")]
        public string? InBiometric { get; set; }

        [Column("outtime")]
        public TimeSpan? OutTime { get; set; }

        [Column("outtimedatetime")]
        public DateTime? OutTimeDateTime { get; set; }

        [Column("outlatitude")]
        public decimal? OutLatitude { get; set; }

        [Column("outlongitude")]
        public decimal? OutLongitude { get; set; }

        [Column("outlocationaddress")]
        [MaxLength(500)]
        public string? OutLocationAddress { get; set; }

        [Column("outselfie")]
        [MaxLength(500)]
        public string? OutSelfie { get; set; }

        [Column("outbiometric")]
        public string? OutBiometric { get; set; }

        [Column("totalhours")]
        public decimal? TotalHours { get; set; }

        [Column("createdon")]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [Column("updatedon")]
        public DateTime? UpdatedOn { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
