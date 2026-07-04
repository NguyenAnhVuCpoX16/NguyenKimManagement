using MudBlazor;

namespace NKCManagement
{
    public class SnackbarService
    {
        private ISnackbar? _snackbar;

        public void Initialize(ISnackbar snackbar)
        {
            _snackbar = snackbar;
        }

        public void Clear()
        {
            _snackbar?.Clear();
        }

        public void Success(string message)
        {
            Show(message, Severity.Success);
        }

        public void Error(string message)
        {
            Show(message, Severity.Error);
        }

        public void Warning(string message)
        {
            Show(message, Severity.Warning);
        }

        public void Info(string message)
        {
            Show(message, Severity.Info);
        }

        public void Show(
            string message,
            Severity severity,
            string position = Defaults.Classes.Position.TopEnd)
        {
            _snackbar.Configuration.PositionClass = position;
            _snackbar?.Add(message, severity);
        }
    }
}
