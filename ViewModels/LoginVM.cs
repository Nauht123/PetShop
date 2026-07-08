// ViewModels/LoginVM.cs
using System.ComponentModel.DataAnnotations;

namespace PetShop.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string MatKhau { get; set; } = "";
    }
}