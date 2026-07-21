using RenderNorth.DisplaySwitcher.Services;
using RenderNorth.DisplaySwitcher.UI;
using Velopack;

namespace RenderNorth.DisplaySwitcher;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        VelopackApp.Build().Run();
        var automaticMode = args.Length > 0;
        var log = new AppLogger();
        using var singleInstance = new Mutex(true, "RenderNorthDisplaySwitcher", out var ownsMutex);
        if (!ownsMutex)
        {
            const string message = "RenderNorth Display Switcher is already running.";
            log.Error(message);
            if (!automaticMode)
                MessageBox.Show(message, "RenderNorth Display Switcher");
            return 3;
        }

        if (automaticMode)
        {
            var command = args.Length == 1 ? args[0].ToLowerInvariant() : "";
            if (command is not ("--game" or "--script"))
            {
                log.Error($"Invalid automatic-mode arguments: {string.Join(' ', args)}");
                return 2;
            }

            try
            {
                var profile = command == "--game" ? ProfileKind.Game : ProfileKind.Script;
                var result = new DisplayProfileService(log).Activate(profile);
                if (!result.Success)
                    log.Error($"Automatic {profile} Mode activation failed: {result.Message.Replace(Environment.NewLine, " | ")}");
                return result.Success ? 0 : 1;
            }
            catch (Exception exception)
            {
                log.Error("Unhandled automatic-mode failure", exception);
                return 1;
            }
        }

        ApplicationConfiguration.Initialize();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => ShowFatal(log, e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => ShowFatal(log, e.ExceptionObject as Exception ?? new Exception("Unknown fatal error"));

        var service = new DisplayProfileService(log);
        var updates = new UpdateService(log);
        Application.Run(new MainForm(service, updates, log));
        return 0;
    }

    private static void ShowFatal(AppLogger log, Exception exception)
    {
        log.Error("Unhandled error", exception);
        MessageBox.Show($"Unexpected failure. No display change was requested.\n\n{exception.Message}\n\nSee: {log.LogFolder}",
            "RenderNorth Display Switcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
