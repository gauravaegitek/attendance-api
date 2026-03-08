// // ======================= Services/DocumentPdfService.cs =======================
// using QuestPDF.Fluent;
// using QuestPDF.Helpers;
// using QuestPDF.Infrastructure;
// using attendance_api.Models;

// namespace attendance_api.Services
// {
//     public interface IDocumentPdfService
//     {
//         byte[] GenerateDocumentListPdf(List<EmployeeDocument> documents, string employeeName);
//     }

//     public class DocumentPdfService : IDocumentPdfService
//     {
//         public DocumentPdfService()
//         {
//             QuestPDF.Settings.License = LicenseType.Community;
//         }

//         public byte[] GenerateDocumentListPdf(List<EmployeeDocument> documents, string employeeName)
//         {
//             var document = Document.Create(container =>
//             {
//                 container.Page(page =>
//                 {
//                     page.Size(PageSizes.A4);
//                     page.Margin(2, Unit.Centimetre);
//                     page.PageColor(Colors.White);
//                     page.DefaultTextStyle(x => x.FontSize(10));

//                     // ─── Header ───────────────────────────────────────────
//                     page.Header()
//                         .Height(90)
//                         .Background(Colors.Blue.Lighten3)
//                         .Padding(10)
//                         .Column(column =>
//                         {
//                             column.Item()
//                                 .Text("Attendance Management System")
//                                 .FontSize(20).Bold()
//                                 .FontColor(Colors.Blue.Darken2);

//                             column.Item()
//                                 .Text("Employee Document Report")
//                                 .FontSize(14).SemiBold();

//                             column.Item().PaddingTop(5).Row(row =>
//                             {
//                                 row.RelativeItem().Text($"Employee: {employeeName}").FontSize(10);
//                                 row.RelativeItem().AlignRight()
//                                     .Text($"Generated: {DateTime.Now:dd-MMM-yyyy HH:mm}")
//                                     .FontSize(10);
//                             });
//                         });

//                     // ─── Content ──────────────────────────────────────────
//                     page.Content()
//                         .PaddingVertical(10)
//                         .Column(column =>
//                         {
//                             column.Item().Table(table =>
//                             {
//                                 table.ColumnsDefinition(columns =>
//                                 {
//                                     columns.ConstantColumn(30);    // #
//                                     columns.RelativeColumn(1.5f);  // Document Type
//                                     columns.RelativeColumn(2.5f);  // File Name
//                                     columns.RelativeColumn(1.5f);  // Extension
//                                     columns.RelativeColumn(1.2f);  // Size
//                                     columns.RelativeColumn(1.8f);  // Uploaded At
//                                     columns.RelativeColumn(1.5f);  // Status
//                                 });

//                                 // Header row
//                                 table.Header(header =>
//                                 {
//                                     header.Cell().Element(CellStyle).Text("#").Bold();
//                                     header.Cell().Element(CellStyle).Text("Type").Bold();
//                                     header.Cell().Element(CellStyle).Text("File Name").Bold();
//                                     header.Cell().Element(CellStyle).Text("Extension").Bold();
//                                     header.Cell().Element(CellStyle).Text("Size (KB)").Bold();
//                                     header.Cell().Element(CellStyle).Text("Uploaded At").Bold();
//                                     header.Cell().Element(CellStyle).Text("Status").Bold();

//                                     static IContainer CellStyle(IContainer c) =>
//                                         c.Background(Colors.Grey.Lighten2)
//                                          .BorderBottom(1)
//                                          .BorderColor(Colors.Grey.Medium)
//                                          .Padding(5);
//                                 });

//                                 // Data rows
//                                 int rowNumber = 1;
//                                 foreach (var doc in documents)
//                                 {
//                                     var isAlt = rowNumber % 2 == 0;

//                                     // Status color
//                                     var statusColor = doc.VerifyStatus switch
//                                     {
//                                         "approved" => Colors.Green.Darken1,
//                                         "rejected" => Colors.Red.Darken1,
//                                         _          => Colors.Orange.Darken1   // pending
//                                     };

//                                     table.Cell().Element(c => DataCell(c, isAlt)).Text(rowNumber.ToString());
//                                     table.Cell().Element(c => DataCell(c, isAlt)).Text(doc.DocumentType);
//                                     table.Cell().Element(c => DataCell(c, isAlt)).Text(doc.FileName).FontSize(8);
//                                     table.Cell().Element(c => DataCell(c, isAlt)).Text(doc.FileExtension.ToUpper());
//                                     table.Cell().Element(c => DataCell(c, isAlt)).Text($"{doc.FileSize / 1024} KB");
//                                     table.Cell().Element(c => DataCell(c, isAlt)).Text(doc.UploadedAt.ToString("dd-MMM-yyyy"));
//                                     table.Cell().Element(c => DataCell(c, isAlt))
//                                          .Text(doc.VerifyStatus.ToUpper())
//                                          .FontColor(statusColor)
//                                          .Bold();

//                                     rowNumber++;
//                                 }

//                                 static IContainer DataCell(IContainer c, bool isAlt) =>
//                                     c.Background(isAlt ? Colors.Grey.Lighten4 : Colors.White)
//                                      .BorderBottom(1)
//                                      .BorderColor(Colors.Grey.Lighten2)
//                                      .Padding(5);
//                             });

//                             // Summary counts
//                             var pending  = documents.Count(d => d.VerifyStatus == "pending");
//                             var approved = documents.Count(d => d.VerifyStatus == "approved");
//                             var rejected = documents.Count(d => d.VerifyStatus == "rejected");

//                             column.Item().PaddingTop(15).Row(row =>
//                             {
//                                 row.RelativeItem()
//                                    .Background(Colors.Orange.Lighten4)
//                                    .Padding(8)
//                                    .Text($"Pending: {pending}")
//                                    .Bold().FontSize(10)
//                                    .FontColor(Colors.Orange.Darken2);

//                                 row.ConstantItem(10);

//                                 row.RelativeItem()
//                                    .Background(Colors.Green.Lighten4)
//                                    .Padding(8)
//                                    .Text($"Approved: {approved}")
//                                    .Bold().FontSize(10)
//                                    .FontColor(Colors.Green.Darken2);

//                                 row.ConstantItem(10);

//                                 row.RelativeItem()
//                                    .Background(Colors.Red.Lighten4)
//                                    .Padding(8)
//                                    .Text($"Rejected: {rejected}")
//                                    .Bold().FontSize(10)
//                                    .FontColor(Colors.Red.Darken2);

//                                 row.ConstantItem(10);

//                                 row.RelativeItem()
//                                    .Background(Colors.Blue.Lighten4)
//                                    .Padding(8)
//                                    .Text($"Total: {documents.Count}")
//                                    .Bold().FontSize(10)
//                                    .FontColor(Colors.Blue.Darken2);
//                             });
//                         });

//                     // ─── Footer ───────────────────────────────────────────
//                     page.Footer()
//                         .Height(40)
//                         .Background(Colors.Grey.Lighten3)
//                         .Padding(8)
//                         .Column(column =>
//                         {
//                             column.Item().AlignCenter().Text(text =>
//                             {
//                                 text.Span("Page ");
//                                 text.CurrentPageNumber();
//                                 text.Span(" of ");
//                                 text.TotalPages();
//                             });

//                             column.Item().AlignCenter()
//                                 .Text($"Generated on: {DateTime.Now:dd-MMM-yyyy HH:mm:ss}")
//                                 .FontSize(7);

//                             column.Item().AlignCenter()
//                                 .Text("** CONFIDENTIAL **")
//                                 .FontSize(7).Italic();
//                         });
//                 });
//             });

//             return document.GeneratePdf();
//         }
//     }
// }











// ======================= Services/DocumentPdfService.cs =======================
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using attendance_api.Models;

namespace attendance_api.Services
{
    public interface IDocumentPdfService
    {
        byte[] GenerateDocumentListPdf(List<EmployeeDocument> documents, string employeeName);
    }

    public class DocumentPdfService : IDocumentPdfService
    {
        public DocumentPdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateDocumentListPdf(List<EmployeeDocument> documents, string employeeName)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    // ─── Header ───────────────────────────────────────────
                    page.Header()
                        .Height(70)
                        .Background(Colors.Blue.Lighten3)
                        .Padding(10)
                        .Column(col =>
                        {
                            col.Item()
                               .Text("Attendance Management System")
                               .FontSize(16).Bold()
                               .FontColor(Colors.Blue.Darken2);

                            col.Item()
                               .Text("Employee Document Report")
                               .FontSize(12).SemiBold();

                            col.Item().PaddingTop(3).Row(row =>
                            {
                                row.RelativeItem()
                                   .Text($"Employee: {employeeName}")
                                   .FontSize(9);
                                row.RelativeItem().AlignRight()
                                   .Text($"Generated: {DateTime.Now:dd-MMM-yyyy HH:mm}")
                                   .FontSize(9);
                            });
                        });

                    // ─── Content ──────────────────────────────────────────
                    page.Content()
                        .PaddingVertical(8)
                        .Column(col =>
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(25);   // #
                                    columns.RelativeColumn(2);    // Type
                                    columns.RelativeColumn(3.5f); // File Name
                                    columns.RelativeColumn(1.2f); // Ext
                                    columns.RelativeColumn(1.2f); // Size
                                    columns.RelativeColumn(2);    // Uploaded At
                                    columns.RelativeColumn(1.5f); // Status
                                });

                                // ── Header row ────────────────────────────
                                table.Header(header =>
                                {
                                    void H(string text) =>
                                        header.Cell()
                                              .Background(Colors.Grey.Lighten2)
                                              .BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                              .Padding(4)
                                              .Text(text).Bold().FontSize(9);

                                    H("#");
                                    H("Type");
                                    H("File Name");
                                    H("Extension");
                                    H("Size");
                                    H("Uploaded At");
                                    H("Status");
                                });

                                // ── Data rows ─────────────────────────────
                                if (documents.Count == 0)
                                {
                                    table.Cell().ColumnSpan(7)
                                         .Padding(10).AlignCenter()
                                         .Text("No documents found.")
                                         .Italic().FontColor(Colors.Grey.Medium);
                                }
                                else
                                {
                                    int rowNo = 1;
                                    foreach (var doc in documents)
                                    {
                                        var isAlt = rowNo % 2 == 0;
                                        var bg    = isAlt ? Colors.Grey.Lighten4 : Colors.White;

                                        var statusColor = (doc.VerifyStatus ?? "pending").ToLower() switch
                                        {
                                            "approved" => Colors.Green.Darken1,
                                            "rejected" => Colors.Red.Darken1,
                                            _          => Colors.Orange.Darken2
                                        };

                                        void D(string text)
                                        {
                                            table.Cell()
                                                 .Background(bg)
                                                 .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                                 .Padding(4)
                                                 .Text(text).FontSize(8);
                                        }

                                        void DS(string text, string color)
                                        {
                                            table.Cell()
                                                 .Background(bg)
                                                 .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                                 .Padding(4)
                                                 .Text(text).FontSize(8).Bold().FontColor(color);
                                        }

                                        D(rowNo.ToString());
                                        D(doc.DocumentType ?? "-");
                                        D(doc.FileName ?? "-");
                                        D(doc.FileExtension?.ToUpper() ?? "-");
                                        D($"{doc.FileSize / 1024} KB");
                                        D(doc.UploadedAt.ToString("dd-MMM-yyyy"));
                                        DS((doc.VerifyStatus ?? "pending").ToUpper(), statusColor);

                                        rowNo++;
                                    }
                                }
                            });

                            // ── Summary boxes ─────────────────────────────
                            var pending  = documents.Count(d => (d.VerifyStatus ?? "pending") == "pending");
                            var approved = documents.Count(d => d.VerifyStatus == "approved");
                            var rejected = documents.Count(d => d.VerifyStatus == "rejected");

                            col.Item().PaddingTop(12).Row(row =>
                            {
                                void Box(string label, int count, string bg, string fg)
                                {
                                    row.RelativeItem()
                                       .Background(bg).Padding(6)
                                       .Text($"{label}: {count}")
                                       .Bold().FontSize(9).FontColor(fg);
                                }

                                Box("Pending",  pending,         Colors.Orange.Lighten3, Colors.Orange.Darken2);
                                row.ConstantItem(8);
                                Box("Approved", approved,        Colors.Green.Lighten3,  Colors.Green.Darken2);
                                row.ConstantItem(8);
                                Box("Rejected", rejected,        Colors.Red.Lighten3,    Colors.Red.Darken2);
                                row.ConstantItem(8);
                                Box("Total",    documents.Count, Colors.Blue.Lighten3,   Colors.Blue.Darken2);
                            });
                        });

                    // ─── Footer ───────────────────────────────────────────
                    // ✅ Fix: FontSize inside text spans, NOT chained after .Text()
                    page.Footer()
                        .Height(35)
                        .Background(Colors.Grey.Lighten3)
                        .Padding(6)
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
                               .Text($"Generated: {DateTime.Now:dd-MMM-yyyy HH:mm:ss} | ** CONFIDENTIAL **")
                               .FontSize(7).Italic();
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}