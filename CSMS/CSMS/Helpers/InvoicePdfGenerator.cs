using CSMS.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace CSMS.Helpers
{
    public static class InvoicePdfGenerator
    {
        public static void Generate(Invoice invoice, string filePath)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);

                    page.Header().Text($"Invoice {invoice.InvoiceNumber}").FontSize(20).Bold();

                    page.Content().Column(column =>
                    {
                        column.Item().Text($"Date: {invoice.IssueDate:yyyy-MM-dd}");
                        column.Item().Text($"Customer ID: {invoice.CustomerId}");
                        column.Item().Text($"Repair Order ID: {invoice.RepairOrderId}");

                        column.Item().LineHorizontal(1);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(200); // Description
                                columns.RelativeColumn();   // Qty
                                columns.RelativeColumn();   // Unit price
                                columns.RelativeColumn();   // Total
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Description");
                                header.Cell().Text("Qty");
                                header.Cell().Text("Unit Price");
                                header.Cell().Text("Total");
                            });

                            foreach (var item in invoice.Items)
                            {
                                table.Cell().Text(item.Description);
                                table.Cell().Text(item.Quantity.ToString());
                                table.Cell().Text(item.UnitPrice.ToString("C"));
                                table.Cell().Text(item.TotalPrice.ToString("C"));
                            }
                        });

                        column.Item().LineHorizontal(1);

                        column.Item().Text($"Subtotal: {invoice.Subtotal:C}");
                        column.Item().Text($"Tax: {invoice.Tax:C}");
                        column.Item().Text($"Total: {invoice.TotalAmount:C}");
                    });

                    page.Footer().AlignCenter().Text("Thank you for your business!");
                });
            })
            .GeneratePdf(filePath);
        }
    }
}
