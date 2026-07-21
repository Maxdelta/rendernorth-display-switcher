using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RenderNorth.GameModeLauncher;

internal static class Program
{
    [STAThread]
    private static int Main() => Launcher.Start("--game", "Game Mode");
}

internal static class Launcher
{
    private const string MainExecutable = "RenderNorthDisplaySwitcher.exe";

    internal static int Start(string argument, string modeName)
    {
        try
        {
            var executable = Path.Combine(AppContext.BaseDirectory, MainExecutable);
            if (!File.Exists(executable))
            {
                ShowError($"Could not find {MainExecutable}.\n\nKeep this launcher in the same folder as the main application.");
                return 1;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = executable,
                Arguments = argument,
                WorkingDirectory = AppContext.BaseDirectory,
                UseShellExecute = false
            });
            return 0;
        }
        catch (Exception exception)
        {
            ShowError($"Could not launch {modeName}.\n\n{exception.Message}");
            return 1;
        }
    }

    private static void ShowError(string message) => MessageBox(IntPtr.Zero, message,
        "RenderNorth Display Switcher", 0x00000010);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr window, string text, string caption, uint type);
}
