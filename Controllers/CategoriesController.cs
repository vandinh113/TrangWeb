
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using DBSportStore.Models;

namespace DBSportStore.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly DBSportStoreEntities db =
            new DBSportStoreEntities();

        // HIEN THI DANH SACH DANH MUC
        public ActionResult Index()
        {
            var categories = db.Categories
                .OrderBy(c => c.CategoryID)
                .ToList();

            return View(categories);
        }

        // XEM CHI TIET DANH MUC
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            Category category = db.Categories.Find(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        // HIEN THI FORM THEM DANH MUC
        public ActionResult Create()
        {
            return View();
        }

        // XU LY THEM DANH MUC
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "CategoryName")] Category category)
        {
            category.CategoryName =
                (category.CategoryName ?? "").Trim();

            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                ModelState.AddModelError(
                    "CategoryName",
                    "Vui lòng nhập tên danh mục.");
            }

            if (db.Categories.Any(c =>
                c.CategoryName == category.CategoryName))
            {
                ModelState.AddModelError(
                    "CategoryName",
                    "Tên danh mục đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();

                TempData["Success"] =
                    "Thêm danh mục thành công!";

                return RedirectToAction("Index");
            }

            return View(category);
        }

        // HIEN THI FORM SUA DANH MUC
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            Category category = db.Categories.Find(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        // XU LY SUA DANH MUC
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "CategoryID,CategoryName")]
            Category category)
        {
            category.CategoryName =
                (category.CategoryName ?? "").Trim();

            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                ModelState.AddModelError(
                    "CategoryName",
                    "Vui lòng nhập tên danh mục.");
            }

            if (db.Categories.Any(c =>
                c.CategoryName == category.CategoryName &&
                c.CategoryID != category.CategoryID))
            {
                ModelState.AddModelError(
                    "CategoryName",
                    "Tên danh mục đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                Category existing =
                    db.Categories.Find(category.CategoryID);

                if (existing == null)
                {
                    return HttpNotFound();
                }

                existing.CategoryName = category.CategoryName;

                db.SaveChanges();

                TempData["Success"] =
                    "Cập nhật danh mục thành công!";

                return RedirectToAction("Index");
            }

            return View(category);
        }

        // HIEN THI XAC NHAN XOA DANH MUC
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            Category category = db.Categories.Find(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        // XU LY XOA DANH MUC
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Categories.Find(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            bool hasProducts = db.Products.Any(
                p => p.CategoryID == id);

            if (hasProducts)
            {
                TempData["Error"] =
                    "Không thể xóa danh mục đang có sản phẩm!";

                return RedirectToAction("Index");
            }

            try
            {
                db.Categories.Remove(category);
                db.SaveChanges();

                TempData["Success"] =
                    "Xóa danh mục thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Không thể xóa danh mục vì đang được sử dụng.";
            }

            return RedirectToAction("Index");
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
