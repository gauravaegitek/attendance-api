using System.ComponentModel.DataAnnotations;

namespace attendance_api.DTOs
{
    public class HolidayDto
    {
        public int? HolidayId { get; set; }

        [Required(ErrorMessage = "Holiday name is required")]
        [MaxLength(100)]
        public string HolidayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Holiday date is required")]
        public DateTime HolidayDate { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class HolidayListDto
    {
        public int HolidayId { get; set; }
        public string HolidayName { get; set; } = string.Empty;
        public DateTime HolidayDate { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}