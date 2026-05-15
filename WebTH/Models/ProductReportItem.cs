using System;

namespace WebTH.Models
{
    public class ProductReportItem
    {
        public string Product { get; set; }
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AveragePrice { get; set; }
    }
}
