using Microsoft.AspNetCore.Mvc;
using PetShop.Data;
using PetShop.Helpers;
using PetShop.ViewModels;

namespace PetShop.Controllers
{
    public class ProfileController : Controller
    {
        private readonly AppDbContext _db;

        public ProfileController(AppDbContext db)
        {
            _db = db;
        }

        private bool IsLoggedIn() =>
            HttpContext.Session.GetString("UserId") != null;

        private int GetUserId() =>
            HttpContext.Session.GetInt32("UserId") ?? 0;

        // ── XEM PROFILE ───────────────────────────────────
        public IActionResult Index()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var user = _db.Users.Find(GetUserId());
            if (user == null) return NotFound();

            var model = new ProfileVM
            {
                Id = user.Id,
                HoTen = user.HoTen,
                Email = user.Email,
                SoDienThoai = user.SoDienThoai,
                DiaChi1 = user.DiaChi1,
                DiaChi2 = user.DiaChi2,
                NgaySinh = user.NgaySinh
            };

            ViewBag.DiemTichLuy = user.DiemTichLuy;
            ViewBag.VaiTro = user.VaiTro;
            ViewBag.NgayTao = user.NgayTao;

            return View(model);
        }

        // ── CẬP NHẬT PROFILE ──────────────────────────────
        [HttpPost]
        public IActionResult Index(ProfileVM model)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            var user = _db.Users.Find(GetUserId());
            if (user == null) return NotFound();

            // Kiểm tra email trùng với người khác
            bool emailTrung = _db.Users.Any(u =>
                u.Email == model.Email && u.Id != user.Id);

            if (emailTrung)
            {
                ModelState.AddModelError("Email",
                    "Email này đã được sử dụng bởi tài khoản khác");
                return View(model);
            }

            user.HoTen = model.HoTen;
            user.Email = model.Email;
            user.SoDienThoai = model.SoDienThoai;
            user.DiaChi1 = model.DiaChi1;
            user.DiaChi2 = model.DiaChi2;
            user.NgaySinh = model.NgaySinh;

            _db.SaveChanges();

            // Cập nhật tên trong Session
            HttpContext.Session.SetString("HoTen", user.HoTen);

            TempData["Success"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Index");
        }

        // ── ĐỔI MẬT KHẨU ──────────────────────────────────
        [HttpGet]
        public IActionResult DoiMatKhau()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        public IActionResult DoiMatKhau(DoiMatKhauVM model)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            var user = _db.Users.Find(GetUserId());
            if (user == null) return NotFound();

            // Kiểm tra mật khẩu cũ
            if (!PasswordHelper.Verify(model.MatKhauCu, user.MatKhau))
            {
                ModelState.AddModelError("MatKhauCu",
                    "Mật khẩu hiện tại không đúng");
                return View(model);
            }

            user.MatKhau = PasswordHelper.Hash(model.MatKhauMoi);
            _db.SaveChanges();

            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Index");
        }
    }
}