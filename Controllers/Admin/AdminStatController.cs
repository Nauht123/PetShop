using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShop.Data;
using PetShop.ViewModels;

namespace PetShop.Controllers
{
    [ServiceFilter(typeof(PetShop.Filters.AdminAuthFilter))]
    public class AdminStatController : Controller
    {
        private readonly AppDbContext _db;

        public AdminStatController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index(DateTime? tuNgay, DateTime? denNgay)
        {
            var model = new ThongKeVM
            {
                TuNgay = tuNgay ?? DateTime.Today.AddDays(-30),
                DenNgay = denNgay ?? DateTime.Today
            };

            // DenNgay lấy đến cuối ngày
            var denNgayCuoiNgay = model.DenNgay.Date.AddDays(1).AddTicks(-1);

            // Lấy đơn hàng trong khoảng
            var orders = _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
                .Where(o => o.NgayDat >= model.TuNgay.Date
                         && o.NgayDat <= denNgayCuoiNgay)
                .OrderByDescending(o => o.NgayDat)
                .ToList();

            // Tổng quan
            model.TongDonHang = orders.Count;
            model.DonHoanThanh = orders.Count(o => o.TrangThai == "HoanThanh");
            model.DonDangGiao = orders.Count(o => o.TrangThai == "DangGiao");
            model.DonChoXacNhan = orders.Count(o => o.TrangThai == "ChoXacNhan");
            model.DonDaHuy = orders.Count(o => o.TrangThai == "DaHuy");
            model.TongDoanhThu = orders
                .Where(o => o.TrangThai == "HoanThanh")
                .Sum(o => o.TongTien);

            // Doanh thu theo ngày
            model.DoanhThuTheoNgay = orders
                .Where(o => o.TrangThai == "HoanThanh")
                .GroupBy(o => o.NgayDat.Date)
                .OrderBy(g => g.Key)
                .Select(g => new DoanhThuNgay
                {
                    Ngay = g.Key.ToString("dd/MM"),
                    DoanhThu = g.Sum(o => o.TongTien),
                    SoDon = g.Count()
                })
                .ToList();

            // Top 5 sản phẩm bán chạy
            model.TopSanPhams = orders
                .Where(o => o.TrangThai == "HoanThanh")
                .SelectMany(o => o.OrderDetails)
                .GroupBy(d => new
                {
                    d.Product!.TenSanPham,
                    d.Product.HinhAnh
                })
                .Select(g => new TopSanPham
                {
                    TenSanPham = g.Key.TenSanPham,
                    HinhAnh = g.Key.HinhAnh,
                    SoLuongBan = g.Sum(d => d.SoLuong),
                    DoanhThu = g.Sum(d => d.SoLuong * d.DonGia)
                })
                .OrderByDescending(x => x.SoLuongBan)
                .Take(5)
                .ToList();

            model.DonHangs = orders;

            return View("~/Views/Admin/Stat/Index.cshtml", model);
        }
    }
}