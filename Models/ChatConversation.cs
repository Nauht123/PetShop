using Microsoft.EntityFrameworkCore;
using PetShop.ViewModels;

namespace PetShop.Models
{
    public class ChatConversation
    {
        public int Id { get; set; }
        public int? UserId { get; set; }       // null nếu là khách vãng lai
        public string? GuestId { get; set; }   // GUID lưu Session cho khách chưa đăng nhập
        public string HoTen { get; set; } = "";
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime NgayCapNhat { get; set; } = DateTime.Now;
        public bool DaDong { get; set; } = false;

        public User? User { get; set; }
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

        public DbSet<ChatConversation> ChatConversations { get; set; }
    }
}