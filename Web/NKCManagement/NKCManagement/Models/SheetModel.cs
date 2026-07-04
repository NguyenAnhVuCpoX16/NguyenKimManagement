namespace NKCManagement
{
    public class SheetModel
    {
        public string Name { get; set; } = DateTime.Now.ToString("mm");
        public List<RowModel> Rows { get; set; } = new();
    }

    public class RowModel 
    {
        public int RowIndex { get; set; }
        public List<CellModel> Cells { get; set; } = new();
    }
    public class CellModel
    {
        public int ColumnIndex { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}
