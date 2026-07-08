using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetShop.Data;
using PetShop.Models;
using PetShop.ViewModels;

namespace PetShop.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext _db;

        public BookingController(AppDbContext db)
        {
            _db = db;
        }

        private bool IsLoggedIn() =>
            HttpContext.Session.GetString("UserId") != null;

        private int GetUserId() =>
            HttpContext.Session.GetInt32("UserId") ?? 0;

        // ── TRANG ĐẶT LỊCH ───────────────────────────────
        [HttpGet]
        public IActionResult Index()
        {
            var services = _db.Services.ToList();
            ViewBag.Services = services;

            if (!IsLoggedIn())
            {
                ViewBag.RequireLogin = true;
                return View(new BookingVM());
            }

            var user = _db.Users.Find(GetUserId());
            var model = new BookingVM
            {
                HoTen = user?.HoTen ?? "",
                SoDienThoai = user?.SoDienThoai ?? ""
            };

            ViewBag.GioHenList = GetGioHenList();
            return View(model);
        }

        [HttpPost]
        public IActionResult Index(BookingVM model)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            ViewBag.Services = _db.Services.ToList();
            ViewBag.GioHenList = GetGioHenList();

            if (!ModelState.IsValid)
                return View(model);

            // Kiểm tra ngày hẹn phải từ ngày mai trở đi
            if (model.NgayHen.Date <= DateTime.Today)
            {
                ModelState.AddModelError("NgayHen",
                    "Ngày hẹn phải từ ngày mai trở đi");
                return View(model);
            }

            var booking = new Booking
            {
                UserId = GetUserId(),
                ServiceId = model.ServiceId,
                NgayHen = model.NgayHen,
                GioHen = model.GioHen,
                GhiChu = model.GhiChu,
                TrangThai = "ChoXacNhan"
            };

            _db.Bookings.Add(booking);
            _db.SaveChanges();

            TempData["BookingId"] = booking.Id;
            return RedirectToAction("BookingSuccess");
        }

        // ── ĐẶT LỊCH THÀNH CÔNG ──────────────────────────
        public IActionResult BookingSuccess()
        {
            if (TempData["BookingId"] == null)
                return RedirectToAction("Index", "Home");

            ViewBag.BookingId = TempData["BookingId"];
            return View();
        }

        // ── LỊCH HẸN CỦA TÔI ────────────────────────────
        public IActionResult MyBookings()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var bookings = _db.Bookings
                .Include(b => b.Service)
                .Where(b => b.UserId == GetUserId())
                .OrderByDescending(b => b.NgayHen)
                .ToList();

            return View(bookings);
        }

        // ── HỦY LỊCH HẸN ─────────────────────────────────
        [HttpPost]
        public IActionResult Cancel(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var booking = _db.Bookings
                .FirstOrDefault(b => b.Id == id && b.UserId == GetUserId());

            if (booking == null) return NotFound();

            if (booking.TrangThai == "ChoXacNhan")
            {
                booking.TrangThai = "DaHuy";
                _db.SaveChanges();
                TempData["Success"] = "Đã hủy lịch hẹn thành công.";
            }
            else
            {
                TempData["Error"] = "Không thể hủy lịch hẹn này.";
            }

            return RedirectToAction("MyBookings");
        }

        // ── HELPER: danh sách giờ hẹn ────────────────────
        private List<SelectListItem> GetGioHenList()
        {
            var list = new List<SelectListItem>();
            for (int h = 8; h <= 17; h++)
            {
                list.Add(new SelectListItem($"{h:00}:00", $"{h:00}:00"));
                if (h < 17)
                    list.Add(new SelectListItem($"{h:00}:30", $"{h:00}:30"));
            }
            return list;
        }
    }
}