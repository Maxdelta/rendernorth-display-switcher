using System.Runtime.InteropServices;
using Velopack.Locators;

namespace RenderNorth.DisplaySwitcher.Services;

internal static class InstalledIntegration
{
    private const string MainExecutable = "RenderNorthDisplaySwitcher.exe";
    private const string GameShortcut = "RenderNorth Display Switcher - Game Mode.lnk";
    private const string ScriptShortcut = "RenderNorth Display Switcher - Script Mode.lnk";

    public static void InstallOrRefresh()
    {
        if (VelopackLocator.Current.IsPortable) return;
        AppPaths.MigrateLegacyProfiles();
        CreateShortcut(GameShortcut, "--game", "Silently activate the saved Game Mode display profile.");
        CreateShortcut(ScriptShortcut, "--script", "Silently activate the saved Script Mode display profile.");
    }

    public static void RemoveShortcuts()
    {
        DeleteShortcut(GameShortcut);
        DeleteShortcut(ScriptShortcut);
    }

    private static void CreateShortcut(string fileName, string arguments, string description)
    {
        var programs = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
        if (string.IsNullOrWhiteSpace(programs)) return;
        Directory.CreateDirectory(programs);

        var root = VelopackLocator.Current.RootAppDir
            ?? throw new InvalidOperationException("Velopack did not provide an installed application root.");
        var stableExecutable = Path.Combine(root, MainExecutable);
        var content = VelopackLocator.Current.AppContentDir
            ?? throw new InvalidOperationException("Velopack did not provide an application content directory.");
        var icon = Path.Combine(content, MainExecutable);
        var shortcutPath = Path.Combine(programs, fileName);
        var shellType = Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new InvalidOperationException("Windows Script Host is unavailable.");
        dynamic? shell = null;
        dynamic? shortcut = null;
        try
        {
            shell = Activator.CreateInstance(shellType);
            shortcut = shell!.CreateShortcut(shortcutPath);
            shortcut.TargetPath = stableExecutable;
            shortcut.Arguments = arguments;
            shortcut.WorkingDirectory = root;
            shortcut.Description = description;
            shortcut.IconLocation = icon + ",0";
            shortcut.Save();
        }
        finally
        {
            if (shortcut is not null && Marshal.IsComObject(shortcut)) Marshal.FinalReleaseComObject(shortcut);
            if (shell is not null && Marshal.IsComObject(shell)) Marshal.FinalReleaseComObject(shell);
        }
    }

    private static void DeleteShortcut(string fileName)
    {
        var programs = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
        if (string.IsNullOrWhiteSpace(programs)) return;
        var path = Path.Combine(programs, fileName);
        if (File.Exists(path)) File.Delete(path);
    }
}
