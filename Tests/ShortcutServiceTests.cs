using RenderNorth.DisplaySwitcher.Domain.Environments;
using RenderNorth.DisplaySwitcher.Services;
using Xunit;

namespace RenderNorth.Environments.Tests;

public sealed class ShortcutServiceTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "RenderNorthShortcutTests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void Shortcut_UsesStableRootTargetAndGuidArgument()
    {
        var writer = new RecordingWriter(); var root = Path.Combine(_folder, "stable", "RenderNorthDisplaySwitcher.exe");
        var service = new ShortcutService(root, Path.Combine(_folder, "managed.json"), writer); var environment = Environment("Gaming");
        service.Create(environment, Path.Combine(_folder, "shortcuts"));
        Assert.Equal(Path.GetFullPath(root), writer.Last!.TargetPath);
        Assert.Equal($"--environment-id {environment.Id:D}", writer.Last.Arguments);
        Assert.DoesNotContain("current", writer.Last.TargetPath, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Rename_DoesNotChangeLaunchCommandIdentity()
    {
        var service = Service(); var environment = Environment("Gaming"); var before = service.LaunchCommand(environment);
        environment.Name = "Studio"; var after = service.LaunchCommand(environment);
        Assert.Equal(before, after); Assert.Contains(environment.Id.ToString(), after);
    }

    [Fact]
    public void Recreate_UsesRenamedFileButSameGuidArgument()
    {
        var writer = new RecordingWriter(); var service = Service(writer); var environment = Environment("Gaming");
        service.Create(environment, Path.Combine(_folder, "shortcuts")); environment.Name = "Studio";
        var recreated = Assert.Single(service.Recreate(environment));
        Assert.Contains("Studio", recreated); Assert.Equal($"--environment-id {environment.Id:D}", writer.Last!.Arguments);
    }

    private ShortcutService Service(IShortcutWriter? writer = null) => new(Path.Combine(_folder, "root", "RenderNorthDisplaySwitcher.exe"), Path.Combine(_folder, "managed.json"), writer ?? new RecordingWriter());
    private static EnvironmentDefinition Environment(string name) => new() { Id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), Name = name };
    public void Dispose() { if (Directory.Exists(_folder)) Directory.Delete(_folder, true); }
    private sealed class RecordingWriter : IShortcutWriter { public ShortcutSpecification? Last { get; private set; } public void Write(ShortcutSpecification specification) => Last = specification; }
}
