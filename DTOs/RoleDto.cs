using System.ComponentModel.DataAnnotations;

namespace attendance_api.DTOs
{
    public class RoleDto
    {
        public int? RoleId { get; set; }

        [Required(ErrorMessage = "Role name is required")]
        [MaxLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        public bool RequiresSelfie { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }

    public class RoleListDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool RequiresSelfie { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}