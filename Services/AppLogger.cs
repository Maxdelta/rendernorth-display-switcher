namespace RenderNorth.DisplaySwitcher.Services;

internal sealed class AppLogger
{
    private readonly object _gate = new();
    public string LogFolder { get; } = Path.Combine(AppContext.BaseDirectory, "logs");
    private string LogFile => Path.Combine(LogFolder, $"display-switcher-{DateTime.Now:yyyyMMdd}.log");

    public AppLogger() => Directory.CreateDirectory(LogFolder);
    public void Info(string message) => Write("INFO", message, null);
    public void Error(string message, Exception? ex = null) => Write("ERROR", message, ex);

    private void Write(string level, string message, Exception? ex)
    {
        var line = $"{DateTimeOffset.Now:O} [{level}] {message}{(ex is null ? "" : Environment.NewLine + ex)}{Environment.NewLine}";
        lock (_gate) File.AppendAllText(LogFile, line);
    }
}
