// Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;
using PetShop.Data;
using PetShop.Helpers;
using PetShop.Models;
using PetShop.ViewModels;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace PetShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;

        public AccountController(AppDbContext db)
        {
            _db = db;
        }

        // ── ĐĂNG KÝ ──────────────────────────────────────────
        [HttpGet]
        public IActionResult Register()
        {
            // Nếu đã đăng nhập thì về trang chủ
            if (HttpContext.Session.GetString("UserId") != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Kiểm tra email đã tồn tại chưa
            bool emailExists = _db.Users.Any(u => u.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng");
                return View(model);
            }

            // Tạo user mới
            var user = new User
            {
                HoTen = model.HoTen,
                Email = model.Email,
                SoDienThoai = model.SoDienThoai,
                MatKhau = PasswordHelper.Hash(model.MatKhau),
                VaiTro = "KhachHang",
                NgayTao = DateTime.Now
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // ── ĐĂNG NHẬP ────────────────────────────────────────
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserId") != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _db.Users.FirstOrDefault(u => u.Email == model.Email);

            if (user == null || !PasswordHelper.Verify(model.MatKhau, user.MatKhau))
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
                return View(model);
            }

            // Lưu thông tin vào Session
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("HoTen", user.HoTen);
            HttpContext.Session.SetString("VaiTro", user.VaiTro);

            // Phân quyền điều hướng
            if (user.VaiTro == "Admin")
                return RedirectToAction("Index", "AdminDashboard", new { area = "" });

            return RedirectToAction("Index", "Home");
        }

        // ── ĐĂNG XUẤT ────────────────────────────────────────
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ── ĐĂNG NHẬP GOOGLE ─────────────────────────────
        [HttpGet]
        public IActionResult LoginWithGoogle()
        {
            var redirectUrl = Url.Action("GoogleCallback", "Account");
            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };
            return Challenge(properties, "Google");
        }

        [HttpGet]
        public async Task<IActionResult> GoogleCallback()
        {
            // Lấy thông tin từ Google
            var result = await HttpContext.AuthenticateAsync("Cookies");
            if (!result.Succeeded)
            {
                TempData["Error"] = "Đăng nhập Google thất bại.";
                return RedirectToAction("Login");
            }

            var claims = result.Principal?.Claims;
            string? email = claims?
                .FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            string? hoTen = claims?
                .FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Không lấy được email từ Google.";
                return RedirectToAction("Login");
            }

            // Tìm user trong db theo email
            var user = _db.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                // Tự động tạo tài khoản mới
                user = new Models.User
                {
                    HoTen = hoTen ?? email,
                    Email = email,
                    SoDienThoai = "",
                    MatKhau = PasswordHelper.Hash(Guid.NewGuid().ToString()),
                    VaiTro = "KhachHang",
                    NgayTao = DateTime.Now
                };
                _db.Users.Add(user);
                _db.SaveChanges();
            }

            // Lưu session như đăng nhập thường
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("HoTen", user.HoTen);
            HttpContext.Session.SetString("VaiTro", user.VaiTro);

            // Đăng xuất khỏi Google Cookie
            // (chỉ dùng Session của mình, không cần Google Cookie)
            await HttpContext.SignOutAsync("Cookies");

            if (user.VaiTro == "Admin")
                return RedirectToAction("Index", "AdminDashboard");

            return RedirectToAction("Index", "Home");
        }
        // ── QUÊN MẬT KHẨU ────────────────────────────────
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordVM model, [FromServices] IConfiguration config)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _db.Users.FirstOrDefault(u => u.Email == model.Email);

            // Luôn báo thành công dù email có tồn tại hay không (tránh lộ thông tin ai có tài khoản)
            if (user != null)
            {
                string token = Guid.NewGuid().ToString("N");
                user.ResetPasswordToken = token;
                user.ResetPasswordExpiry = DateTime.Now.AddMinutes(15);
                _db.SaveChanges();

                string resetLink = Url.Action("ResetPassword", "Account",
                    new { token = token }, Request.Scheme)!;

                try
                {
                    EmailHelper.SendResetPasswordEmail(config, user.Email, resetLink);
                }
                catch (Exception)
                {
                    TempData["Error"] = "Không thể gửi email lúc này. Vui lòng thử lại sau.";
                    return View(model);
                }
            }

            TempData["Success"] = "Nếu email tồn tại trong hệ thống, link đặt lại mật khẩu đã được gửi. Vui lòng kiểm tra hộp thư.";
            return RedirectToAction("Login");
        }

        // ── ĐẶT LẠI MẬT KHẨU ─────────────────────────────
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            var user = _db.Users.FirstOrDefault(u =>
                u.ResetPasswordToken == token &&
                u.ResetPasswordExpiry > DateTime.Now);

            if (user == null)
            {
                TempData["Error"] = "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction("Login");
            }

            return View(new ResetPasswordVM { Token = token });
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _db.Users.FirstOrDefault(u =>
                u.ResetPasswordToken == model.Token &&
                u.ResetPasswordExpiry > DateTime.Now);

            if (user == null)
            {
                TempData["Error"] = "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction("Login");
            }

            user.MatKhau = PasswordHelper.Hash(model.MatKhauMoi);
            user.ResetPasswordToken = null;
            user.ResetPasswordExpiry = null;
            _db.SaveChanges();

            TempData["Success"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }
    }
}