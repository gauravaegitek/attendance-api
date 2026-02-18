using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using attendance_api.DTOs;

namespace attendance_api.Services
{
    public interface IPdfService
    {
        byte[] GenerateUserSummaryPdf(List<AttendanceSummaryDto> data, string userName, string role, DateTime? fromDate, DateTime? toDate);
        byte[] GenerateAdminSummaryPdf(List<AttendanceSummaryDto> data, DateTime? fromDate, DateTime? toDate);
    }

    public class PdfService : IPdfService
    {
        public PdfService()
        {
            // Set QuestPDF license
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateUserSummaryPdf(List<AttendanceSummaryDto> data, string userName, string role, DateTime? fromDate, DateTime? toDate)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Height(100)
                        .Background(Colors.Blue.Lighten3)
                        .Padding(10)
                        .Column(column =>
                        {
                            column.Item().Text("Attendance Management System")
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);

                            column.Item().Text("Personal Attendance Summary Report")
                                .FontSize(14)
                                .SemiBold();

                            column.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text($"Name: {userName}").FontSize(10);
                                row.RelativeItem().Text($"Role: {role.ToUpper()}").FontSize(10);
                            });

                            if (fromDate.HasValue && toDate.HasValue)
                            {
                                column.Item().Text($"Period: {fromDate.Value:dd-MMM-yyyy} to {toDate.Value:dd-MMM-yyyy}")
                                    .FontSize(10);
                            }
                        });

                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(30);  // #
                                    columns.RelativeColumn(2);   // Date
                                    columns.RelativeColumn(1.5f); // In Time
                                    columns.RelativeColumn(1.5f); // Out Time
                                    columns.RelativeColumn(3);   // In Location
                                    columns.RelativeColumn(3);   // Out Location
                                    columns.RelativeColumn(1);   // Hours
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("#").Bold();
                                    header.Cell().Element(CellStyle).Text("Date").Bold();
                                    header.Cell().Element(CellStyle).Text("In Time").Bold();
                                    header.Cell().Element(CellStyle).Text("Out Time").Bold();
                                    header.Cell().Element(CellStyle).Text("In Location").Bold();
                                    header.Cell().Element(CellStyle).Text("Out Location").Bold();
                                    header.Cell().Element(CellStyle).Text("Hours").Bold();

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container
                                            .Background(Colors.Grey.Lighten2)
                                            .BorderBottom(1)
                                            .BorderColor(Colors.Grey.Medium)
                                            .Padding(5);
                                    }
                                });

                                // Data rows
                                int rowNumber = 1;
                                foreach (var item in data)
                                {
                                    var isAlternate = rowNumber % 2 == 0;

                                    table.Cell().Element(c => DataCellStyle(c, isAlternate)).Text(rowNumber.ToString());
                                    table.Cell().Element(c => DataCellStyle(c, isAlternate)).Text(item.AttendanceDate.ToString("dd-MMM-yyyy"));
                                    table.Cell().Element(c => DataCellStyle(c, isAlternate)).Text(item.InTime ?? "-");
                                    table.Cell().Element(c => DataCellStyle(c, isAlternate)).Text(item.OutTime ?? "-");
                                    table.Cell().Element(c => DataCellStyle(c, isAlternate)).Text(item.InLocation ?? "-").FontSize(8);
                                    table.Cell().Element(c => DataCellStyle(c, isAlternate)).Text(item.OutLocation ?? "-").FontSize(8);
                                    table.Cell().Element(c => DataCellStyle(c, isAlternate)).Text(item.TotalHours?.ToString("0.00") ?? "-");

                                    rowNumber++;
                                }

                                static IContainer DataCellStyle(IContainer container, bool isAlternate)
                                {
                                    return container
                                        .Background(isAlternate ? Colors.Grey.Lighten4 : Colors.White)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .Padding(5);
                                }
                            });

                            // Summary
                            var totalHours = data.Sum(x => x.TotalHours ?? 0);
                            column.Item().PaddingTop(10).Row(row =>
                            {
                                row.RelativeItem().Text("");
                                row.ConstantItem(200).AlignRight().Text($"Total Hours: {totalHours:0.00}")
                                    .Bold()
                                    .FontSize(12);
                            });

                            column.Item().PaddingTop(5).Text($"Total Records: {data.Count}")
                                .FontSize(10)
                                .Italic();
                        });

                    page.Footer()
                        .Height(50)
                        .Background(Colors.Grey.Lighten3)
                        .Padding(10)
                        .Column(column =>
                        {
                            column.Item().AlignCenter().Text(text =>
                            {
                                text.Span("Page ");
                                text.CurrentPageNumber();
                                text.Span(" of ");
                                text.TotalPages();
                            });

                            column.Item().AlignCenter().Text($"Generated on: {DateTime.Now:dd-MMM-yyyy HH:mm:ss}")
                                .FontSize(8);

                            column.Item().AlignCenter().Text("** CONFIDENTIAL **")
                                .FontSize(8)
                                .Italic();
                        });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateAdminSummaryPdf(List<AttendanceSummaryDto> data, DateTime? fromDate, DateTime? toDate)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header()
                        .Height(80)
                        .Background(Colors.Blue.Lighten3)
                        .Padding(10)
                        .Column(column =>
                        {
                            column.Item().Text("Attendance Management System")
                                .FontSize(18)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);

                            column.Item().Text("Admin - Comprehensive Attendance Report")
                                .FontSize(12)
                                .SemiBold();

                            if (fromDate.HasValue && toDate.HasValue)
                            {
                                column.Item().PaddingTop(3).Text($"Period: {fromDate.Value:dd-MMM-yyyy} to {toDate.Value:dd-MMM-yyyy}")
                                    .FontSize(10);
                            }
                        });

                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(25);  // #
                                    columns.RelativeColumn(2);   // Name
                                    columns.RelativeColumn(1.5f); // Role
                                    columns.RelativeColumn(1.5f); // Date
                                    columns.RelativeColumn(1.2f); // In Time
                                    columns.RelativeColumn(1.2f); // Out Time
                                    columns.RelativeColumn(2.5f); // In Location
                                    columns.RelativeColumn(2.5f); // Out Location
                                    columns.RelativeColumn(1);   // Hours
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("#").Bold();
                                    header.Cell().Element(CellStyle).Text("User Name").Bold();
                                    header.Cell().Element(CellStyle).Text("Role").Bold();
                                    header.Cell().Element(CellStyle).Text("Date").Bold();
                                    header.Cell().Element(CellStyle).Text("In Time").Bold();
                                    header.Cell().Element(CellStyle).Text("Out Time").Bold();
                                    header.Cell().Element(CellStyle).Text("In Location").Bold();
                                    header.Cell().Element(CellStyle).Text("Out Location").Bold();
                                    header.Cell().Element(CellStyle).Text("Hours").Bold();

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container
                                            .Background(Colors.Grey.Lighten2)
                                            .BorderBottom(1)
                                            .BorderColor(Colors.Grey.Medium)
                                            .Padding(4);
                                    }
                                });

                                // Data rows
                                int rowNumber = 1;
                                foreach (var item in data)
                                {
                                    var isAlternate = rowNumber % 2 == 0;

                                    table.Cell().Element(c => DataCellStyle(c, isAlternate)).Text(rowNumber.ToString());
                                    table.Cell().Element(c => DataCellStyle(c, isAlternate)).Text(item.UserName);
                                    table.Cell().Element(c => DataCellStyle(c, isAlternate)).Text(item.Role.ToUpper());
                                    table.Cell().Element(c => DataCellStyle(c, isAlternate)).Text(item.AttendanceDate.ToString("dd-MMM-yy"));
                                    table.Cell().Element(c => DataCellStyle(c, isAlternate)).Text(item.InTime ?? "-");
                                    table.Cell().Element(c => DataCellStyle(c, isAlternate)).Text(item.OutTime ?? "-");
                                    table.Cell().Element(c => DataCellStyle(c, isAlternate)).Text(item.InLocation ?? "-").FontSize(7);
                                    table.Cell().Element(c => DataCellStyle(c, isAlternate)).Text(item.OutLocation ?? "-").FontSize(7);
                                    table.Cell().Element(c => DataCellStyle(c, isAlternate)).Text(item.TotalHours?.ToString("0.00") ?? "-");

                                    rowNumber++;
                                }

                                static IContainer DataCellStyle(IContainer container, bool isAlternate)
                                {
                                    return container
                                        .Background(isAlternate ? Colors.Grey.Lighten4 : Colors.White)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .Padding(4);
                                }
                            });

                            // Summary
                            var totalHours = data.Sum(x => x.TotalHours ?? 0);
                            column.Item().PaddingTop(10).Row(row =>
                            {
                                row.RelativeItem().Text($"Total Records: {data.Count}")
                                    .FontSize(10)
                                    .Bold();
                                row.ConstantItem(200).AlignRight().Text($"Total Hours: {totalHours:0.00}")
                                    .Bold()
                                    .FontSize(11);
                            });
                        });

                    page.Footer()
                        .Height(40)
                        .Background(Colors.Grey.Lighten3)
                        .Padding(8)
                        .Column(column =>
                        {
                            column.Item().AlignCenter().Text(text =>
                            {
                                text.Span("Page ");
                                text.CurrentPageNumber();
                                text.Span(" of ");
                                text.TotalPages();
                            });

                            column.Item().AlignCenter().Text($"Generated on: {DateTime.Now:dd-MMM-yyyy HH:mm:ss}")
                                .FontSize(7);
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}
