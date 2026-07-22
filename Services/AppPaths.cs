using Velopack.Locators;

namespace RenderNorth.DisplaySwitcher.Services;

internal static class AppPaths
{
    private static bool _initialized;

    public static bool IsPortable => VelopackLocator.Current.IsPortable || VelopackLocator.Current.RootAppDir is null;
    public static string DataFolder => IsPortable
        ? AppContext.BaseDirectory
        : Path.Combine(RequireRootAppDir(), "UserData");
    public static string ProfilesFolder => Path.Combine(DataFolder, "profiles");
    public static string LogsFolder => Path.Combine(DataFolder, "logs");

    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        Directory.CreateDirectory(ProfilesFolder);
        Directory.CreateDirectory(LogsFolder);
        if (!IsPortable) MigrateLegacyProfiles();
    }

    internal static void MigrateLegacyProfiles()
    {
        var root = RequireRootAppDir();
        if (!Directory.Exists(root)) return;

        IEnumerable<string> candidates;
        try
        {
            candidates = Directory.EnumerateDirectories(root, "profiles", SearchOption.AllDirectories)
                .Where(path => !string.Equals(path, ProfilesFolder, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }
        catch
        {
            return;
        }

        Directory.CreateDirectory(ProfilesFolder);
        foreach (var sourceFolder in candidates)
        {
            foreach (var fileName in new[] { "game.json", "script.json", "status.json" })
            {
                var source = Path.Combine(sourceFolder, fileName);
                var destination = Path.Combine(ProfilesFolder, fileName);
                if (!File.Exists(source) || File.Exists(destination)) continue;
                try { File.Copy(source, destination, false); }
                catch { }
            }
        }
    }

    private static string RequireRootAppDir() => VelopackLocator.Current.RootAppDir
        ?? throw new InvalidOperationException("Velopack did not provide an installed application root.");
}
