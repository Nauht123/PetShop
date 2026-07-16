using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShop.Data;
using PetShop.Models;

namespace PetShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _db;

        public ProductController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index(
    int? categoryId,
    string? search,
    decimal? giaMin,
    decimal? giaMax,
    int? saoToiThieu,
    string? sapXep, // "gia_tang" | "gia_giam" | "moi_nhat" | "ban_chay"
    int page = 1)
        {
            int pageSize = 9;

            var query = _db.Products
                .Include(p => p.Category)
                .Where(p => p.SoLuongKho > 0)
                .AsQueryable();

            // Lọc theo danh mục
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            // Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.TenSanPham.Contains(search));

            // Lọc theo khoảng giá
            if (giaMin.HasValue)
                query = query.Where(p => p.Gia >= giaMin.Value);

            if (giaMax.HasValue)
                query = query.Where(p => p.Gia <= giaMax.Value);

            // Sắp xếp
            query = sapXep switch
            {
                "gia_tang" => query.OrderBy(p => p.Gia),
                "gia_giam" => query.OrderByDescending(p => p.Gia),
                "ban_chay" => query.OrderByDescending(p => p.IsBanChay)
                                   .ThenByDescending(p => p.Id),
                _ => query.OrderByDescending(p => p.Id) // "moi_nhat" mặc định
            };

            // Lọc theo số sao tối thiểu — cần tính rating trước khi lọc
            // nên xử lý sau khi đã lấy danh sách sản phẩm ứng viên
            List<Product> candidateProducts;

            if (saoToiThieu.HasValue && saoToiThieu.Value > 0)
            {
                // Lấy productId có rating trung bình >= saoToiThieu
                var qualifiedIds = _db.Reviews
                    .GroupBy(r => r.ProductId)
                    .Where(g => g.Average(r => r.SoSao) >= saoToiThieu.Value)
                    .Select(g => g.Key)
                    .ToList();

                query = query.Where(p => qualifiedIds.Contains(p.Id));
            }

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var products = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var productIds = products.Select(p => p.Id).ToList();
            var ratingDict = _db.Reviews
                .Where(r => productIds.Contains(r.ProductId))
                .GroupBy(r => r.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => new RatingInfo
                    {
                        Avg = Math.Round(g.Average(r => r.SoSao), 1),
                        Count = g.Count()
                    }
                );

            ViewBag.RatingDict = ratingDict;
            ViewBag.Categories = _db.Categories.ToList();
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentSearch = search;
            ViewBag.GiaMin = giaMin;
            ViewBag.GiaMax = giaMax;
            ViewBag.SaoToiThieu = saoToiThieu;
            ViewBag.SapXep = sapXep;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(products);
        }

        // ── CHI TIẾT SẢN PHẨM ────────────────────────────────
        public IActionResult Detail(int id)
        {
            var product = _db.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id);

            if (product == null) return NotFound();

            // Sản phẩm liên quan
            ViewBag.RelatedProducts = _db.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                .Take(4)
                .ToList();

            // Đánh giá sản phẩm
            ViewBag.Reviews = _db.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == id)
                .OrderByDescending(r => r.NgayTao)
                .ToList();

            // Điểm trung bình
            var reviews = (List<PetShop.Models.Review>)ViewBag.Reviews;
            ViewBag.AvgStar = reviews.Any()
                ? Math.Round(reviews.Average(r => r.SoSao), 1)
                : 0.0;

            // Kiểm tra user đã đánh giá chưa
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            ViewBag.DaDanhGia = userId > 0 &&
                _db.Reviews.Any(r => r.ProductId == id && r.UserId == userId);

            return View(product);
        }
    }
}