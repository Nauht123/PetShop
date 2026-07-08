using Microsoft.AspNetCore.Mvc;
using PetShop.Data;
using PetShop.Filters;
using PetShop.Models;

namespace PetShop.Controllers
{
    [ServiceFilter(typeof(AdminAuthFilter))]
    public class AdminCategoryController : Controller
    {
        private readonly AppDbContext _db;

        public AdminCategoryController(AppDbContext db)
        {
            _db = db;
        }

        

        // ── DANH SÁCH ─────────────────────────────────────────
        public IActionResult Index()
        {

            var categories = _db.Categories.ToList();
            return View("~/Views/Admin/Category/Index.cshtml", categories);
        }

        // ── TẠO MỚI ───────────────────────────────────────────
        [HttpGet]
        public IActionResult Create()
        {
            return View("~/Views/Admin/Category/Create.cshtml");
        }

        [HttpPost]
        public IActionResult Create(Category model)
        {

            if (!ModelState.IsValid)
                return View("~/Views/Admin/Category/Create.cshtml", model);

            _db.Categories.Add(model);
            _db.SaveChanges();

            TempData["Success"] = "Thêm danh mục thành công!";
            return RedirectToAction("Index");
        }

        // ── CHỈNH SỬA ─────────────────────────────────────────
        [HttpGet]
        public IActionResult Edit(int id)
        {

            var category = _db.Categories.Find(id);
            if (category == null) return NotFound();

            return View("~/Views/Admin/Category/Edit.cshtml", category);
        }

        [HttpPost]
        public IActionResult Edit(Category model)
        {

            if (!ModelState.IsValid)
                return View("~/Views/Admin/Category/Edit.cshtml", model);

            _db.Categories.Update(model);
            _db.SaveChanges();

            TempData["Success"] = "Cập nhật danh mục thành công!";
            return RedirectToAction("Index");
        }

        // ── XÓA ───────────────────────────────────────────────
        [HttpPost]
        public IActionResult Delete(int id)
        {

            var category = _db.Categories.Find(id);
            if (category == null) return NotFound();

            _db.Categories.Remove(category);
            _db.SaveChanges();

            TempData["Success"] = "Xóa danh mục thành công!";
            return RedirectToAction("Index");
        }
    }
}