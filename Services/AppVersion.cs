using System.Reflection;

namespace RenderNorth.DisplaySwitcher.Services;

internal static class AppVersion
{
    public static string Current { get; } =
        (Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.3.0");
}
