using Microsoft.AspNetCore.Mvc;
using PetShop.Data;
using PetShop.Helpers;
using PetShop.Models;

namespace PetShop.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _db;

        public CartController(AppDbContext db)
        {
            _db = db;
        }

        // ── XEM GIỎ HÀNG ─────────────────────────────────────
        public IActionResult Index()
        {
            var cart = CartHelper.GetCart(HttpContext.Session);
            return View(cart);
        }

        // ── THÊM VÀO GIỎ ─────────────────────────────────────
        public IActionResult Add(int id, int qty = 1)
        {
            var product = _db.Products.Find(id);
            if (product == null) return NotFound();

            var cart = CartHelper.GetCart(HttpContext.Session);
            var existing = cart.FirstOrDefault(x => x.ProductId == id);

            if (existing != null)
            {
                existing.SoLuong += qty;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    TenSanPham = product.TenSanPham,
                    Gia = product.Gia,
                    SoLuong = qty,
                    HinhAnh = product.HinhAnh
                });
            }

            CartHelper.SaveCart(HttpContext.Session, cart);
            TempData["Success"] = $"Đã thêm \"{product.TenSanPham}\" vào giỏ hàng!";

            // Quay lại trang trước
            return Redirect(Request.Headers["Referer"].ToString()
                   ?? Url.Action("Index", "Product")!);
        }

        // ── CẬP NHẬT SỐ LƯỢNG ────────────────────────────────
        [HttpPost]
        public IActionResult Update(int productId, int soLuong)
        {
            var cart = CartHelper.GetCart(HttpContext.Session);
            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                if (soLuong <= 0)
                    cart.Remove(item);
                else
                    item.SoLuong = soLuong;
            }

            CartHelper.SaveCart(HttpContext.Session, cart);
            return RedirectToAction("Index");
        }

        // ── XÓA 1 SẢN PHẨM ───────────────────────────────────
        public IActionResult Remove(int productId)
        {
            var cart = CartHelper.GetCart(HttpContext.Session);
            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
                cart.Remove(item);

            CartHelper.SaveCart(HttpContext.Session, cart);
            return RedirectToAction("Index");
        }

        // ── XÓA TOÀN BỘ GIỎ ─────────────────────────────────
        public IActionResult Clear()
        {
            CartHelper.ClearCart(HttpContext.Session);
            return RedirectToAction("Index");
        }
        // ── ÁP DỤNG MÃ GIẢM GIÁ ─────────────────────────
        [HttpPost]
        public IActionResult ApplyCode(string maCode)
        {
            if (string.IsNullOrEmpty(maCode))
            {
                TempData["Error"] = "Vui lòng nhập mã khuyến mãi.";
                return RedirectToAction("Index");
            }

            var cart = CartHelper.GetCart(HttpContext.Session);
            decimal tongTien = cart.Sum(x => x.ThanhTien);

            var km = _db.KhuyenMais.FirstOrDefault(k =>
                k.MaCode == maCode.ToUpper().Trim() &&
                k.IsActive &&
                k.SoLuong > 0 &&
                k.NgayBatDau <= DateTime.Today &&
                k.NgayKetThuc >= DateTime.Today);

            if (km == null)
            {
                TempData["Error"] = "Mã khuyến mãi không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction("Index");
            }

            if (tongTien < km.DonHangToiThieu)
            {
                TempData["Error"] = $"Đơn hàng tối thiểu {km.DonHangToiThieu.ToString("N0")}đ để dùng mã này.";
                return RedirectToAction("Index");
            }

            // Tính số tiền giảm
            decimal soTienGiam = km.LoaiGiam == 1
                ? tongTien * km.GiaTriGiam / 100
                : km.GiaTriGiam;

            // Áp dụng giảm tối đa nếu giảm %
            if (km.LoaiGiam == 1 && km.GiamToiDa > 0)
                soTienGiam = Math.Min(soTienGiam, km.GiamToiDa);

            // Lưu vào Session
            HttpContext.Session.SetString("MaKhuyenMai", km.MaCode);
            HttpContext.Session.SetString("MoTaKhuyenMai", km.MoTa);
            HttpContext.Session.SetInt32("KhuyenMaiId", km.Id);
            HttpContext.Session.SetString("SoTienGiam",
                soTienGiam.ToString());

            TempData["Success"] = $"Áp dụng mã \"{km.MaCode}\" thành công! Giảm {soTienGiam.ToString("N0")}đ";
            return RedirectToAction("Index");
        }

        // ── XÓA MÃ GIẢM GIÁ ─────────────────────────────
        public IActionResult RemoveCode()
        {
            HttpContext.Session.Remove("MaKhuyenMai");
            HttpContext.Session.Remove("MoTaKhuyenMai");
            HttpContext.Session.Remove("KhuyenMaiId");
            HttpContext.Session.Remove("SoTienGiam");

            TempData["Success"] = "Đã xóa mã khuyến mãi.";
            return RedirectToAction("Index");
        }
    }
}