using RenderNorth.Launchers;

namespace RenderNorth.GameModeLauncher;

internal static class Program
{
    [STAThread]
    private static int Main() => LauncherCore.Start("--game", "Game Mode");
}
