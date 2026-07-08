namespace PetShop.Models
{
    public class KhuyenMai
    {
        public int Id { get; set; }
        public string MaCode { get; set; } = "";        // VD: SUMMER20
        public string MoTa { get; set; } = "";
        public int LoaiGiam { get; set; }               // 1 = % , 2 = số tiền cố định
        public decimal GiaTriGiam { get; set; }         // 20 (%) hoặc 50000 (đ)
        public decimal DonHangToiThieu { get; set; }    // Đơn tối thiểu để áp dụng
        public decimal GiamToiDa { get; set; }          // Giảm tối đa (áp dụng khi % )
        public int SoLuong { get; set; }                // Số lần dùng còn lại
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public bool IsActive { get; set; } = true;
    }
}