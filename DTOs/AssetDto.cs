// isme 5 api h 

// using System.ComponentModel.DataAnnotations;

// namespace attendance_api.DTOs
// {
//     // ─── Assign Asset ─────────────────────────────────────────────────────
//     public class AssignAssetRequest
//     {
//         [Required]
//         public int AssetId { get; set; }

//         [Required]
//         public int AssignedToUserId { get; set; }

//         public DateTime? ExpectedReturnDate { get; set; }

//         [MaxLength(500)]
//         public string? AssignmentNote { get; set; }
//     }

//     // ─── Create / Add Asset to Inventory ─────────────────────────────────
//     public class CreateAssetRequest
//     {
//         [Required, MaxLength(100)]
//         public string AssetName { get; set; } = string.Empty;

//         [Required, MaxLength(50)]
//         public string AssetType { get; set; } = string.Empty;  // laptop / phone / tablet / other

//         [MaxLength(100)]
//         public string? AssetCode { get; set; }

//         [MaxLength(100)]
//         public string? SerialNumber { get; set; }

//         [MaxLength(100)]
//         public string? Brand { get; set; }

//         [MaxLength(100)]
//         public string? Model { get; set; }

//         [MaxLength(500)]
//         public string? Description { get; set; }
//     }

//     // ─── Return Asset ─────────────────────────────────────────────────────
//     public class ReturnAssetRequest
//     {
//         [Required]
//         public int AssetId { get; set; }

//         [MaxLength(500)]
//         public string? ReturnNote { get; set; }

//         /// <summary>good / damaged / lost</summary>
//         [MaxLength(20)]
//         public string ReturnCondition { get; set; } = "good";
//     }

//     // ─── Response DTOs ────────────────────────────────────────────────────
//     public class AssetResponse
//     {
//         public int AssetId { get; set; }
//         public string AssetName { get; set; } = string.Empty;
//         public string AssetType { get; set; } = string.Empty;
//         public string? AssetCode { get; set; }
//         public string? SerialNumber { get; set; }
//         public string? Brand { get; set; }
//         public string? Model { get; set; }
//         public string? Description { get; set; }
//         public string Status { get; set; } = string.Empty;

//         // Assignment info
//         public int? AssignedToUserId { get; set; }
//         public string? AssignedToUserName { get; set; }
//         public DateTime? AssignedDate { get; set; }
//         public DateTime? ExpectedReturnDate { get; set; }
//         public string? AssignmentNote { get; set; }

//         // Return info
//         public DateTime? ReturnedDate { get; set; }
//         public string? ReturnNote { get; set; }
//         public string? ReturnCondition { get; set; }

//         public DateTime CreatedOn { get; set; }
//         public bool IsActive { get; set; }
//     }

//     public class AssetHistoryResponse
//     {
//         public int HistoryId { get; set; }
//         public int AssetId { get; set; }
//         public string? AssetName { get; set; }
//         public string? AssetType { get; set; }
//         public int? UserId { get; set; }
//         public string? UserName { get; set; }
//         public string Action { get; set; } = string.Empty;
//         public string? Note { get; set; }
//         public string? Condition { get; set; }
//         public DateTime ActionDate { get; set; }
//         public string? ActionByUserName { get; set; }
//     }
// }












// isme 10 api h 

// using System.ComponentModel.DataAnnotations;

// namespace attendance_api.DTOs
// {
//     // ─── Assign Asset ─────────────────────────────────────────────────────
//     public class AssignAssetRequest
//     {
//         [Required]
//         public int AssetId { get; set; }

//         [Required]
//         public int AssignedToUserId { get; set; }

//         public DateTime? ExpectedReturnDate { get; set; }

//         [MaxLength(500)]
//         public string? AssignmentNote { get; set; }
//     }

//     // ─── Create / Add Asset to Inventory ─────────────────────────────────
//     public class CreateAssetRequest
//     {
//         [Required, MaxLength(100)]
//         public string AssetName { get; set; } = string.Empty;

//         [Required, MaxLength(50)]
//         public string AssetType { get; set; } = string.Empty;

//         [MaxLength(100)]
//         public string? AssetCode { get; set; }

//         [MaxLength(100)]
//         public string? SerialNumber { get; set; }

//         [MaxLength(100)]
//         public string? Brand { get; set; }

//         [MaxLength(100)]
//         public string? Model { get; set; }

//         [MaxLength(500)]
//         public string? Description { get; set; }
//     }

//     // ✅ NEW ─── Update Asset Info ─────────────────────────────────────────
//     public class UpdateAssetRequest
//     {
//         [MaxLength(100)]
//         public string? AssetName { get; set; }

//         [MaxLength(50)]
//         public string? AssetType { get; set; }

//         [MaxLength(100)]
//         public string? AssetCode { get; set; }

//         [MaxLength(100)]
//         public string? SerialNumber { get; set; }

//         [MaxLength(100)]
//         public string? Brand { get; set; }

//         [MaxLength(100)]
//         public string? Model { get; set; }

//         [MaxLength(500)]
//         public string? Description { get; set; }
//     }

//     // ─── Return Asset ─────────────────────────────────────────────────────
//     public class ReturnAssetRequest
//     {
//         [Required]
//         public int AssetId { get; set; }

//         [MaxLength(500)]
//         public string? ReturnNote { get; set; }

//         /// <summary>good / damaged / lost</summary>
//         [MaxLength(20)]
//         public string ReturnCondition { get; set; } = "good";
//     }

//     // ✅ NEW ─── Change Status ─────────────────────────────────────────────
//     public class ChangeAssetStatusRequest
//     {
//         /// <summary>available / maintenance / retired</summary>
//         [Required, MaxLength(20)]
//         public string Status { get; set; } = string.Empty;

//         [MaxLength(500)]
//         public string? Note { get; set; }
//     }

//     // ─── Response DTOs ────────────────────────────────────────────────────
//     public class AssetResponse
//     {
//         public int AssetId { get; set; }
//         public string AssetName { get; set; } = string.Empty;
//         public string AssetType { get; set; } = string.Empty;
//         public string? AssetCode { get; set; }
//         public string? SerialNumber { get; set; }
//         public string? Brand { get; set; }
//         public string? Model { get; set; }
//         public string? Description { get; set; }
//         public string Status { get; set; } = string.Empty;

//         public int? AssignedToUserId { get; set; }
//         public string? AssignedToUserName { get; set; }
//         public DateTime? AssignedDate { get; set; }
//         public DateTime? ExpectedReturnDate { get; set; }
//         public string? AssignmentNote { get; set; }

//         public DateTime? ReturnedDate { get; set; }
//         public string? ReturnNote { get; set; }
//         public string? ReturnCondition { get; set; }

//         public DateTime CreatedOn { get; set; }
//         public bool IsActive { get; set; }
//     }

//     public class AssetHistoryResponse
//     {
//         public int HistoryId { get; set; }
//         public int AssetId { get; set; }
//         public string? AssetName { get; set; }
//         public string? AssetType { get; set; }
//         public int? UserId { get; set; }
//         public string? UserName { get; set; }
//         public string Action { get; set; } = string.Empty;
//         public string? Note { get; set; }
//         public string? Condition { get; set; }
//         public DateTime ActionDate { get; set; }
//         public string? ActionByUserName { get; set; }
//     }

//     // ✅ NEW ─── Summary / Dashboard ───────────────────────────────────────
//     public class AssetSummaryResponse
//     {
//         public int Total { get; set; }
//         public int Available { get; set; }
//         public int Assigned { get; set; }
//         public int Maintenance { get; set; }
//         public int Retired { get; set; }
//         public List<AssetTypeCount> ByType { get; set; } = new();
//     }

//     public class AssetTypeCount
//     {
//         public string AssetType { get; set; } = string.Empty;
//         public int Count { get; set; }
//     }
// }







// isme 6 api h 

// using System.ComponentModel.DataAnnotations;

// namespace attendance_api.DTOs
// {
//     // ─── POST /api/Asset/add ──────────────────────────────────────────────
//     public class CreateAssetRequest
//     {
//         [Required, MaxLength(100)]
//         public string AssetName { get; set; } = string.Empty;

//         [Required, MaxLength(50)]
//         public string AssetType { get; set; } = string.Empty;  // laptop / phone / tablet / other

//         [MaxLength(100)]
//         public string? AssetCode { get; set; }

//         [MaxLength(100)]
//         public string? SerialNumber { get; set; }

//         [MaxLength(100)]
//         public string? Brand { get; set; }

//         [MaxLength(100)]
//         public string? Model { get; set; }

//         [MaxLength(500)]
//         public string? Description { get; set; }
//     }

//     // ─── POST /api/Asset/assign ───────────────────────────────────────────
//     public class AssignAssetRequest
//     {
//         [Required]
//         public int AssetId { get; set; }

//         [Required]
//         public int AssignedToUserId { get; set; }

//         public DateTime? ExpectedReturnDate { get; set; }

//         [MaxLength(500)]
//         public string? AssignmentNote { get; set; }
//     }

//     // ─── PUT /api/Asset/return ────────────────────────────────────────────
//     public class ReturnAssetRequest
//     {
//         [Required]
//         public int AssetId { get; set; }

//         [MaxLength(500)]
//         public string? ReturnNote { get; set; }

//         /// <summary>good / damaged / lost</summary>
//         [MaxLength(20)]
//         public string ReturnCondition { get; set; } = "good";
//     }

//     // ─── Response ─────────────────────────────────────────────────────────
//     public class AssetResponse
//     {
//         public int      AssetId            { get; set; }
//         public string   AssetName          { get; set; } = string.Empty;
//         public string   AssetType          { get; set; } = string.Empty;
//         public string?  AssetCode          { get; set; }
//         public string?  SerialNumber       { get; set; }
//         public string?  Brand              { get; set; }
//         public string?  Model              { get; set; }
//         public string?  Description        { get; set; }
//         public string   Status             { get; set; } = string.Empty;

//         public int?     AssignedToUserId   { get; set; }
//         public string?  AssignedToUserName { get; set; }
//         public DateTime? AssignedDate      { get; set; }
//         public DateTime? ExpectedReturnDate { get; set; }
//         public string?  AssignmentNote     { get; set; }

//         public DateTime? ReturnedDate      { get; set; }
//         public string?  ReturnNote         { get; set; }
//         public string?  ReturnCondition    { get; set; }

//         public DateTime CreatedOn          { get; set; }
//     }

//     public class AssetHistoryResponse
//     {
//         public int      HistoryId        { get; set; }
//         public int      AssetId          { get; set; }
//         public string?  AssetName        { get; set; }
//         public string?  AssetType        { get; set; }
//         public int?     UserId           { get; set; }
//         public string?  UserName         { get; set; }
//         public string   Action           { get; set; } = string.Empty;  // assigned / returned
//         public string?  Note             { get; set; }
//         public string?  Condition        { get; set; }
//         public DateTime ActionDate       { get; set; }
//         public string?  ActionByUserName { get; set; }
//     }

//     public class AssetSummaryResponse
//     {
//         public int Total       { get; set; }
//         public int Available   { get; set; }
//         public int Assigned    { get; set; }
//         public List<AssetTypeCount> ByType { get; set; } = new();
//     }

//     public class AssetTypeCount
//     {
//         public string AssetType { get; set; } = string.Empty;
//         public int    Count     { get; set; }
//     }
// }










// // ======================= DTOs/AssetDTOs.cs =======================
// using System.ComponentModel.DataAnnotations;

// namespace attendance_api.DTOs
// {
//     // Fix #2: Reusable validation attribute — allowed values check (case-insensitive)
//     // Koi bhi invalid string pass karne par 400 BadRequest milega automatically.
//     public sealed class AllowedValuesAttribute : ValidationAttribute
//     {
//         private readonly string[] _allowed;
//         public AllowedValuesAttribute(params string[] allowed)
//         {
//             _allowed = allowed;
//             ErrorMessage = $"Value must be one of: {string.Join(", ", allowed)}.";
//         }
//         protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
//         {
//             if (value is null) return ValidationResult.Success;
//             return _allowed.Contains(value.ToString()!.ToLower())
//                 ? ValidationResult.Success
//                 : new ValidationResult(ErrorMessage);
//         }
//     }

//     // ─── POST /api/Asset/add ──────────────────────────────────────────────
//     public class CreateAssetRequest
//     {
//         [Required, MaxLength(100)]
//         public string AssetName { get; set; } = string.Empty;

//         [Required, MaxLength(50)]
//         public string AssetType { get; set; } = string.Empty;

//         [MaxLength(100)]
//         public string? AssetCode { get; set; }

//         [MaxLength(100)]
//         public string? SerialNumber { get; set; }

//         [MaxLength(100)]
//         public string? Brand { get; set; }

//         [MaxLength(100)]
//         public string? Model { get; set; }

//         [MaxLength(500)]
//         public string? Description { get; set; }
//     }

//     // ─── POST /api/Asset/assign ───────────────────────────────────────────
//     public class AssignAssetRequest
//     {
//         [Required]
//         public int AssetId { get; set; }

//         [Required]
//         public int AssignedToUserId { get; set; }

//         public DateTime? ExpectedReturnDate { get; set; }

//         [MaxLength(500)]
//         public string? AssignmentNote { get; set; }
//     }

//     // ─── PUT /api/Asset/return ────────────────────────────────────────────
//     public class ReturnAssetRequest
//     {
//         [Required]
//         public int AssetId { get; set; }

//         [MaxLength(500)]
//         public string? ReturnNote { get; set; }

//         // Fix #2: sirf "good", "damaged", "lost" hi valid hain
//         [MaxLength(20)]
//         [AllowedValues("good", "damaged", "lost")]
//         public string ReturnCondition { get; set; } = "good";
//     }

//     // ─── POST /api/Asset/maintenance/start ───────────────────────────────
//     public class StartMaintenanceRequest
//     {
//         [Required]
//         public int AssetId { get; set; }

//         // Fix #2: sirf valid maintenance types allowed
//         [MaxLength(30)]
//         [AllowedValues("preventive", "corrective", "repair", "service")]
//         public string MaintenanceType { get; set; } = "repair";

//         [MaxLength(100)]
//         public string? VendorName { get; set; }

//         [MaxLength(100)]
//         public string? TicketNo { get; set; }

//         [MaxLength(500)]
//         public string? IssueDescription { get; set; }
//     }

//     // ─── PUT /api/Asset/maintenance/complete ─────────────────────────────
//     public class CompleteMaintenanceRequest
//     {
//         // Fix #1: MaintenanceId → AssetId
//         // Koi alag MaintenanceTable nahi hai, isliye yahan AssetId pass karna hai.
//         // Pehle MaintenanceId tha jo confusing tha aur frontend mein wrong ID
//         // bhejne par silently fail ho jata tha.
//         [Required]
//         public int AssetId { get; set; }

//         public decimal? Cost { get; set; }

//         [MaxLength(500)]
//         public string? ResolutionNote { get; set; }
//     }

//     // ─── Query params for GET /api/Asset/list ────────────────────────────
//     // Fix #9: fromDate/toDate ek hi tha jo AssignedDate filter karta tha.
//     // Ab alag-alag filters hain — assigned date aur created date dono ke liye.
//     public class AssetListQuery
//     {
//         public string? Status    { get; set; }
//         public string? AssetType { get; set; }
//         public int?    UserId    { get; set; }

//         // Kab assign kiya gaya?
//         public DateTime? AssignedFrom { get; set; }
//         public DateTime? AssignedTo   { get; set; }

//         // Kab asset create hua?
//         public DateTime? CreatedFrom { get; set; }
//         public DateTime? CreatedTo   { get; set; }
//     }

//     // ─── Responses ───────────────────────────────────────────────────────
//     public class AssetResponse
//     {
//         public int AssetId { get; set; }
//         public string AssetName { get; set; } = string.Empty;
//         public string AssetType { get; set; } = string.Empty;
//         public string? AssetCode { get; set; }
//         public string? SerialNumber { get; set; }
//         public string? Brand { get; set; }
//         public string? Model { get; set; }
//         public string? Description { get; set; }
//         public string Status { get; set; } = string.Empty;

//         public int? AssignedToUserId { get; set; }
//         public string? AssignedToUserName { get; set; }
//         public DateTime? AssignedDate { get; set; }
//         public DateTime? ExpectedReturnDate { get; set; }
//         public string? AssignmentNote { get; set; }

//         public DateTime? ReturnedDate { get; set; }
//         public string? ReturnNote { get; set; }
//         public string? ReturnCondition { get; set; }

//         public DateTime CreatedOn { get; set; }
//     }

//     public class AssetHistoryResponse
//     {
//         public int HistoryId { get; set; }
//         public int AssetId { get; set; }
//         public string? AssetName { get; set; }
//         public string? AssetType { get; set; }
//         public int? UserId { get; set; }
//         public string? UserName { get; set; }
//         public string Action { get; set; } = string.Empty;
//         public string? Note { get; set; }
//         public string? Condition { get; set; }
//         public DateTime ActionDate { get; set; }
//         public string? ActionByUserName { get; set; }
//     }

//     public class MaintenanceResponse
//     {
//         public int MaintenanceId { get; set; }
//         public int AssetId { get; set; }
//         public string? AssetName { get; set; }
//         public string? AssetType { get; set; }
//         public string MaintenanceType { get; set; } = string.Empty;
//         public string? VendorName { get; set; }
//         public string? TicketNo { get; set; }
//         public string? IssueDescription { get; set; }
//         public DateTime StartDate { get; set; }
//         public DateTime? EndDate { get; set; }
//         public string Status { get; set; } = string.Empty;
//         public decimal? Cost { get; set; }
//         public string? ResolutionNote { get; set; }
//     }

//     public class AssetSummaryResponse
//     {
//         public int Total { get; set; }
//         public int Available { get; set; }
//         public int Assigned { get; set; }
//         public List<AssetTypeCount> ByType { get; set; } = new();
//     }

//     public class AssetTypeCount
//     {
//         public string AssetType { get; set; } = string.Empty;
//         public int Count { get; set; }
//     }
// }










// ======================= DTOs/AssetDTOs.cs =======================
using System.ComponentModel.DataAnnotations;

namespace attendance_api.DTOs
{
    // Fix #2: Reusable validation attribute — allowed values check (case-insensitive)
    // Koi bhi invalid string pass karne par 400 BadRequest milega automatically.
    public sealed class AllowedValuesAttribute : ValidationAttribute
    {
        private readonly string[] _allowed;
        public AllowedValuesAttribute(params string[] allowed)
        {
            _allowed = allowed;
            ErrorMessage = $"Value must be one of: {string.Join(", ", allowed)}.";
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            if (value is null) return ValidationResult.Success;
            return _allowed.Contains(value.ToString()!.ToLower())
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage);
        }
    }

    // ─── POST /api/Asset/add ──────────────────────────────────────────────
    public class CreateAssetRequest
    {
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
    }

    // ─── POST /api/Asset/assign ───────────────────────────────────────────
    public class AssignAssetRequest
    {
        [Required]
        public int AssetId { get; set; }

        [Required]
        public int AssignedToUserId { get; set; }

        /// <summary>
        /// Assign karne ki date. Null ho toh server UTC now use karega.
        /// </summary>
        public DateTime? AssignedDate { get; set; }

        public DateTime? ExpectedReturnDate { get; set; }

        [MaxLength(500)]
        public string? AssignmentNote { get; set; }
    }

    // ─── PUT /api/Asset/return ────────────────────────────────────────────
    public class ReturnAssetRequest
    {
        [Required]
        public int AssetId { get; set; }

        [MaxLength(500)]
        public string? ReturnNote { get; set; }

        // Fix #2: sirf "good", "damaged", "lost" hi valid hain
        [MaxLength(20)]
        [AllowedValues("good", "damaged", "lost")]
        public string ReturnCondition { get; set; } = "good";
    }

    // ─── POST /api/Asset/maintenance/start ───────────────────────────────
    public class StartMaintenanceRequest
    {
        [Required]
        public int AssetId { get; set; }

        // Fix #2: sirf valid maintenance types allowed
        [MaxLength(30)]
        [AllowedValues("preventive", "corrective", "repair", "service")]
        public string MaintenanceType { get; set; } = "repair";

        [MaxLength(100)]
        public string? VendorName { get; set; }

        [MaxLength(100)]
        public string? TicketNo { get; set; }

        [MaxLength(500)]
        public string? IssueDescription { get; set; }
    }

    // ─── PUT /api/Asset/maintenance/complete ─────────────────────────────
    public class CompleteMaintenanceRequest
    {
        // Fix #1: MaintenanceId → AssetId
        // Koi alag MaintenanceTable nahi hai, isliye yahan AssetId pass karna hai.
        [Required]
        public int AssetId { get; set; }

        public decimal? Cost { get; set; }

        [MaxLength(500)]
        public string? ResolutionNote { get; set; }
    }

    // ─── Query params for GET /api/Asset/list ────────────────────────────
    // Fix #9: fromDate/toDate ek hi tha jo AssignedDate filter karta tha.
    // Ab alag-alag filters hain — assigned date aur created date dono ke liye.
    public class AssetListQuery
    {
        public string? Status    { get; set; }
        public string? AssetType { get; set; }
        public int?    UserId    { get; set; }

        // Kab assign kiya gaya?
        public DateTime? AssignedFrom { get; set; }
        public DateTime? AssignedTo   { get; set; }

        // Kab asset create hua?
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo   { get; set; }
    }

    // ─── Responses ───────────────────────────────────────────────────────
    public class AssetResponse
    {
        public int AssetId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string AssetType { get; set; } = string.Empty;
        public string? AssetCode { get; set; }
        public string? SerialNumber { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;

        public int? AssignedToUserId { get; set; }
        public string? AssignedToUserName { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
        public string? AssignmentNote { get; set; }

        public DateTime? ReturnedDate { get; set; }
        public string? ReturnNote { get; set; }
        public string? ReturnCondition { get; set; }

        public DateTime CreatedOn { get; set; }
    }

    public class AssetHistoryResponse
    {
        public int HistoryId { get; set; }
        public int AssetId { get; set; }
        public string? AssetName { get; set; }
        public string? AssetType { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? Condition { get; set; }
        public DateTime ActionDate { get; set; }
        public string? ActionByUserName { get; set; }
    }

    public class MaintenanceResponse
    {
        public int MaintenanceId { get; set; }
        public int AssetId { get; set; }
        public string? AssetName { get; set; }
        public string? AssetType { get; set; }
        public string MaintenanceType { get; set; } = string.Empty;
        public string? VendorName { get; set; }
        public string? TicketNo { get; set; }
        public string? IssueDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? Cost { get; set; }
        public string? ResolutionNote { get; set; }
    }

    public class AssetSummaryResponse
    {
        public int Total { get; set; }
        public int Available { get; set; }
        public int Assigned { get; set; }
        public List<AssetTypeCount> ByType { get; set; } = new();
    }

    public class AssetTypeCount
    {
        public string AssetType { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}