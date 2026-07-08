using System.ComponentModel.DataAnnotations;

namespace PetShop.ViewModels
{
    public class ProfileVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string HoTen { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string SoDienThoai { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";

        public string? DiaChi1 { get; set; }
        public string? DiaChi2 { get; set; }
        public DateTime? NgaySinh { get; set; }
    }

    public class DoiMatKhauVM
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        public string MatKhauCu { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string MatKhauMoi { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu không khớp")]
        public string XacNhanMatKhau { get; set; } = "";
    }
}