using Microsoft.AspNet.Identity;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebTH.Models;
using WebTH.Models.EF;

namespace WebTH.Areas.Admin.Controllers
{
    [Authorize(Roles = "Shipper")] // BẮT BUỘC PHẢI LÀ TÀI KHOẢN SHIPPER MỚI VÀO ĐƯỢC
    public class ShipperController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // Màn hình chính của Shipper (Chỉ hiện đơn của ông ấy)
        public ActionResult Index()
        {
            string currentUserId = User.Identity.GetUserId();
            var orders = db.Orders
                .Where(x => x.ShipperId == currentUserId && x.Status >= 2 && x.Status <= 6)
                .OrderByDescending(x => x.CreatedDate)
                .ToList();

            return View(orders);
        }

        // Hàm xử lý khi Shipper bấm các nút (Chuyển luồng)
        [HttpPost]
        public ActionResult UpdateWorkflow(int orderId, int nextStatus, string note, string failedReason, HttpPostedFileBase proofImage)
        {
            var order = db.Orders.Find(orderId);
            string currentUserId = User.Identity.GetUserId();

            // 1. Kiểm tra an toàn: Đơn có phải của ông này không? Đơn đã bị khóa chưa?
            if (order == null || order.ShipperId != currentUserId || order.IsLocked)
                return Json(new { success = false, msg = "Đơn hàng đã khóa hoặc không thuộc quyền của bạn." });

            // 2. NGUYÊN TẮC VÀNG: KHÔNG NHẢY BƯỚC
            bool isValidStep =
                (order.Status == 2 && nextStatus == 3) ||  // Admin gán(2) -> Shipper nhận đơn(3)
                (order.Status == 3 && nextStatus == 4) ||  // Đã nhận(3) -> Đã lấy hàng(4)
                (order.Status == 4 && nextStatus == 5) ||  // Đã lấy hàng(4) -> Giao thành công(5)
                (order.Status == 4 && nextStatus == 6);    // Đã lấy hàng(4) -> Giao thất bại(6)

            if (!isValidStep)
                return Json(new { success = false, msg = "Lỗi luồng xử lý! Vui lòng refresh lại trang." });

            // 3. Xử lý lưu ảnh Bằng chứng (Proof)
            string imagePath = null;
            if (nextStatus >= 4) // Bắt đầu từ Lấy hàng trở đi phải có ảnh
            {
                if (proofImage == null || proofImage.ContentLength == 0)
                    return Json(new { success = false, msg = "Bắt buộc phải chụp ảnh xác thực!" });

                // Tạo thư mục nếu chưa có
                string directory = Server.MapPath("~/Uploads/ShipperProofs/");
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                string fileName = DateTime.Now.Ticks + "_" + Path.GetFileName(proofImage.FileName);
                string path = Path.Combine(directory, fileName);
                proofImage.SaveAs(path);
                imagePath = "/Uploads/ShipperProofs/" + fileName;
            }

            // 4. Cập nhật Trạng thái đơn hàng
            order.PreviousStatus = order.Status; // Lưu lại trạng thái cũ
            order.Status = nextStatus;
            string actionNote = "";

            switch (nextStatus)
            {
                case 3:
                    actionNote = "Shipper xác nhận Nhận Đơn";
                    break;
                case 4:
                    order.PickupProof = imagePath; // Lưu ảnh lấy hàng
                    actionNote = "Shipper xác nhận đã Lấy Hàng";
                    break;
                case 5:
                    order.DeliveryProof = imagePath; // Lưu ảnh giao hàng
                    order.DeliveredAt = DateTime.Now;
                    order.IsLocked = true; // KHÓA ĐƠN VĨNH VIỄN
                    actionNote = "Giao hàng thành công";
                    break;
                case 6:
                    order.DeliveryProof = imagePath; // Lưu ảnh chứng minh thất bại
                    order.FailedReason = failedReason;
                    order.FailedAt = DateTime.Now;
                    order.IsLocked = true; // KHÓA ĐƠN
                    actionNote = "Giao thất bại: " + note;
                    break;
            }

            // 5. NGUYÊN TẮC VÀNG: BẮT BUỘC CÓ ORDER HISTORY
            db.OrderHistories.Add(new OrderHistory
            {
                OrderId = order.Id,
                StatusId = nextStatus,
                Role = "Shipper",
                Note = actionNote,
                CreatedBy = currentUserId,
                CreatedDate = DateTime.Now
            });

            db.SaveChanges();
            return Json(new { success = true, msg = "Cập nhật trạng thái thành công!" });
        }
    }
}