using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace attendance_api.Models
{
    [Table("faqs")]
    public class Faq
    {
        [Key]
        [Column("faqid")]
        public int FaqId { get; set; }

        [Column("question")]
        public string Question { get; set; } = string.Empty;

        [Column("answer")]
        public string Answer { get; set; } = string.Empty;

        [Column("category")]
        [MaxLength(100)]
        public string Category { get; set; } = "general";

        [Column("sortorder")]
        public int SortOrder { get; set; } = 0;

        [Column("isactive")]
        public bool IsActive { get; set; } = true;
    }
}