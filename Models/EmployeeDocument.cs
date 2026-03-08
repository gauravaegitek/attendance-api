// // ======================= Models/EmployeeDocument.cs =======================
// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace attendance_api.Models
// {
//     public class EmployeeDocument
//     {
//         [Key]
//         public int DocumentId { get; set; }

//         // ─── Document Info ────────────────────────────────────────────────
//         [Required]
//         public int EmployeeId { get; set; }

//         /// <summary>ID / Certificate / Resume / Other</summary>
//         [Required, MaxLength(100)]
//         public string DocumentType { get; set; } = string.Empty;

//         [MaxLength(255)]
//         public string? Description { get; set; }

//         [Required, MaxLength(255)]
//         public string FileName { get; set; } = string.Empty;

//         [Required, MaxLength(500)]
//         public string FilePath { get; set; } = string.Empty;

//         public long FileSize { get; set; }

//         [MaxLength(20)]
//         public string FileExtension { get; set; } = string.Empty;

//         // ─── Meta ─────────────────────────────────────────────────────────
//         public int? UploadedByUserId { get; set; }
//         public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
//         public bool IsDeleted { get; set; } = false;

//         // ─── Navigation ───────────────────────────────────────────────────
//         [ForeignKey(nameof(EmployeeId))]
//         public User? Employee { get; set; }

//         [ForeignKey(nameof(UploadedByUserId))]
//         public User? UploadedByUser { get; set; }
//     }
// }











// // ======================= Models/EmployeeDocument.cs =======================
// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace attendance_api.Models
// {
//     public class EmployeeDocument
//     {
//         [Key]
//         public int DocumentId { get; set; }

//         // ─── Document Info ────────────────────────────────────────────────
//         [Required]
//         public int EmployeeId { get; set; }

//         /// <summary>ID / Certificate / Resume / Other</summary>
//         [Required, MaxLength(100)]
//         public string DocumentType { get; set; } = string.Empty;

//         [MaxLength(255)]
//         public string? Description { get; set; }

//         [Required, MaxLength(255)]
//         public string FileName { get; set; } = string.Empty;

//         [Required, MaxLength(500)]
//         public string FilePath { get; set; } = string.Empty;

//         public long FileSize { get; set; }

//         [MaxLength(20)]
//         public string FileExtension { get; set; } = string.Empty;

//         // ─── Upload Meta ──────────────────────────────────────────────────
//         public int? UploadedByUserId { get; set; }
//         public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

//         // ─── Verify ───────────────────────────────────────────────────────
//         /// <summary>pending / approved / rejected</summary>
//         [MaxLength(20)]
//         public string VerifyStatus { get; set; } = "pending";

//         public int? VerifiedBy { get; set; }
//         public DateTime? VerifiedAt { get; set; }

//         [MaxLength(500)]
//         public string? VerifyRemarks { get; set; }

//         // ─── Soft Delete ──────────────────────────────────────────────────
//         public bool IsDeleted { get; set; } = false;

//         // ─── Navigation ───────────────────────────────────────────────────
//         [ForeignKey(nameof(EmployeeId))]
//         public User? Employee { get; set; }

//         [ForeignKey(nameof(UploadedByUserId))]
//         public User? UploadedByUser { get; set; }
//     }
// }














// ======================= Models/EmployeeDocument.cs =======================
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace attendance_api.Models
{
    public class EmployeeDocument
    {
        [Key]
        public int DocumentId { get; set; }

        // ─── Document Info ────────────────────────────────────────────────
        [Required]
        public int EmployeeId { get; set; }

        /// <summary>ID / Certificate / Resume / Other</summary>
        [Required, MaxLength(100)]
        public string DocumentType { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }

        [Required, MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }

        [MaxLength(20)]
        public string FileExtension { get; set; } = string.Empty;

        // ─── Upload Meta ──────────────────────────────────────────────────
        public int? UploadedByUserId { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // ─── Verify ───────────────────────────────────────────────────────
        /// <summary>pending / approved / rejected</summary>
        [MaxLength(20)]
        public string VerifyStatus { get; set; } = "pending";

        public int? VerifiedBy { get; set; }
        public DateTime? VerifiedAt { get; set; }

        [MaxLength(500)]
        public string? VerifyRemarks { get; set; }

        // ─── Soft Delete ──────────────────────────────────────────────────
        public bool IsDeleted { get; set; } = false;

        // ─── Navigation ───────────────────────────────────────────────────
        [ForeignKey(nameof(EmployeeId))]
        public User? Employee { get; set; }

        [ForeignKey(nameof(UploadedByUserId))]
        public User? UploadedByUser { get; set; }
    }
}