// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace attendance_api.Models
// {
//     [Table("users")]
//     public class User
//     {
//         [Key]
//         [Column("userid")]
//         public int UserId { get; set; }

//         [Required]
//         [Column("username")]
//         [MaxLength(100)]
//         public string UserName { get; set; } = string.Empty;

//         [Required]
//         [Column("email")]
//         [MaxLength(100)]
//         [EmailAddress]
//         public string Email { get; set; } = string.Empty;

//         [Required]
//         [Column("passwordhash")]
//         [MaxLength(255)]
//         public string PasswordHash { get; set; } = string.Empty;

//         [Column("confirmpassword")]
//         [MaxLength(255)]
//         public string? ConfirmPassword { get; set; }

//         // You can keep this for JWT simplicity (optional)
//         [Required]
//         [Column("role")]
//         [MaxLength(50)]
//         public string Role { get; set; } = string.Empty;

//         // ✅ ADD THIS (FK)
//         [Column("roleid")]
//         public int? RoleId { get; set; }

//         // ✅ ADD THIS (Navigation)
//         public Role? RoleEntity { get; set; }

//         [Column("deviceid")]
//         [MaxLength(255)]
//         public string? DeviceId { get; set; }

//         // ✅ ADD THIS (your DB screenshot has macaddress column)
//         [Column("macaddress")]
//         [MaxLength(255)]
//         public string? MacAddress { get; set; }

//         [Column("lastseen")]
//         public DateTime? LastSeen { get; set; }

//         [Column("createdon")]
//         public DateTime CreatedOn { get; set; } = DateTime.Now;

//         [Column("isactive")]
//         public bool IsActive { get; set; } = true;

//         public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
//     }
// }







using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace attendance_api.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("userid")]
        public int UserId { get; set; }

        [Required]
        [Column("username")]
        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [Column("email")]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("passwordhash")]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("confirmpassword")]
        [MaxLength(255)]
        public string? ConfirmPassword { get; set; }

        [Required]
        [Column("role")]
        [MaxLength(50)]
        public string Role { get; set; } = string.Empty;

        [Column("roleid")]
        public int? RoleId { get; set; }

        public Role? RoleEntity { get; set; }

        [Column("deviceid")]
        [MaxLength(255)]
        public string? DeviceId { get; set; }

        [Column("macaddress")]
        [MaxLength(255)]
        public string? MacAddress { get; set; }

        [Column("lastseen")]
        public DateTime? LastSeen { get; set; }

        [Column("createdon")]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        // ─── Profile Fields ───────────────────────────────────────────
        [Column("phone")]
        [MaxLength(20)]
        public string? Phone { get; set; }

        [Column("department")]
        [MaxLength(100)]
        public string? Department { get; set; }

        [Column("designation")]
        [MaxLength(100)]
        public string? Designation { get; set; }

        [Column("profilephoto")]
        [MaxLength(500)]
        public string? ProfilePhoto { get; set; }

        [Column("dateofbirth")]
        public DateTime? DateOfBirth { get; set; }

        [Column("address")]
        [MaxLength(500)]
        public string? Address { get; set; }

        [Column("emergencycontact")]
        [MaxLength(100)]
        public string? EmergencyContact { get; set; }

        // ─── Navigation Properties ────────────────────────────────────
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<WFHRequest>? WFHRequests { get; set; }
        public ICollection<PerformanceReview>? PerformanceReviews { get; set; }
    }
}