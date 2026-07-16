using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShop.Data;
using PetShop.Filters;

namespace PetShop.Controllers
{
    [ServiceFilter(typeof(AdminAuthFilter))]
    public class AdminOrderController : Controller
    {
        private readonly AppDbContext _db;

        public AdminOrderController(AppDbContext db)
        {
            _db = db;
        }


        public IActionResult Index(
    string? trangThai, string? search, int page = 1)
        {
            var query = _db.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.NgayDat)
                .AsQueryable();

            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(o => o.TrangThai == trangThai);

            if (!string.IsNullOrEmpty(search))
            {
                if (int.TryParse(search, out int orderId))
                    query = query.Where(o => o.Id == orderId);
                else
                    query = query.Where(o =>
                        o.User!.HoTen.Contains(search) ||
                        o.User.SoDienThoai.Contains(search) ||
                        o.DiaChiGiao.Contains(search));
            }

            var result = PetShop.Helpers.PaginationHelper.Paginate(query, page, 15);

            ViewBag.TrangThai = trangThai;
            ViewBag.Search = search;
            ViewBag.PagedResult = result;

            return View("~/Views/Admin/Order/Index.cshtml", result.Items);
        }

        [HttpPost]
        public IActionResult UpdateStatus(int id, string trangThai)
        {

            var order = _db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.Id == id);

            if (order == null) return NotFound();

            // Nếu chuyển sang DaHuy → cộng lại tồn kho
            if (trangThai == "DaHuy" && order.TrangThai != "DaHuy")
            {
                foreach (var detail in order.OrderDetails)
                {
                    var product = _db.Products.Find(detail.ProductId);
                    if (product != null)
                        product.SoLuongKho += detail.SoLuong;
                }
            }

            // Nếu chuyển từ DaHuy sang trạng thái khác → trừ lại tồn kho
            if (order.TrangThai == "DaHuy" && trangThai != "DaHuy")
            {
                foreach (var detail in order.OrderDetails)
                {
                    var product = _db.Products.Find(detail.ProductId);
                    if (product != null)
                        product.SoLuongKho -= detail.SoLuong;
                }
            }
            // Nếu chuyển sang HoanThanh → cộng điểm tích lũy
            // Cứ 10.000đ = 1 điểm
            if (trangThai == "HoanThanh" && order.TrangThai != "HoanThanh")
            {
                var user = _db.Users.Find(order.UserId);
                if (user != null)
                {
                    int diemCong = (int)(order.TongTien / 10000);
                    user.DiemTichLuy += diemCong;
                }
            }

            // Nếu chuyển từ HoanThanh sang trạng thái khác → trừ điểm lại
            if (order.TrangThai == "HoanThanh" && trangThai != "HoanThanh")
            {
                var user = _db.Users.Find(order.UserId);
                if (user != null)
                {
                    int diemTru = (int)(order.TongTien / 10000);
                    user.DiemTichLuy = Math.Max(0, user.DiemTichLuy - diemTru);
                }
            }

            order.TrangThai = trangThai;
            _db.SaveChanges();

            TempData["Success"] = $"Cập nhật trạng thái đơn #{id} thành công!";
            return RedirectToAction("Index");
        }
    }
}