// Models/Service.cs
namespace PetShop.Models
{
    public class Service
    {
        public int Id { get; set; }
        public string TenDichVu { get; set; } = "";
        public string? MoTa { get; set; }
        public decimal Gia { get; set; }
        public int ThoiGianPhut { get; set; } // thời gian thực hiện (phút)

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}