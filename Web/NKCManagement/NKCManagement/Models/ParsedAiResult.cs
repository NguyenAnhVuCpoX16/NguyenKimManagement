using Models;

namespace NKCManagement
{
    public class ParsedAiResult
    {
        public bool Success { get; set; }
        public ParsedProduct? Product { get; set; }
        public string? Error { get; set; }
    }
}
