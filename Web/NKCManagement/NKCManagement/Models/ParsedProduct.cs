using NKCManagement;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class ParsedProduct
    {
        public string TenHangRaw { get; set; } = string.Empty;

        [AiFieldAttribute("CellExcel")]
        public string LoaiThietBi { get; set; } = "";

        [AiFieldAttribute("CellExcel")]
        public string Brand { get; set; } = "";
        [AiFieldAttribute("CellExcel")]

        public string Model { get; set; } = "";
        [AiFieldAttribute("CellExcel")]
        public string PartNumber { get; set; } = "";
        [AiFieldAttribute("CellExcel")]
        public string Cpu { get; set; } = "";
        [AiFieldAttribute("CellExcel")]
        public string Ram { get; set; } = "";

        [AiFieldAttribute("CellExcel")]
        public string Storage { get; set; } = "";

        [AiFieldAttribute("CellExcel")]
        public string Gpu { get; set; } = "";

        [AiFieldAttribute("CellExcel")]
        public string TinhTrang { get; set; } = "";
        [AiFieldAttribute("CellExcel")]
        public string ManHinh { get; set; } = "";

        [AiFieldAttribute("CellExcel")]
        public string Pin { get; set; } = "";

        [AiFieldAttribute("CellExcel")]
        public string HeDieuHanh { get; set; } = "";

        [AiFieldAttribute("CellExcel")]
        public string Khac { get; set; } = "";

        public string SheetName { get; set; } = string.Empty;
        public int RowIndex { get; set; }

        public List<string> MissingFields { get; set; } = new List<string>();
        public int Confidence { get; set; }


        public static readonly string[] OutputHeaders = new string[]
        {
            "Ten_Hang_Raw", "Loai_Thiet_Bi", "Brand", "Model", "Part_Number",
        "CPU", "RAM", "Storage", "GPU", "Tinh_Trang", "Man_Hinh", "Pin",
        "He_Dieu_Hanh", "Khac"
        };

        public object[] ToRow() => new object[]
        {
            TenHangRaw, LoaiThietBi, Brand, Model, PartNumber,
            Cpu, Ram, Storage, Gpu, TinhTrang, ManHinh, Pin, HeDieuHanh, Khac
        };

        public void ComputeConfidence()
        {
            var coreFields = new (string Name, string Value)[]
            {
            ("Loai_Thiet_Bi", LoaiThietBi),
            ("Brand", Brand),
            ("Model", Model),
            ("Part_Number", PartNumber),
            ("Tinh_Trang", TinhTrang)
            };

            MissingFields.Clear();
            var filled = 0;
            foreach (var (name, value) in coreFields)
            {
                if (IsNull(value))
                    MissingFields.Add(name);
                else
                    filled++;
            }

            Confidence = coreFields.Length == 0 ? 0 : (int)Math.Round(100.0 * filled / coreFields.Length);
        }

        public bool HasLowConfidence => Confidence < 70;

        private static bool IsNull(string value) =>
            string.IsNullOrWhiteSpace(value) || value == "";
    }

}
