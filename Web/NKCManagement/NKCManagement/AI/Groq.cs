using Microsoft.AspNetCore.DataProtection.KeyManagement;
using NKCManagement.Interface;
using System.Text;
using System.Text.Json;

namespace NKCManagement
{
    public class Groq : IAiService
    {
        public string Provider => "Groq";
        private readonly HttpClient _http = new();
        public string CreateRequest(List<string> list)
        {
            return $$"""
                            Bạn là chuyên gia phân tích thông tin thiết bị CNTT.
                            Hãy phân tích chuỗi dưới đây và trả về list json đúng theo cấu trúc:

                            {
                                "TenHangRaw": "",
                                "LoaiThietBi": "",
                                "Brand": "",
                                "Model": "",
                                "PartNumber": "",
                                "CPU": "",
                                "RAM": "",
                                "Storage": "",
                                "GPU": "",
                                "TinhTrang": "",
                                "ManHinh": "",
                                "Pin": "",
                                "HeDieuHanh": "",
                                "Khac": ""
                            }

                            Quy tắc:
                            - không cần kí tự thừa
                            - Chỉ trả về JSON.
                            - Không giải thích.
                            - Không dùng Markdown.
                            - Không thêm thuộc tính.
                            - Giá trị không xác định để null.
                            - Không tự suy đoán.
                            - Khác chứa thông tin chưa được phân loại.

                            Ý nghĩa các thuộc tính:

                            - TenHangRaw: Chuỗi gốc.
                            - LoaiThietBi: Laptop, Desktop, Mini PC, Tablet, Monitor, Printer, Server, Workstation, All In One...
                            - Brand: Dell, HP, Lenovo, Asus, Acer, Apple, MSI...
                            - Model: Latitude 7440, ThinkPad T14 Gen 5...
                            - PartNumber: P/N, MTM, SKU...
                            - CPU: Intel Core i7-1365U, Intel Core Ultra 7 165H, AMD Ryzen 7 8845HS...
                            - RAM: 16GB DDR5...
                            - Storage: 512GB SSD, 1TB NVMe...
                            - GPU: Intel Iris Xe, RTX 4060...
                            - TinhTrang: New, Used, Like New, Refurbished...
                            - ManHinh: 14 inch FHD IPS...
                            - Pin: 56Wh, 3 Cell...
                            - HeDieuHanh: Windows 11 Pro, Ubuntu...
                            - Khac: Các thông tin còn lại.
                            Chuỗi cần phân tích:
                            {{string.Join('\n', list)}}
                            """;
        }

        public async Task<string> Prompt(string prompt)
        {
            try
            {
                _http.DefaultRequestHeaders.Clear();
                _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {AppStatic.AIKey?.Groq ?? "---------------------------------"}");
                var body = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = new[]
                    {
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                    temperature = 0
                };
                var json = JsonSerializer.Serialize(body);

                var response = await _http.PostAsync(
                    "https://api.groq.com/openai/v1/chat/completions",
                    new StringContent(json, Encoding.UTF8, "application/json"));

                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(result);

                return doc.RootElement
                          .GetProperty("choices")[0]
                          .GetProperty("message")
                          .GetProperty("content")
                          .GetString()!;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
    }
}
