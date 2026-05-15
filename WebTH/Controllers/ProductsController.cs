using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebTH.Models;
using PagedList; // ← thêm dòng này vào đầu file

namespace WebTH.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Products
        //public ActionResult Index()
        //{
        //    // Lấy sản phẩm bán chạy
        //    var bestSellingProducts = db.Products
        //                                .Where(p => p.ViewCount > 10) // giả sử sản phẩm bán chạy có lượt xem > 100
        //                                .OrderByDescending(p => p.ViewCount)
        //                                .Take(25) // lấy 5 sản phẩm bán chạy
        //                                .ToList();

        //    // Lấy sản phẩm bán chậm
        //    var slowSellingProducts = db.Products
        //                                 .Where(p => p.ViewCount <= 10) // giả sử sản phẩm bán chậm có tồn kho > 100
        //                                 .OrderByDescending(p => p.ViewCount)
        //                                 .Take(25) // lấy 5 sản phẩm bán chậm
        //                                 .ToList();

        //    // Truyền dữ liệu vào View
        //    ViewBag.BestSellingProducts = bestSellingProducts;
        //    ViewBag.SlowSellingProducts = slowSellingProducts;

        //    return View();
        //}

        public ActionResult Index(int page = 1)
        {
            const int pageSize = 12; // 12 sản phẩm mỗi trang

            // ✅ Lấy TẤT CẢ sản phẩm đang active, không chia nhóm
            var allProducts = db.Products
                .Where(p => p.IsActive)
                .Include("ProductCategory")
                .OrderByDescending(p => p.CreatedDate)
                //.Take(30)
                // ❌ Bỏ .ToList()
                .ToPagedList(page, pageSize); // ✅ trả về IPagedList<Product>

            // ✅ Đổ toàn bộ vào BestSelling, SlowSelling để rỗng
            // → View vẫn dùng allProducts = bestSelling.Concat(slowSelling) như cũ, không cần sửa View

            ViewBag.BestSellingProducts = allProducts;// giờ là IPagedList
            ViewBag.SlowSellingProducts = new List<WebTH.Models.EF.Product>();

            return View();
        }

        public ActionResult Detail(string alias, int id)
        {
            var item = db.Products.Find(id);
            if(item != null)
            {
                db.Products.Attach(item);
                item.ViewCount = item.ViewCount + 1;
                db.Entry(item).Property(x => x.ViewCount).IsModified = true;
                db.SaveChanges();

                // Lấy danh sách sản phẩm liên quan (cùng Category)
                var relatedProducts = db.Products
                    .Where(p => p.ProductCategoryId == item.ProductCategoryId && p.Id != item.Id)
                    .Take(5) // Lấy tối đa 5 sản phẩm liên quan
                    .ToList();

                // Lấy khuyến cáo từ bác sĩ thú y
                var vetRecommendations = db.VetRecommendations
                    .Where(r => r.ProductId == id && r.IsApproved && r.VetClinic.IsActive)
                    .Include(r => r.VetClinic)
                    .Include(r => r.VetDoctor)
                    .OrderByDescending(r => r.TotalReferences)
                    .ToList();

                // Truyền sản phẩm chi tiết, sản phẩm liên quan và khuyến cáo vào View
                ViewBag.RelatedProducts = relatedProducts;
                ViewBag.VetRecommendations = vetRecommendations;
            }
            return View(item);
        }

        public ActionResult Category(string alias)
        {
            // Lấy danh mục sản phẩm theo alias
            var category = db.ProductCategories.FirstOrDefault(c => c.Alias == alias);
            if (category == null)
            {
                return HttpNotFound();
            }

            // Lấy các sản phẩm của danh mục này
            var products = db.Products
                .Where(p => p.ProductCategoryId == category.Id && p.IsActive)
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            ViewBag.CategoryName = category.Title;
            ViewBag.CategoryDescription = category.Description;
            ViewBag.SeoTitle = category.SeoTitle;
            ViewBag.SeoDescription = category.SeoDescription;
            ViewBag.SeoKeywords = category.SeoKeywords;

            return View(products);
        }

    }
}