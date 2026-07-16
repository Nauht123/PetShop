using Microsoft.AspNetCore.Mvc;
using PetShop.Data;
using PetShop.Filters;
using PetShop.Helpers;
using PetShop.Models;
using System.Linq;

namespace PetShop.Controllers
{
    [ServiceFilter(typeof(AdminAuthFilter))]

    public class AdminUserController : Controller
    {
        private readonly AppDbContext _context;

        public AdminUserController(AppDbContext context)
        {
            _context = context;
        }



        public IActionResult Index(string? search, int page = 1)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(u =>
                    u.HoTen.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.SoDienThoai.Contains(search));

            query = query.OrderByDescending(u => u.NgayTao);

            var result = PetShop.Helpers.PaginationHelper.Paginate(query, page, 15);

            ViewBag.Search = search;
            ViewBag.PagedResult = result;

            return View("~/Views/Admin/User/Index.cshtml", result.Items);
        }

        // GET: /AdminUser/Edit/5
        public IActionResult Edit(int id)
        {
            
            var user = _context.Users.Find(id);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("Index");
            }

            return View("~/Views/Admin/User/Edit.cshtml", user);   // 👈 chỉ định rõ đường dẫn
        }

        // POST: /AdminUser/Edit/5
        [HttpPost]
        public IActionResult Edit(int id, User model, string? newPassword)
        {
            
            var user = _context.Users.Find(id);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("Index");
            }

            // Kiểm tra trùng email với người dùng khác
            bool emailExists = _context.Users.Any(u => u.Email == model.Email && u.Id != id);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng bởi tài khoản khác.");
                return View("~/Views/Admin/User/Edit.cshtml", model);   // 👈 nhớ sửa cả nhánh lỗi
            }

            user.HoTen = model.HoTen;
            user.Email = model.Email;
            user.SoDienThoai = model.SoDienThoai;
            user.VaiTro = model.VaiTro;

            // Nếu admin nhập mật khẩu mới thì mới đổi, để trống thì giữ nguyên
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                user.MatKhau = PasswordHelper.Hash(newPassword);
            }

            _context.SaveChanges();

            TempData["Success"] = "Cập nhật thông tin người dùng thành công.";
            return RedirectToAction("Index");
        }

        // GET: /AdminUser/Delete/5
        public IActionResult Delete(int id)
        {
            
            var user = _context.Users.Find(id);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("Index");
            }

            // Không cho xóa chính tài khoản Admin đang đăng nhập
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == id)
            {
                TempData["Error"] = "Không thể xóa tài khoản đang đăng nhập.";
                return RedirectToAction("Index");
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            TempData["Success"] = "Đã xóa người dùng thành công.";
            return RedirectToAction("Index");
        }
    }
}