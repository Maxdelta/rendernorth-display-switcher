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
            var displayService = new DisplayModuleService();
            registry.Register(DisplayModule.Type, () => new DisplayModule(displayService));
            var manager = new EnvironmentManager(repository, registry, log);
            var command = new CommandLineService(manager, log).ExecuteAsync(args).GetAwaiter().GetResult();
            if (!command.ShowGui) return command.ExitCode;

            using var singleInstance = new Mutex(true, "RenderNorthDisplaySwitcher", out var ownsMutex);
            if (!ownsMutex)
            {
                const string message = "RenderNorth Environments is already running.";
                log.Error(message); MessageBox.Show(message, "RenderNorth Environments"); return 3;
            }

            ApplicationConfiguration.Initialize();
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (_, e) => ShowFatal(log, e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (_, e) => ShowFatal(log, e.ExceptionObject as Exception ?? new Exception("Unknown fatal error"));
            ModuleDocument CaptureDisplays()
            {
                var module = new DisplayModule(displayService); module.SetConfiguration(displayService.Capture()); return module.Save();
            }
            Application.Run(new MainForm(manager, new UpdateService(log), log, CaptureDisplays,
                () => displayService.DescribeTargets(displayService.Capture()),
                () => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("ms-settings:display") { UseShellExecute = true }),
                ShortcutService.CreateDefault()));
            return 0;
        }
        catch (Exception exception)
        {
            log.Error("Environment initialization or command execution failed", exception);
            if (args.Length > 0) return CommandLineService.ActivationFailedExitCode;
            MessageBox.Show($"RenderNorth Environments could not initialize.\n\n{exception.Message}\n\nSee: {log.LogFolder}",
                "RenderNorth Environments", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return CommandLineService.ActivationFailedExitCode;
        }

    }

    private static void ShowFatal(AppLogger log, Exception exception)
    {
        log.Error("Unhandled error", exception);
        MessageBox.Show($"Unexpected failure. No display change was requested.\n\n{exception.Message}\n\nSee: {log.LogFolder}",
            "RenderNorth Environments", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
