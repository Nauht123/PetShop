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
    }
}