using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PetShop.Data;
using PetShop.Hubs;
using PetShop.Models;

namespace PetShop.Controllers
{
    public class LiveChatController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<ChatHub> _hub;

        public LiveChatController(AppDbContext db, IHubContext<ChatHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        private int? GetUserId() => HttpContext.Session.GetInt32("UserId");

        // ── LẤY / TẠO CUỘC TRÒ CHUYỆN ────────────────────
        [HttpGet]
        public IActionResult Init()
        {
            var userId = GetUserId();
            ChatConversation? conv;

            if (userId.HasValue)
            {
                conv = _db.ChatConversations
                    .Include(c => c.Messages)
                    .FirstOrDefault(c => c.UserId == userId.Value && !c.DaDong);

                if (conv == null)
                {
                    var user = _db.Users.Find(userId.Value);
                    conv = new ChatConversation
                    {
                        UserId = userId.Value,
                        HoTen = user?.HoTen ?? "Khách hàng"
                    };
                    _db.ChatConversations.Add(conv);
                    _db.SaveChanges();
                }
            }
            else
            {
                // Khách vãng lai — định danh bằng GUID lưu trong Session
                var guestId = HttpContext.Session.GetString("GuestChatId");
                if (string.IsNullOrEmpty(guestId))
                {
                    guestId = Guid.NewGuid().ToString();
                    HttpContext.Session.SetString("GuestChatId", guestId);
                }

                conv = _db.ChatConversations
                    .Include(c => c.Messages)
                    .FirstOrDefault(c => c.GuestId == guestId && !c.DaDong);

                if (conv == null)
                {
                    conv = new ChatConversation
                    {
                        GuestId = guestId,
                        HoTen = "Khách vãng lai"
                    };
                    _db.ChatConversations.Add(conv);
                    _db.SaveChanges();
                }
            }

            var messages = conv.Messages
                .OrderBy(m => m.NgayGui)
                .Select(m => new
                {
                    m.LaAdmin,
                    m.NoiDung,
                    NgayGui = m.NgayGui.ToString("HH:mm")
                })
                .ToList();

            return Json(new { conversationId = conv.Id, messages });
        }

        // ── GỬI TIN NHẮN ──────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Send(int conversationId, string noiDung)
        {
            if (string.IsNullOrWhiteSpace(noiDung))
                return BadRequest();

            var conv = _db.ChatConversations.Find(conversationId);
            if (conv == null) return NotFound();

            var msg = new ChatMessage
            {
                ConversationId = conversationId,
                LaAdmin = false,
                NoiDung = noiDung.Trim(),
                NgayGui = DateTime.Now,
                DaDoc = false
            };

            _db.ChatMessages.Add(msg);
            conv.NgayCapNhat = DateTime.Now;
            _db.SaveChanges();

            var payload = new
            {
                conv.Id,
                LaAdmin = false,
                NoiDung = msg.NoiDung,
                NgayGui = msg.NgayGui.ToString("HH:mm"),
                HoTen = conv.HoTen
            };

            // Đẩy realtime tới chính khách này (nếu có tab khác đang mở)
            await _hub.Clients.Group($"conv_{conversationId}")
                .SendAsync("ReceiveMessage", payload);

            // Thông báo cho tất cả Admin đang online
            await _hub.Clients.Group("admins")
                .SendAsync("NewCustomerMessage", payload);

            return Ok();
        }
    }
}
