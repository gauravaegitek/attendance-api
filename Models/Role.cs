using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace attendance_api.Models
{
    [Table("roles")]
    public class Role
    {
        [Key]
        [Column("roleid")]
        public int RoleId { get; set; }

        [Required]
        [Column("rolename")]
        [MaxLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [Column("description")]
        [MaxLength(200)]
        public string? Description { get; set; }

        [Required]
        [Column("requiresselfie")]
        public bool RequiresSelfie { get; set; } = false;

        [Required]
        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [Column("createdon")]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        // Navigation property
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}