using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace attendance_api.Models
{
    [Table("holidays")]
    public class Holiday
    {
        [Key]
        [Column("holidayid")]
        public int HolidayId { get; set; }

        [Required]
        [Column("holidayname")]
        [MaxLength(100)]
        public string HolidayName { get; set; } = string.Empty;

        [Required]
        [Column("holidaydate")]
        public DateTime HolidayDate { get; set; }

        [Column("description")]
        [MaxLength(200)]
        public string? Description { get; set; }

        [Required]
        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [Column("createdon")]
        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}