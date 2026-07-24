using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PetShop.Data;
using PetShop.Filters;
using PetShop.Hubs;
using PetShop.Models;

namespace PetShop.Controllers
{
    [ServiceFilter(typeof(AdminAuthFilter))]
    public class AdminLiveChatController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<ChatHub> _hub;

        public AdminLiveChatController(AppDbContext db, IHubContext<ChatHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        // ── DANH SÁCH CUỘC TRÒ CHUYỆN ────────────────────
        public IActionResult Index()
        {
            var conversations = _db.ChatConversations
                .Include(c => c.Messages)
                .Where(c => !c.DaDong)
                .OrderByDescending(c => c.NgayCapNhat)
                .Select(c => new
                {
                    Conversation = c,
                    LastMessage = c.Messages
                        .OrderByDescending(m => m.NgayGui)
                        .Select(m => m.NoiDung)
                        .FirstOrDefault(),
                    UnreadCount = c.Messages
                        .Count(m => !m.LaAdmin && !m.DaDoc)
                })
                .ToList();

            return View("~/Views/Admin/LiveChat/Index.cshtml", conversations);
        }

        // ── XEM CHI TIẾT 1 CUỘC TRÒ CHUYỆN ───────────────
        public IActionResult Conversation(int id)
        {
            var conv = _db.ChatConversations
                .Include(c => c.Messages)
                .FirstOrDefault(c => c.Id == id);

            if (conv == null) return NotFound();

            // Đánh dấu đã đọc toàn bộ tin nhắn của khách trong cuộc trò chuyện này
            var unread = conv.Messages.Where(m => !m.LaAdmin && !m.DaDoc).ToList();
            foreach (var m in unread) m.DaDoc = true;
            if (unread.Any()) _db.SaveChanges();

            return View("~/Views/Admin/LiveChat/Conversation.cshtml", conv);
        }

        // ── ADMIN GỬI TIN NHẮN ────────────────────────────
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
                LaAdmin = true,
                NoiDung = noiDung.Trim(),
                NgayGui = DateTime.Now,
                DaDoc = true // admin gửi thì coi như đã đọc phía admin
            };

            _db.ChatMessages.Add(msg);
            conv.NgayCapNhat = DateTime.Now;
            _db.SaveChanges();

            var payload = new
            {
                conv.Id,
                LaAdmin = true,
                NoiDung = msg.NoiDung,
                NgayGui = msg.NgayGui.ToString("HH:mm")
            };

            await _hub.Clients.Group($"conv_{conversationId}")
                .SendAsync("ReceiveMessage", payload);

            return Ok();
        }

        // ── ĐÓNG CUỘC TRÒ CHUYỆN ──────────────────────────
        [HttpPost]
        public IActionResult Close(int id)
        {
            var conv = _db.ChatConversations.Find(id);
            if (conv != null)
            {
                conv.DaDong = true;
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}