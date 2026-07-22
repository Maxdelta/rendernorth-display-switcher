using System.Runtime.InteropServices;
using System.Text.Json;
using RenderNorth.DisplaySwitcher.Domain.Environments;
using Velopack.Locators;

namespace RenderNorth.DisplaySwitcher.Services;

internal sealed record ShortcutSpecification(string Path, string TargetPath, string Arguments, string WorkingDirectory, string Description, string IconLocation);
internal sealed class ManagedShortcutCollection { public int SchemaVersion { get; set; } = 1; public List<ManagedShortcut> Shortcuts { get; set; } = []; }
internal sealed class ManagedShortcut { public Guid EnvironmentId { get; set; } public string Path { get; set; } = ""; }

internal interface IShortcutWriter { void Write(ShortcutSpecification specification); }

internal sealed class WindowsShortcutWriter : IShortcutWriter
{
    public void Write(ShortcutSpecification specification)
    {
        var shellType = Type.GetTypeFromProgID("WScript.Shell") ?? throw new InvalidOperationException("Windows Script Host is unavailable.");
        dynamic? shell = null; dynamic? shortcut = null;
        try
        {
            shell = Activator.CreateInstance(shellType); shortcut = shell!.CreateShortcut(specification.Path);
            shortcut.TargetPath = specification.TargetPath; shortcut.Arguments = specification.Arguments;
            shortcut.WorkingDirectory = specification.WorkingDirectory; shortcut.Description = specification.Description;
            shortcut.IconLocation = specification.IconLocation; shortcut.Save();
        }
        finally
        {
            if (shortcut is not null && Marshal.IsComObject(shortcut)) Marshal.FinalReleaseComObject(shortcut);
            if (shell is not null && Marshal.IsComObject(shell)) Marshal.FinalReleaseComObject(shell);
        }
    }
}

internal sealed class ShortcutService
{
    private const string MainExecutable = "RenderNorthDisplaySwitcher.exe";
    private readonly string _stableExecutable;
    private readonly string _registryPath;
    private readonly IShortcutWriter _writer;
    private readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true, WriteIndented = true };

    public ShortcutService(string stableExecutable, string registryPath, IShortcutWriter? writer = null)
    {
        _stableExecutable = Path.GetFullPath(stableExecutable); _registryPath = registryPath; _writer = writer ?? new WindowsShortcutWriter();
    }

    public static ShortcutService CreateDefault()
    {
        var root = VelopackLocator.Current.RootAppDir;
        var executable = root is null ? Path.Combine(AppContext.BaseDirectory, MainExecutable) : Path.Combine(root, MainExecutable);
        return new ShortcutService(executable, Path.Combine(AppPaths.DataFolder, "managed-shortcuts.json"));
    }

    public string CreateDesktop(EnvironmentDefinition environment) => Create(environment, Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
    public string CreateStartMenu(EnvironmentDefinition environment) => Create(environment, Environment.GetFolderPath(Environment.SpecialFolder.Programs));
    public string CreateCustom(EnvironmentDefinition environment, string folder) => Create(environment, folder);

    public string Create(EnvironmentDefinition environment, string folder)
    {
        if (string.IsNullOrWhiteSpace(folder)) throw new ArgumentException("Shortcut folder is required.", nameof(folder));
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, FileName(environment));
        _writer.Write(Specification(environment, path)); Register(environment.Id, path); return path;
    }

    public IReadOnlyList<string> Recreate(EnvironmentDefinition environment)
    {
        var registry = LoadRegistry(); var entries = registry.Shortcuts.Where(item => item.EnvironmentId == environment.Id).ToArray(); var paths = new List<string>();
        foreach (var entry in entries)
        {
            var folder = Path.GetDirectoryName(entry.Path); if (string.IsNullOrWhiteSpace(folder)) continue;
            Directory.CreateDirectory(folder); var newPath = Path.Combine(folder, FileName(environment));
            _writer.Write(Specification(environment, newPath));
            if (!string.Equals(entry.Path, newPath, StringComparison.OrdinalIgnoreCase) && File.Exists(entry.Path)) File.Delete(entry.Path);
            entry.Path = newPath; paths.Add(newPath);
        }
        SaveRegistry(registry); return paths;
    }

    public int DeleteManaged(Guid environmentId)
    {
        var registry = LoadRegistry(); var entries = registry.Shortcuts.Where(item => item.EnvironmentId == environmentId).ToArray();
        foreach (var entry in entries) if (File.Exists(entry.Path)) File.Delete(entry.Path);
        registry.Shortcuts.RemoveAll(item => item.EnvironmentId == environmentId); SaveRegistry(registry); return entries.Length;
    }

    public IReadOnlyList<string> ManagedFolders(Guid environmentId) => LoadRegistry().Shortcuts.Where(item => item.EnvironmentId == environmentId)
        .Select(item => Path.GetDirectoryName(item.Path)).Where(path => !string.IsNullOrWhiteSpace(path)).Select(path => path!).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    public string LaunchCommand(EnvironmentDefinition environment) => $"\"{_stableExecutable}\" --environment-id {environment.Id:D}";
    internal ShortcutSpecification Specification(EnvironmentDefinition environment, string path) => new(path, _stableExecutable,
        $"--environment-id {environment.Id:D}", Path.GetDirectoryName(_stableExecutable)!, $"Activate the {environment.Name} environment.", _stableExecutable + ",0");

    private void Register(Guid environmentId, string path)
    {
        var registry = LoadRegistry(); if (!registry.Shortcuts.Any(item => item.EnvironmentId == environmentId && string.Equals(item.Path, path, StringComparison.OrdinalIgnoreCase))) registry.Shortcuts.Add(new ManagedShortcut { EnvironmentId = environmentId, Path = path }); SaveRegistry(registry);
    }
    private ManagedShortcutCollection LoadRegistry()
    {
        if (!File.Exists(_registryPath)) return new ManagedShortcutCollection();
        var registry = JsonSerializer.Deserialize<ManagedShortcutCollection>(File.ReadAllText(_registryPath), _json) ?? throw new InvalidDataException("Managed shortcut registry is empty.");
        if (registry.SchemaVersion != 1) throw new InvalidDataException($"Unsupported shortcut registry schema {registry.SchemaVersion}."); return registry;
    }
    private void SaveRegistry(ManagedShortcutCollection registry)
    {
        var folder = Path.GetDirectoryName(_registryPath)!; Directory.CreateDirectory(folder); var temporary = _registryPath + ".tmp";
        try { File.WriteAllText(temporary, JsonSerializer.Serialize(registry, _json)); File.Move(temporary, _registryPath, true); }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
    private static string FileName(EnvironmentDefinition environment)
    {
        var invalid = Path.GetInvalidFileNameChars().ToHashSet(); var safe = new string(environment.Name.Select(character => invalid.Contains(character) ? '_' : character).ToArray()).Trim();
        if (safe.Length == 0) safe = "Environment"; return $"RenderNorth Environments - {safe} - {environment.Id:N}.lnk";
    }
}
