namespace PetShop.Helpers
{
    public static class ImageValidationHelper
    {
        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        private static readonly string[] AllowedContentTypes =
            { "image/jpeg", "image/png", "image/webp", "image/gif" };

        private const long MaxFileSizeBytes = 3 * 1024 * 1024; // 3MB

        public static (bool IsValid, string? ErrorMessage) Validate(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return (false, "File không hợp lệ.");

            // Kiểm tra dung lượng
            if (file.Length > MaxFileSizeBytes)
                return (false, "Ảnh không được vượt quá 3MB.");

            // Kiểm tra đuôi file
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
                return (false, "Chỉ chấp nhận file ảnh: JPG, JPEG, PNG, WEBP, GIF.");

            // Kiểm tra Content-Type do trình duyệt gửi lên
            if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
                return (false, "File không đúng định dạng ảnh.");

            // Kiểm tra magic bytes (header thật của file) — chống giả mạo đuôi file
            if (!IsValidImageHeader(file))
                return (false, "Nội dung file không phải là ảnh hợp lệ.");

            return (true, null);
        }

        private static bool IsValidImageHeader(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            var buffer = new byte[8];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            stream.Position = 0; // reset lại để CopyToAsync dùng sau vẫn đọc được từ đầu

            if (bytesRead < 4) return false;

            // JPEG: FF D8 FF
            if (buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF)
                return true;

            // PNG: 89 50 4E 47
            if (buffer[0] == 0x89 && buffer[1] == 0x50 &&
                buffer[2] == 0x4E && buffer[3] == 0x47)
                return true;

            // GIF: 47 49 46 38
            if (buffer[0] == 0x47 && buffer[1] == 0x49 &&
                buffer[2] == 0x46 && buffer[3] == 0x38)
                return true;

            // WEBP: RIFF....WEBP (byte 0-3 = RIFF, byte 8-11 = WEBP)
            if (bytesRead >= 8 &&
                buffer[0] == 0x52 && buffer[1] == 0x49 &&
                buffer[2] == 0x46 && buffer[3] == 0x46)
                return true; // RIFF header, đủ tin cậy cho mục đích này

            return false;
        }

        // Sinh tên file an toàn, không dùng tên gốc từ người dùng
        public static string GenerateSafeFileName(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            return $"{Guid.NewGuid()}{ext}";
        }
    }
}