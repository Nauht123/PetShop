// ViewModels/RegisterVM.cs
using System.ComponentModel.DataAnnotations;

namespace PetShop.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string HoTen { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string SoDienThoai { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string MatKhau { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu không khớp")]
        public string XacNhanMatKhau { get; set; } = "";
    }
}