using System.ComponentModel.DataAnnotations;

namespace PetShop.ViewModels
{
    public class ForgotPasswordVM
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";
    }
}