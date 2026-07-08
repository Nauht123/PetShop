namespace PetShop.Models
{
    public class User
    {
        public int Id { get; set; }
        public string HoTen { get; set; } = "";
        public string Email { get; set; } = "";
        public string SoDienThoai { get; set; } = "";
        public string MatKhau { get; set; } = "";  // đã hash
        public string VaiTro { get; set; } = "KhachHang"; // "KhachHang" | "Admin"
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Các trường mới cho trang hồ sơ cá nhân
        public string? DiaChi1 { get; set; }
        public string? DiaChi2 { get; set; }
        public DateTime? NgaySinh { get; set; }
        public int DiemTichLuy { get; set; } = 0;
        // ── Quên mật khẩu ──────────────────────────────
        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordExpiry { get; set; }
    }
}