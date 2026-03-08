// // ======================= Models/PayrollRecord.cs =======================
// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace attendance_api.Models
// {
//     public class PayrollRecord
//     {
//         [Key]
//         public int PayrollId { get; set; }

//         // ─── Employee ─────────────────────────────────────────────────────
//         [Required]
//         public int EmployeeId { get; set; }

//         [Required]
//         public int Month { get; set; }   // 1–12

//         [Required]
//         public int Year { get; set; }

//         // ─── Salary Base ──────────────────────────────────────────────────
//         [Column(TypeName = "decimal(18,2)")]
//         public decimal BasicSalary { get; set; }

//         public int TotalWorkingDays { get; set; }
//         public int PresentDays      { get; set; }
//         public int AbsentDays       { get; set; }
//         public int LateDays         { get; set; }   // check-in > office start time

//         // ─── Deductions ───────────────────────────────────────────────────
//         [Column(TypeName = "decimal(18,2)")]
//         public decimal PerDaySalary { get; set; }

//         [Column(TypeName = "decimal(18,2)")]
//         public decimal AbsentDeduction { get; set; }

//         [Column(TypeName = "decimal(18,2)")]
//         public decimal LateDeduction { get; set; }

//         [Column(TypeName = "decimal(18,2)")]
//         public decimal ManualDeduction { get; set; }   // POST /deduction se

//         [MaxLength(500)]
//         public string? ManualDeductionReason { get; set; }

//         [Column(TypeName = "decimal(18,2)")]
//         public decimal TotalDeduction { get; set; }

//         [Column(TypeName = "decimal(18,2)")]
//         public decimal NetSalary { get; set; }

//         // ─── Status ───────────────────────────────────────────────────────
//         /// <summary>draft / approved / paid</summary>
//         [MaxLength(20)]
//         public string Status { get; set; } = "draft";

//         public int? ApprovedBy  { get; set; }
//         public DateTime? ApprovedAt { get; set; }

//         [MaxLength(500)]
//         public string? Remarks { get; set; }

//         // ─── Meta ─────────────────────────────────────────────────────────
//         public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
//         public int GeneratedBy { get; set; }
//         public bool IsDeleted { get; set; } = false;

//         // ─── Navigation ───────────────────────────────────────────────────
//         [ForeignKey(nameof(EmployeeId))]
//         public User? Employee { get; set; }
//     }
// }













// // ======================= Models/PayrollRecord.cs =======================
// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace attendance_api.Models
// {
//     public class PayrollRecord
//     {
//         [Key]
//         public int PayrollId { get; set; }

//         // ─── Employee ─────────────────────────────────────────────────────
//         [Required]
//         public int EmployeeId { get; set; }

//         [Required]
//         public int Month { get; set; }   // 1–12

//         [Required]
//         public int Year { get; set; }

//         // ─── Salary Base ──────────────────────────────────────────────────
//         [Column(TypeName = "decimal(18,2)")]
//         public decimal BasicSalary { get; set; }

//         public int TotalWorkingDays { get; set; }
//         public int PresentDays      { get; set; }
//         public int AbsentDays       { get; set; }
//         public int LateDays         { get; set; }

//         // ─── Deductions ───────────────────────────────────────────────────
//         [Column(TypeName = "decimal(18,2)")]
//         public decimal PerDaySalary { get; set; }

//         [Column(TypeName = "decimal(18,2)")]
//         public decimal AbsentDeduction { get; set; }

//         [Column(TypeName = "decimal(18,2)")]
//         public decimal LateDeduction { get; set; }

//         [Column(TypeName = "decimal(18,2)")]
//         public decimal ManualDeduction { get; set; }

//         [MaxLength(500)]
//         public string? ManualDeductionReason { get; set; }

//         [Column(TypeName = "decimal(18,2)")]
//         public decimal TotalDeduction { get; set; }

//         [Column(TypeName = "decimal(18,2)")]
//         public decimal NetSalary { get; set; }

//         // ─── Status ───────────────────────────────────────────────────────
//         /// <summary>draft → approved → paid</summary>
//         [MaxLength(20)]
//         public string Status { get; set; } = "draft";

//         // Approval
//         public int?      ApprovedBy  { get; set; }
//         public DateTime? ApprovedAt  { get; set; }

//         // Payment
//         public int?      PaidBy      { get; set; }
//         public DateTime? PaidAt      { get; set; }

//         [MaxLength(500)]
//         public string? Remarks { get; set; }

//         // ─── Meta ─────────────────────────────────────────────────────────
//         public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
//         public int      GeneratedBy { get; set; }
//         public bool     IsDeleted   { get; set; } = false;

//         // ─── Navigation ───────────────────────────────────────────────────
//         [ForeignKey(nameof(EmployeeId))]
//         public User? Employee { get; set; }
//     }
// }











// ======================= Models/PayrollRecord.cs =======================
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace attendance_api.Models
{
    public class PayrollRecord
    {
        [Key]
        public int PayrollId { get; set; }

        // ─── Employee ─────────────────────────────────────────────────────
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int Month { get; set; }   // 1–12

        [Required]
        public int Year { get; set; }

        // ─── Salary Base ──────────────────────────────────────────────────
        [Column(TypeName = "decimal(18,2)")]
        public decimal BasicSalary { get; set; }

        public int TotalCalendarDays { get; set; }  // Actual days in month: 28 / 30 / 31
        public int TotalWorkingDays  { get; set; }  // Mon–Fri only
        public int PresentDays       { get; set; }
        public int AbsentDays        { get; set; }
        public int LateDays          { get; set; }

        // ─── Deductions ───────────────────────────────────────────────────
        // PerDaySalary = BasicSalary ÷ TotalCalendarDays (NOT working days)
        [Column(TypeName = "decimal(18,2)")]
        public decimal PerDaySalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AbsentDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LateDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ManualDeduction { get; set; }

        [MaxLength(500)]
        public string? ManualDeductionReason { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetSalary { get; set; }

        // ─── Status ───────────────────────────────────────────────────────
        /// <summary>draft → approved → paid</summary>
        [MaxLength(20)]
        public string Status { get; set; } = "draft";

        // Approval
        public int?      ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        // Payment
        public int?      PaidBy { get; set; }
        public DateTime? PaidAt { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        // ─── Meta ─────────────────────────────────────────────────────────
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public int      GeneratedBy { get; set; }
        public bool     IsDeleted   { get; set; } = false;

        // ─── Navigation ───────────────────────────────────────────────────
        [ForeignKey(nameof(EmployeeId))]
        public User? Employee { get; set; }
    }
}