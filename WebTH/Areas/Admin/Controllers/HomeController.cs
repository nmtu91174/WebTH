using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebTH.Models;
using WebTH.Models.EF;

namespace WebTH.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Home
        public ActionResult Index()
        {
            var model = BuildMarketingDashboard();
            return View(model);
        }

        private MarketingDashboardViewModel BuildMarketingDashboard()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfLastMonth = startOfMonth.AddMonths(-1);
            var startOf30Days = now.AddDays(-30);
            var startOf60Days = now.AddDays(-60);

            var allOrders = db.Orders.ToList();
            var ordersThisMonth = allOrders
                .Where(x => x.CreatedDate >= startOfMonth && x.CreatedDate <= now)
                .ToList();
            var ordersLastMonth = allOrders
                .Where(x => x.CreatedDate >= startOfLastMonth && x.CreatedDate < startOfMonth)
                .ToList();

            var revenueThisMonth = ordersThisMonth.Sum(x => x.TotalAmount);
            var revenueLastMonth = ordersLastMonth.Sum(x => x.TotalAmount);
            var averageOrderValue = ordersThisMonth.Count > 0 ? revenueThisMonth / ordersThisMonth.Count : 0m;

            var recentCustomers = allOrders
                .Where(x => x.CreatedDate >= startOf30Days && x.CreatedDate <= now)
                .Select(GetCustomerKey)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            var previousCustomers = allOrders
                .Where(x => x.CreatedDate >= startOf60Days && x.CreatedDate < startOf30Days)
                .Select(GetCustomerKey)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            var thisMonthCustomers = ordersThisMonth
                .Select(GetCustomerKey)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            var lastMonthCustomers = ordersLastMonth
                .Select(GetCustomerKey)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            var retainedCustomers = lastMonthCustomers.Intersect(thisMonthCustomers).Count();
            var retentionRate = lastMonthCustomers.Count > 0
                ? (retainedCustomers * 100m) / lastMonthCustomers.Count
                : 0m;

            var leadsFromContacts = db.Contacts.Count(x => x.CreatedDate >= startOfMonth && x.CreatedDate <= now);
            var leadsFromSubscribers = db.Subscribes.Count(x => x.CreatedDate >= startOfMonth && x.CreatedDate <= now);
            var leadsThisMonth = leadsFromContacts + leadsFromSubscribers;

            var convertedOrders = ordersThisMonth.Count(x => x.Status >= 2);
            var qualifiedLeads = leadsThisMonth;
            var pipelineCoverage = qualifiedLeads > 0 ? (convertedOrders * 100m) / qualifiedLeads : 0m;
            pipelineCoverage = Clamp(pipelineCoverage, 0m, 100m);

            var revenueTarget = Math.Max(revenueLastMonth * 1.18m, revenueThisMonth * 1.08m);

            var forecastNextMonth = revenueThisMonth + ((revenueThisMonth - revenueLastMonth) * 0.65m);
            if (forecastNextMonth <= 0m)
            {
                forecastNextMonth = revenueTarget;
            }

            var forecastConfidence = 72m;
            if (ordersThisMonth.Count >= ordersLastMonth.Count)
            {
                forecastConfidence += 7m;
            }
            if (retentionRate >= 35m)
            {
                forecastConfidence += 6m;
            }
            if (leadsThisMonth > 0)
            {
                forecastConfidence += 3m;
            }
            forecastConfidence = Clamp(forecastConfidence, 45m, 95m);

            var model = new MarketingDashboardViewModel
            {
                SnapshotAt = now,
                RevenueThisMonth = revenueThisMonth,
                RevenueLastMonth = revenueLastMonth,
                RevenueTarget = revenueTarget,
                ForecastNextMonth = forecastNextMonth,
                ForecastConfidence = forecastConfidence,
                OrdersThisMonth = ordersThisMonth.Count,
                OrdersLastMonth = ordersLastMonth.Count,
                ActiveCustomers30Days = recentCustomers.Count,
                NewCustomers30Days = recentCustomers.Except(previousCustomers).Count(),
                RetentionRate = retentionRate,
                AverageOrderValue = averageOrderValue,
                LeadsThisMonth = leadsThisMonth,
                LeadsFromContacts = leadsFromContacts,
                LeadsFromSubscribes = leadsFromSubscribers,
                QualifiedLeadsThisMonth = qualifiedLeads,
                ConvertedOrdersThisMonth = convertedOrders,
                ReturningCustomersThisMonth = retainedCustomers,
                PipelineCoverage = pipelineCoverage
            };

            model.RevenueTrend = BuildRevenueTrend(now, allOrders);
            model.CategoryMomentum = BuildCategoryMomentum(ordersThisMonth, ordersLastMonth);
            model.ChannelFocus = BuildChannelFocus(model);
            model.ActionPlaybook = BuildPlaybook(model);

            return model;
        }

        private List<MonthlyTrendPoint> BuildRevenueTrend(DateTime now, List<Order> allOrders)
        {
            var trend = new List<MonthlyTrendPoint>();
            var firstMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
            for (var i = 0; i < 6; i++)
            {
                var monthStart = firstMonth.AddMonths(i);
                var monthEnd = monthStart.AddMonths(1);
                var monthOrders = allOrders
                    .Where(x => x.CreatedDate >= monthStart && x.CreatedDate < monthEnd)
                    .ToList();

                trend.Add(new MonthlyTrendPoint
                {
                    MonthLabel = monthStart.ToString("MM/yyyy"),
                    Revenue = monthOrders.Sum(x => x.TotalAmount),
                    Orders = monthOrders.Count
                });
            }
            return trend;
        }

        private List<CategoryMomentumPoint> BuildCategoryMomentum(List<Order> ordersThisMonth, List<Order> ordersLastMonth)
        {
            var monthOrderIds = ordersThisMonth.Select(x => x.Id).ToList();
            var lastMonthOrderIds = ordersLastMonth.Select(x => x.Id).ToList();
            var orderDetails = db.OrderDetails.ToList();
            var products = db.Products.Select(x => new { x.Id, x.ProductCategoryId }).ToList();
            var categories = db.ProductCategories.Select(x => new { x.Id, x.Title }).ToList();

            var thisMonthRevenue = (
                from detail in orderDetails
                join product in products on detail.ProductId equals product.Id
                where monthOrderIds.Contains(detail.OrderId)
                group detail by product.ProductCategoryId into grouped
                select new
                {
                    CategoryId = grouped.Key,
                    Revenue = grouped.Sum(x => x.Price * x.Quantity)
                }).ToList();

            var lastMonthRevenue = (
                from detail in orderDetails
                join product in products on detail.ProductId equals product.Id
                where lastMonthOrderIds.Contains(detail.OrderId)
                group detail by product.ProductCategoryId into grouped
                select new
                {
                    CategoryId = grouped.Key,
                    Revenue = grouped.Sum(x => x.Price * x.Quantity)
                }).ToDictionary(x => x.CategoryId, x => x.Revenue);

            var momentum = thisMonthRevenue
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .Select(x =>
                {
                    var prev = lastMonthRevenue.ContainsKey(x.CategoryId) ? lastMonthRevenue[x.CategoryId] : 0m;
                    var growth = prev > 0m ? ((x.Revenue - prev) / prev) * 100m : (x.Revenue > 0 ? 100m : 0m);
                    var score = Clamp((growth * 0.45m) + 55m, 0m, 100m);
                    var categoryName = categories.FirstOrDefault(c => c.Id == x.CategoryId)?.Title ?? "Nhóm khác";

                    return new CategoryMomentumPoint
                    {
                        Category = categoryName,
                        Revenue = x.Revenue,
                        GrowthPercent = growth,
                        MomentumScore = score
                    };
                })
                .ToList();

            if (!momentum.Any())
            {
                momentum.Add(new CategoryMomentumPoint
                {
                    Category = "Chưa có dữ liệu",
                    Revenue = 0m,
                    GrowthPercent = 0m,
                    MomentumScore = 0m
                });
            }

            return momentum;
        }

        private static List<ChannelFocusPoint> BuildChannelFocus(MarketingDashboardViewModel model)
        {
            var signals = new List<ChannelFocusPoint>
            {
                new ChannelFocusPoint { Channel = "Khách quay lại (CRM)", SignalCount = model.ReturningCustomersThisMonth },
                new ChannelFocusPoint { Channel = "Khách mới 30 ngày", SignalCount = model.NewCustomers30Days },
                new ChannelFocusPoint { Channel = "Liên hệ tư vấn", SignalCount = model.LeadsFromContacts },
                new ChannelFocusPoint { Channel = "Đăng ký nhận tin", SignalCount = model.LeadsFromSubscribes }
            };

            var totalSignals = signals.Sum(x => x.SignalCount);
            var baseRevenue = model.RevenueThisMonth > 0 ? model.RevenueThisMonth : model.RevenueTarget;
            var conversionBase = model.QualifiedLeadsThisMonth > 0 ? model.QualifiedLeadsThisMonth : 1;

            foreach (var signal in signals)
            {
                signal.ContributionPercent = totalSignals > 0
                    ? (signal.SignalCount * 100m) / totalSignals
                    : 0m;

                signal.ConversionRate = (signal.SignalCount * 100m) / conversionBase;
                signal.ConversionRate = Clamp(signal.ConversionRate, 0m, 100m);

                signal.ProjectedRevenue = baseRevenue * (signal.ContributionPercent / 100m);
                signal.PotentialScore = Clamp((signal.ContributionPercent * 0.6m) + (signal.ConversionRate * 0.4m), 0m, 100m);
            }

            return signals
                .OrderByDescending(x => x.PotentialScore)
                .ToList();
        }

        private static List<ActionPriorityItem> BuildPlaybook(MarketingDashboardViewModel model)
        {
            var playbook = new List<ActionPriorityItem>
            {
                new ActionPriorityItem
                {
                    Objective = "Mở rộng nhóm khách quay lại bằng CRM automation",
                    Owner = "Nhóm Growth",
                    ExpectedImpact = "+" + Math.Round(model.RetentionRate * 0.2m, 1) + "% retention",
                    Horizon = "30 ngày",
                    Priority = model.RetentionRate < 35m ? "Cao" : "Trung bình"
                },
                new ActionPriorityItem
                {
                    Objective = "Tăng đầu tư nhóm sản phẩm có momentum cao",
                    Owner = "Nhóm Performance",
                    ExpectedImpact = "+15% lead chất lượng",
                    Horizon = "2 tuần",
                    Priority = model.PipelineCoverage < 65m ? "Cao" : "Trung bình"
                },
                new ActionPriorityItem
                {
                    Objective = "Ra mắt combo cross-sell theo hành vi mua gần đây",
                    Owner = "Merchandising",
                    ExpectedImpact = "+10% AOV",
                    Horizon = "45 ngày",
                    Priority = model.AverageOrderValue < 450000m ? "Cao" : "Trung bình"
                },
                new ActionPriorityItem
                {
                    Objective = "Xây dựng content pillar cho 2 danh mục tăng trưởng mạnh",
                    Owner = "Nhóm Brand",
                    ExpectedImpact = "+8% organic conversion",
                    Horizon = "60 ngày",
                    Priority = "Trung bình"
                }
            };

            return playbook;
        }

        private static string GetCustomerKey(Order order)
        {
            if (!string.IsNullOrWhiteSpace(order.CustomerId))
            {
                return order.CustomerId.Trim().ToLowerInvariant();
            }

            if (!string.IsNullOrWhiteSpace(order.Email))
            {
                return order.Email.Trim().ToLowerInvariant();
            }

            if (!string.IsNullOrWhiteSpace(order.Phone))
            {
                return order.Phone.Trim();
            }

            return (order.CustomerName ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static decimal Clamp(decimal value, decimal min, decimal max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
