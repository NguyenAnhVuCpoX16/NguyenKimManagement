using Models;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NKCManagement
{
    public class ParsedService
    {
        private static readonly PropertyInfo[] AiProperties =
                typeof(ParsedProduct)
                .GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(AiFieldAttribute)))
                .ToArray();
        private static readonly string[] KnownBrands = new string[]
        {
            "Samsung", "Lenovo", "Dell", "Acer", "Asus", "ASUS", "HP", "Apple", "Macbook",
            "REDMI", "Redmi", "TOMKO", "Amdox", "Yite", "Microsoft", "Huawei", "Xiaomi",
            "MSI", "Gigabyte", "Intel", "Tomko"
        };

        public static string CreateRequestAI(List<ParsedProduct> products)
        {
            if (products == null || products.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();

            sb.AppendLine("Extract only the requested fields from each product description.");
            sb.AppendLine();

            sb.AppendLine("Field definitions:");
            sb.AppendLine("- Brand: manufacturer");
            sb.AppendLine("- Model: model name");
            sb.AppendLine("- PartNumber: P/N, SKU, MPN");
            sb.AppendLine("- Cpu: processor");
            sb.AppendLine("- Ram: memory");
            sb.AppendLine("- Storage: SSD/HDD capacity");
            sb.AppendLine("- Gpu: graphics");
            sb.AppendLine("- TinhTrang: condition");
            sb.AppendLine("- ManHinh: screen");
            sb.AppendLine("- Pin: battery");
            sb.AppendLine("- HeDieuHanh: operating system");
            sb.AppendLine("- Khac: remaining specifications");
            sb.AppendLine();

            sb.AppendLine("Rules:");
            sb.AppendLine("- Return ONLY a JSON array.");
            sb.AppendLine("- One JSON object per product.");
            sb.AppendLine("- Each object matches the input order.");
            sb.AppendLine("- Include ONLY the requested fields.");
            sb.AppendLine("- Unknown => null.");
            sb.AppendLine("- Do not guess.");
            sb.AppendLine("- No markdown.");
            sb.AppendLine("- No explanation.");
            sb.AppendLine();

            sb.AppendLine("Products:");

            foreach (var item in products.Where(x => x.MissingFields?.Count > 0))
            {
                sb.AppendLine($"Fields: {string.Join(", ", item.MissingFields)}");
                sb.AppendLine($"Description: {item.TenHangRaw}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static async Task<List<ParsedProduct>> ParsedAi(List<ParsedProduct> products)
        {
            var request = CreateRequestAI(products);

            if (string.IsNullOrWhiteSpace(request))
                return products;

            try
            {
                var response = await AppStatic.AiProcess.Prompt(request);

                if (response == null)
                    return products;

                var aiResults = JsonSerializer.Deserialize<List<ParsedProduct>>(response.AICleanResponse());

                if (aiResults == null)
                {
                    foreach (var item in products)
                    {
                        item.LogAiResult =
                            $"{item.SheetName} - {item.RowIndex}: Không deserialize được kết quả AI.";
                    }

                    return products;
                }

                int count = Math.Min(products.Count, aiResults.Count);

                for (int i = 0; i < count; i++)
                {
                    var source = products[i];
                    var ai = aiResults[i];

                    foreach (var property in AiProperties)
                    {
                        var currentValue = property.GetValue(source) as string;

                        if (!string.IsNullOrWhiteSpace(currentValue))
                            continue;

                        var newValue = property.GetValue(ai) as string;

                        if (!string.IsNullOrWhiteSpace(newValue))
                            property.SetValue(source, newValue);
                    }
                }

                return products;
            }
            catch (Exception ex)
            {
                foreach (var item in products)
                {
                    item.LogAiResult =
                        $"{item.SheetName} - {item.RowIndex} - {item.TenHangRaw}: {ex.Message}";
                }

                return products;
            }
        }

        public static async Task<ParsedProduct> ParseCode(string raw)
        {
            var text = Normalize(raw);
            var result = new ParsedProduct { TenHangRaw = raw.Trim() };
            ExtractTinhTrang(text, result);
            ExtractLoaiThietBi(text, result);
            ExtractBrand(text, result);
            ExtractModel(text, result);
            ExtractPartNumber(text, result);
            ExtractCpu(text, result);
            //ExtractRam(text, result);
            //ExtractStorage(text, result);
            ExtractGpu(text, result);
            ExtractManHinh(text, result);
            ExtractPin(text, result);
            ExtractHeDieuHanh(text, result);
            ExtractKhac(text, result);
            result.MissingFields = GetNullProperties(result);
            return result;
        }

        public static List<string> GetNullProperties<T>(T obj)
        {
            if (obj == null)
                return new List<string>();
            var result = new List<string>();
            foreach (var property in AiProperties)
            {
                var value = property.GetValue(obj);

                if (value == null ||
                    (value is string s && string.IsNullOrWhiteSpace(s)))
                {
                    result.Add(property.Name);
                }
            }

            return result;
        }

        private static string Normalize(string raw) =>
            Regex.Replace(raw.Trim(), @"\s+", " ");

        private static void ExtractTinhTrang(string text, ParsedProduct r)
        {
            if (Regex.IsMatch(text, @"(?i)(h[àa]ng\s+m[ớo]i\s*100%|m[ớo]i\s*100%|hang\s+moi\s*100%)"))
                r.TinhTrang = "Mới 100%";
        }

        private static void ExtractLoaiThietBi(string text, ParsedProduct r)
        {
            if (Regex.IsMatch(text, @"(?i)m[áa]y\s+t[íi]nh\s+b[ảa]ng"))
                r.LoaiThietBi = "Máy tính bảng";
            else if (Regex.IsMatch(text, @"(?i)m[áa]y\s+(vi\s+)?t[íi]nh\s+x[áa]ch\s+tay"))
                r.LoaiThietBi = "Laptop";
            else if (Regex.IsMatch(text, @"(?i)m[áa]y\s+(vi\s+)?t[íi]nh\s+(c[áa]\s+nh[âa]n\s+)?(đ[ểe]\s+b[àa]n|đ[ểe]\s+b[àa]n)"))
                r.LoaiThietBi = "Desktop";
            else if (Regex.IsMatch(text, @"(?i)(b[ộo]\s+m[áa]y\s+x[ửu]\s+l[ý]|m[áa]y\s+x[ửu]\s+l[íi]\s+d[ữu]\s+li[ệe]u)"))
                r.LoaiThietBi = "Desktop";
        }

        private static void ExtractBrand(string text, ParsedProduct r)
        {
            var m = Regex.Match(text, @"(?i)hi[ệe]u\s+([A-Za-z0-9]+)");
            if (m.Success) { r.Brand = NormalizeBrand(m.Groups[1].Value); return; }

            m = Regex.Match(text, @"(?i)\bNH\s*:\s*([A-Za-z0-9]+)");
            if (m.Success) { r.Brand = NormalizeBrand(m.Groups[1].Value); return; }

            if (Regex.IsMatch(text, @"(?i)\b(Macbook|MBA|MBP)\b"))
            {
                r.Brand = "Apple";
                return;
            }

            foreach (var brand in KnownBrands.OrderByDescending(b => b.Length))
            {
                if (Regex.IsMatch(text, $@"(?i)\b{Regex.Escape(brand)}\b"))
                {
                    r.Brand = NormalizeBrand(brand);
                    return;
                }
            }
        }

        private static string NormalizeBrand(string brand)
        {
            if (string.IsNullOrWhiteSpace(brand))
                return brand;

            switch (brand.ToUpperInvariant())
            {
                case "MACBOOK":
                case "MBA":
                case "MBP":
                    return "Apple";

                case "ASUS":
                    return "Asus";

                case "REDMI":
                    return "Redmi";

                case "TOMKO":
                    return "TOMKO";

                default:
                    return char.ToUpper(brand[0]) +
                           brand.Substring(1).ToLowerInvariant();
            }
        }

        private static void ExtractModel(string text, ParsedProduct r)
        {
            var patterns = new[]
            {
            @"(?i)Model\s*No\.?\s*:\s*([A-Za-z0-9\-]+)",
            @"(?i)Model\s*:\s*([A-Za-z0-9\-]+(?:\s+Gen\s*\d+)?)",
            @"(?i)model\s*:\s*([A-Za-z0-9\-]+)",
            @"(?i)RMN\s*:\s*\w+\s*\(([^)]+)\)",
            @"(?i)ThinkPad\s+([A-Z]\d+(?:\s+Gen\s*\d+)?)",
            @"(?i)ThinkCentre\s+([A-Za-z0-9\s]+?)(?:/|,|\s+P/N)",
            @"(?i)ThinkSmart\s+([A-Za-z0-9\s]+?)(?:/|,|\s+P/N)",
            @"(?i)Galaxy\s+Tab\s+([A-Za-z0-9\s]+?)(?:\.|,|$)",
            @"(?i)ProBook\s+(\d+\s+G\d+)",
            @"(?i)Pro\s+Tower\s+([A-Za-z0-9]+)",
            @"(?i)model\s+Tower\s+([A-Za-z0-9]+)",
            @"(?i)VERITON\s+([A-Za-z0-9]+)",
            @"(?i)_Model\s*:\s*([A-Za-z0-9]+)"
        };

            foreach (var pattern in patterns)
            {
                var m = Regex.Match(text, pattern);
                if (m.Success)
                {
                    r.Model = m.Groups[1].Value.Trim().TrimEnd('.');
                    return;
                }
            }
        }

        private static void ExtractPartNumber(string text, ParsedProduct r)
        {
            var patterns = new[]
            {
            @"(?i)P/N\s*:\s*([A-Za-z0-9\.\-/]+)",
            @"(?i)PN\s*:\s*([A-Za-z0-9\.\-/]+)",
            @"(?i)M[ãa]\s+s[ảa]n\s+ph[ẩa]m\s*:\s*([A-Za-z0-9\.\-/]+)",
            @"(?i)NH\.[A-Z0-9\.\-]+",
            @"^([A-Z0-9]+)\s*:\s"
        };

            foreach (var pattern in patterns)
            {
                var m = Regex.Match(text, pattern);
                if (m.Success)
                {
                    r.PartNumber = m.Groups.Count > 1 ? m.Groups[1].Value.Trim().TrimEnd('.') : m.Value.Trim().TrimEnd('.');
                    return;
                }
            }
        }

        private static void ExtractCpu(string text, ParsedProduct r)
        {
            var patterns = new[]
            {
            @"(?i)Intel\s+Core\s+(?:Ultra\s+)?[\w\s\-]+?(?=\s*[\(/,]|$)",
            @"(?i)Intel\s+Ci\d-\d+",
            @"(?i)Core\s+[iIuU]\d[\w\-]*",
            @"(?i)AMD\s+Ryzen\s+[\w\s\-]+",
            @"(?i)CPU\s*:\s*([^,\./]+)",
            @"(?i)MTK\s*[A-Za-z0-9\s]+",
            @"(?i)MTK6769\s+octa\s+core",
            @"(?i)Intel\s+Core\s+Ultra"
        };

            foreach (var pattern in patterns)
            {
                var m = Regex.Match(text, pattern);
                if (m.Success)
                {
                    string cpu = Regex.Replace(
                                m.Value,
                                "CPU:",
                                "",
                                RegexOptions.IgnoreCase);
                    r.Cpu = CleanSpec(cpu.Trim());
                    return;
                }
            }
        }

        private static void ExtractRam(string text, ParsedProduct r)
        {
            var patterns = new[]
            {
            @"(?i)Ram\s*:\s*(\d+)\s*GB",
            @"(?i)(?:\(|,|\s)(\d+)\s*GB\s*RAM",
            @"(?i)(\d+)GB\s*Ram\b",
            @"(?i)(\d+)GB\s*RAM\b",
            @"(?i)(\d+)G\s*RAM\b",
            @"(?i)1\s*\*\s*(\d+)G\b",
            @"(?i)/(\d+)GB(?=/|\s|,|$)"
        };

            foreach (var pattern in patterns)
            {
                var m = Regex.Match(text, pattern);
                if (m.Success)
                {
                    r.Ram = $"{m.Groups[1].Value}GB";
                    return;
                }
            }
        }

        private static void ExtractStorage(string text, ParsedProduct r)
        {
            var patterns = new[]
            {
            @"(?i)Rom\s*:\s*(\d+)\s*GB",
            @"(?i)(?:ổ\s+)?SSD\s+(\d+)\s*GB",
            @"(?i)(\d+)\s*GB\s*SSD",
            @"(?i)(\d+)GB\s*SSD",
            @"(?i)(\d+)G\s*SSD",
            @"(?i)(\d+)GB\s*Rom\b",
            @"(?i)/(\d+)T(?:B)?(?:\b|[-_])",
            @"(?i)/(\d+)TB\b",
            @"(?i)(\d+)G\s*ROM\b"
        };

            foreach (var pattern in patterns)
            {
                var m = Regex.Match(text, pattern);
                if (m.Success)
                {
                    var val = m.Groups[1].Value;
                    r.Storage = val.EndsWith("GB", StringComparison.OrdinalIgnoreCase) ||
                                val.EndsWith("TB", StringComparison.OrdinalIgnoreCase)
                        ? val.ToUpperInvariant()
                        : int.TryParse(val, out var n) && n <= 4 ? $"{val}GB" : $"{val}GB";
                    if (Regex.IsMatch(text, $@"(?i){val}\s*T\b") || Regex.IsMatch(text, $@"(?i)/{val}T\b"))
                        r.Storage = $"{val}TB";
                    return;
                }
            }
        }

        private static void ExtractGpu(string text, ParsedProduct r)
        {
            var patterns = new[]
            {
            @"(?i)VGA\s*:\s*([^,\.]+)",
            @"(?i)RTX\s*\d+/?\d*\s*GB?",
            @"(?i)Integrated\s+(?:Intel\s+)?Graphics",
            @"(?i)UHD\s+Graphics\s*\d*",
            @"(?i)\d+C\s+GPU",
            @"(?i)Integrated\s+Graphics"
        };

            foreach (var pattern in patterns)
            {
                var m = Regex.Match(text, pattern);
                if (m.Success)
                {
                    string cpu = Regex.Replace(
                                    m.Value,
                                    "VGA:",
                                    "",
                                    RegexOptions.IgnoreCase);
                    r.Gpu = CleanSpec(cpu.Trim());
                    return;
                }
            }
        }

        private static void ExtractManHinh(string text, ParsedProduct r)
        {
            var patterns = new[]
            {
            @"(?i)M[àa]n\s+h[ìi]nh\s*:\s*([\d.]+"")",
            @"(?i)M\.h[ìi]nh\s*:\s*([\d.]+""[^,]*)",
            @"(?i)(\d+\.?\d*)\s*inch",
            @"(?i)(\d+\.?\d*)""\s*(?:FHD|WUXGA|WUXGA_AG_\d+N)?",
            @"(?i)(\d+\.?\d*)""\s*\("
        };

            foreach (var pattern in patterns)
            {
                var m = Regex.Match(text, pattern);
                if (m.Success)
                {
                    var val = m.Groups[1].Value.Trim();
                    r.ManHinh = val.Contains('"') ? val : $"{val}\"";
                    return;
                }
            }
        }

        private static void ExtractPin(string text, ParsedProduct r)
        {
            var m = Regex.Match(text, @"(?i)(\d+)\s*mAh");
            if (m.Success)
                r.Pin = $"{m.Groups[1].Value}mAh";
        }

        private static void ExtractHeDieuHanh(string text, ParsedProduct r)
        {
            var patterns = new[]
            {
            @"(?i)Windows\s+\d+(?:\s+\w+)?",
            @"(?i)Win\s*\d+(?:\s+\w+)?",
            @"(?i)Win11\w*",
            @"(?i)EndlessOS",
            @"(?i)\bNOS\b",
            @"(?i)No\s+OS",
            @"(?i)Android\s*\d*",
            @"(?i)Office\s+Home\s+\d+"
        };

            foreach (var pattern in patterns)
            {
                var m = Regex.Match(text, pattern);
                if (m.Success)
                {
                    r.HeDieuHanh = CleanSpec(m.Value);
                    return;
                }
            }
        }

        private static void ExtractKhac(string text, ParsedProduct r)
        {
            var extras = new List<string>();

            foreach (Match m in Regex.Matches(text, @"(?i)m[àa]u\s+[a-zà-ỹ\s]+"))
                extras.Add(m.Value.Trim().TrimEnd('.'));

            foreach (Match m in Regex.Matches(text, @"(?i)b[ăa]ng\s+t[ầa]n\s+[\d.]+\s*GHz"))
                extras.Add(m.Value.Trim());

            foreach (Match m in Regex.Matches(text, @"(?i)(?:truy[eê]n|truynhap)[^,\.]{0,40}5\s*GHz"))
                extras.Add(m.Value.Trim());

            if (Regex.IsMatch(text, @"(?i)kh[ôo]ng\s+c[óo]\s+m[àa]n\s+h[ìi]nh"))
                extras.Add("Không có màn hình");

            if (Regex.IsMatch(text, @"(?i)k[èe]m\s+chu[ộo]t"))
                extras.Add("kèm chuột");

            if (Regex.IsMatch(text, @"(?i)b[ảoa]o\s+h[àa]nh\s+\d+\s*Th[áa]ng"))
            {
                var m = Regex.Match(text, @"(?i)b[ảoa]o\s+h[àa]nh\s+\d+\s*Th[áa]ng");
                extras.Add(m.Value);
            }

            var colorInParen = Regex.Match(text, @"(?i)\([^)]*m[àa]u\s+[^)]+\)");
            if (colorInParen.Success)
            {
                var inner = colorInParen.Value.Trim('(', ')');
                if (inner.Contains(':'))
                    extras.Add(inner.Split(':')[1].Trim());
            }

            if (extras.Count > 0)
                r.Khac = string.Join(", ", extras.Distinct(StringComparer.OrdinalIgnoreCase));
        }

        private static string CleanSpec(string value) =>
            Regex.Replace(value.Trim().TrimEnd('.', ','), @"\s+", " ");
    }
}
