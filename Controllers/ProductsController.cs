
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DBSportStore.Models;

namespace DBSportStore.Controllers
{
    public class ProductsController : Controller
    {
        private readonly DBSportStoreEntities db =
            new DBSportStoreEntities();

        private const int MaxImageSize = 2 * 1024 * 1024;

        // DANH SACH SAN PHAM
        public ActionResult Index()
        {
            var products = db.Products
                .Include(p => p.Category)
                .OrderBy(p => p.ProductID)
                .ToList();

            return View(products);
        }

        // CHI TIET SAN PHAM
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);

            Product product = db.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductID == id.Value);

            if (product == null)
                return HttpNotFound();

            return View(product);
        }

        // FORM THEM SAN PHAM
        public ActionResult Create()
        {
            LoadCategories();
            return View();
        }

        // XU LY THEM SAN PHAM
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include =
                "ProductName,Description,CategoryID,Price,StockQuantity,IsActive")]
            Product product,
            HttpPostedFileBase ImageFile)
        {
            ValidateImage(ImageFile);

            // ImageUrl duoc quan ly boi server
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                string savedImage = null;

                try
                {
                    if (ImageFile != null &&
                        ImageFile.ContentLength > 0)
                    {
                        savedImage = SaveImage(ImageFile);
                        product.ImageUrl = savedImage;
                    }
                    else
                    {
                        // Ho tro ten anh co san trong form cu
                        product.ImageUrl = GetExistingImageName();
                    }

                    db.Products.Add(product);
                    db.SaveChanges();

                    TempData["Success"] =
                        "Thêm sản phẩm thành công!";

                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    if (!string.IsNullOrEmpty(savedImage))
                        DeleteUploadedImage(savedImage);

                    ModelState.AddModelError(
                        "",
                        "Không thể lưu sản phẩm. Vui lòng kiểm tra dữ liệu.");
                }
            }

            LoadCategories(product.CategoryID);
            return View(product);
        }

        // FORM SUA SAN PHAM
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);

            Product product = db.Products.Find(id);

            if (product == null)
                return HttpNotFound();

            LoadCategories(product.CategoryID);
            return View(product);
        }

        // XU LY SUA SAN PHAM
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include =
                "ProductID,ProductName,Description,CategoryID,Price,StockQuantity,IsActive")]
            Product input,
            HttpPostedFileBase ImageFile)
        {
            Product product = db.Products.Find(input.ProductID);

            if (product == null)
                return HttpNotFound();

            ValidateImage(ImageFile);
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                string savedImage = null;
                string oldImage = product.ImageUrl;

                try
                {
                    if (ImageFile != null &&
                        ImageFile.ContentLength > 0)
                    {
                        savedImage = SaveImage(ImageFile);
                    }

                    product.ProductName = input.ProductName;
                    product.Description = input.Description;
                    product.CategoryID = input.CategoryID;
                    product.Price = input.Price;
                    product.StockQuantity = input.StockQuantity;
                    product.IsActive = input.IsActive;

                    // Khong chon anh moi thi giu anh cu
                    if (!string.IsNullOrEmpty(savedImage))
                        product.ImageUrl = savedImage;

                    db.SaveChanges();

                    TempData["Success"] =
                        "Cập nhật sản phẩm thành công!";

                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    if (!string.IsNullOrEmpty(savedImage))
                        DeleteUploadedImage(savedImage);

                    product.ImageUrl = oldImage;

                    ModelState.AddModelError(
                        "",
                        "Không thể cập nhật sản phẩm.");
                }
            }

            input.ImageUrl = product.ImageUrl;
            LoadCategories(input.CategoryID);

            return View(input);
        }

        // FORM XOA SAN PHAM
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);

            Product product = db.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductID == id.Value);

            if (product == null)
                return HttpNotFound();

            return View(product);
        }

        // XAC NHAN XOA SAN PHAM
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);

            if (product == null)
                return HttpNotFound();

            try
            {
                db.Products.Remove(product);
                db.SaveChanges();

                TempData["Success"] =
                    "Xóa sản phẩm thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Không thể xóa sản phẩm vì đang được sử dụng trong đơn hàng.";
            }

            return RedirectToAction("Index");
        }

        // NAP DANH MUC VAO DROPDOWN
        private void LoadCategories(object selectedId = null)
        {
            ViewBag.CategoryID = new SelectList(
                db.Categories.OrderBy(c => c.CategoryName),
                "CategoryID",
                "CategoryName",
                selectedId);
        }

        // KIEM TRA FILE ANH
        private void ValidateImage(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength <= 0)
                return;

            if (file.ContentLength > MaxImageSize)
            {
                ModelState.AddModelError(
                    "ImageFile",
                    "Dung lượng ảnh không được vượt quá 2 MB.");

                return;
            }

            string extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            string[] allowedExtensions =
            {
                ".jpg", ".jpeg", ".png"
            };

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    "ImageFile",
                    "Chỉ chấp nhận ảnh JPG, JPEG hoặc PNG.");

                return;
            }

            string contentType = (file.ContentType ?? "")
                .ToLowerInvariant();

            bool validType =
                (extension == ".jpg" || extension == ".jpeg")
                    ? contentType == "image/jpeg"
                    : contentType == "image/png";

            if (!validType)
            {
                ModelState.AddModelError(
                    "ImageFile",
                    "Định dạng tệp ảnh không hợp lệ.");

                return;
            }

            // Kiem tra chu ky file de tranh file gia mao
            try
            {
                Stream stream = file.InputStream;
                long position = stream.Position;

                byte[] header = new byte[8];
                int read = stream.Read(header, 0, header.Length);

                stream.Position = position;

                bool validSignature = false;

                if (extension == ".jpg" || extension == ".jpeg")
                {
                    validSignature = read >= 3 &&
                        header[0] == 0xFF &&
                        header[1] == 0xD8 &&
                        header[2] == 0xFF;
                }
                else if (extension == ".png")
                {
                    byte[] png =
                    {
                        137, 80, 78, 71, 13, 10, 26, 10
                    };

                    validSignature = read >= 8 &&
                        header.SequenceEqual(png);
                }

                if (!validSignature)
                {
                    ModelState.AddModelError(
                        "ImageFile",
                        "Nội dung tệp không đúng định dạng ảnh.");
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "ImageFile",
                    "Không thể đọc tệp ảnh.");
            }
        }

        // LUU ANH, TU DONG DOI TEN NEU BI TRUNG
        private string SaveImage(HttpPostedFileBase file)
        {
            string folder = Server.MapPath("~/Content/images");

            Directory.CreateDirectory(folder);

            string originalName =
                Path.GetFileName(file.FileName.Replace('\\', '/'));

            string extension =
                Path.GetExtension(originalName).ToLowerInvariant();

            string baseName =
                Path.GetFileNameWithoutExtension(originalName);

            // Loai bo ky tu khong hop le
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                baseName = baseName.Replace(c, '_');
            }

            if (string.IsNullOrWhiteSpace(baseName))
                baseName = "product";

            string fileName = baseName + extension;
            int counter = 1;

            while (System.IO.File.Exists(
                Path.Combine(folder, fileName)))
            {
                fileName = baseName + "-" + counter + extension;
                counter++;
            }

            string fullPath = Path.Combine(folder, fileName);

            file.SaveAs(fullPath);

            return fileName;
        }

        // TEN ANH CO SAN (NEU FORM CO TRUONG IMAGEURL)
        private string GetExistingImageName()
        {
            string value = Request.Form["ImageUrl"];

            if (string.IsNullOrWhiteSpace(value))
                return null;

            string fileName =
                Path.GetFileName(value.Replace('\\', '/'));

            return fileName;
        }

        // XOA ANH MOI NEU LUU DATABASE THAT BAI
        private void DeleteUploadedImage(string fileName)
        {
            try
            {
                string folder = Server.MapPath("~/Content/images");

                string fullPath = Path.Combine(
                    folder,
                    Path.GetFileName(fileName));

                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }
            catch
            {
                // Khong che loi xoa file che mat loi chinh
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
