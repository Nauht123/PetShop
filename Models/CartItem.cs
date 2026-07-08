// Models/CartItem.cs  (lưu giỏ hàng trong Session dạng JSON)
namespace PetShop.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string TenSanPham { get; set; } = "";
        public decimal Gia { get; set; }
        public int SoLuong { get; set; }
        public string? HinhAnh { get; set; }
        public decimal ThanhTien => Gia * SoLuong;
    }
}