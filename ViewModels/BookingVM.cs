using System.ComponentModel.DataAnnotations;

namespace PetShop.ViewModels
{
    public class BookingVM
    {
        [Required(ErrorMessage = "Vui lòng chọn dịch vụ")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string HoTen { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string SoDienThoai { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng chọn ngày hẹn")]
        public DateTime NgayHen { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Vui lòng chọn giờ hẹn")]
        public string GioHen { get; set; } = "";

        public string? GhiChu { get; set; }
    }
}