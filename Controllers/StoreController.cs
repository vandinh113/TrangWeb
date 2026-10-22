
using DBSportStore.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace DBSportStore.Controllers
{
    public class StoreController : Controller
    {
        private readonly DBSportStoreEntities db =
            new DBSportStoreEntities();

        // BUOI 05: LAY SAN PHAM TU SQL SERVER
        public ActionResult Index()
        {
            var products = db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive == true)
                .OrderBy(p => p.ProductID)
                .Select(p => new
                {
                    p.ProductID,
                    p.ProductName,
                    p.Description,
                    p.Price,
                    p.ImageUrl,
                    CategoryName = p.Category.CategoryName
                })
                .ToList()
                .Select(p => new ProductCardViewModel
                {
                    ProductID = p.ProductID,

                    NamePro = p.ProductName,

                    CategoryName = p.CategoryName,

                    Price = p.Price,

                    ImagePro = p.ImageUrl,

                    ShortDescription = p.Description
                })
                .ToList();

            return View(products);
        }

        // TRANG GIOI THIEU CUA HANG
        public ActionResult AboutStore()
        {
            return View();
        }

        // GIAI PHONG KET NOI DATABASE
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
