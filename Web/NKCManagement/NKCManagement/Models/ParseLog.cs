namespace Models
{
    public class ParseLog
    {
        public string Sheet { get; set; } = string.Empty;
        public int Row { get; set; }
        public string TenHangRaw { get; set; } = string.Empty;
        public string MissingFields { get; set; } = string.Empty;
        public int Confidence { get; set; }

        public static readonly string[] Headers = new string[] { "Sheet", "Row", "Ten_Hang_Raw", "MissingFields", "Confidence" };

        public object[] ToRow() => new object[]{ Sheet, Row, TenHangRaw, MissingFields, Confidence };
    }
}
