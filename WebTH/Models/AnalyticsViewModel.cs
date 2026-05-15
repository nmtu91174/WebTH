using System;
using System.Collections.Generic;

namespace WebTH.Models
{
    // Chart Data Models
    public class SalesChartData
    {
        public List<string> Labels { get; set; }
        public List<decimal> Data { get; set; }
        public string Title { get; set; }
    }

    public class CategoryPerformanceData
    {
        public List<string> Categories { get; set; }
        public List<decimal> Sales { get; set; }
        public List<int> OrderCount { get; set; }
    }

    public class CustomerAnalyticsData
    {
        public int TotalCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public List<string> TopCustomers { get; set; }
        public List<decimal> CustomerSpending { get; set; }
    }

    public class ProductPerformanceData
    {
        public List<string> ProductNames { get; set; }
        public List<int> QuantitySold { get; set; }
        public List<decimal> Revenue { get; set; }
        public List<decimal> AverageRating { get; set; }
    }

    public class DailyRevenueData
    {
        public List<DateTime> Dates { get; set; }
        public List<decimal> Revenue { get; set; }
        public List<int> OrderCount { get; set; }
    }

    public class AnalyticsOverviewViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueLastMonth { get; set; }
        public decimal RevenueGrowth { get; set; }

        public int TotalOrders { get; set; }
        public int OrdersThisMonth { get; set; }
        public int OrdersLastMonth { get; set; }
        public decimal OrderGrowth { get; set; }

        public int TotalProducts { get; set; }
        public int TotalCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public decimal AverageOrderValue { get; set; }

        public decimal ConversionRate { get; set; }
        public decimal CustomerRetentionRate { get; set; }

        // Chart Data
        public DailyRevenueData DailyRevenue { get; set; }
        public CategoryPerformanceData CategoryPerformance { get; set; }
        public ProductPerformanceData TopProducts { get; set; }
        public CustomerAnalyticsData CustomerAnalytics { get; set; }

        public List<string> TopCities { get; set; }
        public List<int> CityOrderCounts { get; set; }

        // Status Distribution
        public List<string> OrderStatusNames { get; set; }
        public List<int> OrderStatusCounts { get; set; }

        // Payment Method Distribution
        public List<string> PaymentMethodNames { get; set; }
        public List<int> PaymentMethodCounts { get; set; }
    }

    public class AnalyticsFilterViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int ProductCategory { get; set; }
        public string CustomerSegment { get; set; } // New, Returning, VIP
    }

    public class ExportAnalyticsViewModel
    {
        public DateTime ExportDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public AnalyticsOverviewViewModel Data { get; set; }
    }

    // Pie/Donut Chart Data
    public class PieChartData
    {
        public List<string> Labels { get; set; }
        public List<int> Data { get; set; }
        public List<string> Colors { get; set; }
    }
}
