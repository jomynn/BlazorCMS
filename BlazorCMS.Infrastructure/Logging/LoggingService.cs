using System;
using System.IO;
using System.Threading.Tasks;

namespace BlazorCMS.Infrastructure.Logging
{
    public class LoggingService
    {
        private readonly string _logFilePath;

        public LoggingService()
        {
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "app.log");
            Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath));
        }

        public async Task LogAsync(string message)
        {
            string logMessage = $"{DateTime.UtcNow}: {message}{Environment.NewLine}";
            await File.AppendAllTextAsync(_logFilePath, logMessage);
        }
    }
}
