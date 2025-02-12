namespace BlazorCMS.Admin.Services
{
    public class UILoggerService
    {
        private readonly List<string> _logs = new();

        public event Action OnLogUpdated;

        public void Log(string message)
        {
            var logMessage = $"[{DateTime.UtcNow:HH:mm:ss}] {message}";
            _logs.Insert(0, logMessage); // Insert latest logs at the top
            OnLogUpdated?.Invoke();
        }

        public IReadOnlyList<string> GetLogs() => _logs;
    }
}
