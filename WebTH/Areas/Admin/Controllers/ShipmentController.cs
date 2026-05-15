using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using WebTH.Models;
using WebTH.Models.EF;

namespace WebTH.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class ShipmentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // Danh sách đơn cần xử lý vận chuyển
        public ActionResult Index()
        {
            var orders = db.Orders
                .Include("Shipments.Status")
                .Include("Shipments.Provider")
                .OrderByDescending(x => x.CreatedDate)
                .ToList();
            return View(orders);
        }

        // Tạo vận đơn cho đơn hàng
        public ActionResult Create(int id)
        {
            //int orderId = id;

            var order = db.Orders.Find(id);
            if (order == null) return HttpNotFound();

            ViewBag.Order = order;
            ViewBag.Providers = db.ShippingProviders.Where(p => p.IsActive).ToList();
            ViewBag.Statuses = db.OrderStatuses.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Shipment model)
        {
            model.CreatedDate = DateTime.Now;
            model.StatusId = db.OrderStatuses
                                  .Where(s => s.Code == "pending")
                                  .Select(s => s.Id).FirstOrDefault();
            db.Shipments.Add(model);

            // Cập nhật trạng thái đơn hàng → Đang giao (3)
            var order = db.Orders.Find(model.OrderId);
            if (order != null) order.Status = 3;

            // Ghi lịch sử đầu tiên
            db.ShipmentHistories.Add(new ShipmentHistory
            {
                ShipmentId = 0, // sẽ set sau SaveChanges
                StatusId = model.StatusId,
                Note = "Vận đơn được tạo",
                CreatedBy = User.Identity.Name,
                CreatedDate = DateTime.Now
            });

            db.SaveChanges();

            // Gán ShipmentId thực vào history vừa tạo
            var history = db.ShipmentHistories
                            .OrderByDescending(h => h.Id).First();
            history.ShipmentId = model.Id;
            db.SaveChanges();

            return RedirectToAction("Detail", new { id = model.Id });
        }

        // Chi tiết vận đơn + timeline
        public ActionResult Detail(int id)
        {
            var shipment = db.Shipments
                .Include("Order.OrderDetails.Product")
                .Include("Provider")
                .Include("Status")
                .Include("Histories.Status")
                .FirstOrDefault(s => s.Id == id);

            if (shipment == null) return HttpNotFound();

            ViewBag.Statuses = db.OrderStatuses.ToList();
            return View(shipment);
        }

        // Cập nhật trạng thái vận chuyển
        [HttpPost]
        public ActionResult UpdateStatus(int shipmentId, int statusId,
                                         string note, string location)
        {
            var shipment = db.Shipments.Find(shipmentId);
            if (shipment == null)
                return Json(new { Success = false, msg = "Không tìm thấy vận đơn" });

            shipment.StatusId = statusId;

            var statusCode = db.OrderStatuses.Find(statusId)?.Code;
            if (statusCode == "delivered")
            {
                shipment.DeliveredDate = DateTime.Now;
                // Tự động hoàn thành đơn hàng
                var order = db.Orders.Find(shipment.OrderId);
                if (order != null) order.Status = 4;
            }
            if (statusCode == "shipping")
                shipment.ShippedDate = DateTime.Now;

            db.ShipmentHistories.Add(new ShipmentHistory
            {
                ShipmentId = shipmentId,
                StatusId = statusId,
                Note = note,
                Location = location,
                CreatedBy = User.Identity.Name,
                CreatedDate = DateTime.Now
            });

            db.SaveChanges();
            return Json(new { Success = true, msg = "Cập nhật thành công" });
        }

        // Tra cứu vận đơn (dành cho khách hàng gọi qua AJAX)
        [AllowAnonymous]
        public ActionResult Track(string code)
        {
            var shipment = db.Shipments
                .Include("Histories.Status")
                .Include("Status")
                .Include("Provider")
                .FirstOrDefault(s => s.TrackingCode == code
                                  || s.Order.Code == code);

            if (shipment == null)
                return Json(new { Found = false }, JsonRequestBehavior.AllowGet);

            var data = new
            {
                Found = true,
                TrackingCode = shipment.TrackingCode,
                Provider = shipment.Provider?.Name,
                Status = shipment.Status?.Name,
                StatusColor = shipment.Status?.Color,
                Estimated = shipment.EstimatedDelivery?.ToString("dd/MM/yyyy"),
                Histories = shipment.Histories
                    .OrderByDescending(h => h.CreatedDate)
                    .Select(h => new {
                        Status = h.Status?.Name,
                        Color = h.Status?.Color,
                        Note = h.Note,
                        Location = h.Location,
                        Date = h.CreatedDate.ToString("dd/MM/yyyy HH:mm")
                    }).ToList()
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}