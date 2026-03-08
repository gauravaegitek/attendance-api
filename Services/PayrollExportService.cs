// // ======================= Services/PayrollExportService.cs =======================
// using ClosedXML.Excel;
// using QuestPDF.Fluent;
// using QuestPDF.Helpers;
// using QuestPDF.Infrastructure;
// using attendance_api.DTOs;
// using attendance_api.Models;

// namespace attendance_api.Services
// {
//     public interface IPayrollExportService
//     {
//         byte[] GeneratePayslipPdf(PayrollRecord payroll, string employeeName, string department, string designation);
//         byte[] GeneratePayrollPdf(List<(PayrollRecord Payroll, string EmployeeName)> records, int month, int year);
//         byte[] GeneratePayrollExcel(List<(PayrollRecord Payroll, string EmployeeName)> records, int month, int year);
//     }

//     public class PayrollExportService : IPayrollExportService
//     {
//         private static readonly string[] MonthNames =
//             { "", "January","February","March","April","May","June",
//               "July","August","September","October","November","December" };

//         public PayrollExportService()
//         {
//             QuestPDF.Settings.License = LicenseType.Community;
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  1. Individual Payslip PDF
//         // ════════════════════════════════════════════════════════════════
//         public byte[] GeneratePayslipPdf(
//             PayrollRecord payroll,
//             string employeeName,
//             string department,
//             string designation)
//         {
//             var monthName = MonthNames[payroll.Month];

//             var doc = Document.Create(container =>
//             {
//                 container.Page(page =>
//                 {
//                     page.Size(PageSizes.A4);
//                     page.Margin(2, Unit.Centimetre);
//                     page.PageColor(Colors.White);
//                     page.DefaultTextStyle(x => x.FontSize(10));

//                     // ── Header ────────────────────────────────────────────
//                     page.Header()
//                         .Height(80)
//                         .Background(Colors.Blue.Lighten3)
//                         .Padding(10)
//                         .Column(col =>
//                         {
//                             col.Item().Text("Attendance Management System")
//                                .FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
//                             col.Item().Text($"Salary Slip — {monthName} {payroll.Year}")
//                                .FontSize(13).SemiBold();
//                             col.Item().PaddingTop(3)
//                                .Text($"Generated: {DateTime.Now:dd-MMM-yyyy HH:mm}")
//                                .FontSize(8).Italic();
//                         });

//                     page.Content().PaddingVertical(12).Column(col =>
//                     {
//                         // ── Employee Info Box ──────────────────────────────
//                         col.Item()
//                            .Border(1).BorderColor(Colors.Grey.Lighten2)
//                            .Background(Colors.Grey.Lighten5)
//                            .Padding(10)
//                            .Column(info =>
//                         {
//                             info.Item().Text("EMPLOYEE DETAILS")
//                                 .FontSize(10).Bold()
//                                 .FontColor(Colors.Blue.Darken2);

//                             info.Item().PaddingTop(6).Row(row =>
//                             {
//                                 row.RelativeItem().Column(c =>
//                                 {
//                                     c.Item().Text($"Name        :  {employeeName}").FontSize(10);
//                                     c.Item().Text($"Department  :  {department}").FontSize(10);
//                                     c.Item().Text($"Designation :  {designation}").FontSize(10);
//                                 });
//                                 row.RelativeItem().Column(c =>
//                                 {
//                                     c.Item().Text($"Employee ID :  {payroll.EmployeeId}").FontSize(10);
//                                     c.Item().Text($"Month       :  {monthName} {payroll.Year}").FontSize(10);
//                                     c.Item().Text($"Status      :  {payroll.Status.ToUpper()}").FontSize(10)
//                                             .Bold().FontColor(
//                                                 payroll.Status == "paid"     ? Colors.Green.Darken1 :
//                                                 payroll.Status == "approved" ? Colors.Blue.Darken1 :
//                                                 Colors.Orange.Darken2);
//                                 });
//                             });
//                         });

//                         col.Item().PaddingTop(12);

//                         // ── Attendance Summary ─────────────────────────────
//                         col.Item().Text("ATTENDANCE SUMMARY")
//                            .FontSize(10).Bold().FontColor(Colors.Blue.Darken2);

//                         col.Item().PaddingTop(4).Table(t =>
//                         {
//                             t.ColumnsDefinition(c =>
//                             {
//                                 c.RelativeColumn();
//                                 c.RelativeColumn();
//                                 c.RelativeColumn();
//                                 c.RelativeColumn();
//                             });

//                             void AH(string text) =>
//                                 t.Cell().Background(Colors.Grey.Lighten2)
//                                  .BorderBottom(1).BorderColor(Colors.Grey.Medium)
//                                  .Padding(5).Text(text).Bold().FontSize(9);

//                             AH("Working Days");
//                             AH("Present Days");
//                             AH("Absent Days");
//                             AH("Late Days");

//                             void AV(string text) =>
//                                 t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
//                                  .Padding(5).Text(text).FontSize(10).Bold();

//                             AV(payroll.TotalWorkingDays.ToString());
//                             AV(payroll.PresentDays.ToString());
//                             AV(payroll.AbsentDays.ToString());
//                             AV(payroll.LateDays.ToString());
//                         });

//                         col.Item().PaddingTop(12);

//                         // ── Earnings & Deductions ──────────────────────────
//                         col.Item().Row(row =>
//                         {
//                             // Earnings
//                             row.RelativeItem().Column(c =>
//                             {
//                                 c.Item().Text("EARNINGS")
//                                  .FontSize(10).Bold().FontColor(Colors.Green.Darken2);

//                                 c.Item().PaddingTop(4).Table(t =>
//                                 {
//                                     t.ColumnsDefinition(cols =>
//                                     {
//                                         cols.RelativeColumn(2);
//                                         cols.RelativeColumn(1);
//                                     });

//                                     void EH(string text) =>
//                                         t.Cell().Background(Colors.Grey.Lighten2)
//                                          .BorderBottom(1).BorderColor(Colors.Grey.Medium)
//                                          .Padding(4).Text(text).Bold().FontSize(9);

//                                     EH("Description");
//                                     EH("Amount (₹)");

//                                     void ER(string label, decimal amt) {
//                                         t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
//                                          .Padding(4).Text(label).FontSize(9);
//                                         t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
//                                          .Padding(4).Text($"{amt:N2}").FontSize(9);
//                                     }

//                                     ER("Basic Salary", payroll.BasicSalary);
//                                     ER("Per Day Salary", payroll.PerDaySalary);

//                                     // Gross
//                                     t.Cell().Background(Colors.Green.Lighten4)
//                                      .Padding(4).Text("Gross Salary").FontSize(9).Bold();
//                                     t.Cell().Background(Colors.Green.Lighten4)
//                                      .Padding(4).Text($"{payroll.BasicSalary:N2}").FontSize(9).Bold()
//                                      .FontColor(Colors.Green.Darken2);
//                                 });
//                             });

//                             row.ConstantItem(15);

//                             // Deductions
//                             row.RelativeItem().Column(c =>
//                             {
//                                 c.Item().Text("DEDUCTIONS")
//                                  .FontSize(10).Bold().FontColor(Colors.Red.Darken2);

//                                 c.Item().PaddingTop(4).Table(t =>
//                                 {
//                                     t.ColumnsDefinition(cols =>
//                                     {
//                                         cols.RelativeColumn(2);
//                                         cols.RelativeColumn(1);
//                                     });

//                                     void DH(string text) =>
//                                         t.Cell().Background(Colors.Grey.Lighten2)
//                                          .BorderBottom(1).BorderColor(Colors.Grey.Medium)
//                                          .Padding(4).Text(text).Bold().FontSize(9);

//                                     DH("Description");
//                                     DH("Amount (₹)");

//                                     void DR(string label, decimal amt) {
//                                         t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
//                                          .Padding(4).Text(label).FontSize(9);
//                                         t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
//                                          .Padding(4).Text($"{amt:N2}").FontSize(9);
//                                     }

//                                     DR("Absent Deduction", payroll.AbsentDeduction);
//                                     DR("Late Deduction", payroll.LateDeduction);
//                                     DR($"Other ({payroll.ManualDeductionReason ?? "-"})", payroll.ManualDeduction);

//                                     // Total deduction
//                                     t.Cell().Background(Colors.Red.Lighten4)
//                                      .Padding(4).Text("Total Deduction").FontSize(9).Bold();
//                                     t.Cell().Background(Colors.Red.Lighten4)
//                                      .Padding(4).Text($"{payroll.TotalDeduction:N2}").FontSize(9).Bold()
//                                      .FontColor(Colors.Red.Darken2);
//                                 });
//                             });
//                         });

//                         // ── Net Salary Box ─────────────────────────────────
//                         col.Item().PaddingTop(16)
//                            .Background(Colors.Blue.Darken2).Padding(12)
//                            .Row(row =>
//                         {
//                             row.RelativeItem()
//                                .Text("NET SALARY PAYABLE")
//                                .FontSize(13).Bold().FontColor(Colors.White);
//                             row.ConstantItem(200).AlignRight()
//                                .Text($"₹ {payroll.NetSalary:N2}")
//                                .FontSize(16).Bold().FontColor(Colors.Yellow.Medium);
//                         });

//                         if (!string.IsNullOrEmpty(payroll.Remarks))
//                         {
//                             col.Item().PaddingTop(8)
//                                .Text($"Remarks: {payroll.Remarks}")
//                                .FontSize(9).Italic().FontColor(Colors.Grey.Darken2);
//                         }
//                     });

//                     // ── Footer ────────────────────────────────────────────
//                     page.Footer()
//                         .Height(35)
//                         .Background(Colors.Grey.Lighten3)
//                         .Padding(6)
//                         .Column(col =>
//                         {
//                             col.Item().AlignCenter().Text(text =>
//                             {
//                                 text.Span("Page ").FontSize(8);
//                                 text.CurrentPageNumber().FontSize(8);
//                                 text.Span(" of ").FontSize(8);
//                                 text.TotalPages().FontSize(8);
//                             });
//                             col.Item().AlignCenter()
//                                .Text("** CONFIDENTIAL — FOR EMPLOYEE USE ONLY **")
//                                .FontSize(7).Italic();
//                         });
//                 });
//             });

//             return doc.GeneratePdf();
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  2. Admin Payroll PDF (all employees)
//         // ════════════════════════════════════════════════════════════════
//         public byte[] GeneratePayrollPdf(
//             List<(PayrollRecord Payroll, string EmployeeName)> records,
//             int month, int year)
//         {
//             var monthName = MonthNames[month];

//             var doc = Document.Create(container =>
//             {
//                 container.Page(page =>
//                 {
//                     page.Size(PageSizes.A4.Landscape());
//                     page.Margin(1.5f, Unit.Centimetre);
//                     page.PageColor(Colors.White);
//                     page.DefaultTextStyle(x => x.FontSize(8));

//                     page.Header()
//                         .Height(60)
//                         .Background(Colors.Blue.Lighten3)
//                         .Padding(10)
//                         .Column(col =>
//                         {
//                             col.Item().Text("Attendance Management System — Payroll Report")
//                                .FontSize(15).Bold().FontColor(Colors.Blue.Darken2);
//                             col.Item().Row(row =>
//                             {
//                                 row.RelativeItem()
//                                    .Text($"Period: {monthName} {year}").FontSize(9);
//                                 row.RelativeItem().AlignRight()
//                                    .Text($"Generated: {DateTime.Now:dd-MMM-yyyy HH:mm}").FontSize(9);
//                             });
//                         });

//                     page.Content().PaddingVertical(8).Column(col =>
//                     {
//                         col.Item().Table(table =>
//                         {
//                             table.ColumnsDefinition(columns =>
//                             {
//                                 columns.ConstantColumn(22);   // #
//                                 columns.RelativeColumn(2.5f); // Name
//                                 columns.RelativeColumn(1.2f); // Working
//                                 columns.RelativeColumn(1.2f); // Present
//                                 columns.RelativeColumn(1.2f); // Absent
//                                 columns.RelativeColumn(1);    // Late
//                                 columns.RelativeColumn(1.5f); // Basic
//                                 columns.RelativeColumn(1.5f); // Deduction
//                                 columns.RelativeColumn(1.5f); // Net
//                                 columns.RelativeColumn(1.2f); // Status
//                             });

//                             table.Header(header =>
//                             {
//                                 void H(string text) =>
//                                     header.Cell()
//                                           .Background(Colors.Grey.Lighten2)
//                                           .BorderBottom(1).BorderColor(Colors.Grey.Medium)
//                                           .Padding(4).Text(text).Bold().FontSize(8);

//                                 H("#"); H("Employee"); H("Working"); H("Present");
//                                 H("Absent"); H("Late"); H("Basic (₹)");
//                                 H("Deduction (₹)"); H("Net (₹)"); H("Status");
//                             });

//                             int rowNo = 1;
//                             foreach (var (p, name) in records)
//                             {
//                                 var isAlt = rowNo % 2 == 0;
//                                 var bg    = isAlt ? Colors.Grey.Lighten4 : Colors.White;

//                                 void D(string text) =>
//                                     table.Cell().Background(bg)
//                                          .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
//                                          .Padding(4).Text(text).FontSize(8);

//                                 var statusColor = p.Status switch
//                                 {
//                                     "paid"     => Colors.Green.Darken1,
//                                     "approved" => Colors.Blue.Darken1,
//                                     _          => Colors.Orange.Darken2
//                                 };

//                                 D(rowNo.ToString());
//                                 D(name);
//                                 D(p.TotalWorkingDays.ToString());
//                                 D(p.PresentDays.ToString());
//                                 D(p.AbsentDays.ToString());
//                                 D(p.LateDays.ToString());
//                                 D($"{p.BasicSalary:N2}");
//                                 D($"{p.TotalDeduction:N2}");

//                                 // Net salary — highlighted
//                                 table.Cell().Background(bg)
//                                      .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
//                                      .Padding(4).Text($"{p.NetSalary:N2}")
//                                      .FontSize(8).Bold().FontColor(Colors.Blue.Darken2);

//                                 table.Cell().Background(bg)
//                                      .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
//                                      .Padding(4).Text(p.Status.ToUpper())
//                                      .FontSize(7).Bold().FontColor(statusColor);

//                                 rowNo++;
//                             }
//                         });

//                         // Totals row
//                         var totalBasic     = records.Sum(r => r.Payroll.BasicSalary);
//                         var totalDeduction = records.Sum(r => r.Payroll.TotalDeduction);
//                         var totalNet       = records.Sum(r => r.Payroll.NetSalary);

//                         col.Item().PaddingTop(10).Row(row =>
//                         {
//                             row.RelativeItem()
//                                .Text($"Total Employees: {records.Count}")
//                                .Bold().FontSize(9);

//                             void T(string label, decimal amount, string bg, string fg)
//                             {
//                                 row.ConstantItem(8);
//                                 row.ConstantItem(160)
//                                    .Background(bg).Padding(6)
//                                    .Text($"{label}: ₹ {amount:N2}")
//                                    .Bold().FontSize(9).FontColor(fg);
//                             }

//                             T("Total Basic",     totalBasic,     Colors.Blue.Lighten4,   Colors.Blue.Darken2);
//                             T("Total Deductions",totalDeduction,  Colors.Red.Lighten4,    Colors.Red.Darken2);
//                             T("Total Net Salary",totalNet,        Colors.Green.Lighten3,  Colors.Green.Darken2);
//                         });
//                     });

//                     page.Footer()
//                         .Height(30)
//                         .Background(Colors.Grey.Lighten3)
//                         .Padding(5)
//                         .Row(row =>
//                         {
//                             row.RelativeItem().AlignCenter().Text(text =>
//                             {
//                                 text.Span("Page ").FontSize(7);
//                                 text.CurrentPageNumber().FontSize(7);
//                                 text.Span(" of ").FontSize(7);
//                                 text.TotalPages().FontSize(7);
//                             });
//                         });
//                 });
//             });

//             return doc.GeneratePdf();
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  3. Admin Payroll Excel
//         // ════════════════════════════════════════════════════════════════
//         public byte[] GeneratePayrollExcel(
//             List<(PayrollRecord Payroll, string EmployeeName)> records,
//             int month, int year)
//         {
//             var monthName = MonthNames[month];

//             using var wb = new XLWorkbook();
//             var ws = wb.Worksheets.Add($"Payroll {monthName} {year}");

//             // ── Title ────────────────────────────────────────────────────
//             ws.Cell("A1").Value = "Attendance Management System — Payroll Report";
//             ws.Range("A1:K1").Merge().Style
//                 .Font.SetBold(true).Font.SetFontSize(14)
//                 .Fill.SetBackgroundColor(XLColor.FromHtml("#1565C0"))
//                 .Font.SetFontColor(XLColor.White)
//                 .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

//             ws.Cell("A2").Value = $"Period: {monthName} {year}";
//             ws.Cell("F2").Value = $"Generated: {DateTime.Now:dd-MMM-yyyy HH:mm}";
//             ws.Range("A2:E2").Merge().Style.Font.SetItalic(true).Font.SetFontSize(10);
//             ws.Range("F2:K2").Merge().Style.Font.SetItalic(true).Font.SetFontSize(10)
//                 .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

//             // ── Header Row ───────────────────────────────────────────────
//             var headers = new[]
//             {
//                 "#", "Employee Name", "Working Days", "Present Days",
//                 "Absent Days", "Late Days", "Basic Salary (₹)",
//                 "Absent Deduction (₹)", "Late Deduction (₹)",
//                 "Other Deduction (₹)", "Total Deduction (₹)", "Net Salary (₹)", "Status"
//             };

//             int headerRow = 4;
//             for (int i = 0; i < headers.Length; i++)
//             {
//                 var cell = ws.Cell(headerRow, i + 1);
//                 cell.Value = headers[i];
//                 cell.Style
//                     .Font.SetBold(true)
//                     .Fill.SetBackgroundColor(XLColor.FromHtml("#37474F"))
//                     .Font.SetFontColor(XLColor.White)
//                     .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
//                     .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
//             }

//             // ── Data Rows ────────────────────────────────────────────────
//             int dataRow = headerRow + 1;
//             int rowNo   = 1;

//             foreach (var (p, name) in records)
//             {
//                 var isAlt = rowNo % 2 == 0;
//                 var bgColor = isAlt
//                     ? XLColor.FromHtml("#F5F5F5")
//                     : XLColor.White;

//                 ws.Cell(dataRow, 1).Value  = rowNo;
//                 ws.Cell(dataRow, 2).Value  = name;
//                 ws.Cell(dataRow, 3).Value  = p.TotalWorkingDays;
//                 ws.Cell(dataRow, 4).Value  = p.PresentDays;
//                 ws.Cell(dataRow, 5).Value  = p.AbsentDays;
//                 ws.Cell(dataRow, 6).Value  = p.LateDays;
//                 ws.Cell(dataRow, 7).Value  = (double)p.BasicSalary;
//                 ws.Cell(dataRow, 8).Value  = (double)p.AbsentDeduction;
//                 ws.Cell(dataRow, 9).Value  = (double)p.LateDeduction;
//                 ws.Cell(dataRow, 10).Value = (double)p.ManualDeduction;
//                 ws.Cell(dataRow, 11).Value = (double)p.TotalDeduction;
//                 ws.Cell(dataRow, 12).Value = (double)p.NetSalary;
//                 ws.Cell(dataRow, 13).Value = p.Status.ToUpper();

//                 // Format currency columns
//                 for (int c = 7; c <= 12; c++)
//                     ws.Cell(dataRow, c).Style.NumberFormat.Format = "#,##0.00";

//                 // Style row
//                 var range = ws.Range(dataRow, 1, dataRow, 13);
//                 range.Style.Fill.SetBackgroundColor(bgColor);
//                 range.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
//                 range.Style.Border.SetInsideBorder(XLBorderStyleValues.Hair);

//                 // Net salary bold + colored
//                 ws.Cell(dataRow, 12).Style
//                     .Font.SetBold(true)
//                     .Font.SetFontColor(XLColor.FromHtml("#1565C0"));

//                 // Status color
//                 var statusColor = p.Status switch
//                 {
//                     "paid"     => XLColor.FromHtml("#2E7D32"),
//                     "approved" => XLColor.FromHtml("#1565C0"),
//                     _          => XLColor.FromHtml("#E65100")
//                 };
//                 ws.Cell(dataRow, 13).Style.Font.SetFontColor(statusColor).Font.SetBold(true);

//                 dataRow++;
//                 rowNo++;
//             }

//             // ── Totals Row ───────────────────────────────────────────────
//             int totRow = dataRow;
//             ws.Cell(totRow, 1).Value = "TOTAL";
//             ws.Range(totRow, 1, totRow, 6).Merge();
//             ws.Cell(totRow, 1).Style
//                 .Font.SetBold(true)
//                 .Fill.SetBackgroundColor(XLColor.FromHtml("#1565C0"))
//                 .Font.SetFontColor(XLColor.White)
//                 .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

//             var totCols = new[] { 7, 8, 9, 10, 11, 12 };
//             foreach (var c in totCols)
//             {
//                 ws.Cell(totRow, c).FormulaA1 =
//                     $"=SUM({ws.Cell(headerRow + 1, c).Address}:{ws.Cell(dataRow - 1, c).Address})";
//                 ws.Cell(totRow, c).Style
//                     .Font.SetBold(true)
//                     .Fill.SetBackgroundColor(XLColor.FromHtml("#1565C0"))
//                     .Font.SetFontColor(XLColor.White)
//                     .NumberFormat.Format = "#,##0.00";
//             }

//             // ── Column Widths ────────────────────────────────────────────
//             ws.Column(1).Width  = 5;
//             ws.Column(2).Width  = 22;
//             for (int c = 3; c <= 6;  c++) ws.Column(c).Width = 12;
//             for (int c = 7; c <= 12; c++) ws.Column(c).Width = 18;
//             ws.Column(13).Width = 12;

//             ws.SheetView.FreezeRows(headerRow);

//             using var ms = new MemoryStream();
//             wb.SaveAs(ms);
//             return ms.ToArray();
//         }
//     }
// }










// // ======================= Services/PayrollExportService.cs =======================
// using ClosedXML.Excel;
// using QuestPDF.Fluent;
// using QuestPDF.Helpers;
// using QuestPDF.Infrastructure;
// using attendance_api.DTOs;
// using attendance_api.Models;

// namespace attendance_api.Services
// {
//     public interface IPayrollExportService
//     {
//         byte[] GeneratePayslipPdf(
//             PayrollRecord payroll,
//             string employeeName,
//             string department,
//             string designation,
//             string approvedByName = "-",
//             string paidByName     = "-");

//         byte[] GeneratePayrollPdf(
//             List<(PayrollRecord Payroll, string EmployeeName)> records,
//             int month, int year);

//         byte[] GeneratePayrollExcel(
//             List<(PayrollRecord Payroll, string EmployeeName)> records,
//             int month, int year);
//     }

//     public class PayrollExportService : IPayrollExportService
//     {
//         private static readonly string[] MonthNames =
//             { "", "January","February","March","April","May","June",
//               "July","August","September","October","November","December" };

//         public PayrollExportService()
//         {
//             QuestPDF.Settings.License = LicenseType.Community;
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  1. Individual Payslip PDF
//         // ════════════════════════════════════════════════════════════════
//         public byte[] GeneratePayslipPdf(
//             PayrollRecord payroll,
//             string employeeName,
//             string department,
//             string designation,
//             string approvedByName = "-",
//             string paidByName     = "-")
//         {
//             var monthName     = MonthNames[payroll.Month];
//             var paymentStatus = payroll.Status == "paid" ? "Pay Completed" : "Pending";

//             var doc = Document.Create(container =>
//             {
//                 container.Page(page =>
//                 {
//                     page.Size(PageSizes.A4);
//                     page.Margin(2, Unit.Centimetre);
//                     page.PageColor(Colors.White);
//                     page.DefaultTextStyle(x => x.FontSize(10));

//                     // Header
//                     page.Header()
//                         .Height(80)
//                         .Background(Colors.Blue.Lighten3)
//                         .Padding(10)
//                         .Column(col =>
//                         {
//                             col.Item().Text("Attendance Management System")
//                                .FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
//                             col.Item().Text($"Salary Slip — {monthName} {payroll.Year}")
//                                .FontSize(13).SemiBold();
//                             col.Item().PaddingTop(3)
//                                .Text($"Generated: {DateTime.Now:dd-MMM-yyyy HH:mm}")
//                                .FontSize(8).Italic();
//                         });

//                     page.Content().PaddingVertical(12).Column(col =>
//                     {
//                         // Employee Info Box
//                         col.Item()
//                            .Border(1).BorderColor(Colors.Grey.Lighten2)
//                            .Background(Colors.Grey.Lighten5)
//                            .Padding(10)
//                            .Column(info =>
//                         {
//                             info.Item().Text("EMPLOYEE DETAILS")
//                                 .FontSize(10).Bold().FontColor(Colors.Blue.Darken2);

//                             info.Item().PaddingTop(6).Row(row =>
//                             {
//                                 row.RelativeItem().Column(c =>
//                                 {
//                                     c.Item().Text($"Name        :  {employeeName}").FontSize(10);
//                                     c.Item().Text($"Department  :  {department}").FontSize(10);
//                                     c.Item().Text($"Designation :  {designation}").FontSize(10);
//                                 });
//                                 row.RelativeItem().Column(c =>
//                                 {
//                                     c.Item().Text($"Employee ID :  {payroll.EmployeeId}").FontSize(10);
//                                     c.Item().Text($"Month       :  {monthName} {payroll.Year}").FontSize(10);

//                                     var statusColor = payroll.Status == "paid"     ? Colors.Green.Darken1 :
//                                                       payroll.Status == "approved" ? Colors.Blue.Darken1 :
//                                                                                      Colors.Orange.Darken2;
//                                     c.Item().Text($"Status      :  {payroll.Status.ToUpper()}")
//                                             .FontSize(10).Bold().FontColor(statusColor);

//                                     var pmtColor = payroll.Status == "paid"
//                                         ? Colors.Green.Darken1 : Colors.Red.Darken1;
//                                     c.Item().Text($"Payment     :  {paymentStatus}")
//                                             .FontSize(10).Bold().FontColor(pmtColor);

//                                     if (payroll.ApprovedAt.HasValue)
//                                     {
//                                         c.Item().Text($"Approved By :  {approvedByName}").FontSize(10);
//                                         c.Item().Text($"Approved On :  {payroll.ApprovedAt:dd-MMM-yyyy}").FontSize(10);
//                                     }

//                                     if (payroll.PaidAt.HasValue)
//                                     {
//                                         c.Item().Text($"Paid By     :  {paidByName}").FontSize(10);
//                                         c.Item().Text($"Paid On     :  {payroll.PaidAt:dd-MMM-yyyy}").FontSize(10);
//                                     }
//                                 });
//                             });
//                         });

//                         col.Item().PaddingTop(12);

//                         // Attendance Summary
//                         col.Item().Text("ATTENDANCE SUMMARY")
//                            .FontSize(10).Bold().FontColor(Colors.Blue.Darken2);

//                         col.Item().PaddingTop(4).Table(t =>
//                         {
//                             t.ColumnsDefinition(c =>
//                             {
//                                 c.RelativeColumn(); c.RelativeColumn();
//                                 c.RelativeColumn(); c.RelativeColumn();
//                             });

//                             void AH(string text) =>
//                                 t.Cell().Background(Colors.Grey.Lighten2)
//                                  .BorderBottom(1).BorderColor(Colors.Grey.Medium)
//                                  .Padding(5).Text(text).Bold().FontSize(9);

//                             AH("Working Days"); AH("Present Days");
//                             AH("Absent Days");  AH("Late Days (½ day each)");

//                             void AV(string text) =>
//                                 t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
//                                  .Padding(5).Text(text).FontSize(10).Bold();

//                             AV(payroll.TotalWorkingDays.ToString());
//                             AV(payroll.PresentDays.ToString());
//                             AV(payroll.AbsentDays.ToString());
//                             AV(payroll.LateDays.ToString());
//                         });

//                         col.Item().PaddingTop(4)
//                            .Text("* Office: 10:00 AM – 6:00 PM  |  Late after 10:15 AM = ½ day deducted  |  Sat & Sun off")
//                            .FontSize(8).Italic().FontColor(Colors.Grey.Darken1);

//                         col.Item().PaddingTop(12);

//                         // Earnings & Deductions
//                         col.Item().Row(row =>
//                         {
//                             // Earnings
//                             row.RelativeItem().Column(c =>
//                             {
//                                 c.Item().Text("EARNINGS")
//                                  .FontSize(10).Bold().FontColor(Colors.Green.Darken2);

//                                 c.Item().PaddingTop(4).Table(t =>
//                                 {
//                                     t.ColumnsDefinition(cols =>
//                                     {
//                                         cols.RelativeColumn(2);
//                                         cols.RelativeColumn(1);
//                                     });

//                                     void EH(string text) =>
//                                         t.Cell().Background(Colors.Grey.Lighten2)
//                                          .BorderBottom(1).BorderColor(Colors.Grey.Medium)
//                                          .Padding(4).Text(text).Bold().FontSize(9);
//                                     EH("Description"); EH("Amount (₹)");

//                                     void ER(string label, decimal amt)
//                                     {
//                                         t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
//                                          .Padding(4).Text(label).FontSize(9);
//                                         t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
//                                          .Padding(4).Text($"{amt:N2}").FontSize(9);
//                                     }
//                                     ER("Basic Salary",   payroll.BasicSalary);
//                                     ER("Per Day Salary", payroll.PerDaySalary);

//                                     t.Cell().Background(Colors.Green.Lighten4)
//                                      .Padding(4).Text("Gross Salary").FontSize(9).Bold();
//                                     t.Cell().Background(Colors.Green.Lighten4)
//                                      .Padding(4).Text($"{payroll.BasicSalary:N2}").FontSize(9).Bold()
//                                      .FontColor(Colors.Green.Darken2);
//                                 });
//                             });

//                             row.ConstantItem(15);

//                             // Deductions
//                             row.RelativeItem().Column(c =>
//                             {
//                                 c.Item().Text("DEDUCTIONS")
//                                  .FontSize(10).Bold().FontColor(Colors.Red.Darken2);

//                                 c.Item().PaddingTop(4).Table(t =>
//                                 {
//                                     t.ColumnsDefinition(cols =>
//                                     {
//                                         cols.RelativeColumn(2);
//                                         cols.RelativeColumn(1);
//                                     });

//                                     void DH(string text) =>
//                                         t.Cell().Background(Colors.Grey.Lighten2)
//                                          .BorderBottom(1).BorderColor(Colors.Grey.Medium)
//                                          .Padding(4).Text(text).Bold().FontSize(9);
//                                     DH("Description"); DH("Amount (₹)");

//                                     void DR(string label, decimal amt)
//                                     {
//                                         t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
//                                          .Padding(4).Text(label).FontSize(9);
//                                         t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
//                                          .Padding(4).Text($"{amt:N2}").FontSize(9);
//                                     }
//                                     DR("Absent Deduction", payroll.AbsentDeduction);
//                                     DR($"Late Deduction (½ × {payroll.LateDays})", payroll.LateDeduction);
//                                     DR($"Other ({payroll.ManualDeductionReason ?? "-"})", payroll.ManualDeduction);

//                                     t.Cell().Background(Colors.Red.Lighten4)
//                                      .Padding(4).Text("Total Deduction").FontSize(9).Bold();
//                                     t.Cell().Background(Colors.Red.Lighten4)
//                                      .Padding(4).Text($"{payroll.TotalDeduction:N2}").FontSize(9).Bold()
//                                      .FontColor(Colors.Red.Darken2);
//                                 });
//                             });
//                         });

//                         // Net Salary
//                         col.Item().PaddingTop(16)
//                            .Background(Colors.Blue.Darken2).Padding(12)
//                            .Row(row =>
//                         {
//                             row.RelativeItem()
//                                .Text("NET SALARY PAYABLE")
//                                .FontSize(13).Bold().FontColor(Colors.White);
//                             row.ConstantItem(200).AlignRight()
//                                .Text($"₹ {payroll.NetSalary:N2}")
//                                .FontSize(16).Bold().FontColor(Colors.Yellow.Medium);
//                         });

//                         // Payment Status Banner
//                         var bannerBg = payroll.Status == "paid" ? Colors.Green.Lighten3 : Colors.Orange.Lighten3;
//                         var bannerFg = payroll.Status == "paid" ? Colors.Green.Darken2  : Colors.Orange.Darken2;

//                         col.Item().PaddingTop(8)
//                            .Background(bannerBg).Padding(8).AlignCenter()
//                            .Text($"PAYMENT STATUS :  {paymentStatus.ToUpper()}")
//                            .FontSize(11).Bold().FontColor(bannerFg);

//                         if (!string.IsNullOrEmpty(payroll.Remarks))
//                             col.Item().PaddingTop(8)
//                                .Text($"Remarks: {payroll.Remarks}")
//                                .FontSize(9).Italic().FontColor(Colors.Grey.Darken2);
//                     });

//                     // Footer
//                     page.Footer().Height(35).Background(Colors.Grey.Lighten3).Padding(6)
//                         .Column(col =>
//                         {
//                             col.Item().AlignCenter().Text(text =>
//                             {
//                                 text.Span("Page ").FontSize(8);
//                                 text.CurrentPageNumber().FontSize(8);
//                                 text.Span(" of ").FontSize(8);
//                                 text.TotalPages().FontSize(8);
//                             });
//                             col.Item().AlignCenter()
//                                .Text("** CONFIDENTIAL — FOR EMPLOYEE USE ONLY **")
//                                .FontSize(7).Italic();
//                         });
//                 });
//             });

//             return doc.GeneratePdf();
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  2. Admin Payroll Summary PDF
//         // ════════════════════════════════════════════════════════════════
//         public byte[] GeneratePayrollPdf(
//             List<(PayrollRecord Payroll, string EmployeeName)> records,
//             int month, int year)
//         {
//             var monthName = MonthNames[month];

//             var doc = Document.Create(container =>
//             {
//                 container.Page(page =>
//                 {
//                     page.Size(PageSizes.A4.Landscape());
//                     page.Margin(1.5f, Unit.Centimetre);
//                     page.PageColor(Colors.White);
//                     page.DefaultTextStyle(x => x.FontSize(8));

//                     page.Header().Height(60).Background(Colors.Blue.Lighten3).Padding(10)
//                         .Column(col =>
//                         {
//                             col.Item().Text("Attendance Management System — Payroll Report")
//                                .FontSize(15).Bold().FontColor(Colors.Blue.Darken2);
//                             col.Item().Row(row =>
//                             {
//                                 row.RelativeItem()
//                                    .Text($"Period: {monthName} {year}  |  Total Employees: {records.Count}")
//                                    .FontSize(9);
//                                 row.RelativeItem().AlignRight()
//                                    .Text($"Generated: {DateTime.Now:dd-MMM-yyyy HH:mm}").FontSize(9);
//                             });
//                         });

//                     page.Content().PaddingVertical(8).Column(col =>
//                     {
//                         col.Item().Table(table =>
//                         {
//                             table.ColumnsDefinition(columns =>
//                             {
//                                 columns.ConstantColumn(22);
//                                 columns.RelativeColumn(2.5f);
//                                 columns.RelativeColumn(1.2f);
//                                 columns.RelativeColumn(1.2f);
//                                 columns.RelativeColumn(1.2f);
//                                 columns.RelativeColumn(1);
//                                 columns.RelativeColumn(1.5f);
//                                 columns.RelativeColumn(1.5f);
//                                 columns.RelativeColumn(1.5f);
//                                 columns.RelativeColumn(1.5f); // Payment
//                             });

//                             table.Header(header =>
//                             {
//                                 void H(string text) =>
//                                     header.Cell()
//                                           .Background(Colors.Grey.Lighten2)
//                                           .BorderBottom(1).BorderColor(Colors.Grey.Medium)
//                                           .Padding(4).Text(text).Bold().FontSize(8);

//                                 H("#"); H("Employee"); H("Working"); H("Present");
//                                 H("Absent"); H("Late"); H("Basic (₹)");
//                                 H("Deduction (₹)"); H("Net (₹)"); H("Payment");
//                             });

//                             int rowNo = 1;
//                             foreach (var (p, name) in records)
//                             {
//                                 var isAlt     = rowNo % 2 == 0;
//                                 var bg        = isAlt ? Colors.Grey.Lighten4 : Colors.White;
//                                 var pmtStatus = p.Status == "paid" ? "Pay Completed" : "Pending";

//                                 void D(string text) =>
//                                     table.Cell().Background(bg)
//                                          .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
//                                          .Padding(4).Text(text).FontSize(8);

//                                 D(rowNo.ToString()); D(name);
//                                 D(p.TotalWorkingDays.ToString()); D(p.PresentDays.ToString());
//                                 D(p.AbsentDays.ToString());       D(p.LateDays.ToString());
//                                 D($"{p.BasicSalary:N2}");         D($"{p.TotalDeduction:N2}");

//                                 table.Cell().Background(bg)
//                                      .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
//                                      .Padding(4).Text($"{p.NetSalary:N2}")
//                                      .FontSize(8).Bold().FontColor(Colors.Blue.Darken2);

//                                 var pmtColor = p.Status == "paid"
//                                     ? Colors.Green.Darken1 : Colors.Orange.Darken2;
//                                 table.Cell().Background(bg)
//                                      .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
//                                      .Padding(4).Text(pmtStatus)
//                                      .FontSize(7).Bold().FontColor(pmtColor);

//                                 rowNo++;
//                             }
//                         });

//                         var totalBasic     = records.Sum(r => r.Payroll.BasicSalary);
//                         var totalDeduction = records.Sum(r => r.Payroll.TotalDeduction);
//                         var totalNet       = records.Sum(r => r.Payroll.NetSalary);
//                         var paidCount      = records.Count(r => r.Payroll.Status == "paid");

//                         col.Item().PaddingTop(10).Row(row =>
//                         {
//                             row.RelativeItem()
//                                .Text($"Total: {records.Count}  |  Paid: {paidCount}  |  Pending: {records.Count - paidCount}")
//                                .Bold().FontSize(9);

//                             void T(string label, decimal amount, string bg, string fg)
//                             {
//                                 row.ConstantItem(8);
//                                 row.ConstantItem(170).Background(bg).Padding(6)
//                                    .Text($"{label}: ₹ {amount:N2}")
//                                    .Bold().FontSize(9).FontColor(fg);
//                             }

//                             T("Total Basic",      totalBasic,     Colors.Blue.Lighten4,  Colors.Blue.Darken2);
//                             T("Total Deductions", totalDeduction,  Colors.Red.Lighten4,   Colors.Red.Darken2);
//                             T("Total Net Salary", totalNet,        Colors.Green.Lighten3, Colors.Green.Darken2);
//                         });
//                     });

//                     page.Footer().Height(30).Background(Colors.Grey.Lighten3).Padding(5)
//                         .Row(row =>
//                         {
//                             row.RelativeItem().AlignCenter().Text(text =>
//                             {
//                                 text.Span("Page ").FontSize(7);
//                                 text.CurrentPageNumber().FontSize(7);
//                                 text.Span(" of ").FontSize(7);
//                                 text.TotalPages().FontSize(7);
//                             });
//                         });
//                 });
//             });

//             return doc.GeneratePdf();
//         }

//         // ════════════════════════════════════════════════════════════════
//         //  3. Admin Payroll Excel — Sheet Protected (read-only)
//         // ════════════════════════════════════════════════════════════════
//         public byte[] GeneratePayrollExcel(
//             List<(PayrollRecord Payroll, string EmployeeName)> records,
//             int month, int year)
//         {
//             var monthName = MonthNames[month];

//             using var wb = new XLWorkbook();
//             var ws = wb.Worksheets.Add($"Payroll {monthName} {year}");

//             // Unlock all first, then re-lock data area
//             ws.Style.Protection.Locked = false;

//             // Title
//             ws.Cell("A1").Value = "Attendance Management System — Payroll Report";
//             ws.Range("A1:N1").Merge().Style
//                 .Font.SetBold(true).Font.SetFontSize(14)
//                 .Fill.SetBackgroundColor(XLColor.FromHtml("#1565C0"))
//                 .Font.SetFontColor(XLColor.White)
//                 .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
//                 .Protection.SetLocked(true);

//             ws.Cell("A2").Value = $"Period: {monthName} {year}";
//             ws.Cell("F2").Value = $"Generated: {DateTime.Now:dd-MMM-yyyy HH:mm}";
//             ws.Range("A2:E2").Merge().Style.Font.SetItalic(true).Font.SetFontSize(10);
//             ws.Range("F2:N2").Merge().Style.Font.SetItalic(true).Font.SetFontSize(10)
//                 .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
//             ws.Range("A2:N2").Style.Protection.Locked = true;

//             // Headers
//             var headers = new[]
//             {
//                 "#", "Employee Name", "Working Days", "Present Days",
//                 "Absent Days", "Late Days (½ day)", "Basic Salary (₹)",
//                 "Absent Deduction (₹)", "Late Deduction (₹)",
//                 "Other Deduction (₹)", "Total Deduction (₹)",
//                 "Net Salary (₹)", "Status", "Payment Status"
//             };

//             int headerRow = 4;
//             for (int i = 0; i < headers.Length; i++)
//             {
//                 var cell = ws.Cell(headerRow, i + 1);
//                 cell.Value = headers[i];
//                 cell.Style
//                     .Font.SetBold(true)
//                     .Fill.SetBackgroundColor(XLColor.FromHtml("#37474F"))
//                     .Font.SetFontColor(XLColor.White)
//                     .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
//                     .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
//                     .Protection.SetLocked(true);
//             }

//             // Data rows
//             int dataRow = headerRow + 1;
//             int rowNo   = 1;

//             foreach (var (p, name) in records)
//             {
//                 var isAlt     = rowNo % 2 == 0;
//                 var bgColor   = isAlt ? XLColor.FromHtml("#F5F5F5") : XLColor.White;
//                 var pmtStatus = p.Status == "paid" ? "Pay Completed" : "Pending";

//                 ws.Cell(dataRow, 1).Value  = rowNo;
//                 ws.Cell(dataRow, 2).Value  = name;
//                 ws.Cell(dataRow, 3).Value  = p.TotalWorkingDays;
//                 ws.Cell(dataRow, 4).Value  = p.PresentDays;
//                 ws.Cell(dataRow, 5).Value  = p.AbsentDays;
//                 ws.Cell(dataRow, 6).Value  = p.LateDays;
//                 ws.Cell(dataRow, 7).Value  = (double)p.BasicSalary;
//                 ws.Cell(dataRow, 8).Value  = (double)p.AbsentDeduction;
//                 ws.Cell(dataRow, 9).Value  = (double)p.LateDeduction;
//                 ws.Cell(dataRow, 10).Value = (double)p.ManualDeduction;
//                 ws.Cell(dataRow, 11).Value = (double)p.TotalDeduction;
//                 ws.Cell(dataRow, 12).Value = (double)p.NetSalary;
//                 ws.Cell(dataRow, 13).Value = p.Status.ToUpper();
//                 ws.Cell(dataRow, 14).Value = pmtStatus;

//                 for (int c = 7; c <= 12; c++)
//                     ws.Cell(dataRow, c).Style.NumberFormat.Format = "#,##0.00";

//                 var range = ws.Range(dataRow, 1, dataRow, 14);
//                 range.Style
//                     .Fill.SetBackgroundColor(bgColor)
//                     .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
//                     .Border.SetInsideBorder(XLBorderStyleValues.Hair)
//                     .Protection.SetLocked(true);   // lock data

//                 ws.Cell(dataRow, 12).Style
//                     .Font.SetBold(true)
//                     .Font.SetFontColor(XLColor.FromHtml("#1565C0"));

//                 var statusColor = p.Status switch
//                 {
//                     "paid"     => XLColor.FromHtml("#2E7D32"),
//                     "approved" => XLColor.FromHtml("#1565C0"),
//                     _          => XLColor.FromHtml("#E65100")
//                 };
//                 ws.Cell(dataRow, 13).Style.Font.SetFontColor(statusColor).Font.SetBold(true);

//                 var pmtColor = p.Status == "paid"
//                     ? XLColor.FromHtml("#2E7D32") : XLColor.FromHtml("#E65100");
//                 ws.Cell(dataRow, 14).Style.Font.SetFontColor(pmtColor).Font.SetBold(true);

//                 dataRow++;
//                 rowNo++;
//             }

//             // Totals row
//             int totRow = dataRow;
//             ws.Cell(totRow, 1).Value = "TOTAL";
//             ws.Range(totRow, 1, totRow, 6).Merge();
//             ws.Cell(totRow, 1).Style
//                 .Font.SetBold(true)
//                 .Fill.SetBackgroundColor(XLColor.FromHtml("#1565C0"))
//                 .Font.SetFontColor(XLColor.White)
//                 .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
//                 .Protection.SetLocked(true);

//             foreach (var c in new[] { 7, 8, 9, 10, 11, 12 })
//             {
//                 ws.Cell(totRow, c).FormulaA1 =
//                     $"=SUM({ws.Cell(headerRow + 1, c).Address}:{ws.Cell(dataRow - 1, c).Address})";
//                 ws.Cell(totRow, c).Style
//                     .Font.SetBold(true)
//                     .Fill.SetBackgroundColor(XLColor.FromHtml("#1565C0"))
//                     .Font.SetFontColor(XLColor.White)
//                     .NumberFormat.Format = "#,##0.00"
//                     .ToString();
//                 ws.Cell(totRow, c).Style.Protection.Locked = true;
//             }

//             // Column widths
//             ws.Column(1).Width = 5;
//             ws.Column(2).Width = 22;
//             for (int c = 3; c <= 6;  c++) ws.Column(c).Width = 14;
//             for (int c = 7; c <= 12; c++) ws.Column(c).Width = 18;
//             ws.Column(13).Width = 14;
//             ws.Column(14).Width = 16;

//             ws.SheetView.FreezeRows(headerRow);

//             // Protect sheet — no editing allowed
//             ws.Protect()
//               .AllowElement(XLSheetProtectionElements.SelectLockedCells)
//               .AllowElement(XLSheetProtectionElements.SelectUnlockedCells);

//             using var ms = new MemoryStream();
//             wb.SaveAs(ms);
//             return ms.ToArray();
//         }
//     }
// }












// ======================= Services/PayrollExportService.cs =======================
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using attendance_api.Models;

namespace attendance_api.Services
{
    public interface IPayrollExportService
    {
        byte[] GeneratePayslipPdf(
            PayrollRecord payroll,
            string employeeName,
            string department,
            string designation,
            string approvedByName = "-",
            string paidByName     = "-");

        byte[] GeneratePayrollPdf(
            List<(PayrollRecord Payroll, string EmployeeName)> records,
            int month, int year);

        byte[] GeneratePayrollExcel(
            List<(PayrollRecord Payroll, string EmployeeName)> records,
            int month, int year);
    }

    public class PayrollExportService : IPayrollExportService
    {
        private static readonly string[] MonthNames =
            { "", "January","February","March","April","May","June",
              "July","August","September","October","November","December" };

        public PayrollExportService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // ════════════════════════════════════════════════════════════════
        //  1. Individual Payslip PDF
        // ════════════════════════════════════════════════════════════════
        public byte[] GeneratePayslipPdf(
            PayrollRecord payroll,
            string employeeName,
            string department,
            string designation,
            string approvedByName = "-",
            string paidByName     = "-")
        {
            var monthName     = MonthNames[payroll.Month];
            var paymentStatus = payroll.Status == "paid" ? "Pay Completed" : "Pending";

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // ── Header ────────────────────────────────────────────
                    page.Header()
                        .Height(80)
                        .Background(Colors.Blue.Lighten3)
                        .Padding(10)
                        .Column(col =>
                        {
                            col.Item().Text("Attendance Management System")
                               .FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                            col.Item().Text($"Salary Slip — {monthName} {payroll.Year}")
                               .FontSize(13).SemiBold();
                            col.Item().PaddingTop(3)
                               .Text($"Generated: {DateTime.Now:dd-MMM-yyyy HH:mm}")
                               .FontSize(8).Italic();
                        });

                    page.Content().PaddingVertical(12).Column(col =>
                    {
                        // ── Employee Info Box ──────────────────────────────
                        col.Item()
                           .Border(1).BorderColor(Colors.Grey.Lighten2)
                           .Background(Colors.Grey.Lighten5)
                           .Padding(10)
                           .Column(info =>
                        {
                            info.Item().Text("EMPLOYEE DETAILS")
                                .FontSize(10).Bold().FontColor(Colors.Blue.Darken2);

                            info.Item().PaddingTop(6).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text($"Name        :  {employeeName}").FontSize(10);
                                    c.Item().Text($"Department  :  {department}").FontSize(10);
                                    c.Item().Text($"Designation :  {designation}").FontSize(10);
                                });
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text($"Employee ID :  {payroll.EmployeeId}").FontSize(10);
                                    c.Item().Text($"Month       :  {monthName} {payroll.Year}").FontSize(10);

                                    var statusColor = payroll.Status == "paid"     ? Colors.Green.Darken1 :
                                                      payroll.Status == "approved" ? Colors.Blue.Darken1 :
                                                                                     Colors.Orange.Darken2;
                                    c.Item().Text($"Status      :  {payroll.Status.ToUpper()}")
                                            .FontSize(10).Bold().FontColor(statusColor);

                                    var pmtColor = payroll.Status == "paid"
                                        ? Colors.Green.Darken1 : Colors.Red.Darken1;
                                    c.Item().Text($"Payment     :  {paymentStatus}")
                                            .FontSize(10).Bold().FontColor(pmtColor);

                                    if (payroll.ApprovedAt.HasValue)
                                    {
                                        c.Item().Text($"Approved By :  {approvedByName}").FontSize(10);
                                        c.Item().Text($"Approved On :  {payroll.ApprovedAt:dd-MMM-yyyy}").FontSize(10);
                                    }
                                    if (payroll.PaidAt.HasValue)
                                    {
                                        c.Item().Text($"Paid By     :  {paidByName}").FontSize(10);
                                        c.Item().Text($"Paid On     :  {payroll.PaidAt:dd-MMM-yyyy}").FontSize(10);
                                    }
                                });
                            });
                        });

                        col.Item().PaddingTop(12);

                        // ── Attendance Summary — 5 columns ────────────────
                        // Calendar Days | Working Days | Present | Absent | Late
                        col.Item().Text("ATTENDANCE SUMMARY")
                           .FontSize(10).Bold().FontColor(Colors.Blue.Darken2);

                        col.Item().PaddingTop(4).Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn();
                                c.RelativeColumn(); c.RelativeColumn();
                            });

                            void AH(string text) =>
                                t.Cell().Background(Colors.Blue.Lighten4)
                                 .BorderBottom(1).BorderColor(Colors.Blue.Lighten2)
                                 .Padding(5).Text(text).Bold().FontSize(8.5f)
                                 .FontColor(Colors.Blue.Darken2);

                            // 5 column headers
                            AH($"Calendar Days\n({monthName})");
                            AH("Working Days\n(Mon–Fri)");
                            AH("Present Days");
                            AH("Absent Days");
                            AH("Late Days\n(½ day each)");

                            void AV(string text) =>
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                 .Padding(5).Text(text).FontSize(11).Bold()
                                 .FontColor(Colors.Grey.Darken2);

                            // 5 values
                            AV(payroll.TotalCalendarDays.ToString());
                            AV(payroll.TotalWorkingDays.ToString());
                            AV(payroll.PresentDays.ToString());
                            AV(payroll.AbsentDays.ToString());
                            AV(payroll.LateDays.ToString());
                        });

                        col.Item().PaddingTop(4)
                           .Text("* Office: 10:00 AM – 6:00 PM  |  Late after 10:15 AM = ½ day deducted  |  Sat & Sun off")
                           .FontSize(8).Italic().FontColor(Colors.Grey.Darken1);

                        col.Item().PaddingTop(12);

                        // ── Earnings & Deductions ──────────────────────────
                        col.Item().Row(row =>
                        {
                            // Earnings
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("EARNINGS")
                                 .FontSize(10).Bold().FontColor(Colors.Green.Darken2);

                                c.Item().PaddingTop(4).Table(t =>
                                {
                                    t.ColumnsDefinition(cols =>
                                    {
                                        cols.RelativeColumn(2);
                                        cols.RelativeColumn(1);
                                    });

                                    void EH(string text) =>
                                        t.Cell().Background(Colors.Grey.Lighten2)
                                         .BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                         .Padding(4).Text(text).Bold().FontSize(9);
                                    EH("Description"); EH("Amount (₹)");

                                    void ER(string label, decimal amt)
                                    {
                                        t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                         .Padding(4).Text(label).FontSize(9);
                                        t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                         .Padding(4).Text($"{amt:N2}").FontSize(9);
                                    }

                                    ER("Basic Salary", payroll.BasicSalary);
                                    // Show per day with calendar days context
                                    ER($"Per Day ({payroll.TotalCalendarDays} days)", payroll.PerDaySalary);

                                    t.Cell().Background(Colors.Green.Lighten4)
                                     .Padding(4).Text("Gross Salary").FontSize(9).Bold();
                                    t.Cell().Background(Colors.Green.Lighten4)
                                     .Padding(4).Text($"{payroll.BasicSalary:N2}").FontSize(9).Bold()
                                     .FontColor(Colors.Green.Darken2);
                                });
                            });

                            row.ConstantItem(15);

                            // Deductions
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("DEDUCTIONS")
                                 .FontSize(10).Bold().FontColor(Colors.Red.Darken2);

                                c.Item().PaddingTop(4).Table(t =>
                                {
                                    t.ColumnsDefinition(cols =>
                                    {
                                        cols.RelativeColumn(2);
                                        cols.RelativeColumn(1);
                                    });

                                    void DH(string text) =>
                                        t.Cell().Background(Colors.Grey.Lighten2)
                                         .BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                         .Padding(4).Text(text).Bold().FontSize(9);
                                    DH("Description"); DH("Amount (₹)");

                                    void DR(string label, decimal amt)
                                    {
                                        t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                         .Padding(4).Text(label).FontSize(9);
                                        t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                         .Padding(4).Text($"{amt:N2}").FontSize(9);
                                    }

                                    DR("Absent Deduction", payroll.AbsentDeduction);
                                    DR($"Late Deduction (½ × {payroll.LateDays})", payroll.LateDeduction);
                                    DR($"Other ({payroll.ManualDeductionReason ?? "-"})", payroll.ManualDeduction);

                                    t.Cell().Background(Colors.Red.Lighten4)
                                     .Padding(4).Text("Total Deduction").FontSize(9).Bold();
                                    t.Cell().Background(Colors.Red.Lighten4)
                                     .Padding(4).Text($"{payroll.TotalDeduction:N2}").FontSize(9).Bold()
                                     .FontColor(Colors.Red.Darken2);
                                });
                            });
                        });

                        // ── Net Salary ─────────────────────────────────────
                        col.Item().PaddingTop(16)
                           .Background(Colors.Blue.Darken2).Padding(12)
                           .Row(row =>
                        {
                            row.RelativeItem()
                               .Text("NET SALARY PAYABLE")
                               .FontSize(13).Bold().FontColor(Colors.White);
                            row.ConstantItem(200).AlignRight()
                               .Text($"₹ {payroll.NetSalary:N2}")
                               .FontSize(16).Bold().FontColor(Colors.Yellow.Medium);
                        });

                        // ── Payment Status Banner ──────────────────────────
                        var bannerBg = payroll.Status == "paid" ? Colors.Green.Lighten3 : Colors.Orange.Lighten3;
                        var bannerFg = payroll.Status == "paid" ? Colors.Green.Darken2  : Colors.Orange.Darken2;

                        col.Item().PaddingTop(8)
                           .Background(bannerBg).Padding(8).AlignCenter()
                           .Text($"PAYMENT STATUS :  {paymentStatus.ToUpper()}")
                           .FontSize(11).Bold().FontColor(bannerFg);

                        if (!string.IsNullOrEmpty(payroll.Remarks))
                            col.Item().PaddingTop(8)
                               .Text($"Remarks: {payroll.Remarks}")
                               .FontSize(9).Italic().FontColor(Colors.Grey.Darken2);
                    });

                    // ── Footer ────────────────────────────────────────────
                    page.Footer().Height(35).Background(Colors.Grey.Lighten3).Padding(6)
                        .Column(col =>
                        {
                            col.Item().AlignCenter().Text(text =>
                            {
                                text.Span("Page ").FontSize(8);
                                text.CurrentPageNumber().FontSize(8);
                                text.Span(" of ").FontSize(8);
                                text.TotalPages().FontSize(8);
                            });
                            col.Item().AlignCenter()
                               .Text("** CONFIDENTIAL — FOR EMPLOYEE USE ONLY **")
                               .FontSize(7).Italic();
                        });
                });
            });

            return doc.GeneratePdf();
        }

        // ════════════════════════════════════════════════════════════════
        //  2. Admin Payroll Summary PDF (landscape)
        //  Columns: # | Name | Cal Days | Working | Present | Absent | Late | Basic | Deduction | Net | Payment
        // ════════════════════════════════════════════════════════════════
        public byte[] GeneratePayrollPdf(
            List<(PayrollRecord Payroll, string EmployeeName)> records,
            int month, int year)
        {
            var monthName = MonthNames[month];
            int calDays   = DateTime.DaysInMonth(year, month);

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    page.Header().Height(60).Background(Colors.Blue.Lighten3).Padding(10)
                        .Column(col =>
                        {
                            col.Item().Text("Attendance Management System — Payroll Report")
                               .FontSize(15).Bold().FontColor(Colors.Blue.Darken2);
                            col.Item().Row(row =>
                            {
                                row.RelativeItem()
                                   .Text($"Period: {monthName} {year}  |  Calendar Days: {calDays}  |  Total Employees: {records.Count}")
                                   .FontSize(9);
                                row.RelativeItem().AlignRight()
                                   .Text($"Generated: {DateTime.Now:dd-MMM-yyyy HH:mm}").FontSize(9);
                            });
                        });

                    page.Content().PaddingVertical(8).Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            // 11 columns
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(22);   // #
                                columns.RelativeColumn(2.5f); // Name
                                columns.RelativeColumn(1f);   // Cal Days
                                columns.RelativeColumn(1.2f); // Working
                                columns.RelativeColumn(1.2f); // Present
                                columns.RelativeColumn(1.2f); // Absent
                                columns.RelativeColumn(1f);   // Late
                                columns.RelativeColumn(1.5f); // Basic
                                columns.RelativeColumn(1.5f); // Deduction
                                columns.RelativeColumn(1.5f); // Net
                                columns.RelativeColumn(1.5f); // Payment
                            });

                            table.Header(header =>
                            {
                                void H(string text) =>
                                    header.Cell()
                                          .Background(Colors.Grey.Lighten2)
                                          .BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                          .Padding(4).Text(text).Bold().FontSize(8);

                                H("#"); H("Employee"); H("Cal Days"); H("Working");
                                H("Present"); H("Absent"); H("Late");
                                H("Basic (₹)"); H("Deduction (₹)"); H("Net (₹)"); H("Payment");
                            });

                            int rowNo = 1;
                            foreach (var (p, name) in records)
                            {
                                var isAlt     = rowNo % 2 == 0;
                                var bg        = isAlt ? Colors.Grey.Lighten4 : Colors.White;
                                var pmtStatus = p.Status == "paid" ? "Pay Completed" : "Pending";

                                void D(string text) =>
                                    table.Cell().Background(bg)
                                         .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                         .Padding(4).Text(text).FontSize(8);

                                D(rowNo.ToString());
                                D(name);
                                D(p.TotalCalendarDays.ToString());   // ← Calendar Days
                                D(p.TotalWorkingDays.ToString());
                                D(p.PresentDays.ToString());
                                D(p.AbsentDays.ToString());
                                D(p.LateDays.ToString());
                                D($"{p.BasicSalary:N2}");
                                D($"{p.TotalDeduction:N2}");

                                table.Cell().Background(bg)
                                     .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                     .Padding(4).Text($"{p.NetSalary:N2}")
                                     .FontSize(8).Bold().FontColor(Colors.Blue.Darken2);

                                var pmtColor = p.Status == "paid"
                                    ? Colors.Green.Darken1 : Colors.Orange.Darken2;
                                table.Cell().Background(bg)
                                     .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                     .Padding(4).Text(pmtStatus)
                                     .FontSize(7).Bold().FontColor(pmtColor);

                                rowNo++;
                            }
                        });

                        var totalBasic     = records.Sum(r => r.Payroll.BasicSalary);
                        var totalDeduction = records.Sum(r => r.Payroll.TotalDeduction);
                        var totalNet       = records.Sum(r => r.Payroll.NetSalary);
                        var paidCount      = records.Count(r => r.Payroll.Status == "paid");

                        col.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem()
                               .Text($"Total: {records.Count}  |  Paid: {paidCount}  |  Pending: {records.Count - paidCount}")
                               .Bold().FontSize(9);

                            void T(string label, decimal amount, string bg, string fg)
                            {
                                row.ConstantItem(8);
                                row.ConstantItem(170).Background(bg).Padding(6)
                                   .Text($"{label}: ₹ {amount:N2}")
                                   .Bold().FontSize(9).FontColor(fg);
                            }

                            T("Total Basic",      totalBasic,    Colors.Blue.Lighten4,  Colors.Blue.Darken2);
                            T("Total Deductions", totalDeduction, Colors.Red.Lighten4,   Colors.Red.Darken2);
                            T("Total Net Salary", totalNet,       Colors.Green.Lighten3, Colors.Green.Darken2);
                        });
                    });

                    page.Footer().Height(30).Background(Colors.Grey.Lighten3).Padding(5)
                        .Row(row =>
                        {
                            row.RelativeItem().AlignCenter().Text(text =>
                            {
                                text.Span("Page ").FontSize(7);
                                text.CurrentPageNumber().FontSize(7);
                                text.Span(" of ").FontSize(7);
                                text.TotalPages().FontSize(7);
                            });
                        });
                });
            });

            return doc.GeneratePdf();
        }

        // ════════════════════════════════════════════════════════════════
        //  3. Admin Payroll Excel — Protected (read-only)
        //  Columns: # | Name | Cal Days | Working | Present | Absent | Late | Basic | Absent Ded | Late Ded | Other Ded | Total Ded | Net | Status | Payment
        // ════════════════════════════════════════════════════════════════
        public byte[] GeneratePayrollExcel(
            List<(PayrollRecord Payroll, string EmployeeName)> records,
            int month, int year)
        {
            var monthName = MonthNames[month];
            int calDays   = DateTime.DaysInMonth(year, month);

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add($"Payroll {monthName} {year}");

            ws.Style.Protection.Locked = false;

            // Title
            ws.Cell("A1").Value = "Attendance Management System — Payroll Report";
            ws.Range("A1:O1").Merge().Style
                .Font.SetBold(true).Font.SetFontSize(14)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#1565C0"))
                .Font.SetFontColor(XLColor.White)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Protection.SetLocked(true);

            ws.Cell("A2").Value = $"Period: {monthName} {year}  |  Calendar Days: {calDays}";
            ws.Cell("G2").Value = $"Generated: {DateTime.Now:dd-MMM-yyyy HH:mm}";
            ws.Range("A2:F2").Merge().Style.Font.SetItalic(true).Font.SetFontSize(10);
            ws.Range("G2:O2").Merge().Style.Font.SetItalic(true).Font.SetFontSize(10)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            ws.Range("A2:O2").Style.Protection.Locked = true;

            // Headers — 15 columns
            var headers = new[]
            {
                "#", "Employee Name",
                "Calendar Days", "Working Days (Mon–Fri)",
                "Present Days", "Absent Days", "Late Days (½ day)",
                "Basic Salary (₹)", "Absent Deduction (₹)", "Late Deduction (₹)",
                "Other Deduction (₹)", "Total Deduction (₹)",
                "Net Salary (₹)", "Status", "Payment Status"
            };

            int headerRow = 4;
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(headerRow, i + 1);
                cell.Value = headers[i];
                cell.Style
                    .Font.SetBold(true)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#37474F"))
                    .Font.SetFontColor(XLColor.White)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                    .Protection.SetLocked(true);
            }

            // Data rows
            int dataRow = headerRow + 1;
            int rowNo   = 1;

            foreach (var (p, name) in records)
            {
                var isAlt     = rowNo % 2 == 0;
                var bgColor   = isAlt ? XLColor.FromHtml("#F5F5F5") : XLColor.White;
                var pmtStatus = p.Status == "paid" ? "Pay Completed" : "Pending";

                ws.Cell(dataRow, 1).Value  = rowNo;
                ws.Cell(dataRow, 2).Value  = name;
                ws.Cell(dataRow, 3).Value  = p.TotalCalendarDays;   // ← Calendar Days
                ws.Cell(dataRow, 4).Value  = p.TotalWorkingDays;
                ws.Cell(dataRow, 5).Value  = p.PresentDays;
                ws.Cell(dataRow, 6).Value  = p.AbsentDays;
                ws.Cell(dataRow, 7).Value  = p.LateDays;
                ws.Cell(dataRow, 8).Value  = (double)p.BasicSalary;
                ws.Cell(dataRow, 9).Value  = (double)p.AbsentDeduction;
                ws.Cell(dataRow, 10).Value = (double)p.LateDeduction;
                ws.Cell(dataRow, 11).Value = (double)p.ManualDeduction;
                ws.Cell(dataRow, 12).Value = (double)p.TotalDeduction;
                ws.Cell(dataRow, 13).Value = (double)p.NetSalary;
                ws.Cell(dataRow, 14).Value = p.Status.ToUpper();
                ws.Cell(dataRow, 15).Value = pmtStatus;

                // Currency format for salary columns
                for (int c = 8; c <= 13; c++)
                    ws.Cell(dataRow, c).Style.NumberFormat.Format = "#,##0.00";

                var range = ws.Range(dataRow, 1, dataRow, 15);
                range.Style
                    .Fill.SetBackgroundColor(bgColor)
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                    .Border.SetInsideBorder(XLBorderStyleValues.Hair)
                    .Protection.SetLocked(true);

                // Net salary in blue bold
                ws.Cell(dataRow, 13).Style.Font.SetBold(true)
                    .Font.SetFontColor(XLColor.FromHtml("#1565C0"));

                // Status color
                var statusColor = p.Status switch
                {
                    "paid"     => XLColor.FromHtml("#2E7D32"),
                    "approved" => XLColor.FromHtml("#1565C0"),
                    _          => XLColor.FromHtml("#E65100")
                };
                ws.Cell(dataRow, 14).Style.Font.SetFontColor(statusColor).Font.SetBold(true);

                var pmtColor = p.Status == "paid"
                    ? XLColor.FromHtml("#2E7D32") : XLColor.FromHtml("#E65100");
                ws.Cell(dataRow, 15).Style.Font.SetFontColor(pmtColor).Font.SetBold(true);

                dataRow++;
                rowNo++;
            }

            // Totals row
            int totRow = dataRow;
            ws.Cell(totRow, 1).Value = "TOTAL";
            ws.Range(totRow, 1, totRow, 7).Merge();
            ws.Cell(totRow, 1).Style
                .Font.SetBold(true)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#1565C0"))
                .Font.SetFontColor(XLColor.White)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Protection.SetLocked(true);

            foreach (var c in new[] { 8, 9, 10, 11, 12, 13 })
            {
                ws.Cell(totRow, c).FormulaA1 =
                    $"=SUM({ws.Cell(headerRow + 1, c).Address}:{ws.Cell(dataRow - 1, c).Address})";
                ws.Cell(totRow, c).Style
                    .Font.SetBold(true)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#1565C0"))
                    .Font.SetFontColor(XLColor.White)
                    .NumberFormat.Format = "#,##0.00"
                    .ToString();
                ws.Cell(totRow, c).Style.Protection.Locked = true;
            }

            // Column widths
            ws.Column(1).Width  = 5;
            ws.Column(2).Width  = 22;
            ws.Column(3).Width  = 14;  // Calendar Days
            ws.Column(4).Width  = 18;  // Working Days
            for (int c = 5; c <= 7;  c++) ws.Column(c).Width = 13;
            for (int c = 8; c <= 13; c++) ws.Column(c).Width = 18;
            ws.Column(14).Width = 13;
            ws.Column(15).Width = 16;

            ws.SheetView.FreezeRows(headerRow);

            // Protect — read only
            ws.Protect()
              .AllowElement(XLSheetProtectionElements.SelectLockedCells)
              .AllowElement(XLSheetProtectionElements.SelectUnlockedCells);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}