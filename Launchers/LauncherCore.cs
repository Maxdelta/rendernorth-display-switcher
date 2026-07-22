using System.Diagnostics;

namespace RenderNorth.Launchers;

internal static class LauncherCore
{
    private const string MainExecutable = "RenderNorthDisplaySwitcher.exe";

    internal static int Start(string argument, string modeName)
    {
        var baseDirectory = AppContext.BaseDirectory;
        var executable = Path.GetFullPath(Path.Combine(baseDirectory, MainExecutable));
        try
        {
            Log($"{modeName} launcher started. BaseDirectory={baseDirectory}; Executable={executable}; Argument={argument}");
            if (!File.Exists(executable))
                throw new FileNotFoundException($"Could not find {MainExecutable} beside the launcher.", executable);

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = executable,
                ArgumentList = { argument },
                WorkingDirectory = baseDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }) ?? throw new InvalidOperationException("Windows did not start the display switcher process.");

            process.WaitForExit();
            if (process.ExitCode != 0)
                Log($"{modeName} activation returned exit code {process.ExitCode}.");
            else
                Log($"{modeName} activation completed successfully.");
            return process.ExitCode;
        }
        catch (Exception exception)
        {
            Log($"{modeName} launcher failed. {exception}");
            return 1;
        }
    }

    private static void Log(string message)
    {
        try
        {
            var baseDirectory = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
            var parent = Directory.GetParent(baseDirectory)?.FullName;
            var installed = string.Equals(Path.GetFileName(baseDirectory), "current", StringComparison.OrdinalIgnoreCase)
                && parent is not null;
            var logFolder = installed
                ? Path.Combine(parent!, "UserData", "logs")
                : Path.Combine(baseDirectory, "logs");
            Directory.CreateDirectory(logFolder);
            var logFile = Path.Combine(logFolder, $"launcher-{DateTime.Now:yyyyMMdd}.log");
            File.AppendAllText(logFile, $"{DateTimeOffset.Now:O} {message}{Environment.NewLine}");
        }
        catch
        {
            // A logging failure must not interrupt display switching.
        }
    }
}
