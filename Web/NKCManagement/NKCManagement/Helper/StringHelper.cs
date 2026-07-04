namespace NKCManagement
{
    public static class StringHelper
    {
        public static string GetAfterLast(this string str, string key)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            int idx = str.LastIndexOf(key);

            if (idx > -1)
                return str.Substring(idx + key.Length);
            else
                return str;
        }

        public static string GetBeforeLast(this string str, string key)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            int idx = str.LastIndexOf(key);

            if (idx > -1)
                return str.Substring(0, idx);
            else
                return str;
        }
        public static string AICleanResponse(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return text
                .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
                .Replace("```", "")
                .Replace("\uFEFF", "") 
                .Replace("\u200B", "")
                .Replace("\u200C", "")
                .Replace("\u200D", "")
                .Trim();
        }
    }
}
