using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShop.Data;
using PetShop.Filters;

namespace PetShop.Controllers
{
    [ServiceFilter(typeof(AdminAuthFilter))]
    public class AdminBookingController : Controller
    {
        private readonly AppDbContext _db;

        public AdminBookingController(AppDbContext db)
        {
            _db = db;
        }



        public IActionResult Index(string? trangThai, int page = 1)
        {
            var query = _db.Bookings
                .Include(b => b.User)
                .Include(b => b.Service)
                .OrderByDescending(b => b.NgayHen)
                .AsQueryable();

            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(b => b.TrangThai == trangThai);

            var result = PetShop.Helpers.PaginationHelper.Paginate(query, page, 15);

            ViewBag.TrangThai = trangThai;
            ViewBag.PagedResult = result;

            return View("~/Views/Admin/Booking/Index.cshtml", result.Items);
        }

        [HttpPost]
        public IActionResult UpdateStatus(int id, string trangThai)
        {

            var booking = _db.Bookings.Find(id);
            if (booking == null) return NotFound();

            booking.TrangThai = trangThai;
            _db.SaveChanges();

            TempData["Success"] = $"Cập nhật lịch hẹn #{id} thành công!";
            return RedirectToAction("Index");
        }
    }
}