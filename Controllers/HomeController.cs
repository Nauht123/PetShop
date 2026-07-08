using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShop.Data;
using PetShop.Models;
using System.Diagnostics;

namespace PetShop.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult About()
        {
            return View();
        }

        public IActionResult Error404()
        {
            return View("~/Views/Shared/Error404.cshtml");
        }
        private readonly AppDbContext _db;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            // S?n ph?m HOT (k? c? h?t hàng — hi?n "Cháy hàng")
            var hotProducts = _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsHot)
                .Take(8)
                .ToList();

            // S?n ph?m bán ch?y (ch? còn hàng)
            var banChayProducts = _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsBanChay && p.SoLuongKho > 0)
                .Take(8)
                .ToList();

            // S?n ph?m m?i nh?t (ch? còn hàng)
            var newProducts = _db.Products
                .Include(p => p.Category)
                .Where(p => p.SoLuongKho > 0)
                .OrderByDescending(p => p.Id)
                .Take(8)
                .ToList();

            // Rating
            var allIds = hotProducts
                .Concat(banChayProducts)
                .Concat(newProducts)
                .Select(p => p.Id)
                .Distinct()
                .ToList();

            var ratingDict = _db.Reviews
                .Where(r => allIds.Contains(r.ProductId))
                .GroupBy(r => r.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => new RatingInfo
                    {
                        Avg = Math.Round(g.Average(r => r.SoSao), 1),
                        Count = g.Count()
                    }
                );

            ViewBag.HotProducts = hotProducts;
            ViewBag.BanChayProducts = banChayProducts;
            ViewBag.RatingDict = ratingDict;
            ViewBag.Categories = _db.Categories.ToList();

            return View(newProducts); // Model = s?n ph?m m?i nh?t
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
