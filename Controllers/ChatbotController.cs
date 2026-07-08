using Microsoft.AspNetCore.Mvc;
using PetShop.Data;
using PetShop.Services;
using PetShop.ViewModels;
using System.Text;

namespace PetShop.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly AppDbContext _db;
        private readonly GeminiService _gemini;

        public ChatbotController(AppDbContext db, GeminiService gemini)
        {
            _db = db;
            _gemini = gemini;
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] ChatRequestVM request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return Json(new { reply = "Bạn muốn hỏi gì về sản phẩm hoặc dịch vụ của PetShop nhỉ? 🐾" });

            var systemContext = BuildSystemContext();

            var reply = await _gemini.AskAsync(systemContext, request.History, request.Message);

            return Json(new { reply });
        }

        private string BuildSystemContext()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Bạn là trợ lý AI tư vấn bán hàng của cửa hàng thú cưng PetShop.");
            sb.AppendLine("Hãy trả lời thân thiện, ngắn gọn, dùng tiếng Việt, có thể dùng emoji 🐾🐶🐱 vừa phải.");
            sb.AppendLine("Chỉ tư vấn dựa trên thông tin sản phẩm/dịch vụ dưới đây, không bịa thông tin không có.");
            sb.AppendLine("Nếu khách hỏi ngoài phạm vi cửa hàng, lịch sự từ chối và hướng họ quay lại chủ đề mua sắm.");
            sb.AppendLine();

            sb.AppendLine("── DANH SÁCH SẢN PHẨM HIỆN CÓ ──");
            var products = _db.Products.Take(50).ToList(); // giới hạn để không quá dài
            foreach (var p in products)
            {
                sb.AppendLine($"- {p.TenSanPham} | Giá: {p.Gia:N0}đ | Còn lại: {p.SoLuongKho} | {p.MoTa}");
            }

            sb.AppendLine();
            sb.AppendLine("── DANH SÁCH DỊCH VỤ HIỆN CÓ ──");
            var services = _db.Services.ToList();
            foreach (var s in services)
            {
                sb.AppendLine($"- {s.TenDichVu} | Giá: {s.Gia:N0}đ | Thời gian: ~{s.ThoiGianPhut} phút | {s.MoTa}");
            }

            sb.AppendLine();
            sb.AppendLine("Nếu khách hỏi giá, số lượng, mô tả sản phẩm/dịch vụ, hãy trả lời chính xác theo dữ liệu trên.");
            sb.AppendLine("Nếu khách muốn mua/đặt lịch, hướng dẫn họ vào trang Sản phẩm hoặc Đặt lịch dịch vụ trên website.");

            return sb.ToString();
        }
    }
}