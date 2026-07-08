using Microsoft.AspNetCore.Mvc;
using PetShop.Data;
using PetShop.Models;

namespace PetShop.Controllers
{
    [ServiceFilter(typeof(PetShop.Filters.AdminAuthFilter))]
    public class AdminKhuyenMaiController : Controller
    {
        private readonly AppDbContext _db;

        public AdminKhuyenMaiController(AppDbContext db)
        {
            _db = db;
        }

        // ── DANH SÁCH ─────────────────────────────────────
        public IActionResult Index()
        {
            var list = _db.KhuyenMais
                .OrderByDescending(k => k.NgayBatDau)
                .ToList();
            return View("~/Views/Admin/KhuyenMai/Index.cshtml", list);
        }

        // ── TẠO MỚI ───────────────────────────────────────
        [HttpGet]
        public IActionResult Create()
        {
            return View("~/Views/Admin/KhuyenMai/Create.cshtml",
                new KhuyenMai
                {
                    NgayBatDau = DateTime.Today,
                    NgayKetThuc = DateTime.Today.AddMonths(1),
                    SoLuong = 100,
                    IsActive = true
                });
        }

        [HttpPost]
        public IActionResult Create(KhuyenMai model)
        {
            // Kiểm tra mã trùng
            if (_db.KhuyenMais.Any(k => k.MaCode == model.MaCode.ToUpper()))
            {
                ModelState.AddModelError("MaCode", "Mã này đã tồn tại");
                return View("~/Views/Admin/KhuyenMai/Create.cshtml", model);
            }

            model.MaCode = model.MaCode.ToUpper().Trim();
            _db.KhuyenMais.Add(model);
            _db.SaveChanges();

            TempData["Success"] = "Tạo mã khuyến mãi thành công!";
            return RedirectToAction("Index");
        }

        // ── CHỈNH SỬA ─────────────────────────────────────
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var km = _db.KhuyenMais.Find(id);
            if (km == null) return NotFound();
            return View("~/Views/Admin/KhuyenMai/Edit.cshtml", km);
        }

        [HttpPost]
        public IActionResult Edit(KhuyenMai model)
        {
            model.MaCode = model.MaCode.ToUpper().Trim();
            _db.KhuyenMais.Update(model);
            _db.SaveChanges();

            TempData["Success"] = "Cập nhật mã khuyến mãi thành công!";
            return RedirectToAction("Index");
        }

        // ── XÓA ───────────────────────────────────────────
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var km = _db.KhuyenMais.Find(id);
            if (km != null)
            {
                _db.KhuyenMais.Remove(km);
                _db.SaveChanges();
            }

            TempData["Success"] = "Đã xóa mã khuyến mãi.";
            return RedirectToAction("Index");
        }

        // ── BẬT/TẮT ───────────────────────────────────────
        [HttpPost]
        public IActionResult Toggle(int id)
        {
            var km = _db.KhuyenMais.Find(id);
            if (km != null)
            {
                km.IsActive = !km.IsActive;
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}