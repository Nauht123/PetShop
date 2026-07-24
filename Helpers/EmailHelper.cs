using System.Net;
using System.Net.Mail;

namespace PetShop.Helpers
{
    public static class EmailHelper
    {
        public static void SendResetPasswordEmail(
            IConfiguration config, string toEmail, string resetLink)
        {
            var smtpHost = config["Smtp:Host"];
            var smtpPort = int.Parse(config["Smtp:Port"] ?? "587");
            var smtpUser = config["Smtp:Username"];
            var smtpPass = config["Smtp:Password"];
            var fromName = config["Smtp:FromName"] ?? "PetShop";

            var message = new MailMessage
            {
                From = new MailAddress(smtpUser!, fromName),
                Subject = "Đặt lại mật khẩu - PetShop",
                IsBodyHtml = true,
                Body = $@"
                    <div style='font-family:Arial,sans-serif; max-width:500px; margin:auto;'>
                        <h3>🐾 PetShop - Yêu cầu đặt lại mật khẩu</h3>
                        <p>Bạn (hoặc ai đó) vừa yêu cầu đặt lại mật khẩu cho tài khoản này.</p>
                        <p>Nhấn vào nút bên dưới để đặt mật khẩu mới. Link có hiệu lực trong <strong>15 phút</strong>.</p>
                        <p style='text-align:center; margin:24px 0'>
                            <a href='{resetLink}' 
                               style='background:#0d6efd;color:#fff;padding:12px 24px;
                                      border-radius:6px;text-decoration:none;font-weight:bold'>
                                Đặt lại mật khẩu
                            </a>
                        </p>
                        <p style='font-size:13px;color:#888'>
                            Nếu bạn không yêu cầu điều này, vui lòng bỏ qua email này.
                        </p>
                    </div>"
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            client.Send(message);
        }
        public static void SendOrderConfirmationEmail(
            IConfiguration config, string toEmail, string hoTen,
            int orderId, decimal tongTien, string diaChiGiao,
            List<(string TenSanPham, int SoLuong, decimal DonGia)> items)
        {
            var smtpHost = config["Smtp:Host"];
            var smtpPort = int.Parse(config["Smtp:Port"] ?? "587");
            var smtpUser = config["Smtp:Username"];
            var smtpPass = config["Smtp:Password"];
            var fromName = config["Smtp:FromName"] ?? "PetShop";

            var itemsHtml = new System.Text.StringBuilder();
            foreach (var item in items)
            {
                itemsHtml.Append($@"
            <tr>
                <td style='padding:8px; border-bottom:1px solid #eee'>{item.TenSanPham}</td>
                <td style='padding:8px; border-bottom:1px solid #eee; text-align:center'>{item.SoLuong}</td>
                <td style='padding:8px; border-bottom:1px solid #eee; text-align:right'>{item.DonGia:N0} đ</td>
            </tr>");
            }

            var message = new MailMessage
            {
                From = new MailAddress(smtpUser!, fromName),
                Subject = $"Xác nhận đơn hàng #{orderId} - PetShop",
                IsBodyHtml = true,
                Body = $@"
            <div style='font-family:Arial,sans-serif; max-width:600px; margin:auto;'>
                <h3>🐾 PetShop - Đặt hàng thành công!</h3>
                <p>Xin chào <strong>{hoTen}</strong>,</p>
                <p>Cảm ơn bạn đã đặt hàng tại PetShop. Đơn hàng của bạn đã được ghi nhận:</p>
                <p><strong>Mã đơn hàng:</strong> #{orderId}</p>
                <p><strong>Địa chỉ giao hàng:</strong> {diaChiGiao}</p>
                <table style='width:100%; border-collapse:collapse; margin-top:12px'>
                    <thead>
                        <tr style='background:#f5f5f5'>
                            <th style='padding:8px; text-align:left'>Sản phẩm</th>
                            <th style='padding:8px; text-align:center'>SL</th>
                            <th style='padding:8px; text-align:right'>Đơn giá</th>
                        </tr>
                    </thead>
                    <tbody>
                        {itemsHtml}
                    </tbody>
                </table>
                <p style='text-align:right; font-size:16px; margin-top:12px'>
                    <strong>Tổng cộng: <span style='color:#e53935'>{tongTien:N0} đ</span></strong>
                </p>
                <p style='margin-top:20px; color:#666; font-size:13px'>
                    Chúng tôi sẽ liên hệ xác nhận và giao hàng sớm nhất. Cảm ơn bạn đã tin tưởng PetShop!
                </p>
            </div>"
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            try
            {
                client.Send(message);
            }
            catch
            {
                // Không chặn luồng đặt hàng nếu gửi email thất bại
                // (VD: SMTP tạm thời lỗi, cấu hình sai...)
            }
        }
    }
}