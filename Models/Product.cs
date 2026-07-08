// Models/Product.cs
namespace PetShop.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string TenSanPham { get; set; } = "";
        public string? MoTa { get; set; }
        public decimal Gia { get; set; }
        public int SoLuongKho { get; set; }
        public string? HinhAnh { get; set; }
        public int CategoryId { get; set; }

        // ── Mới thêm ──────────────────────────
        public bool IsHot { get; set; } = false;
        public bool IsBanChay { get; set; } = false;
        // Navigation
        public Category? Category { get; set; }
    }
}