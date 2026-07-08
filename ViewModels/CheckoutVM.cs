using System.ComponentModel.DataAnnotations;

namespace PetShop.ViewModels
{
    public class CheckoutVM
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string HoTen { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string SoDienThoai { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        public string DiaChiGiao { get; set; } = "";

        public string? GhiChu { get; set; }
    }
}