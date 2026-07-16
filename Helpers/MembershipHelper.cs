namespace PetShop.Helpers
{
    public class MembershipRank
    {
        public string TenHang { get; set; } = "";
        public string Icon { get; set; } = "";
        public string MauChu { get; set; } = "";      // class Bootstrap text-*
        public string MauNen { get; set; } = "";       // màu nền cho badge
        public decimal PhanTramGiam { get; set; }       // % giảm giá tự động khi checkout
        public int DiemToiThieu { get; set; }
        public int? DiemCanDeLenHang { get; set; }       // null nếu đã ở hạng cao nhất
        public string? TenHangKeTiep { get; set; }
        public double PhanTramTienDo { get; set; }       // % thanh tiến độ tới hạng kế tiếp
    }

    public static class MembershipHelper
    {
        // Định nghĩa các mốc hạng: (Tên, Icon, màu chữ, màu nền, % giảm giá, điểm tối thiểu)
        private static readonly (string Ten, string Icon, string MauChu, string MauNen, decimal PhanTram, int DiemToiThieu)[] Hangs =
        {
            ("Đồng",       "🥉", "text-secondary", "#6c757d1a", 0m,   0),
            ("Bạc",        "🥈", "text-info",       "#0dcaf01a", 2m, 100),
            ("Vàng",       "🥇", "text-warning",    "#ffc1071a", 5m, 300),
            ("Bạch Kim",   "🔷", "text-dark",       "#adb5bd33", 8m, 700),
            ("Kim Cương",  "💎", "text-primary",    "#0d6efd1a", 15m, 1500),
        };

        public static MembershipRank GetRank(int diemTichLuy)
        {
            // Tìm hạng hiện tại: hạng cao nhất mà điểm hiện có vẫn đạt đủ điều kiện
            int index = 0;
            for (int i = 0; i < Hangs.Length; i++)
            {
                if (diemTichLuy >= Hangs[i].DiemToiThieu)
                    index = i;
            }

            var hienTai = Hangs[index];
            var ketQua = new MembershipRank
            {
                TenHang = hienTai.Ten,
                Icon = hienTai.Icon,
                MauChu = hienTai.MauChu,
                MauNen = hienTai.MauNen,
                PhanTramGiam = hienTai.PhanTram,
                DiemToiThieu = hienTai.DiemToiThieu
            };

            // Nếu chưa phải hạng cao nhất, tính khoảng cách tới hạng kế tiếp
            if (index < Hangs.Length - 1)
            {
                var ketTiep = Hangs[index + 1];
                ketQua.TenHangKeTiep = ketTiep.Ten;
                ketQua.DiemCanDeLenHang = ketTiep.DiemToiThieu - diemTichLuy;

                int khoangCach = ketTiep.DiemToiThieu - hienTai.DiemToiThieu;
                int daDat = diemTichLuy - hienTai.DiemToiThieu;
                ketQua.PhanTramTienDo = khoangCach > 0
                    ? Math.Min(100, Math.Max(0, (double)daDat / khoangCach * 100))
                    : 100;
            }
            else
            {
                // Đã ở hạng cao nhất
                ketQua.TenHangKeTiep = null;
                ketQua.DiemCanDeLenHang = null;
                ketQua.PhanTramTienDo = 100;
            }

            return ketQua;
        }
    }
}