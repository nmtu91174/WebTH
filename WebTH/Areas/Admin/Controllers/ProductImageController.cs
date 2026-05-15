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
    public class ProductImageController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/ProductImages
        public ActionResult Index(int id)
        {
            ViewBag.ProductId = id;
            var items = db.ProductImages.Where(x => x.ProductId == id).ToList();
            return View(items);
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddImage(int productId, string url)
        {
            db.ProductImages.Add(new ProductImage
            {
                ProductId = productId,
                Image = url,
                IsDefault = false
            });
            db.SaveChanges();
            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.ProductImages.Find(id);
            db.ProductImages.Remove(item);
            db.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult SetDefault(int id)
        {
            var item = db.ProductImages.Find(id);
            if (item != null)
            {
                // Lấy tất cả ảnh sản phẩm
                var allImages = db.ProductImages.Where(x => x.ProductId == item.ProductId).ToList();

                // Đặt tất cả ảnh không phải mặc định
                foreach (var img in allImages)
                {
                    img.IsDefault = false;
                }

                // Đặt ảnh hiện tại làm mặc định
                item.IsDefault = true;

                // Cập nhật ảnh sản phẩm chính
                var product = db.Products.Find(item.ProductId);
                if (product != null)
                {
                    product.Image = item.Image;
                }

                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}
