// Models/Category.cs
namespace PetShop.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string TenDanhMuc { get; set; } = "";
        public string? MoTa { get; set; }
        public string? HinhAnh { get; set; }

        // Navigation
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}