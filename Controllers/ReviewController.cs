using Microsoft.AspNetCore.Mvc;
using PetShop.Data;
using PetShop.Models;
using PetShop.ViewModels;

namespace PetShop.Controllers
{
    public class ReviewController : Controller
    {
        private readonly AppDbContext _db;

        public ReviewController(AppDbContext db)
        {
            _db = db;
        }

        private bool IsLoggedIn() =>
            HttpContext.Session.GetString("UserId") != null;

        private int GetUserId() =>
            HttpContext.Session.GetInt32("UserId") ?? 0;

        // ── GỬI ĐÁNH GIÁ ─────────────────────────────────
        [HttpPost]
        public IActionResult Submit(ReviewVM model)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin đánh giá.";
                return RedirectToAction("Detail", "Product",
                    new { id = model.ProductId });
            }

            // Kiểm tra đã đánh giá chưa
            bool daXem = _db.Reviews.Any(r =>
                r.ProductId == model.ProductId &&
                r.UserId == GetUserId());

            if (daXem)
            {
                TempData["Error"] = "Bạn đã đánh giá sản phẩm này rồi.";
                return RedirectToAction("Detail", "Product",
                    new { id = model.ProductId });
            }

            var review = new Review
            {
                ProductId = model.ProductId,
                UserId = GetUserId(),
                SoSao = model.SoSao,
                NoiDung = model.NoiDung,
                NgayTao = DateTime.Now
            };

            _db.Reviews.Add(review);
            _db.SaveChanges();

            TempData["Success"] = "Cảm ơn bạn đã đánh giá sản phẩm!";
            return RedirectToAction("Detail", "Product",
                new { id = model.ProductId });
        }

        // ── XÓA ĐÁNH GIÁ (chỉ Admin) ─────────────────────
        [HttpPost]
        public IActionResult Delete(int id, int productId)
        {
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
                return Forbid();

            var review = _db.Reviews.Find(id);
            if (review != null)
            {
                _db.Reviews.Remove(review);
                _db.SaveChanges();
                TempData["Success"] = "Đã xóa đánh giá.";
            }

            return RedirectToAction("Detail", "Product",
                new { id = productId });
        }
    }
}