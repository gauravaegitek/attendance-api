// // ======================= DTOs/DocumentDTOs.cs =======================
// using System.ComponentModel.DataAnnotations;
// using Microsoft.AspNetCore.Http;

// namespace attendance_api.DTOs
// {
//     // ─── POST /api/Document/upload ────────────────────────────────────────
//     public class DocumentUploadRequest
//     {
//         [Required]
//         public int EmployeeId { get; set; }

//         /// <summary>ID / Certificate / Resume / Other</summary>
//         [Required, MaxLength(100)]
//         public string DocumentType { get; set; } = string.Empty;

//         [MaxLength(255)]
//         public string? Description { get; set; }

//         [Required]
//         public IFormFile File { get; set; } = null!;
//     }

//     // ─── Query params for GET /api/Document/list ─────────────────────────
//     public class DocumentListQuery
//     {
//         public int?    EmployeeId    { get; set; }
//         public string? DocumentType  { get; set; }

//         // Kab upload hua?
//         public DateTime? UploadedFrom { get; set; }
//         public DateTime? UploadedTo   { get; set; }
//     }

//     // ─── Responses ────────────────────────────────────────────────────────
//     public class DocumentResponse
//     {
//         public int    DocumentId    { get; set; }
//         public int    EmployeeId    { get; set; }
//         public string? EmployeeName { get; set; }
//         public string DocumentType  { get; set; } = string.Empty;
//         public string? Description  { get; set; }
//         public string FileName      { get; set; } = string.Empty;
//         public string FilePath      { get; set; } = string.Empty;
//         public long   FileSize      { get; set; }
//         public string FileExtension { get; set; } = string.Empty;
//         public DateTime UploadedAt  { get; set; }
//         public int?  UploadedByUserId { get; set; }
//         public string? UploadedByUserName { get; set; }
//     }

//     public class DocumentSummaryResponse
//     {
//         public int Total { get; set; }
//         public List<DocumentTypeCount> ByType { get; set; } = new();
//     }

//     public class DocumentTypeCount
//     {
//         public string DocumentType { get; set; } = string.Empty;
//         public int Count { get; set; }
//     }
// }









// // ======================= DTOs/DocumentDTOs.cs =======================
// using System.ComponentModel.DataAnnotations;
// using Microsoft.AspNetCore.Http;

// namespace attendance_api.DTOs
// {
//     // ─── POST /api/Document/upload ────────────────────────────────────────
//     public class DocumentUploadRequest
//     {
//         [Required]
//         public int EmployeeId { get; set; }

//         /// <summary>ID / Certificate / Resume / Other</summary>
//         [Required, MaxLength(100)]
//         public string DocumentType { get; set; } = string.Empty;

//         [MaxLength(255)]
//         public string? Description { get; set; }

//         [Required]
//         public IFormFile File { get; set; } = null!;
//     }

//     // ─── PUT /api/Document/update ─────────────────────────────────────────
//     public class DocumentUpdateRequest
//     {
//         [Required]
//         public int DocumentId { get; set; }

//         [MaxLength(100)]
//         public string? DocumentType { get; set; }

//         [MaxLength(255)]
//         public string? Description { get; set; }

//         [MaxLength(255)]
//         public string? FileName { get; set; }
//     }

//     // ─── PUT /api/Document/verify ─────────────────────────────────────────
//     public class DocumentVerifyRequest
//     {
//         [Required]
//         public int DocumentId { get; set; }

//         /// <summary>approved / rejected</summary>
//         [Required, MaxLength(20)]
//         public string Status { get; set; } = string.Empty;

//         [MaxLength(500)]
//         public string? Remarks { get; set; }
//     }

//     // ─── Query params for GET /api/Document/list ─────────────────────────
//     public class DocumentListQuery
//     {
//         /// <summary>Filter by employee ID</summary>
//         public int? EmployeeId { get; set; }

//         /// <summary>Filter by document type — ID / Certificate / Resume / Other</summary>
//         public string? DocumentType { get; set; }

//         /// <summary>Filter by verify status — pending / approved / rejected</summary>
//         public string? VerifyStatus { get; set; }

//         /// <summary>Upload date from (yyyy-MM-dd)</summary>
//         public DateTime? UploadedFrom { get; set; }

//         /// <summary>Upload date to (yyyy-MM-dd)</summary>
//         public DateTime? UploadedTo { get; set; }
//     }

//     // ─── Responses ────────────────────────────────────────────────────────
//     public class DocumentResponse
//     {
//         public int     DocumentId          { get; set; }
//         public int     EmployeeId          { get; set; }
//         public string? EmployeeName        { get; set; }
//         public string  DocumentType        { get; set; } = string.Empty;
//         public string? Description         { get; set; }
//         public string  FileName            { get; set; } = string.Empty;
//         public string  FilePath            { get; set; } = string.Empty;
//         public long    FileSize            { get; set; }
//         public string  FileExtension       { get; set; } = string.Empty;
//         public DateTime UploadedAt         { get; set; }
//         public int?    UploadedByUserId    { get; set; }
//         public string? UploadedByUserName  { get; set; }
//         public string  VerifyStatus        { get; set; } = string.Empty;
//         public DateTime? VerifiedAt        { get; set; }
//         public string? VerifyRemarks       { get; set; }
//     }

//     public class DocumentSummaryResponse
//     {
//         public int Total    { get; set; }
//         public int Pending  { get; set; }
//         public int Approved { get; set; }
//         public int Rejected { get; set; }
//         public List<DocumentTypeCount> ByType { get; set; } = new();
//     }

//     public class DocumentTypeCount
//     {
//         public string DocumentType { get; set; } = string.Empty;
//         public int    Count        { get; set; }
//     }
// }












// ======================= DTOs/DocumentDTOs.cs =======================
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace attendance_api.DTOs
{
    // ─── POST /api/Document/upload ────────────────────────────────────────
    public class DocumentUploadRequest
    {
        [Required]
        public int EmployeeId { get; set; }

        /// <summary>ID / Certificate / Resume / Other</summary>
        [Required, MaxLength(100)]
        public string DocumentType { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }

        [Required]
        public IFormFile File { get; set; } = null!;
    }

    // ─── PUT /api/Document/update ─────────────────────────────────────────
    public class DocumentUpdateRequest
    {
        [Required]
        public int DocumentId { get; set; }

        [MaxLength(100)]
        public string? DocumentType { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        [MaxLength(255)]
        public string? FileName { get; set; }
    }

    // ─── PUT /api/Document/verify ─────────────────────────────────────────
    public class DocumentVerifyRequest
    {
        [Required]
        public int DocumentId { get; set; }

        /// <summary>approved / rejected</summary>
        [Required, MaxLength(20)]
        public string Status { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Remarks { get; set; }
    }

    // ─── Query params for GET /api/Document/list — only EmployeeId ────────
    public class DocumentListQuery
    {
        /// <summary>Employee ID — required to get data</summary>
        public int? EmployeeId { get; set; }
    }

    // ─── Responses ────────────────────────────────────────────────────────
    public class DocumentResponse
    {
        public int      DocumentId         { get; set; }
        public int      EmployeeId         { get; set; }
        public string?  EmployeeName       { get; set; }
        public string   DocumentType       { get; set; } = string.Empty;
        public string?  Description        { get; set; }
        public string   FileName           { get; set; } = string.Empty;
        public string   FilePath           { get; set; } = string.Empty;
        public long     FileSize           { get; set; }
        public string   FileExtension      { get; set; } = string.Empty;
        public DateTime UploadedAt         { get; set; }
        public int?     UploadedByUserId   { get; set; }
        public string?  UploadedByUserName { get; set; }
        public string   VerifyStatus       { get; set; } = string.Empty;
        public DateTime? VerifiedAt        { get; set; }
        public string?  VerifyRemarks      { get; set; }
    }

    public class DocumentSummaryResponse
    {
        public int Total    { get; set; }
        public int Pending  { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public List<DocumentTypeCount> ByType { get; set; } = new();
    }

    public class DocumentTypeCount
    {
        public string DocumentType { get; set; } = string.Empty;
        public int    Count        { get; set; }
    }
}