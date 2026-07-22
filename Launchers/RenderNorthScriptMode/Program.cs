using RenderNorth.Launchers;

namespace RenderNorth.ScriptModeLauncher;

internal static class Program
{
    [STAThread]
    private static int Main() => LauncherCore.Start("--script", "Script Mode");
}
