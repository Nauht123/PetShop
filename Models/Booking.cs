// Models/Booking.cs
namespace PetShop.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ServiceId { get; set; }
        public DateTime NgayHen { get; set; }
        public string GioHen { get; set; } = "";
        public string? GhiChu { get; set; }
        public string TrangThai { get; set; } = "ChoXacNhan";
        // "ChoXacNhan" | "DaXacNhan" | "HoanThanh" | "DaHuy"

        // Navigation
        public User? User { get; set; }
        public Service? Service { get; set; }
    }
}