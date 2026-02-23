using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace attendance_api.Models
{
    [Table("contactmessages")]
    public class ContactMessage
    {
        [Key]
        [Column("contactid")]
        public int ContactId { get; set; }

        [Column("userid")]
        public int UserId { get; set; }

        [Column("subject")]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Column("message")]
        public string Message { get; set; } = string.Empty;

        [Column("status")]
        [MaxLength(50)]
        public string Status { get; set; } = "pending";

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User? User { get; set; }
    }
}