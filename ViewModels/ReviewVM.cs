using System.ComponentModel.DataAnnotations;

namespace PetShop.ViewModels
{
    public class ReviewVM
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn số sao")]
        [Range(1, 5, ErrorMessage = "Số sao từ 1 đến 5")]
        public int SoSao { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá")]
        [MinLength(10, ErrorMessage = "Nội dung tối thiểu 10 ký tự")]
        public string NoiDung { get; set; } = "";
    }
}