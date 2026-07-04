using DocumentFormat.OpenXml.Office2016.Excel;
using Google.GenAI;
using Google.GenAI.Types;
using MudBlazor;
using NKCManagement.Interface;
using Syncfusion.Blazor.Inputs;
using System.Diagnostics;

namespace NKCManagement
{
    public class GoogleGenmini : IAiService
    {
        public const string OpenKeyAI = "AQ.Ab8RN6IrbiNC88kSRa0kH8WKTYxNTqtl9xMs4gSbmjyJ6WjeUA";
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        static int i = 0;
        Google.GenAI.Client Client = new Google.GenAI.Client(apiKey: OpenKeyAI);
        static Stopwatch Sw = new();

        public string Provider => "Google.Genmini";

        public async Task<string> Prompt(string prompt)
        {
            await _semaphore.WaitAsync();
            Sw.Restart();
            try
            {
                Console.WriteLine($"Prompt length: {prompt.Length}");
                Console.WriteLine(i++);
                var response = await Client.Models.GenerateContentAsync(
                                    model: "gemini-2.5-flash-lite",
                                    contents: prompt,
                                    config: new GenerateContentConfig
                                    {
                                        Temperature = 0,
                                        ResponseMimeType = "application/json"
                                    });

                Sw.Stop();
                Console.WriteLine($"Gemini: {Sw.ElapsedMilliseconds} ms");
                return response.Text;
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

        public string CreateRequest(List<string> list)
        {
            return $$"""
                Trích xuất mỗi dòng thành một object JSON.

                Fields:
                TenHangRaw, LoaiThietBi, Brand, Model, PartNumber, CPU, RAM,
                Storage, GPU, TinhTrang, ManHinh, Pin, HeDieuHanh, Khac.

                Rules:
                - JSON array only.
                - Unknown => null.
                - No markdown.
                - No explanation.
                - No invented values.

                Input:
                {{string.Join('\n', list)}}
                """;
        }
    }
}