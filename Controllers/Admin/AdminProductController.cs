using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetShop.Data;
using PetShop.Filters;
using PetShop.Helpers;
using PetShop.Models;

namespace PetShop.Controllers
{
    [ServiceFilter(typeof(AdminAuthFilter))]
    public class AdminProductController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public AdminProductController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

       
        // ── DANH SÁCH ─────────────────────────────────────────
        public IActionResult Index()
        {

            var products = _db.Products
                .Include(p => p.Category)
                .ToList();

            return View("~/Views/Admin/Product/Index.cshtml", products);
        }

        // ── TẠO MỚI ───────────────────────────────────────────
        [HttpGet]
        public IActionResult Create()
        {

            ViewBag.Categories = new SelectList(_db.Categories, "Id", "TenDanhMuc");
            return View("~/Views/Admin/Product/Create.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product model, IFormFile? imageFile)
        {
            // Validate ảnh trước khi làm bất cứ điều gì khác
            if (imageFile != null)
            {
                var (isValid, error) = ImageValidationHelper.Validate(imageFile);
                if (!isValid)
                {
                    ModelState.AddModelError("", error!);
                    ViewBag.Categories = new SelectList(_db.Categories, "Id", "TenDanhMuc");
                    return View("~/Views/Admin/Product/Create.cshtml", model);
                }
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = ImageValidationHelper.GenerateSafeFileName(imageFile);
                var savePath = Path.Combine(_env.WebRootPath, "images", "products", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
                using var stream = new FileStream(savePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);

                model.HinhAnh = "/images/products/" + fileName;
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_db.Categories, "Id", "TenDanhMuc");
                return View("~/Views/Admin/Product/Create.cshtml", model);
            }

            _db.Products.Add(model);
            _db.SaveChanges();

            TempData["Success"] = "Thêm sản phẩm thành công!";
            return RedirectToAction("Index");
        }

        // ── CHỈNH SỬA ─────────────────────────────────────────
        [HttpGet]
        public IActionResult Edit(int id)
        {

            var product = _db.Products.Find(id);
            if (product == null) return NotFound();

            ViewBag.Categories = new SelectList(_db.Categories, "Id", "TenDanhMuc", product.CategoryId);
            return View("~/Views/Admin/Product/Edit.cshtml", product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product model, IFormFile? imageFile)
        {
            var existing = _db.Products.AsNoTracking()
                .FirstOrDefault(p => p.Id == model.Id);

            if (imageFile != null)
            {
                var (isValid, error) = ImageValidationHelper.Validate(imageFile);
                if (!isValid)
                {
                    ModelState.AddModelError("", error!);
                    ViewBag.Categories = new SelectList(
                        _db.Categories, "Id", "TenDanhMuc", model.CategoryId);
                    model.HinhAnh = existing?.HinhAnh; // giữ ảnh cũ để hiển thị lại form
                    return View("~/Views/Admin/Product/Edit.cshtml", model);
                }
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = ImageValidationHelper.GenerateSafeFileName(imageFile);
                var savePath = Path.Combine(_env.WebRootPath, "images", "products", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
                using var stream = new FileStream(savePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);

                model.HinhAnh = "/images/products/" + fileName;

                // Xóa ảnh cũ để tránh rác file tích lũy
                if (!string.IsNullOrEmpty(existing?.HinhAnh) &&
                    existing.HinhAnh.StartsWith("/images/products/"))
                {
                    var oldPath = Path.Combine(
                        _env.WebRootPath,
                        existing.HinhAnh.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                    );
                    if (System.IO.File.Exists(oldPath))
                    {
                        try { System.IO.File.Delete(oldPath); }
                        catch { /* bỏ qua nếu không xóa được, không chặn luồng chính */ }
                    }
                }
            }
            else
            {
                model.HinhAnh = existing?.HinhAnh;
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(
                    _db.Categories, "Id", "TenDanhMuc", model.CategoryId);
                return View("~/Views/Admin/Product/Edit.cshtml", model);
            }

            _db.Products.Update(model);
            _db.SaveChanges();

            TempData["Success"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction("Index");
        }

        // ── XÓA ───────────────────────────────────────────────
        [HttpPost]
        public IActionResult Delete(int id)
        {

            var product = _db.Products.Find(id);
            if (product == null) return NotFound();

            _db.Products.Remove(product);
            _db.SaveChanges();

            TempData["Success"] = "Xóa sản phẩm thành công!";
            return RedirectToAction("Index");
        }
    }
}