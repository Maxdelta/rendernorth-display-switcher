using RenderNorth.DisplaySwitcher.Services;
using RenderNorth.DisplaySwitcher.UI;

namespace RenderNorth.DisplaySwitcher;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        using var singleInstance = new Mutex(true, "RenderNorthDisplaySwitcher", out var ownsMutex);
        if (!ownsMutex)
        {
            MessageBox.Show("RenderNorth Display Switcher is already running.", "RenderNorth Display Switcher");
            return;
        }

        var log = new AppLogger();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => ShowFatal(log, e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => ShowFatal(log, e.ExceptionObject as Exception ?? new Exception("Unknown fatal error"));

        var service = new DisplayProfileService(log);
        var command = args.FirstOrDefault()?.ToLowerInvariant();
        if (command is "--game" or "--script")
        {
            var profile = command == "--game" ? ProfileKind.Game : ProfileKind.Script;
            var result = service.Activate(profile);
            MessageBox.Show(result.Message, "RenderNorth Display Switcher",
                MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            Environment.ExitCode = result.Success ? 0 : 1;
            return;
        }

        if (args.Length > 0)
        {
            MessageBox.Show("Usage: RenderNorthDisplaySwitcher.exe [--game|--script]", "RenderNorth Display Switcher",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Environment.ExitCode = 2;
            return;
        }

        Application.Run(new MainForm(service, log));
    }

    private static void ShowFatal(AppLogger log, Exception exception)
    {
        log.Error("Unhandled error", exception);
        MessageBox.Show($"Unexpected failure. No display change was requested.\n\n{exception.Message}\n\nSee: {log.LogFolder}",
            "RenderNorth Display Switcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
