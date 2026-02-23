using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace attendance_api.Models
{
    [Table("notifications")]
    public class Notification
    {
        [Key]
        [Column("notificationid")]
        public int NotificationId { get; set; }

        [Column("userid")]
        public int UserId { get; set; }

        [Column("title")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Column("message")]
        public string Message { get; set; } = string.Empty;

        [Column("type")]
        [MaxLength(50)]
        public string Type { get; set; } = "alert"; // alert / reminder

        [Column("isread")]
        public bool IsRead { get; set; } = false;

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User? User { get; set; }
    }
}