using Microsoft.AspNet.Identity;
using PagedList;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebTH.Models;
using WebTH.Models.EF;

namespace WebTH.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Employee")]


    public class OrderController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Order
        public ActionResult Index(int? page)
        {
            var items = db.Orders.OrderByDescending(x => x.CreatedDate).ToList();

            if (page == null)
            {
                page = 1;
            }
            var pageNumber = page ?? 1;
            var pageSize = 10;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = pageNumber;

            return View(items.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult View(int id)
        {
            var item = db.Orders.Find(id);
            if (item == null) return HttpNotFound();

            // LẤY DANH SÁCH SHIPPER TRUYỀN RA VIEW
            var roleManager = new Microsoft.AspNet.Identity.RoleManager<Microsoft.AspNet.Identity.EntityFramework.IdentityRole>(new Microsoft.AspNet.Identity.EntityFramework.RoleStore<Microsoft.AspNet.Identity.EntityFramework.IdentityRole>(db));
            var userManager = new Microsoft.AspNet.Identity.UserManager<ApplicationUser>(new Microsoft.AspNet.Identity.EntityFramework.UserStore<ApplicationUser>(db));

            // Tìm Role Shipper
            var shipperRole = roleManager.FindByName("Shipper");
            if (shipperRole != null)
            {
                var shippers = userManager.Users.Where(x => x.Roles.Any(r => r.RoleId == shipperRole.Id)).ToList();
                ViewBag.ShipperList = new SelectList(shippers, "Id", "FullName"); // Hiển thị tên Shipper
            }

            return View(item);
        }

        public ActionResult Partial_SanPham(int id)
        {
            var items = db.OrderDetails.Where(x => x.OrderId == id).ToList();
            return PartialView(items);

        }


        //[HttpPost]

        //public ActionResult UpdateTT(int id, int trangthai)
        //{
        //    var item = db.Orders.Find(id);
        //    if (item != null)
        //    {
        //        db.Orders.Attach(item);
        //        item.Status = trangthai;
        //        db.Entry(item).Property(x => x.Status).IsModified = true;

        //        db.SaveChanges();
        //        return Json(new { message = "Success", Success = true });
        //    }
        //    return Json(new { message = "Unsuccess", Success = false });

        //}

        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public ActionResult UpdateTT(int id, int trangthai)
        {
            var order = db.Orders.Find(id);
            if (order != null && !order.IsLocked)
            {
                order.PreviousStatus = order.Status;
                order.Status = trangthai;

                // Admin duyệt đơn
                if (trangthai == 2)
                {
                    order.AgentId = User.Identity.GetUserId();
                }

                // Ghi Log lịch sử
                db.OrderHistories.Add(new OrderHistory
                {
                    OrderId = order.Id,
                    StatusId = trangthai,
                    Role = "Admin",
                    Note = trangthai == 2 ? "Admin đã duyệt đơn hàng" : "Admin cập nhật trạng thái",
                    CreatedBy = User.Identity.GetUserId(),
                    CreatedDate = DateTime.Now
                });

                db.SaveChanges();
                return Json(new { Success = true, msg = "Cập nhật thành công!" });
            }
            return Json(new { Success = false, msg = "Đơn hàng đã khóa hoặc không tồn tại!" });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.Orders.Find(id);
            if (item != null)
            {
                db.Orders.Remove(item);
                db.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        public ActionResult ExportInvoice(int id)
        {
            var order = db.Orders.Include("OrderDetails.Product").FirstOrDefault(x => x.Id == id);
            if (order == null)
            {
                return HttpNotFound();
            }

            return new ViewAsPdf("ExportInvoice", order)
            {
                FileName = $"HoaDon_{order.Code}.pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait
            };
        }
        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public ActionResult AssignShipper(int id, string shipperId)
        {
            var order = db.Orders.Find(id);
            if (order != null && order.Status == 2) // Chỉ gán khi Status = 2 (Đã duyệt)
            {
                order.ShipperId = shipperId;

                // Lưu lịch sử
                db.OrderHistories.Add(new OrderHistory
                {
                    OrderId = order.Id,
                    StatusId = 2, // Vẫn giữ status 2, chỉ là Assigned
                    Role = "system",
                    Note = "Admin đã phân công cho Shipper: " + shipperId,
                    CreatedBy = User.Identity.GetUserId(),
                    CreatedDate = DateTime.Now
                });

                db.SaveChanges();
                return Json(new { success = true, msg = "Phân công Shipper thành công!" });
            }
            return Json(new { success = false, msg = "Đơn hàng phải ở trạng thái ĐÃ DUYỆT mới được phân công!" });
        }

    }


}
