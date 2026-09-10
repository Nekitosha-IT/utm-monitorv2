namespace EGAISInspector.Core.Services;

public static class AppLogger
{
    private static readonly object Sync = new();
    public static string LogDirectory { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EGAISInspector", "logs");
    public static string CurrentLogFile => Path.Combine(LogDirectory, $"egais-inspector-{DateTime.Now:yyyy-MM-dd}.log");

    public static void Info(string message) => Write("INFO", message);
    public static void Warning(string message) => Write("WARN", message);
    public static void Error(string message, Exception? exception = null) => Write("ERROR", exception is null ? message : $"{message} | {exception.GetType().Name}: {exception.Message}");

    private static void Write(string level, string message)
    {
        try
        {
            Directory.CreateDirectory(LogDirectory);
            lock (Sync)
            {
                File.AppendAllText(CurrentLogFile, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}");
            }
        }
        catch
        {
            // Logging must never terminate the application.
        }
    }
}
