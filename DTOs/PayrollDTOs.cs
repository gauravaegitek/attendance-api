// // ======================= DTOs/PayrollDTOs.cs =======================
// using System.ComponentModel.DataAnnotations;

// namespace attendance_api.DTOs
// {
//     // ─── GET /api/Payroll/calculate ───────────────────────────────────────
//     public class PayrollCalculateRequest
//     {
//         [Required]
//         public int EmployeeId { get; set; }

//         [Required, Range(1, 12)]
//         public int Month { get; set; }

//         [Required, Range(2000, 2100)]
//         public int Year { get; set; }

//         /// <summary>Monthly gross salary (manual input)</summary>
//         [Required]
//         [Range(1, double.MaxValue, ErrorMessage = "BasicSalary must be greater than 0")]
//         public decimal BasicSalary { get; set; }

//         /// <summary>Office start time e.g. "09:30" — late check-in ke liye</summary>
//         public string OfficeStartTime { get; set; } = "09:30";

//         /// <summary>Minutes grace period before marking late (default 15)</summary>
//         public int GraceMinutes { get; set; } = 15;

//         /// <summary>Per-late-day deduction % of per-day salary (default 50%)</summary>
//         [Range(0, 100)]
//         public decimal LateDeductionPercent { get; set; } = 50;
//     }

//     // ─── POST /api/Payroll/deduction ──────────────────────────────────────
//     public class PayrollDeductionRequest
//     {
//         [Required]
//         public int EmployeeId { get; set; }

//         [Required, Range(1, 12)]
//         public int Month { get; set; }

//         [Required, Range(2000, 2100)]
//         public int Year { get; set; }

//         [Required]
//         [Range(0.01, double.MaxValue, ErrorMessage = "Deduction amount must be > 0")]
//         public decimal Amount { get; set; }

//         [Required, MaxLength(500)]
//         public string Reason { get; set; } = string.Empty;
//     }

//     // ─── GET /api/Payroll/slip ────────────────────────────────────────────
//     public class PayrollSlipQuery
//     {
//         [Required]
//         public int EmployeeId { get; set; }

//         [Required, Range(1, 12)]
//         public int Month { get; set; }

//         [Required, Range(2000, 2100)]
//         public int Year { get; set; }
//     }

//     // ─── POST /api/Payroll/export (Admin) ────────────────────────────────
//     public class PayrollExportRequest
//     {
//         [Required, Range(1, 12)]
//         public int Month { get; set; }

//         [Required, Range(2000, 2100)]
//         public int Year { get; set; }

//         /// <summary>Filter by employee (optional — null = all employees)</summary>
//         public int? EmployeeId { get; set; }

//         /// <summary>pdf / excel</summary>
//         [Required]
//         public string Format { get; set; } = "pdf";
//     }

//     // ─── Responses ────────────────────────────────────────────────────────
//     public class PayrollResponse
//     {
//         public int     PayrollId             { get; set; }
//         public int     EmployeeId            { get; set; }
//         public string? EmployeeName          { get; set; }
//         public string? Department            { get; set; }
//         public string? Designation           { get; set; }
//         public int     Month                 { get; set; }
//         public int     Year                  { get; set; }
//         public string  MonthName             { get; set; } = string.Empty;
//         public decimal BasicSalary           { get; set; }
//         public int     TotalWorkingDays      { get; set; }
//         public int     PresentDays           { get; set; }
//         public int     AbsentDays            { get; set; }
//         public int     LateDays              { get; set; }
//         public decimal PerDaySalary          { get; set; }
//         public decimal AbsentDeduction       { get; set; }
//         public decimal LateDeduction         { get; set; }
//         public decimal ManualDeduction       { get; set; }
//         public string? ManualDeductionReason { get; set; }
//         public decimal TotalDeduction        { get; set; }
//         public decimal NetSalary             { get; set; }
//         public string  Status                { get; set; } = string.Empty;
//         public DateTime GeneratedAt          { get; set; }
//         public string? Remarks               { get; set; }
//     }

//     public class AttendanceDetailDto
//     {
//         public DateTime Date      { get; set; }
//         public string?  InTime    { get; set; }
//         public string?  OutTime   { get; set; }
//         public bool     IsLate    { get; set; }
//         public double   Hours     { get; set; }
//     }
// }












// // ======================= DTOs/PayrollDTOs.cs =======================
// using System.ComponentModel.DataAnnotations;

// namespace attendance_api.DTOs
// {
//     // ─── GET /api/Payroll/calculate ───────────────────────────────────────
//     public class PayrollCalculateRequest
//     {
//         [Required]
//         public int EmployeeId { get; set; }

//         [Required, Range(1, 12)]
//         public int Month { get; set; }

//         [Required, Range(2000, 2100)]
//         public int Year { get; set; }

//         /// <summary>Monthly gross salary</summary>
//         [Required]
//         [Range(1, double.MaxValue, ErrorMessage = "BasicSalary must be greater than 0")]
//         public decimal BasicSalary { get; set; }

//         // Fixed rules (hardcoded in controller):
//         // Office: 10:00 AM – 6:00 PM
//         // Late cutoff: 10:15 AM → ½ day deduction
//         // Off days: Saturday & Sunday
//         // Only allowed after month is complete
//     }

//     // ─── POST /api/Payroll/deduction ──────────────────────────────────────
//     public class PayrollDeductionRequest
//     {
//         [Required]
//         public int EmployeeId { get; set; }

//         [Required, Range(1, 12)]
//         public int Month { get; set; }

//         [Required, Range(2000, 2100)]
//         public int Year { get; set; }

//         [Required]
//         [Range(0, double.MaxValue, ErrorMessage = "Deduction amount must be 0 or greater")]
//         public decimal Amount { get; set; }

//         [Required, MaxLength(500)]
//         public string Reason { get; set; } = string.Empty;
//     }

//     // ─── POST /api/Payroll/approve ────────────────────────────────────────
//     public class PayrollApproveRequest
//     {
//         [Required]
//         public int EmployeeId { get; set; }

//         [Required, Range(1, 12)]
//         public int Month { get; set; }

//         [Required, Range(2000, 2100)]
//         public int Year { get; set; }

//         [MaxLength(500)]
//         public string? Remarks { get; set; }
//     }

//     // ─── POST /api/Payroll/markpaid ───────────────────────────────────────
//     public class PayrollMarkPaidRequest
//     {
//         [Required]
//         public int EmployeeId { get; set; }

//         [Required, Range(1, 12)]
//         public int Month { get; set; }

//         [Required, Range(2000, 2100)]
//         public int Year { get; set; }

//         [MaxLength(500)]
//         public string? Remarks { get; set; }
//     }

//     // ─── GET /api/Payroll/slip ────────────────────────────────────────────
//     public class PayrollSlipQuery
//     {
//         [Required]
//         public int EmployeeId { get; set; }

//         [Required, Range(1, 12)]
//         public int Month { get; set; }

//         [Required, Range(2000, 2100)]
//         public int Year { get; set; }
//     }

//     // ─── POST /api/Payroll/export ─────────────────────────────────────────
//     public class PayrollExportRequest
//     {
//         [Required, Range(1, 12)]
//         public int Month { get; set; }

//         [Required, Range(2000, 2100)]
//         public int Year { get; set; }

//         /// <summary>null or 0 = all employees</summary>
//         public int? EmployeeId { get; set; }

//         /// <summary>pdf / excel</summary>
//         [Required]
//         public string Format { get; set; } = "pdf";
//     }

//     // ─── Response ─────────────────────────────────────────────────────────
//     public class PayrollResponse
//     {
//         public int     PayrollId             { get; set; }
//         public int     EmployeeId            { get; set; }
//         public string? EmployeeName          { get; set; }
//         public string? Department            { get; set; }
//         public string? Designation           { get; set; }
//         public int     Month                 { get; set; }
//         public int     Year                  { get; set; }
//         public string  MonthName             { get; set; } = string.Empty;
//         public decimal BasicSalary           { get; set; }
//         public int     TotalWorkingDays      { get; set; }
//         public int     PresentDays           { get; set; }
//         public int     AbsentDays            { get; set; }
//         public int     LateDays              { get; set; }
//         public decimal PerDaySalary          { get; set; }
//         public decimal AbsentDeduction       { get; set; }
//         public decimal LateDeduction         { get; set; }
//         public decimal ManualDeduction       { get; set; }
//         public string? ManualDeductionReason { get; set; }
//         public decimal TotalDeduction        { get; set; }
//         public decimal NetSalary             { get; set; }

//         // Status
//         public string  Status                { get; set; } = string.Empty;
//         public string  PaymentStatus         { get; set; } = string.Empty; // "Pay Completed" / "Pending"

//         // Approval info
//         public DateTime? ApprovedAt          { get; set; }
//         public string?   ApprovedByName      { get; set; }

//         // Payment info
//         public DateTime? PaidAt              { get; set; }
//         public string?   PaidByName          { get; set; }

//         public DateTime  GeneratedAt         { get; set; }
//         public string?   Remarks             { get; set; }
//     }

//     public class AttendanceDetailDto
//     {
//         public DateTime Date      { get; set; }
//         public string?  InTime    { get; set; }
//         public string?  OutTime   { get; set; }
//         public bool     IsLate    { get; set; }
//         public double   Hours     { get; set; }
//     }
// }











// // ======================= DTOs/PayrollDTOs.cs =======================
// using System.ComponentModel.DataAnnotations;

// namespace attendance_api.DTOs
// {
//     // ─── GET /api/Payroll/calculate ───────────────────────────────────────
//     public class PayrollCalculateRequest
//     {
//         [Required]
//         public int EmployeeId { get; set; }

//         [Required, Range(1, 12)]
//         public int Month { get; set; }

//         [Required, Range(2000, 2100)]
//         public int Year { get; set; }

//         /// <summary>Monthly gross salary</summary>
//         [Required]
//         [Range(1, double.MaxValue, ErrorMessage = "BasicSalary must be greater than 0")]
//         public decimal BasicSalary { get; set; }

//         // Fixed rules (hardcoded in controller):
//         // Office: 10:00 AM – 6:00 PM
//         // Late cutoff: 10:15 AM → ½ day deduction
//         // Off days: Saturday & Sunday
//         // Only allowed after month is complete
//         // PerDaySalary = BasicSalary ÷ TotalCalendarDays (28/30/31)
//     }

//     // ─── POST /api/Payroll/deduction ──────────────────────────────────────
//     public class PayrollDeductionRequest
//     {
//         [Required]
//         public int EmployeeId { get; set; }

//         [Required, Range(1, 12)]
//         public int Month { get; set; }

//         [Required, Range(2000, 2100)]
//         public int Year { get; set; }

//         /// <summary>0 dalne par manual deduction remove ho jayegi</summary>
//         [Required]
//         [Range(0, double.MaxValue, ErrorMessage = "Deduction amount must be 0 or greater")]
//         public decimal Amount { get; set; }

//         [Required, MaxLength(500)]
//         public string Reason { get; set; } = string.Empty;
//     }

//     // ─── POST /api/Payroll/approve ────────────────────────────────────────
//     public class PayrollApproveRequest
//     {
//         [Required]
//         public int EmployeeId { get; set; }

//         [Required, Range(1, 12)]
//         public int Month { get; set; }

//         [Required, Range(2000, 2100)]
//         public int Year { get; set; }

//         [MaxLength(500)]
//         public string? Remarks { get; set; }
//     }

//     // ─── POST /api/Payroll/markpaid ───────────────────────────────────────
//     public class PayrollMarkPaidRequest
//     {
//         [Required]
//         public int EmployeeId { get; set; }

//         [Required, Range(1, 12)]
//         public int Month { get; set; }

//         [Required, Range(2000, 2100)]
//         public int Year { get; set; }

//         [MaxLength(500)]
//         public string? Remarks { get; set; }
//     }

//     // ─── GET /api/Payroll/slip ────────────────────────────────────────────
//     public class PayrollSlipQuery
//     {
//         [Required]
//         public int EmployeeId { get; set; }

//         [Required, Range(1, 12)]
//         public int Month { get; set; }

//         [Required, Range(2000, 2100)]
//         public int Year { get; set; }
//     }

//     // ─── POST /api/Payroll/export ─────────────────────────────────────────
//     public class PayrollExportRequest
//     {
//         [Required, Range(1, 12)]
//         public int Month { get; set; }

//         [Required, Range(2000, 2100)]
//         public int Year { get; set; }

//         /// <summary>null or 0 = all employees</summary>
//         public int? EmployeeId { get; set; }

//         /// <summary>pdf / excel</summary>
//         [Required]
//         public string Format { get; set; } = "pdf";
//     }

//     // ─── Response ─────────────────────────────────────────────────────────
//     public class PayrollResponse
//     {
//         public int     PayrollId             { get; set; }
//         public int     EmployeeId            { get; set; }
//         public string? EmployeeName          { get; set; }
//         public string? Department            { get; set; }
//         public string? Designation           { get; set; }
//         public int     Month                 { get; set; }
//         public int     Year                  { get; set; }
//         public string  MonthName             { get; set; } = string.Empty;
//         public decimal BasicSalary           { get; set; }

//         // ── Attendance ──────────────────────────────────────────────────
//         public int     TotalCalendarDays     { get; set; }  // Actual days: 28/30/31
//         public int     TotalWorkingDays      { get; set; }  // Mon–Fri only
//         public int     PresentDays           { get; set; }
//         public int     AbsentDays            { get; set; }
//         public int     LateDays              { get; set; }

//         // ── Salary Breakdown ────────────────────────────────────────────
//         public decimal PerDaySalary          { get; set; }
//         public decimal AbsentDeduction       { get; set; }
//         public decimal LateDeduction         { get; set; }
//         public decimal ManualDeduction       { get; set; }
//         public string? ManualDeductionReason { get; set; }
//         public decimal TotalDeduction        { get; set; }
//         public decimal NetSalary             { get; set; }

//         // ── Status ──────────────────────────────────────────────────────
//         public string  Status                { get; set; } = string.Empty;
//         public string  PaymentStatus         { get; set; } = string.Empty; // "Pay Completed" / "Pending"

//         // ── Approval info ───────────────────────────────────────────────
//         public DateTime? ApprovedAt          { get; set; }
//         public string?   ApprovedByName      { get; set; }

//         // ── Payment info ────────────────────────────────────────────────
//         public DateTime? PaidAt              { get; set; }
//         public string?   PaidByName          { get; set; }

//         public DateTime  GeneratedAt         { get; set; }
//         public string?   Remarks             { get; set; }
//     }

//     public class AttendanceDetailDto
//     {
//         public DateTime Date      { get; set; }
//         public string?  InTime    { get; set; }
//         public string?  OutTime   { get; set; }
//         public bool     IsLate    { get; set; }
//         public double   Hours     { get; set; }
//     }
// }














// ======================= DTOs/PayrollDTOs.cs =======================
using System.ComponentModel.DataAnnotations;

namespace attendance_api.DTOs
{
    // ─── GET /api/Payroll/calculate ───────────────────────────────────────
    public class PayrollCalculateRequest
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required, Range(1, 12)]
        public int Month { get; set; }

        [Required, Range(2000, 2100)]
        public int Year { get; set; }

        /// <summary>Monthly gross salary</summary>
        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "BasicSalary must be greater than 0")]
        public decimal BasicSalary { get; set; }

        /// <summary>Late cutoff time in HH:mm format (e.g. "10:15")</summary>
        [Required]
        public string LateCutoff { get; set; } = "10:15";
    }

    // ─── POST /api/Payroll/deduction ──────────────────────────────────────
    public class PayrollDeductionRequest
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required, Range(1, 12)]
        public int Month { get; set; }

        [Required, Range(2000, 2100)]
        public int Year { get; set; }

        /// <summary>0 dalne par manual deduction remove ho jayegi</summary>
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Deduction amount must be 0 or greater")]
        public decimal Amount { get; set; }

        [Required, MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    // ─── POST /api/Payroll/approve ────────────────────────────────────────
    public class PayrollApproveRequest
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required, Range(1, 12)]
        public int Month { get; set; }

        [Required, Range(2000, 2100)]
        public int Year { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }
    }

    // ─── POST /api/Payroll/markpaid ───────────────────────────────────────
    public class PayrollMarkPaidRequest
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required, Range(1, 12)]
        public int Month { get; set; }

        [Required, Range(2000, 2100)]
        public int Year { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }
    }

    // ─── GET /api/Payroll/slip ────────────────────────────────────────────
    public class PayrollSlipQuery
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required, Range(1, 12)]
        public int Month { get; set; }

        [Required, Range(2000, 2100)]
        public int Year { get; set; }
    }

    // ─── POST /api/Payroll/export ─────────────────────────────────────────
    public class PayrollExportRequest
    {
        [Required, Range(1, 12)]
        public int Month { get; set; }

        [Required, Range(2000, 2100)]
        public int Year { get; set; }

        /// <summary>null or 0 = all employees</summary>
        public int? EmployeeId { get; set; }

        /// <summary>pdf / excel</summary>
        [Required]
        public string Format { get; set; } = "pdf";
    }

    // ─── Response ─────────────────────────────────────────────────────────
    public class PayrollResponse
    {
        public int     PayrollId             { get; set; }
        public int     EmployeeId            { get; set; }
        public string? EmployeeName          { get; set; }
        public string? Department            { get; set; }
        public string? Designation           { get; set; }
        public int     Month                 { get; set; }
        public int     Year                  { get; set; }
        public string  MonthName             { get; set; } = string.Empty;
        public decimal BasicSalary           { get; set; }

        // ── Attendance ──────────────────────────────────────────────────
        public int     TotalCalendarDays     { get; set; }  // Actual days: 28/30/31
        public int     TotalWorkingDays      { get; set; }  // Mon–Fri only
        public int     PresentDays           { get; set; }
        public int     AbsentDays            { get; set; }
        public int     LateDays              { get; set; }

        // ── Salary Breakdown ────────────────────────────────────────────
        public decimal PerDaySalary          { get; set; }
        public decimal AbsentDeduction       { get; set; }
        public decimal LateDeduction         { get; set; }
        public decimal ManualDeduction       { get; set; }
        public string? ManualDeductionReason { get; set; }
        public decimal TotalDeduction        { get; set; }
        public decimal NetSalary             { get; set; }

        // ── Status ──────────────────────────────────────────────────────
        public string  Status                { get; set; } = string.Empty;
        public string  PaymentStatus         { get; set; } = string.Empty;

        // ── Approval info ───────────────────────────────────────────────
        public DateTime? ApprovedAt          { get; set; }
        public string?   ApprovedByName      { get; set; }

        // ── Payment info ────────────────────────────────────────────────
        public DateTime? PaidAt              { get; set; }
        public string?   PaidByName          { get; set; }

        public DateTime  GeneratedAt         { get; set; }
        public string?   Remarks             { get; set; }
    }

    public class AttendanceDetailDto
    {
        public DateTime Date      { get; set; }
        public string?  InTime    { get; set; }
        public string?  OutTime   { get; set; }
        public bool     IsLate    { get; set; }
        public double   Hours     { get; set; }
    }
}