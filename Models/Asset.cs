// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace attendance_api.Models
// {
//     public class Asset
//     {
//         [Key]
//         public int AssetId { get; set; }

//         // ─── Asset Info ───────────────────────────────────────────────────
//         [Required, MaxLength(100)]
//         public string AssetName { get; set; } = string.Empty;   // e.g. "Dell Laptop"

//         [Required, MaxLength(50)]
//         public string AssetType { get; set; } = string.Empty;   // laptop / phone / tablet / other

//         [MaxLength(100)]
//         public string? AssetCode { get; set; }                  // e.g. "LPT-2024-001" (unique tag)

//         [MaxLength(100)]
//         public string? SerialNumber { get; set; }

//         [MaxLength(100)]
//         public string? Brand { get; set; }

//         [MaxLength(100)]
//         public string? Model { get; set; }

//         [MaxLength(500)]
//         public string? Description { get; set; }

//         // ─── Assignment ───────────────────────────────────────────────────
//         /// <summary>available / assigned / maintenance / retired</summary>
//         [MaxLength(20)]
//         public string Status { get; set; } = "available";

//         public int? AssignedToUserId { get; set; }

//         public DateTime? AssignedDate { get; set; }

//         public DateTime? ExpectedReturnDate { get; set; }

//         [MaxLength(500)]
//         public string? AssignmentNote { get; set; }

//         // ─── Return ───────────────────────────────────────────────────────
//         public DateTime? ReturnedDate { get; set; }

//         [MaxLength(500)]
//         public string? ReturnNote { get; set; }

//         /// <summary>good / damaged / lost</summary>
//         [MaxLength(20)]
//         public string? ReturnCondition { get; set; }

//         // ─── Meta ─────────────────────────────────────────────────────────
//         public int? CreatedByUserId { get; set; }

//         public DateTime CreatedOn { get; set; } = DateTime.Now;

//         public DateTime? UpdatedOn { get; set; }

//         public bool IsActive { get; set; } = true;

//         // ─── Navigation ───────────────────────────────────────────────────
//         [ForeignKey(nameof(AssignedToUserId))]
//         public User? AssignedToUser { get; set; }

//         [ForeignKey(nameof(CreatedByUserId))]
//         public User? CreatedByUser { get; set; }

//         public ICollection<AssetHistory> Histories { get; set; } = new List<AssetHistory>();
//     }

//     // ─── Asset Assignment History ─────────────────────────────────────────
//     public class AssetHistory
//     {
//         [Key]
//         public int HistoryId { get; set; }

//         public int AssetId { get; set; }

//         public int? UserId { get; set; }

//         [MaxLength(20)]
//         public string Action { get; set; } = string.Empty;     // assigned / returned / transferred

//         [MaxLength(500)]
//         public string? Note { get; set; }

//         [MaxLength(20)]
//         public string? Condition { get; set; }                 // good / damaged / lost (on return)

//         public DateTime ActionDate { get; set; } = DateTime.Now;

//         public int? ActionByUserId { get; set; }               // admin who did it

//         // ─── Navigation ───────────────────────────────────────────────────
//         [ForeignKey(nameof(AssetId))]
//         public Asset? Asset { get; set; }

//         [ForeignKey(nameof(UserId))]
//         public User? User { get; set; }
//     }
// }










// ======================= Models/Asset.cs =======================
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace attendance_api.Models
{
    public class Asset
    {
        [Key]
        public int AssetId { get; set; }

        // ─── Asset Info ───────────────────────────────────────────────────
        [Required, MaxLength(100)]
        public string AssetName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string AssetType { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? AssetCode { get; set; }

        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        [MaxLength(100)]
        public string? Brand { get; set; }

        [MaxLength(100)]
        public string? Model { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        // ─── Assignment ───────────────────────────────────────────────────
        /// <summary>available / assigned / maintenance / retired</summary>
        [MaxLength(20)]
        public string Status { get; set; } = "available";

        public int? AssignedToUserId { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }

        [MaxLength(500)]
        public string? AssignmentNote { get; set; }

        // ─── Return ───────────────────────────────────────────────────────
        public DateTime? ReturnedDate { get; set; }

        [MaxLength(500)]
        public string? ReturnNote { get; set; }

        /// <summary>good / damaged / lost</summary>
        [MaxLength(20)]
        public string? ReturnCondition { get; set; }

        // ─── Maintenance (NO NEW TABLE: stored in dbo.Assets) ─────────────
        /// <summary>preventive / corrective / repair / service</summary>
        [MaxLength(30)]
        public string? MaintenanceType { get; set; }

        [MaxLength(100)]
        public string? MaintenanceVendorName { get; set; }

        [MaxLength(100)]
        public string? MaintenanceTicketNo { get; set; }

        [MaxLength(500)]
        public string? MaintenanceIssue { get; set; }

        public DateTime? MaintenanceStartDate { get; set; }
        public DateTime? MaintenanceEndDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaintenanceCost { get; set; }

        [MaxLength(500)]
        public string? MaintenanceResolution { get; set; }

        public int? MaintenanceCreatedByUserId { get; set; }
        public int? MaintenanceCompletedByUserId { get; set; }

        // ─── Meta ─────────────────────────────────────────────────────────
        public int? CreatedByUserId { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;   // Fix #6: Now → UtcNow
        public DateTime? UpdatedOn { get; set; }
        public bool IsActive { get; set; } = true;

        // ─── Navigation ───────────────────────────────────────────────────
        [ForeignKey(nameof(AssignedToUserId))]
        public User? AssignedToUser { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public User? CreatedByUser { get; set; }

        public ICollection<AssetHistory> Histories { get; set; } = new List<AssetHistory>();
    }

    public class AssetHistory
    {
        [Key]
        public int HistoryId { get; set; }

        public int AssetId { get; set; }
        public int? UserId { get; set; }

        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        // Fix #4: MaxLength 2000 — maintenance events store JSON snapshot here
        // so full maintenance details (vendor, cost, ticket, resolution) are
        // never lost when a new maintenance run overwrites the Asset row.
        // Example JSON for maintenance_completed:
        // {"type":"repair","vendor":"Acme","ticket":"T01","issue":"...","resolution":"...","cost":1500.00,"startedAt":"...","completedAt":"..."}
        [MaxLength(2000)]
        public string? Note { get; set; }

        [MaxLength(20)]
        public string? Condition { get; set; }

        public DateTime ActionDate { get; set; } = DateTime.UtcNow;  // Fix #6: Now → UtcNow

        public int? ActionByUserId { get; set; }

        [ForeignKey(nameof(AssetId))]
        public Asset? Asset { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}