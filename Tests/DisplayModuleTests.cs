using RenderNorth.DisplaySwitcher.Domain.Modules;
using RenderNorth.DisplaySwitcher.Models;
using RenderNorth.DisplaySwitcher.Modules.Display;
using RenderNorth.DisplaySwitcher.Native;
using Xunit;

namespace RenderNorth.Environments.Tests;

public sealed class DisplayModuleTests
{
    [Fact]
    public void SaveAndLoad_PreservesLegacyDisplayConfiguration()
    {
        var configuration = Configuration(10, 20, "DISPLAY\\ONE");
        var source = new DisplayModule(new DisplayModuleService());
        source.SetConfiguration(configuration);
        var document = source.Save();
        var restored = new DisplayModule(new DisplayModuleService());
        restored.Load(document);
        Assert.Equal(DisplayModule.Type, document.Type);
        Assert.Equal(configuration.MachineName, restored.Configuration.MachineName);
        Assert.Equal(configuration.Targets[0].MonitorDevicePath, restored.Configuration.Targets[0].MonitorDevicePath);
        Assert.Equal(configuration.Paths[0].SourceInfo.Id, restored.Configuration.Paths[0].SourceInfo.Id);
    }

    [Fact]
    public void Matches_ComparesSourceToTargetTopologyIndependentOfPathOrder()
    {
        var first = Configuration(10, 20, "DISPLAY\\ONE");
        first.Paths.Add(Path(11, 21));
        var second = Configuration(11, 21, "DISPLAY\\TWO");
        second.Paths.Add(Path(10, 20));
        Assert.True(new DisplayModuleService().Matches(first, second));
    }

    [Fact]
    public void Matches_RejectsDifferentTopology()
    {
        Assert.False(new DisplayModuleService().Matches(Configuration(10, 20, "DISPLAY\\ONE"), Configuration(10, 99, "DISPLAY\\ONE")));
    }

    private static DisplayProfile Configuration(uint source, uint target, string path) => new()
    {
        MachineName = Environment.MachineName,
        Paths = [Path(source, target)],
        Targets = [new SavedTargetIdentity { MonitorDevicePath = path, FriendlyName = "Test Display" }]
    };

    private static DisplayConfigPathInfo Path(uint source, uint target) => new()
    {
        SourceInfo = new DisplayConfigPathSourceInfo { AdapterId = new Luid { LowPart = 1 }, Id = source },
        TargetInfo = new DisplayConfigPathTargetInfo { AdapterId = new Luid { LowPart = 1 }, Id = target }
    };
}
