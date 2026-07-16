using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShop.Data;
using PetShop.Helpers;
using PetShop.Models;
using PetShop.ViewModels;

namespace PetShop.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _db;

        public OrderController(AppDbContext db)
        {
            _db = db;
        }

        private bool IsLoggedIn() =>
            HttpContext.Session.GetString("UserId") != null;

        private int GetUserId() =>
            HttpContext.Session.GetInt32("UserId") ?? 0;

        // ── TRANG CHECKOUT ────────────────────────────────
        [HttpGet]
        public IActionResult Checkout()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var cart = CartHelper.GetCart(HttpContext.Session);
            if (!cart.Any())
                return RedirectToAction("Index", "Cart");

            var user = _db.Users.Find(GetUserId());
            var model = new CheckoutVM
            {
                HoTen = user?.HoTen ?? "",
                SoDienThoai = user?.SoDienThoai ?? "",
                DiaChiGiao = user?.DiaChi1 ?? ""
            };

            ViewBag.DiaChi1 = user?.DiaChi1;
            ViewBag.DiaChi2 = user?.DiaChi2;

            // Lấy thông tin khuyến mãi từ Session
            decimal soTienGiam = decimal.TryParse(
                HttpContext.Session.GetString("SoTienGiam"), out var g) ? g : 0;
            string? maKM = HttpContext.Session.GetString("MaKhuyenMai");
            string? moTaKM = HttpContext.Session.GetString("MoTaKhuyenMai");

            decimal tamTinh = cart.Sum(x => x.ThanhTien);

            // ── Ưu đãi hạng thành viên (tự động, cộng dồn với mã khuyến mãi) ──
            var hang = MembershipHelper.GetRank(user?.DiemTichLuy ?? 0);
            decimal giamTheoHang = Math.Round(tamTinh * hang.PhanTramGiam / 100, 0);

            decimal tongThanhToan = tamTinh - soTienGiam - giamTheoHang;
            if (tongThanhToan < 0) tongThanhToan = 0;

            ViewBag.Cart = cart;
            ViewBag.TamTinh = tamTinh;
            ViewBag.SoTienGiam = soTienGiam;
            ViewBag.MaKhuyenMai = maKM;
            ViewBag.MoTaKhuyenMai = moTaKM;
            ViewBag.Hang = hang;
            ViewBag.GiamTheoHang = giamTheoHang;
            ViewBag.Total = tongThanhToan;

            return View(model);
        }

        [HttpPost]
        public IActionResult Checkout(CheckoutVM model)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var cart = CartHelper.GetCart(HttpContext.Session);
            if (!cart.Any())
                return RedirectToAction("Index", "Cart");
            foreach (var item in cart)
            {
                var product = _db.Products.Find(item.ProductId);
                if (product == null || product.SoLuongKho < item.SoLuong)
                {
                    TempData["Error"] = $"\"{item.TenSanPham}\" không đủ số lượng trong kho.";
                    return RedirectToAction("Index", "Cart");
                }
            }
            var user = _db.Users.Find(GetUserId());

            // Lấy lại thông tin khuyến mãi để dùng chung cho cả 2 trường hợp (lỗi & thành công)
            decimal soTienGiam = decimal.TryParse(
                HttpContext.Session.GetString("SoTienGiam"), out var g) ? g : 0;
            int? kmId = HttpContext.Session.GetInt32("KhuyenMaiId");
            string? maKM = HttpContext.Session.GetString("MaKhuyenMai");
            string? moTaKM = HttpContext.Session.GetString("MoTaKhuyenMai");

            decimal tamTinh = cart.Sum(x => x.ThanhTien);

            var hang = MembershipHelper.GetRank(user?.DiemTichLuy ?? 0);
            decimal giamTheoHang = Math.Round(tamTinh * hang.PhanTramGiam / 100, 0);

            decimal tongTien = tamTinh - soTienGiam - giamTheoHang;
            if (tongTien < 0) tongTien = 0;

            if (!ModelState.IsValid)
            {
                ViewBag.DiaChi1 = user?.DiaChi1;
                ViewBag.DiaChi2 = user?.DiaChi2;
                ViewBag.Cart = cart;
                ViewBag.TamTinh = tamTinh;
                ViewBag.SoTienGiam = soTienGiam;
                ViewBag.MaKhuyenMai = maKM;
                ViewBag.MoTaKhuyenMai = moTaKM;
                ViewBag.Hang = hang;
                ViewBag.GiamTheoHang = giamTheoHang;
                ViewBag.Total = tongTien;
                return View(model);
            }

            // Tạo đơn hàng
            var order = new Order
            {
                UserId = GetUserId(),
                DiaChiGiao = $"{model.HoTen} | {model.SoDienThoai} | {model.DiaChiGiao}",
                TongTien = tongTien,
                TrangThai = "ChoXacNhan",
                NgayDat = DateTime.Now
            };

            _db.Orders.Add(order);
            _db.SaveChanges();

            foreach (var item in cart)
            {
                _db.OrderDetails.Add(new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    SoLuong = item.SoLuong,
                    DonGia = item.Gia
                });

                var product = _db.Products.Find(item.ProductId);
                if (product != null)
                    product.SoLuongKho -= item.SoLuong;
            }

            if (kmId.HasValue)
            {
                var km = _db.KhuyenMais.Find(kmId.Value);
                if (km != null && km.SoLuong > 0)
                    km.SoLuong--;
            }

            _db.SaveChanges();

            CartHelper.ClearCart(HttpContext.Session);
            HttpContext.Session.Remove("MaKhuyenMai");
            HttpContext.Session.Remove("MoTaKhuyenMai");
            HttpContext.Session.Remove("KhuyenMaiId");
            HttpContext.Session.Remove("SoTienGiam");

            TempData["OrderId"] = order.Id;
            return RedirectToAction("Success");
        }

        // ── ĐẶT HÀNG THÀNH CÔNG ──────────────────────────
        public IActionResult Success()
        {
            if (TempData["OrderId"] == null)
                return RedirectToAction("Index", "Home");

            ViewBag.OrderId = TempData["OrderId"];
            return View();
        }

        // ── ĐƠN HÀNG CỦA TÔI ────────────────────────────
        public IActionResult MyOrders(DateTime? tuNgay, DateTime? denNgay, string? trangThai)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var query = _db.Orders
                .Where(o => o.UserId == GetUserId())
                .OrderByDescending(o => o.NgayDat)
                .AsQueryable();

            if (tuNgay.HasValue)
                query = query.Where(o => o.NgayDat >= tuNgay.Value.Date);

            if (denNgay.HasValue)
                query = query.Where(o =>
                    o.NgayDat <= denNgay.Value.Date.AddDays(1).AddTicks(-1));

            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(o => o.TrangThai == trangThai);

            ViewBag.TuNgay = tuNgay?.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = denNgay?.ToString("yyyy-MM-dd");
            ViewBag.TrangThai = trangThai;
            ViewBag.TongDon = query.Count();

            return View(query.ToList());
        }

        // ── CHI TIẾT ĐƠN HÀNG ───────────────────────────
        public IActionResult Detail(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var order = _db.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
                .FirstOrDefault(o => o.Id == id && o.UserId == GetUserId());

            if (order == null) return NotFound();

            return View(order);
        }

        // ── HỦY ĐƠN HÀNG ─────────────────────────────────
        [HttpPost]
        public IActionResult Cancel(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var order = _db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.Id == id && o.UserId == GetUserId());

            if (order == null) return NotFound();

            if (order.TrangThai == "ChoXacNhan")
            {
                order.TrangThai = "DaHuy";

                foreach (var detail in order.OrderDetails)
                {
                    var product = _db.Products.Find(detail.ProductId);
                    if (product != null)
                        product.SoLuongKho += detail.SoLuong;
                }

                _db.SaveChanges();
                TempData["Success"] = "Đã hủy đơn hàng thành công.";
            }
            else
            {
                TempData["Error"] = "Không thể hủy đơn hàng này.";
            }

            return RedirectToAction("MyOrders");
        }

        // ── THEO DÕI ĐƠN HÀNG (không cần đăng nhập) ─────
        [HttpGet]
        public IActionResult TrackOrder()
        {
            return View();
        }

        [HttpPost]
        public IActionResult TrackOrder(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                ViewBag.Error = "Vui lòng nhập mã đơn hàng hoặc số điện thoại.";
                return View();
            }

            keyword = keyword.Trim();

            List<Order> orders;

            if (int.TryParse(keyword, out int orderId))
            {
                orders = _db.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                    .Where(o => o.Id == orderId)
                    .OrderByDescending(o => o.NgayDat)
                    .ToList();
            }
            else
            {
                orders = _db.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                    .Where(o => o.DiaChiGiao.Contains(keyword))
                    .OrderByDescending(o => o.NgayDat)
                    .ToList();
            }

            if (!orders.Any())
            {
                ViewBag.Error = $"Không tìm thấy đơn hàng nào với \"{keyword}\".";
                ViewBag.Keyword = keyword;
                return View();
            }

            ViewBag.Keyword = keyword;
            ViewBag.Orders = orders;
            return View();
        }
    }
}