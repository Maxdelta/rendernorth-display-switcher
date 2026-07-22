using RenderNorth.DisplaySwitcher.Domain.Modules;
using RenderNorth.DisplaySwitcher.Modules.Display;
using RenderNorth.DisplaySwitcher.Services;
using RenderNorth.DisplaySwitcher.UI;
using Velopack;

namespace RenderNorth.DisplaySwitcher;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        VelopackApp.Build()
            .OnAfterInstallFastCallback(_ => InstalledIntegration.InstallOrRefresh())
            .OnAfterUpdateFastCallback(_ => InstalledIntegration.InstallOrRefresh())
            .OnBeforeUninstallFastCallback(_ => InstalledIntegration.RemoveShortcuts())
            .Run();
        AppPaths.Initialize();
        var log = new AppLogger();

        try
        {
            var repository = new EnvironmentRepository();
            new LegacyMigrationService(repository, AppPaths.DataFolder, log).MigrateIfNeeded();
            var registry = new EnvironmentModuleRegistry();
            registry.Register(DisplayModule.Type, () => new DisplayModule(new DisplayModuleService()));
            var manager = new EnvironmentManager(repository, registry, log);
            var command = new CommandLineService(manager, log).ExecuteAsync(args).GetAwaiter().GetResult();
            if (!command.ShowGui) return command.ExitCode;
        }
        catch (Exception exception)
        {
            log.Error("Environment initialization or command execution failed", exception);
            if (args.Length > 0) return CommandLineService.ActivationFailedExitCode;
            MessageBox.Show($"RenderNorth Environments could not initialize.\n\n{exception.Message}\n\nSee: {log.LogFolder}",
                "RenderNorth Environments", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return CommandLineService.ActivationFailedExitCode;
        }

        using var singleInstance = new Mutex(true, "RenderNorthDisplaySwitcher", out var ownsMutex);
        if (!ownsMutex)
        {
            const string message = "RenderNorth Display Switcher is already running.";
            log.Error(message); MessageBox.Show(message, "RenderNorth Display Switcher"); return 3;
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
