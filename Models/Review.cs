namespace PetShop.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int SoSao { get; set; }      // 1 - 5
        public string NoiDung { get; set; } = "";
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation
        public Product? Product { get; set; }
        public User? User { get; set; }
    }
}