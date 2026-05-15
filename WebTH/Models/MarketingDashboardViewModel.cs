using System;
using System.Collections.Generic;

namespace WebTH.Models
{
    public class MarketingDashboardViewModel
    {
        public DateTime SnapshotAt { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueLastMonth { get; set; }
        public decimal RevenueTarget { get; set; }
        public decimal ForecastNextMonth { get; set; }
        public decimal ForecastConfidence { get; set; }
        public int OrdersThisMonth { get; set; }
        public int OrdersLastMonth { get; set; }
        public int ActiveCustomers30Days { get; set; }
        public int NewCustomers30Days { get; set; }
        public decimal RetentionRate { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int LeadsThisMonth { get; set; }
        public int LeadsFromContacts { get; set; }
        public int LeadsFromSubscribes { get; set; }
        public int QualifiedLeadsThisMonth { get; set; }
        public int ConvertedOrdersThisMonth { get; set; }
        public int ReturningCustomersThisMonth { get; set; }
        public decimal PipelineCoverage { get; set; }
        public List<MonthlyTrendPoint> RevenueTrend { get; set; } = new List<MonthlyTrendPoint>();
        public List<ChannelFocusPoint> ChannelFocus { get; set; } = new List<ChannelFocusPoint>();
        public List<CategoryMomentumPoint> CategoryMomentum { get; set; } = new List<CategoryMomentumPoint>();
        public List<ActionPriorityItem> ActionPlaybook { get; set; } = new List<ActionPriorityItem>();
    }

    public class MonthlyTrendPoint
    {
        public string MonthLabel { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }

    public class ChannelFocusPoint
    {
        public string Channel { get; set; }
        public int SignalCount { get; set; }
        public decimal ContributionPercent { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal PotentialScore { get; set; }
        public decimal ProjectedRevenue { get; set; }
    }

    public class CategoryMomentumPoint
    {
        public string Category { get; set; }
        public decimal Revenue { get; set; }
        public decimal GrowthPercent { get; set; }
        public decimal MomentumScore { get; set; }
    }

    public class ActionPriorityItem
    {
        public string Objective { get; set; }
        public string Owner { get; set; }
        public string ExpectedImpact { get; set; }
        public string Horizon { get; set; }
        public string Priority { get; set; }
    }
}
