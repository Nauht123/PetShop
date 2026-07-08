// Models/OrderDetail.cs
namespace PetShop.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }

        // Navigation
        public Order? Order { get; set; }
        public Product? Product { get; set; }
    }
}