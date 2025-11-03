using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class LoggerDiscord
    {
        // Thay thế bằng URL Webhook của bạn đã sao chép ở Bước 1
        private static readonly string WebhookUrl = "https://discordapp.com/api/webhooks/1388345974774300712/n4k5jxilGsB-rXMArmIIszr7nPgzkIS4RBow5EAK9CB9UxP23D1aD2H1bRxbWVv-IJLc";

        // Sử dụng HttpClient duy nhất để tối ưu hiệu suất
        private static readonly HttpClient _httpClient = new HttpClient();

        public static void InsertLogDiscord(string simpleLog)
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
                // --- Ví dụ 1: Gửi một tin nhắn văn bản đơn giản ---
                //simpleLog = "test Log thành công.";
                SendSimpleMessageAsync(simpleLog);

                Console.WriteLine("\nĐã gửi log. Vui lòng kiểm tra kênh Discord.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                // --- Ví dụ 2: Gửi một log chi tiết hơn bằng Embed ---
                string logTitle = "Cảnh báo Lỗi";
                string logDescription = ex.ToString();
                string fieldName = "Thời gian";
                string fieldValue = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string fieldName2 = "";
                string fieldValue2 = "";
                //// Màu đỏ cho lỗi
                int color = 0xFF0000; // Định dạng mã màu HEX: FF0000 (Đỏ)
                SendEmbedMessageAsync(logTitle, logDescription, color, fieldName, fieldValue, fieldName2, fieldValue2);
            }

        }

        /// <summary>
        /// Gửi một tin nhắn văn bản đơn giản tới webhook.
        /// </summary>
        /// <param name="message">Nội dung tin nhắn.</param>
        public static async Task SendSimpleMessageAsync(string message)
        {
            // Tạo payload JSON đơn giản
            var payload = new
            {
                content = message,
                username = "Log Notifier", // Tên hiển thị trên Discord
                avatar_url = "https://i.imgur.com/4M34hi2.png" // URL ảnh đại diện
            };

            // Chuyển đổi payload thành chuỗi JSON
            string jsonPayload = JsonConvert.SerializeObject(payload);

            // Tạo HttpContent
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                // Gửi yêu cầu POST
                HttpResponseMessage response = await _httpClient.PostAsync(WebhookUrl, content);

                // Kiểm tra kết quả phản hồi
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("✔️ Gửi tin nhắn đơn giản thành công.");
                }
                else
                {
                    Console.WriteLine($"❌ Gửi tin nhắn thất bại. Mã trạng thái: {response.StatusCode}");
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Chi tiết lỗi: {errorResponse}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Đã xảy ra lỗi khi gửi yêu cầu: {ex.Message}");
            }
        }

        /// <summary>
        /// Gửi một tin nhắn Embed được định dạng.
        /// </summary>
        public static async Task SendEmbedMessageAsync(string title, string description, int color, params string[] fields)
        {
            // Tạo danh sách fields từ tham số truyền vào
            var embedFields = new System.Collections.Generic.List<object>();
            for (int i = 0; i < fields.Length; i += 2)
            {
                if (i + 1 < fields.Length)
                {
                    embedFields.Add(new { name = fields[i], value = fields[i + 1], inline = true });
                }
            }

            // Tạo payload JSON với embed
            var payload = new
            {
                username = "Log Notifier",
                avatar_url = "https://i.imgur.com/4M34hi2.png",
                embeds = new[]
                {
                new
                {
                    title = title,
                    description = description,
                    color = color,
                    timestamp = DateTimeOffset.Now.ToString("o"), // ISO 8601 format
                    fields = embedFields
                }
            }
            };

            string jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(WebhookUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("✔️ Gửi Embed log thành công.");
                }
                else
                {
                    Console.WriteLine($"❌ Gửi Embed log thất bại. Mã trạng thái: {response.StatusCode}");
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Chi tiết lỗi: {errorResponse}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Đã xảy ra lỗi khi gửi yêu cầu: {ex.Message}");
            }
        }
    }
}
