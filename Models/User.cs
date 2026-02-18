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

//         [Required]
//         [Column("role")]
//         [MaxLength(50)]
//         public string Role { get; set; } = string.Empty;

//        [Column("deviceid")]
// [MaxLength(255)]
// public string? DeviceId { get; set; }

// [Column("macaddress")]
// [MaxLength(255)]
// public string? MacAddress { get; set; }

// [Column("roleid")]
// public int? RoleId { get; set; }

// [Column("lastseen")]
// public DateTime? LastSeen { get; set; }

//         [Column("createdon")]
//         public DateTime CreatedOn { get; set; } = DateTime.Now;

//         [Column("isactive")]
//         public bool IsActive { get; set; } = true;

//         // Navigation property
//         // Navigation property
// public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

// [ForeignKey("RoleId")]
// public Role? RoleEntity { get; set; }    }
// }









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

        // You can keep this for JWT simplicity (optional)
        [Required]
        [Column("role")]
        [MaxLength(50)]
        public string Role { get; set; } = string.Empty;

        // ✅ ADD THIS (FK)
        [Column("roleid")]
        public int? RoleId { get; set; }

        // ✅ ADD THIS (Navigation)
        public Role? RoleEntity { get; set; }

        [Column("deviceid")]
        [MaxLength(255)]
        public string? DeviceId { get; set; }

        // ✅ ADD THIS (your DB screenshot has macaddress column)
        [Column("macaddress")]
        [MaxLength(255)]
        public string? MacAddress { get; set; }

        [Column("lastseen")]
        public DateTime? LastSeen { get; set; }

        [Column("createdon")]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
}
