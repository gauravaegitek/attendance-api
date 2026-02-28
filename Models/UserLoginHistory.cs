using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace attendance_api.Models
{
    [Table("UserLoginHistories")]
    public class UserLoginHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;

        // Login Info
        public DateTime? LoginDate { get; set; }
        public TimeSpan? LoginTime { get; set; }

        // Device Info
        [MaxLength(50)]
        public string? DeviceType { get; set; }   // Mobile / Desktop / Tablet / Web

        [MaxLength(200)]
        public string? DeviceName { get; set; }   // e.g. "Samsung Galaxy S21", "Chrome on Windows"

        // Logout Info
        public DateTime? LogoutDate { get; set; }
        public TimeSpan? LogoutTime { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation Property
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}