using ClosedXML.Excel;
using Microsoft.AspNetCore.Components.Forms;
using Models;
using Syncfusion.XlsIO;

namespace NKCManagement.Classes
{
    public class Excel
    {
        public static MemoryStream Write(IReadOnlyList<ParsedProduct> products,IReadOnlyList<ParseLog> logs)
        {
            var stream = new MemoryStream();
            using (var workbook = new XLWorkbook())
            {
                var grouped = products
                    .GroupBy(p => p.SheetName)
                    .OrderBy(g => g.Key);

                foreach (var group in grouped)
                {
                    var sheet = workbook.Worksheets.Add(group.Key);

                    WriteHeaders(sheet, ParsedProduct.OutputHeaders);

                    int rowIndex = 2;

                    foreach (var product in group.OrderBy(p => p.RowIndex))
                    {
                        WriteRow(sheet, rowIndex++, product.ToRow());
                    }
                }

                //var logSheet = workbook.Worksheets.Add("ParseLog");
                //WriteHeaders(logSheet, ParseLog.Headers);
                //for (int i = 0; i < logs.Count; i++)
                //{
                //    WriteRow(logSheet, i + 2, logs[i].ToRow());
                //}

                workbook.SaveAs(stream);
            }

            stream.Position = 0;

            return stream;
        }

        private static void WriteHeaders(IXLWorksheet sheet, string[] headers)
        {
            for (var i = 0; i < headers.Length; i++)
                sheet.Cell(1, i + 1).Value = headers[i];

            sheet.Row(1).Style.Font.Bold = true;
        }

        private static void WriteRow(IXLWorksheet sheet, int rowIndex, object[] values)
        {
            for (var i = 0; i < values.Length; i++)
                sheet.Cell(rowIndex, i + 1).Value = values[i]?.ToString() ?? string.Empty;
        }

        public static async Task<Dictionary<string, List<string>>> ReadAllSheetsAsync(Stream stream)
        {
            using var workbook = new XLWorkbook(stream);

            var result = new Dictionary<string, List<string>>();

            foreach (var worksheet in workbook.Worksheets)
            {
                var rows = new List<string>();

                var headerRow = worksheet.Row(1);
                var lastCol = worksheet.LastColumnUsed()?.ColumnNumber() ?? 1;

                int tenHangCol = 1;

                for (int col = 1; col <= lastCol; col++)
                {
                    var header = headerRow.Cell(col).GetString().Trim();

                    if (header.Equals("Ten_Hang_Raw", StringComparison.OrdinalIgnoreCase))
                    {
                        tenHangCol = col;
                        break;
                    }
                }

                var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

                for (int row = 2; row <= lastRow; row++)
                {
                    var raw = worksheet.Cell(row, tenHangCol).GetString().Trim();

                    if (!string.IsNullOrWhiteSpace(raw))
                        rows.Add(raw);
                }

                result[worksheet.Name] = rows;
            }

            return result;
        }
    }
}
