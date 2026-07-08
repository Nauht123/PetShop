using System.Text;
using System.Text.Json;
using PetShop.ViewModels;

namespace PetShop.Services
{
    public class GeminiService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public GeminiService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task<string> AskAsync(string systemContext, List<ChatMessageVM> history, string userMessage)
        {
            var apiKey = _config["Gemini:ApiKey"];
            var model = _config["Gemini:Model"] ?? "gemini-2.0-flash";
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            // Xây dựng lịch sử hội thoại theo format Gemini
            var contents = new List<object>();

            // Đưa system context vào như 1 lượt mở đầu (Gemini không có role "system" riêng ở API cơ bản)
            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = systemContext } }
            });
            contents.Add(new
            {
                role = "model",
                parts = new[] { new { text = "Đã hiểu, tôi sẽ tư vấn dựa trên thông tin trên." } }
            });

            foreach (var h in history.TakeLast(10)) // giới hạn 10 lượt gần nhất, tránh prompt quá dài
            {
                contents.Add(new
                {
                    role = h.Role == "user" ? "user" : "model",
                    parts = new[] { new { text = h.Text } }
                });
            }

            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = userMessage } }
            });

            var body = new { contents };
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(url, content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Tạm thời in lỗi ra Console để debug
                Console.WriteLine("=== GEMINI API ERROR ===");
                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine($"Response: {responseText}");
                Console.WriteLine("========================");

                return "Xin lỗi, hệ thống tư vấn đang gặp sự cố. Vui lòng thử lại sau ít phút.";
            }

            using var doc = JsonDocument.Parse(responseText);
            try
            {
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? "Xin lỗi, tôi chưa có câu trả lời phù hợp.";
            }
            catch
            {
                return "Xin lỗi, tôi chưa có câu trả lời phù hợp lúc này.";
            }
        }
    }
}