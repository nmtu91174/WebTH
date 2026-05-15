using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebTH.Models;
using WebTH.Models.EF;
using WebTH.Helpers;

namespace WebTH.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AnalyticsController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Analytics
        public ActionResult Index()
        {
            var model = GetAnalyticsOverview(DateTime.Now.AddMonths(-12), DateTime.Now);
            return View(model);
        }

        // GET: Analytics by Date Range
        [HttpPost]
        public ActionResult GetAnalyticsByDateRange(DateTime fromDate, DateTime toDate)
        {
            var model = GetAnalyticsOverview(fromDate, toDate);
            return Json(new { success = true, data = model }, JsonRequestBehavior.AllowGet);
        }

        // Main Analytics Overview
        private AnalyticsOverviewViewModel GetAnalyticsOverview(DateTime fromDate, DateTime toDate)
        {
            var model = new AnalyticsOverviewViewModel();
            var orders = db.Orders.Where(o => o.CreatedDate >= fromDate && o.CreatedDate <= toDate).ToList();
            var allOrders = db.Orders.ToList();

            // Precompute date values to avoid LINQ-to-Entities translation of DateTime methods
            DateTime now = DateTime.Now;
            int currentMonth = now.Month;
            int currentYear = now.Year;
            DateTime oneMonthAgo = now.AddMonths(-1);
            int lastMonth = oneMonthAgo.Month;
            int lastMonthYear = oneMonthAgo.Year;

            // Revenue Metrics
            model.TotalRevenue = allOrders.Sum(o => o.TotalAmount);
            model.RevenueThisMonth = orders.Where(o => o.CreatedDate.Month == currentMonth && o.CreatedDate.Year == currentYear).Sum(o => o.TotalAmount);
            model.RevenueLastMonth = allOrders.Where(o => o.CreatedDate.Month == lastMonth && o.CreatedDate.Year == lastMonthYear).Sum(o => o.TotalAmount);
            model.RevenueGrowth = model.RevenueLastMonth > 0 ? ((model.RevenueThisMonth - model.RevenueLastMonth) / model.RevenueLastMonth) * 100 : 0;

            // Order Metrics
            model.TotalOrders = allOrders.Count();
            model.OrdersThisMonth = orders.Where(o => o.CreatedDate.Month == currentMonth && o.CreatedDate.Year == currentYear).Count();
            model.OrdersLastMonth = allOrders.Where(o => o.CreatedDate.Month == lastMonth && o.CreatedDate.Year == lastMonthYear).Count();
            model.OrderGrowth = model.OrdersLastMonth > 0 ? ((model.OrdersThisMonth - model.OrdersLastMonth) / (decimal)model.OrdersLastMonth) * 100 : 0;

            // Product & Customer Metrics
            model.TotalProducts = db.Products.Count();
            model.TotalCustomers = db.Users.Count();
            
            // Load to list to avoid LINQ-to-Entities issues
            var ordersThisMonth = db.Orders
                .Where(o => o.CreatedDate >= oneMonthAgo && o.CreatedDate <= now)
                .ToList();
            model.NewCustomersThisMonth = ordersThisMonth
                .Select(o => o.CustomerName)
                .Distinct()
                .Count();
            model.AverageOrderValue = model.TotalOrders > 0 ? model.TotalRevenue / model.TotalOrders : 0;

            // Rates
            model.ConversionRate = model.TotalCustomers > 0 ? (decimal)model.TotalOrders / model.TotalCustomers * 100 : 0;
            var lastMonthOrders = allOrders.Where(o => o.CreatedDate.Month == lastMonth && o.CreatedDate.Year == lastMonthYear).Count();
            model.CustomerRetentionRate = lastMonthOrders > 0 ? ((decimal)model.OrdersThisMonth / lastMonthOrders) * 100 : 0;

            // Charts Data
            model.DailyRevenue = GetDailyRevenueChartData(fromDate, toDate);
            model.CategoryPerformance = GetCategoryPerformanceData(fromDate, toDate);
            model.TopProducts = GetTopProductsData(fromDate, toDate, 10);
            model.CustomerAnalytics = GetCustomerAnalyticsData(fromDate, toDate);
            model.TopCities = GetTopCitiesData(fromDate, toDate, out var cityCounts);
            model.CityOrderCounts = cityCounts;
            GetOrderStatusDistribution(out var statusNames, out var statusCounts);
            model.OrderStatusNames = statusNames;
            model.OrderStatusCounts = statusCounts;
            GetPaymentMethodDistribution(out var paymentNames, out var paymentCounts);
            model.PaymentMethodNames = paymentNames;
            model.PaymentMethodCounts = paymentCounts;

            return model;
        }

        // Daily Revenue Chart
        private DailyRevenueData GetDailyRevenueChartData(DateTime fromDate, DateTime toDate)
        {
            var data = new DailyRevenueData
            {
                Dates = new List<DateTime>(),
                Revenue = new List<decimal>(),
                OrderCount = new List<int>()
            };

            // Load dữ liệu một lần vào memory để tránh LINQ-to-Entities issues
            var allOrders = db.Orders
                .Where(o => o.CreatedDate >= fromDate && o.CreatedDate <= toDate)
                .ToList();

            for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
            {
                data.Dates.Add(date);
                var nextDate = date.AddDays(1);
                var dayOrders = allOrders.Where(o => o.CreatedDate >= date && o.CreatedDate < nextDate).ToList();
                data.Revenue.Add(dayOrders.Sum(o => o.TotalAmount));
                data.OrderCount.Add(dayOrders.Count());
            }

            return data;
        }

        // Category Performance
        private CategoryPerformanceData GetCategoryPerformanceData(DateTime fromDate, DateTime toDate)
        {
            var data = new CategoryPerformanceData
            {
                Categories = new List<string>(),
                Sales = new List<decimal>(),
                OrderCount = new List<int>()
            };

            try
            {
                var categories = db.ProductCategories.ToList();
                
                // Load OrderDetails with Order relationship
                var orderDetailsWithOrders = db.OrderDetails.ToList();
                var filteredOrderDetails = orderDetailsWithOrders
                    .Where(od => od.Order != null 
                        && od.Order.CreatedDate >= fromDate 
                        && od.Order.CreatedDate <= toDate)
                    .ToList();

                foreach (var category in categories)
                {
                    var categoryProductIds = db.Products
                        .Where(p => p.ProductCategoryId == category.Id)
                        .Select(p => p.Id)
                        .ToList();

                    var categoryOrderDetails = filteredOrderDetails
                        .Where(od => categoryProductIds.Contains(od.ProductId))
                        .ToList();

                    data.Categories.Add(category.Title);
                    data.Sales.Add(categoryOrderDetails.Sum(od => od.Price * od.Quantity));
                    data.OrderCount.Add(categoryOrderDetails.Count());
                }
            }
            catch
            {
                // Log error nếu cần
            }

            return data;
        }

        // Top Products
        private ProductPerformanceData GetTopProductsData(DateTime fromDate, DateTime toDate, int topCount = 10)
        {
            var data = new ProductPerformanceData
            {
                ProductNames = new List<string>(),
                QuantitySold = new List<int>(),
                Revenue = new List<decimal>(),
                AverageRating = new List<decimal>()
            };

            try
            {
                // Load OrderDetails with Order relationship
                var orderDetailsList = db.OrderDetails.ToList();

                var topProducts = orderDetailsList
                    .Where(od => od.Order != null && od.Order.CreatedDate >= fromDate && od.Order.CreatedDate <= toDate)
                    .GroupBy(od => od.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        TotalQuantity = g.Sum(x => x.Quantity),
                        TotalRevenue = g.Sum(x => x.Price * x.Quantity)
                    })
                    .OrderByDescending(x => x.TotalRevenue)
                    .Take(topCount)
                    .ToList();

                // Get products with reviews
                var productIds = topProducts.Select(p => p.ProductId).ToList();
                var products = db.Products.Where(p => productIds.Contains(p.Id)).ToList();

                foreach (var item in topProducts)
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product != null)
                    {
                        data.ProductNames.Add(product.Title);
                        data.QuantitySold.Add(item.TotalQuantity);
                        data.Revenue.Add(item.TotalRevenue);
                        var avgRating = product.Reviews != null && product.Reviews.Count > 0 
                            ? (decimal)product.Reviews.Average(r => r.Rate) 
                            : 0;
                        data.AverageRating.Add(avgRating);
                    }
                }
            }
            catch
            {
                // Log error nếu cần
            }

            return data;
        }

        // Customer Analytics
        private CustomerAnalyticsData GetCustomerAnalyticsData(DateTime fromDate, DateTime toDate)
        {
            var data = new CustomerAnalyticsData
            {
                TopCustomers = new List<string>(),
                CustomerSpending = new List<decimal>()
            };

            try
            {
                // Precompute date to avoid LINQ-to-Entities translation issues
                DateTime oneMonthAgo = DateTime.Now.AddMonths(-1);

                data.TotalCustomers = db.Users.Count();
                
                // Load dữ liệu vào memory trước
                var allOrders = db.Orders.ToList();
                data.NewCustomersThisMonth = allOrders
                    .Where(o => o.CreatedDate >= oneMonthAgo)
                    .Select(o => o.CustomerName)
                    .Distinct()
                    .Count();

                var topCustomers = allOrders
                    .Where(o => o.CreatedDate >= fromDate && o.CreatedDate <= toDate)
                    .GroupBy(o => o.CustomerName)
                    .Select(g => new
                    {
                        CustomerName = g.Key,
                        TotalSpending = g.Sum(x => x.TotalAmount)
                    })
                    .OrderByDescending(x => x.TotalSpending)
                    .Take(10)
                    .ToList();

                foreach (var customer in topCustomers)
                {
                    data.TopCustomers.Add(customer.CustomerName ?? "Unknown");
                    data.CustomerSpending.Add(customer.TotalSpending);
                }
            }
            catch
            {
                // Log error nếu cần
            }

            return data;
        }

        // Top Cities
        private List<string> GetTopCitiesData(DateTime fromDate, DateTime toDate, out List<int> cityCounts)
        {
            cityCounts = new List<int>();
            var topCities = new List<string>();

            try
            {
                // Load dữ liệu vào memory
                var cityData = db.Orders
                    .Where(o => o.CreatedDate >= fromDate && o.CreatedDate <= toDate)
                    .ToList()
                    .GroupBy(o => o.Address)
                    .Select(g => new
                    {
                        City = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToList();

                foreach (var city in cityData)
                {
                    topCities.Add(!string.IsNullOrEmpty(city.City) ? city.City : "Unknown");
                    cityCounts.Add(city.Count);
                }
            }
            catch
            {
                // Log error nếu cần
            }

            return topCities;
        }

        // Order Status Distribution
        private void GetOrderStatusDistribution(out List<string> statusNames, out List<int> statusCounts)
        {
            statusNames = new List<string>();
            statusCounts = new List<int>();

            try
            {
                var allOrders = db.Orders.ToList();
                
                // Get all unique status values
                var uniqueStatuses = allOrders.Select(o => o.Status).Distinct().OrderBy(s => s).ToList();
                
                // If we have specific statuses, use them
                if (uniqueStatuses.Count > 0)
                {
                    // Status mapping from Order view:
                    // 1 = Chưa thanh toán (Pending)
                    // 2 = Đã thanh toán (Processing)
                    // 3 = Đang giao hàng (Shipping)
                    // 4 = Hoàn thành (Delivered)
                    var statusMap = new Dictionary<int, string>
                    {
                        { 1, "Pending" },
                        { 2, "Processing" },
                        { 3, "Shipping" },
                        { 4, "Delivered" }
                    };
                    
                    foreach (var status in uniqueStatuses)
                    {
                        if (statusMap.ContainsKey(status))
                        {
                            statusNames.Add(statusMap[status]);
                        }
                        else
                        {
                            statusNames.Add($"Status_{status}");
                        }
                        statusCounts.Add(allOrders.Count(o => o.Status == status));
                    }
                }
                else
                {
                    // No data
                    for (int i = 0; i < 5; i++) statusCounts.Add(0);
                }
            }
            catch
            {
                for (int i = 0; i < 5; i++) statusCounts.Add(0);
            }
        }

        // Payment Method Distribution
        private void GetPaymentMethodDistribution(out List<string> paymentNames, out List<int> paymentCounts)
        {
            paymentNames = new List<string>();
            paymentCounts = new List<int>();

            try
            {
                var allOrders = db.Orders.ToList();
                
                // Get all unique payment method values
                var uniquePaymentMethods = allOrders.Select(o => o.TypePayment).Distinct().OrderBy(p => p).ToList();
                
                // If we have specific payment methods, use them
                if (uniquePaymentMethods.Count > 0)
                {
                    // Payment Method mapping from Order view:
                    // 0 = Bank Transfer (Chuyển khoản)
                    // 1 = COD (Cash on Delivery)
                    var paymentMap = new Dictionary<int, string>
                    {
                        { 0, "Bank Transfer" },
                        { 1, "COD" }
                    };
                    
                    foreach (var payment in uniquePaymentMethods)
                    {
                        if (paymentMap.ContainsKey(payment))
                        {
                            paymentNames.Add(paymentMap[payment]);
                        }
                        else
                        {
                            paymentNames.Add($"Payment_{payment}");
                        }
                        paymentCounts.Add(allOrders.Count(o => o.TypePayment == payment));
                    }
                }
                else
                {
                    // No data
                    for (int i = 0; i < 5; i++) paymentCounts.Add(0);
                }
            }
            catch
            {
                for (int i = 0; i < 5; i++) paymentCounts.Add(0);
            }
        }

        // GET: Export Dashboard Data as JSON
        public ActionResult ExportData(DateTime fromDate, DateTime toDate)
        {
            var data = GetAnalyticsOverview(fromDate, toDate);
            return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
        }

        // GET: Test DB connection - returns counts from real database
        public ActionResult TestDb()
        {
            try
            {
                var counts = new
                {
                    Orders = db.Orders.Count(),
                    Products = db.Products.Count(),
                    Users = db.Users.Count()
                };

                return Json(new { success = true, data = counts }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Revenue Report
        public ActionResult RevenueReport()
        {
            var fromDate = DateTime.Now.AddMonths(-1);
            var toDate = DateTime.Now;
            var model = GetAnalyticsOverview(fromDate, toDate);
            return View(model);
        }

        // GET: Customer Report
        public ActionResult CustomerReport()
        {
            var data = db.Orders
                .GroupBy(o => o.CustomerName)
                .Select(g => new WebTH.Models.CustomerReportItem
                {
                    Customer = g.Key,
                    OrderCount = g.Count(),
                    TotalSpent = g.Sum(x => x.TotalAmount),
                    FirstOrder = g.Min(x => x.CreatedDate),
                    LastOrder = g.Max(x => x.CreatedDate)
                })
                .OrderByDescending(x => x.TotalSpent)
                .ToList();

            return View(data);
        }

        // GET: Product Report
        public ActionResult ProductReport()
        {
            var data = db.OrderDetails
                .GroupBy(od => od.ProductId)
                .Select(g => new WebTH.Models.ProductReportItem
                {
                    Product = g.Select(x => x.Product.Title).FirstOrDefault() ?? "Unknown",
                    QuantitySold = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Price * x.Quantity),
                    AveragePrice = g.Average(x => x.Price)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToList();
            return View(data);
        }

        // GET: Export to PDF
        public FileResult ExportPDF(DateTime fromDate, DateTime toDate)
        {
            var model = GetAnalyticsOverview(fromDate, toDate);
            return ExportHelper.ExportToPDF(model, fromDate, toDate);
        }

        // GET: Export to Excel
        public FileResult ExportExcel(DateTime fromDate, DateTime toDate)
        {
            var model = GetAnalyticsOverview(fromDate, toDate);
            return ExportHelper.ExportToExcel(model, fromDate, toDate);
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
