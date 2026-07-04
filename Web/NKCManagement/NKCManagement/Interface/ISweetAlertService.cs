namespace NKCManagement
{
    public interface ISweetAlertService
    {
        Task Success(string message, string title = "");

        Task Error(string message, string title = "");

        Task Warning(string message, string title = "");

        Task<bool> Confirm(string message, string title = "");
        Task Loading(string title = "Loading...");

        Task Close();

        Task Toast(string message);
    }
}
