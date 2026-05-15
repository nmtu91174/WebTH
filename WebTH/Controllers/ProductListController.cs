using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebTH.Models;
using PagedList;

namespace WebTH.Controllers
{
    public class ProductListController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: ProductList
        public ActionResult Index(int page = 1)
        {
            const int pageSize = 12;
            var items = db.Products
                .Where(x => x.IsActive)
                .Include("ProductCategory")
                .Include("ProductImage")
                .OrderByDescending(x => x.CreatedDate)
                .ToPagedList(page, pageSize);

            ViewBag.Title = "Danh sách sản phẩm";
            return View(items);
        }

        public ActionResult Partial_ItemByCateId(int page = 1)
        {
            const int pageSize = 16;
            var items = db.Products
                .Where(x => x.IsActive)
                .Include("ProductCategory")
                .Include("ProductImage")
                .OrderByDescending(x => x.CreatedDate)
                .ToPagedList(page, pageSize);
            return PartialView(items);
        }   


    }
}
