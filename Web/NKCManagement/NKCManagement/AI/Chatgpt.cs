using Google.GenAI.Types;
using NKCManagement.Interface;
using OpenAI.Chat;

namespace NKCManagement
{
    public class Chatgpt : IAiService
    {
        public const string OpenKeyAI = "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";
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private ChatClient Client = new ChatClient("gpt-4o",OpenKeyAI);

        public string Provider => "ChatGpt";

        public string CreateRequest(List<string> list)
        {
            return  $$"""
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
                            {{string.Join('\n',list)}}
                            """;
            //return $$"""
            //    Trích xuất mỗi dòng thành một object JSON.

            //    Fields:
            //    TenHangRaw, LoaiThietBi, Brand, Model, PartNumber, CPU, RAM,
            //    Storage, GPU, TinhTrang, ManHinh, Pin, HeDieuHanh, Khac.

            //    Rules:
            //    - Chỉ cần trả lời list, không trả lời dư thừa
            //    - JSON array only.
            //    - Unknown => null.
            //    - No markdown.
            //    - No explanation.
            //    - No invented values.

            //    Input:
            //    {{string.Join('\n', list)}}
            //    """;
        }

        public async Task<string> Prompt(string prompt)
        {
            await _semaphore.WaitAsync();
            try
            {
                var response = await Client.CompleteChatAsync(prompt);
                if(response.Value.Content.Count == 0)
                {
                    throw new Exception("No response from ChatGPT");
                }
            
                return response.Value.Content[0].Text;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
