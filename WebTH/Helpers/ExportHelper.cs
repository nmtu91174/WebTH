using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using iTextSharp.text;
using iTextSharp.text.pdf;
using WebTH.Models;

namespace WebTH.Helpers
{
    public class ExportHelper
    {
        /// <summary>
        /// Export Analytics Data to PDF
        /// </summary>
        public static FileContentResult ExportToPDF(AnalyticsOverviewViewModel model, DateTime fromDate, DateTime toDate)
        {
            Document doc = new Document(PageSize.A4, 25, 25, 30, 30);
            MemoryStream ms = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(doc, ms);

            doc.Open();

            // Title
            Paragraph title = new Paragraph("Báo cáo Phân tích", new Font(Font.FontFamily.HELVETICA, 20, Font.BOLD));
            title.Alignment = Element.ALIGN_CENTER;
            doc.Add(title);

            // Date Range
            Paragraph dateRange = new Paragraph($"Khoảng thời gian báo cáo: {fromDate:yyyy-MM-dd} đến {toDate:yyyy-MM-dd}");
            dateRange.Alignment = Element.ALIGN_CENTER;
            doc.Add(dateRange);

            doc.Add(new Paragraph("\n"));

            // Summary Table
            PdfPTable summaryTable = new PdfPTable(2);
            summaryTable.WidthPercentage = 100;
            summaryTable.SpacingBefore = 10f;
            summaryTable.SpacingAfter = 10f;

            AddTableHeader(summaryTable, "Chỉ số", "Giá trị");

            AddTableRow(summaryTable, "Tổng doanh thu", $"₫{model.TotalRevenue:N2}");
            AddTableRow(summaryTable, "Doanh thu tháng này", $"₫{model.RevenueThisMonth:N2}");
            AddTableRow(summaryTable, "Doanh thu tháng trước", $"₫{model.RevenueLastMonth:N2}");
            AddTableRow(summaryTable, "Tăng trưởng doanh thu", $"{model.RevenueGrowth:N2}%");
            AddTableRow(summaryTable, "Tổng đơn hàng", model.TotalOrders.ToString());
            AddTableRow(summaryTable, "Đơn hàng tháng này", model.OrdersThisMonth.ToString());
            AddTableRow(summaryTable, "Đơn hàng tháng trước", model.OrdersLastMonth.ToString());
            AddTableRow(summaryTable, "Tăng trưởng đơn hàng", $"{model.OrderGrowth:N2}%");
            AddTableRow(summaryTable, "Giá trị trung bình đơn hàng", $"₫{model.AverageOrderValue:N2}");
            AddTableRow(summaryTable, "Tổng khách hàng", model.TotalCustomers.ToString());
            AddTableRow(summaryTable, "Khách hàng mới trong tháng", model.NewCustomersThisMonth.ToString());
            AddTableRow(summaryTable, "Tỷ lệ chuyển đổi", $"{model.ConversionRate:N2}%");
            AddTableRow(summaryTable, "Tỷ lệ giữ chân khách hàng", $"{model.CustomerRetentionRate:N2}%");

            doc.Add(summaryTable);

            // Category Performance Table
            doc.Add(new Paragraph("\n\nHiệu suất theo danh mục", new Font(Font.FontFamily.HELVETICA, 14, Font.BOLD)));
            PdfPTable categoryTable = new PdfPTable(3);
            categoryTable.WidthPercentage = 100;
            categoryTable.SpacingBefore = 10f;

            AddTableHeader(categoryTable, "Danh mục", "Doanh số", "Đơn hàng");

            if (model.CategoryPerformance != null && model.CategoryPerformance.Categories.Count > 0)
            {
                for (int i = 0; i < model.CategoryPerformance.Categories.Count; i++)
                {
                    AddTableRow(categoryTable, 
                        model.CategoryPerformance.Categories[i],
                        $"${model.CategoryPerformance.Sales[i]:N2}",
                        model.CategoryPerformance.OrderCount[i].ToString());
                }
            }

            doc.Add(categoryTable);

            // Top Products Table
            doc.Add(new Paragraph("\n\nSản phẩm hàng đầu", new Font(Font.FontFamily.HELVETICA, 14, Font.BOLD)));
            PdfPTable productsTable = new PdfPTable(4);
            productsTable.WidthPercentage = 100;
            productsTable.SpacingBefore = 10f;

            AddTableHeader(productsTable, "Sản phẩm", "Số lượng bán", "Doanh thu", "Đánh giá TB");

            if (model.TopProducts != null && model.TopProducts.ProductNames.Count > 0)
            {
                for (int i = 0; i < model.TopProducts.ProductNames.Count; i++)
                {
                    AddTableRow(productsTable,
                        model.TopProducts.ProductNames[i],
                        model.TopProducts.QuantitySold[i].ToString(),
                        $"${model.TopProducts.Revenue[i]:N2}",
                        $"{model.TopProducts.AverageRating[i]:N1}");
                }
            }

            doc.Add(productsTable);

            // Footer
            doc.Add(new Paragraph("\n\n"));
            Paragraph footer = new Paragraph($"Báo cáo tạo lúc: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            footer.Alignment = Element.ALIGN_CENTER;
            doc.Add(footer);

            doc.Close();

            byte[] buffer = ms.ToArray();
            ms.Close();

            return new FileContentResult(buffer, "application/pdf")
            {
                FileDownloadName = $"Analytics_Report_{fromDate:yyyy-MM-dd}_to_{toDate:yyyy-MM-dd}.pdf"
            };
        }

        /// <summary>
        /// Export Analytics Data to Excel
        /// </summary>
        public static FileContentResult ExportToExcel(AnalyticsOverviewViewModel model, DateTime fromDate, DateTime toDate)
        {
            DataSet ds = new DataSet();

            // Summary Sheet
            DataTable summaryTable = new DataTable("Summary");
            summaryTable.Columns.Add("Metric", typeof(string));
            summaryTable.Columns.Add("Value", typeof(string));

            summaryTable.Rows.Add("Khoảng thời gian báo cáo", $"{fromDate:yyyy-MM-dd} đến {toDate:yyyy-MM-dd}");
            summaryTable.Rows.Add("Tổng doanh thu", $"₫{model.TotalRevenue:N2}");
            summaryTable.Rows.Add("Doanh thu tháng này", $"₫{model.RevenueThisMonth:N2}");
            summaryTable.Rows.Add("Doanh thu tháng trước", $"₫{model.RevenueLastMonth:N2}");
            summaryTable.Rows.Add("Tăng trưởng doanh thu", $"{model.RevenueGrowth:N2}%");
            summaryTable.Rows.Add("Tổng đơn hàng", model.TotalOrders.ToString());
            summaryTable.Rows.Add("Đơn hàng tháng này", model.OrdersThisMonth.ToString());
            summaryTable.Rows.Add("Đơn hàng tháng trước", model.OrdersLastMonth.ToString());
            summaryTable.Rows.Add("Tăng trưởng đơn hàng", $"{model.OrderGrowth:N2}%");
            summaryTable.Rows.Add("Giá trị trung bình đơn hàng", $"₫{model.AverageOrderValue:N2}");
            summaryTable.Rows.Add("Tổng khách hàng", model.TotalCustomers.ToString());
            summaryTable.Rows.Add("Khách hàng mới trong tháng", model.NewCustomersThisMonth.ToString());
            summaryTable.Rows.Add("Tỷ lệ chuyển đổi", $"{model.ConversionRate:N2}%");
            summaryTable.Rows.Add("Tỷ lệ giữ chân khách hàng", $"{model.CustomerRetentionRate:N2}%");

            ds.Tables.Add(summaryTable);

            // Category Performance Sheet
            DataTable categoryTable = new DataTable("Categories");
            categoryTable.Columns.Add("Danh mục", typeof(string));
            categoryTable.Columns.Add("Doanh số", typeof(string));
            categoryTable.Columns.Add("Đơn hàng", typeof(int));

            if (model.CategoryPerformance != null && model.CategoryPerformance.Categories.Count > 0)
            {
                for (int i = 0; i < model.CategoryPerformance.Categories.Count; i++)
                {
                    categoryTable.Rows.Add(
                        model.CategoryPerformance.Categories[i],
                        $"₫{model.CategoryPerformance.Sales[i]:N2}",
                        model.CategoryPerformance.OrderCount[i]);
                }
            }

            ds.Tables.Add(categoryTable);

            // Top Products Sheet
            DataTable productsTable = new DataTable("Products");
            productsTable.Columns.Add("Sản phẩm", typeof(string));
            productsTable.Columns.Add("Số lượng bán", typeof(int));
            productsTable.Columns.Add("Doanh thu", typeof(string));
            productsTable.Columns.Add("Đánh giá TB", typeof(string));

            if (model.TopProducts != null && model.TopProducts.ProductNames.Count > 0)
            {
                for (int i = 0; i < model.TopProducts.ProductNames.Count; i++)
                {
                    productsTable.Rows.Add(
                        model.TopProducts.ProductNames[i],
                        model.TopProducts.QuantitySold[i],
                        $"₫{model.TopProducts.Revenue[i]:N2}",
                        $"{model.TopProducts.AverageRating[i]:N1}");
                }
            }

            ds.Tables.Add(productsTable);

            // Daily Revenue Sheet
            DataTable dailyTable = new DataTable("Daily Revenue");
            dailyTable.Columns.Add("Ngày", typeof(string));
            dailyTable.Columns.Add("Doanh thu", typeof(string));
            dailyTable.Columns.Add("Đơn hàng", typeof(int));

            if (model.DailyRevenue != null && model.DailyRevenue.Dates.Count > 0)
            {
                for (int i = 0; i < model.DailyRevenue.Dates.Count; i++)
                {
                    dailyTable.Rows.Add(
                        model.DailyRevenue.Dates[i].ToString("yyyy-MM-dd"),
                        $"₫{model.DailyRevenue.Revenue[i]:N2}",
                        model.DailyRevenue.OrderCount[i]);
                }
            }

            ds.Tables.Add(dailyTable);

            byte[] fileBytes = ExportDataSetToExcel(ds);

            return new FileContentResult(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"Analytics_Report_{fromDate:yyyy-MM-dd}_to_{toDate:yyyy-MM-dd}.xlsx"
            };
        }

        /// <summary>
        /// Convert DataSet to Excel bytes using Open XML
        /// </summary>
        private static byte[] ExportDataSetToExcel(DataSet ds)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Simple CSV-style export (can be enhanced with Open XML SDK)
                // For now, creating Excel-compatible XML
                string excelXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
                excelXml += "<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\">";

                foreach (DataTable table in ds.Tables)
                {
                    excelXml += $"<Worksheet ss:Name=\"{table.TableName}\">";
                    excelXml += "<Table>";

                    // Header Row
                    excelXml += "<Row>";
                    foreach (DataColumn col in table.Columns)
                    {
                        excelXml += $"<Cell><Data ss:Type=\"String\">{col.ColumnName}</Data></Cell>";
                    }
                    excelXml += "</Row>";

                    // Data Rows
                    foreach (DataRow row in table.Rows)
                    {
                        excelXml += "<Row>";
                        foreach (DataColumn col in table.Columns)
                        {
                            excelXml += $"<Cell><Data ss:Type=\"String\">{row[col]}</Data></Cell>";
                        }
                        excelXml += "</Row>";
                    }

                    excelXml += "</Table></Worksheet>";
                }

                excelXml += "</Workbook>";

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(excelXml);
                return buffer;
            }
        }

        private static void AddTableHeader(PdfPTable table, params string[] headers)
        {
            foreach (var header in headers)
            {
                PdfPCell cell = new PdfPCell(new Phrase(header, new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD)))
                {
                    BackgroundColor = new BaseColor(200, 200, 200),
                    Padding = 5
                };
                table.AddCell(cell);
            }
        }

        private static void AddTableRow(PdfPTable table, params string[] values)
        {
            foreach (var value in values)
            {
                PdfPCell cell = new PdfPCell(new Phrase(value, new Font(Font.FontFamily.HELVETICA, 9)))
                {
                    Padding = 5
                };
                table.AddCell(cell);
            }
        }
    }
}
