using System;

namespace WebTH.Models
{
    public class CustomerReportItem
    {
        public string Customer { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime FirstOrder { get; set; }
        public DateTime LastOrder { get; set; }
    }
}
