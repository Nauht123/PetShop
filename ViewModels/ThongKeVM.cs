namespace PetShop.ViewModels
{
    public class ThongKeVM
    {
        // Bộ lọc
        public DateTime TuNgay { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime DenNgay { get; set; } = DateTime.Today;

        // Tổng quan
        public decimal TongDoanhThu { get; set; }
        public int TongDonHang { get; set; }
        public int DonHoanThanh { get; set; }
        public int DonDangGiao { get; set; }
        public int DonChoXacNhan { get; set; }
        public int DonDaHuy { get; set; }

        // Chi tiết theo ngày (cho biểu đồ)
        public List<DoanhThuNgay> DoanhThuTheoNgay { get; set; } = new();

        // Top sản phẩm bán chạy
        public List<TopSanPham> TopSanPhams { get; set; } = new();

        // Danh sách đơn hàng trong khoảng
        public List<PetShop.Models.Order> DonHangs { get; set; } = new();
    }

    public class DoanhThuNgay
    {
        public string Ngay { get; set; } = "";
        public decimal DoanhThu { get; set; }
        public int SoDon { get; set; }
    }

    public class TopSanPham
    {
        public string TenSanPham { get; set; } = "";
        public int SoLuongBan { get; set; }
        public decimal DoanhThu { get; set; }
        public string? HinhAnh { get; set; }
    }
}