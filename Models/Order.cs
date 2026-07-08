// Models/Order.cs
namespace PetShop.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; } = "ChoXacNhan";
        // "ChoXacNhan" | "DangGiao" | "HoanThanh" | "DaHuy"
        public string DiaChiGiao { get; set; } = "";
        public DateTime NgayDat { get; set; } = DateTime.Now;

        // Navigation
        public User? User { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}