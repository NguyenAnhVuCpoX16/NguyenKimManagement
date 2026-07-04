namespace NKCManagement.Interface
{
    public interface IAiService
    {
        string Provider { get; }
        Task<string> Prompt(string prompt);
        string CreateRequest(List<string> list);
    }
}
